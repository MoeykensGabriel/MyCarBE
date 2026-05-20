using MyCarBE.Application.Common.Models;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardRawData> GetRawDataAsync(CancellationToken cancellationToken = default);
}
