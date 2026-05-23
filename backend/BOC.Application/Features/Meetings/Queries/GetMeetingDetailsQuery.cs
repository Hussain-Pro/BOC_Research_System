using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Enums;

namespace BOC.Application.Features.Meetings.Queries;

public record AttendeeDto(
    Guid MemberId,
    string FullName,
    string RoleName,
    bool Attended
);

public record MeetingPaperDto(
    Guid Id,
    string Title,
    string TrackingNumber,
    string ResearcherName,
    decimal? FinalScore,
    string State,
    List<double> EvaluatorScores,
    double? ChairmanScore
);

public record MeetingVoteDto(
    Guid ResearchId,
    Guid MemberId,
    string MemberName,
    string VoteValue
);

public record MeetingDetailsDto(
    Guid Id,
    string MeetingNumber,
    string Title,
    DateTime ScheduledDate,
    string Location,
    string Status,
    List<AttendeeDto> Attendees,
    List<MeetingPaperDto> Papers,
    List<MeetingVoteDto> Votes,
    Guid? MinutesId,
    string? MinutesContent,
    string MinutesStatus
);

public record GetMeetingDetailsQuery(Guid MeetingId) : IRequest<MeetingDetailsDto>;

public class GetMeetingDetailsQueryHandler : IRequestHandler<GetMeetingDetailsQuery, MeetingDetailsDto>
{
    private readonly IBOCDbContext _context;

    public GetMeetingDetailsQueryHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<MeetingDetailsDto> Handle(GetMeetingDetailsQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .AsNoTracking()
            .Include(m => m.Attendances)
                .ThenInclude(a => a.Member)
                    .ThenInclude(u => u.Role)
            .Include(m => m.Votes)
                .ThenInclude(v => v.Member)
            .Include(m => m.Minutes)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken)
            ?? throw new ValidationException("Meeting not found.");

        var papers = await _context.ResearchPapers
            .AsNoTracking()
            .Include(p => p.Researcher)
            .Include(p => p.EvaluatorAssignments)
                .ThenInclude(a => a.Evaluations)
            .Include(p => p.ChairmanScores)
            .Where(p => p.MeetingId == request.MeetingId)
            .ToListAsync(cancellationToken);

        var attendeeDtos = meeting.Attendances.Select(a => new AttendeeDto(
            a.MemberId,
            a.Member.FullName,
            a.Member.Role.Name,
            a.Attended
        )).ToList();

        var paperDtos = papers.Select(p => {
            var evalScores = p.EvaluatorAssignments
                .SelectMany(a => a.Evaluations)
                .Select(e => (double)e.Score)
                .ToList();

            var chScore = p.ChairmanScores
                .OrderByDescending(cs => cs.SubmittedAt)
                .Select(cs => (double?)cs.Score)
                .FirstOrDefault();

            return new MeetingPaperDto(
                p.Id,
                p.Title,
                p.TrackingNumber,
                p.Researcher.FullName,
                p.FinalScore,
                p.State.ToString(),
                evalScores,
                chScore
            );
        }).ToList();

        var voteDtos = meeting.Votes.Select(v => new MeetingVoteDto(
            v.ResearchId,
            v.MemberId,
            v.Member.FullName,
            v.VoteValue.ToString()
        )).ToList();

        var minutes = meeting.Minutes.FirstOrDefault();

        return new MeetingDetailsDto(
            meeting.Id,
            meeting.MeetingNumber,
            meeting.Title ?? "N/A",
            meeting.ScheduledDate,
            meeting.Location ?? "N/A",
            meeting.Status.ToString(),
            attendeeDtos,
            paperDtos,
            voteDtos,
            minutes?.Id,
            minutes?.Content,
            minutes?.Status.ToString() ?? "None"
        );
    }
}
