using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.API.Services;

public class ApprovalLinkBuilder : IApprovalLinkBuilder
{
    private readonly string _baseUrl;

    public ApprovalLinkBuilder(IConfiguration configuration)
    {
        _baseUrl = configuration["AppSettings:ApprovalBaseUrl"]
                   ?? "http://localhost:5000/api/work-orders/approve";
    }

    public string Build(string token) => $"{_baseUrl}?token={token}";
}
