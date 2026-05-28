using Mapster;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.InspectionReports.Mappings;

public class InspectionReportMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WorkOrderPhoto, InspectionReportPhotoDto>();
        config.NewConfig<InspectionReportProposedService, InspectionReportProposedServiceDto>();
        config.NewConfig<InspectionReportProposedPart,    InspectionReportProposedPartDto>();

        config.NewConfig<InspectionReport, InspectionReportDto>()
            .Map(d => d.AreaName,        s => s.Area.Name)
            .Map(d => d.MechanicFullName,
                 s => s.Mechanic == null ? null : (s.Mechanic.FirstName + " " + s.Mechanic.LastName).Trim())
            .Map(d => d.Photos,           s => s.Photos)
            .Map(d => d.ProposedServices, s => s.ProposedServices)
            .Map(d => d.ProposedParts,    s => s.ProposedParts);
    }
}
