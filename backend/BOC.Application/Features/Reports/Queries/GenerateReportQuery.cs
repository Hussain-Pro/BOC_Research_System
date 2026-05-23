using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Reports.Queries;

public record GenerateReportQuery(string ReportType, DateTime? StartDate, DateTime? EndDate) : IRequest<ReportResultDto>;

public class ReportResultDto
{
    public Stream FileStream { get; set; } = null!;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class GenerateReportQueryHandler : IRequestHandler<GenerateReportQuery, ReportResultDto>
{
    private readonly IBOCDbContext _context;
    public GenerateReportQueryHandler(IBOCDbContext context) => _context = context;

    public async Task<ReportResultDto> Handle(GenerateReportQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException(); // Placeholder
    }
}
