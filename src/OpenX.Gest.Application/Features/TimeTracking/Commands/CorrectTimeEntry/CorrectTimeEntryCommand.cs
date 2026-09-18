using MediatR;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;

/// <summary>
/// Command for retroactive manual correction of a time entry with legally mandatory justification (Art. 73 OLL 1).
/// </summary>
public record CorrectTimeEntryCommand(
    Guid TimeEntryId,
    Guid OperatorId,
    DateTime NewClockInUtc,
    DateTime? NewClockOutUtc,
    int NewBreakMinutes,
    string MandatoryReason
) : IRequest<Result<TimeEntryDto>>;
