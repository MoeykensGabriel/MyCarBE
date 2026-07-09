using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Enums;

namespace MyCarBE.API.Services;

/// <summary>
/// Cancela órdenes que quedaron esperando aprobación más allá de su QuoteExpiresAt.
///
/// Corre cada hora. Por cada WO vencida en AwaitingApproval:
///   - La transiciona a Cancelled con nota "Presupuesto vencido sin respuesta del cliente."
///   - El token de aprobación queda inutilizable (la WO ya no está en AwaitingApproval).
///
/// Fallas individuales (una WO con datos raros) no detienen el loop — se logean y se sigue.
/// Apagado limpio: respeta CancellationToken entre WOs y entre ticks.
/// </summary>
public sealed class QuoteExpirationCleanupService : BackgroundService
{
    /// <summary>Pausa inicial antes del primer tick (deja arrancar la app primero).</summary>
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);

    /// <summary>Frecuencia entre cleanups. Suficiente para un TTL de 14 días.</summary>
    private static readonly TimeSpan TickInterval = TimeSpan.FromHours(1);

    /// <summary>UserId marcador para eventos generados por este worker.</summary>
    private static readonly Guid SystemUserId = Guid.Empty;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuoteExpirationCleanupService> _logger;

    public QuoteExpirationCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<QuoteExpirationCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QuoteExpirationCleanup arrancando. Primer tick en {Delay}.", InitialDelay);

        try
        {
            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw; // shutdown
                }
                catch (Exception ex)
                {
                    // Un error global del tick no debe matar el servicio.
                    _logger.LogError(ex, "QuoteExpirationCleanup falló en este tick. Reintenta en {Interval}.", TickInterval);
                }

                await Task.Delay(TickInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("QuoteExpirationCleanup detenido por shutdown.");
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var dbContext           = sp.GetRequiredService<AppDbContext>();
        var workOrderRepository = sp.GetRequiredService<IWorkOrderRepository>();
        var unitOfWork          = sp.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;

        // Query mínima: solo IDs de candidatos. Cargamos cada WO con FullDetails después
        // para que ChangeStatus pueda validar reglas que dependen de las colecciones
        // (la regla de Completed mira services pendientes, etc).
        var candidateIds = await dbContext.WorkOrders
            .Where(w => w.CurrentStatus == WorkOrderStatus.AwaitingApproval
                     && w.QuoteExpiresAt != null
                     && w.QuoteExpiresAt < now)
            .Select(w => w.Id)
            .ToListAsync(cancellationToken);

        if (candidateIds.Count == 0)
        {
            _logger.LogDebug("QuoteExpirationCleanup: sin órdenes vencidas.");
            return;
        }

        _logger.LogInformation("QuoteExpirationCleanup: {Count} orden(es) vencida(s) detectada(s).", candidateIds.Count);

        var cancelledCount = 0;
        var failedCount    = 0;

        foreach (var id in candidateIds)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var workOrder = await workOrderRepository.GetWithFullDetailsAsync(id, cancellationToken);
                if (workOrder is null) continue;

                // Re-chequeo defensivo: alguien pudo haberla aprobado entre la query inicial
                // y este punto. Si ya no está en AwaitingApproval, skip silenciosamente.
                if (workOrder.CurrentStatus != WorkOrderStatus.AwaitingApproval) continue;
                if (workOrder.QuoteExpiresAt is null || workOrder.QuoteExpiresAt >= now) continue;

                workOrder.ChangeStatus(
                    WorkOrderStatus.Cancelled,
                    SystemUserId,
                    "Presupuesto vencido sin respuesta del cliente.");

                workOrderRepository.Update(workOrder);
                cancelledCount++;
            }
            catch (Exception ex)
            {
                failedCount++;
                _logger.LogError(ex, "QuoteExpirationCleanup: error procesando WO {WorkOrderId}.", id);
            }
        }

        if (cancelledCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "QuoteExpirationCleanup: {Cancelled} cancelada(s), {Failed} fallida(s).",
                cancelledCount, failedCount);
        }
    }
}
