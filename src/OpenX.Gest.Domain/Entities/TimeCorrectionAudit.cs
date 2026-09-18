using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Domain.Entities;

/// <summary>
/// Entità immutabile di audit per qualsiasi rettifica apportata a una timbratura di presenza.
/// Conforme all'obbligo di tracciabilità e conservazione quinquennale (Art. 73 OLL 1).
/// </summary>
public class TimeCorrectionAudit : BaseEntity
{
    public Guid TimeEntryId { get; private set; }
    public Guid OperatorId { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    public DateTime PreCorrectionClockInUtc { get; private set; }
    public DateTime? PreCorrectionClockOutUtc { get; private set; }
    public int PreCorrectionBreakMinutes { get; private set; }

    public DateTime PostCorrectionClockInUtc { get; private set; }
    public DateTime? PostCorrectionClockOutUtc { get; private set; }
    public int PostCorrectionBreakMinutes { get; private set; }

    /// <summary>
    /// Motivazione obbligatoria per legge a giustificazione della modifica retroattiva.
    /// </summary>
    public string MandatoryReason { get; private set; } = string.Empty;

    public string? IpAddress { get; private set; }

    protected TimeCorrectionAudit() { }

    public TimeCorrectionAudit(
        Guid id,
        Guid timeEntryId,
        Guid operatorId,
        DateTime timestampUtc,
        DateTime preClockInUtc,
        DateTime? preClockOutUtc,
        int preBreakMinutes,
        DateTime postClockInUtc,
        DateTime? postClockOutUtc,
        int postBreakMinutes,
        string mandatoryReason,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(mandatoryReason))
            throw new ArgumentException("La motivazione della rettifica è obbligatoria.", nameof(mandatoryReason));

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        TimeEntryId = timeEntryId;
        OperatorId = operatorId;
        TimestampUtc = timestampUtc;

        PreCorrectionClockInUtc = preClockInUtc;
        PreCorrectionClockOutUtc = preClockOutUtc;
        PreCorrectionBreakMinutes = preBreakMinutes;

        PostCorrectionClockInUtc = postClockInUtc;
        PostCorrectionClockOutUtc = postClockOutUtc;
        PostCorrectionBreakMinutes = postBreakMinutes;

        MandatoryReason = mandatoryReason.Trim();
        IpAddress = ipAddress;
    }
}
