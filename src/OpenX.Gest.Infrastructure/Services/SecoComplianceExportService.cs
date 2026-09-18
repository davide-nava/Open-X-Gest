using System.Globalization;
using System.Text;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Domain.Services;

namespace OpenX.Gest.Infrastructure.Services;

/// <summary>
/// Servizio di esportazione report per gli ispettorati cantonali del lavoro e SECO
/// conforme all'Art. 73 OLL 1 (obbligo di registrazione delle ore di lavoro).
/// </summary>
public class SecoComplianceExportService : ISecoComplianceExportService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SwissWorktimePolicy _policy;

    public SecoComplianceExportService(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _policy = new SwissWorktimePolicy();
    }

    public Task<byte[]> GenerateCsvReportAsync(
        Employee employee,
        List<TimeEntry> entries,
        DateTime startUtc,
        DateTime endUtc,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        // Intestazione report SECO
        var lang = languageCode.ToLowerInvariant();
        var headers = GetHeaders(lang);

        sb.AppendLine($"# SECO / Cantonal Labor Inspectorate Compliance Report - Art. 73 OLL 1");
        sb.AppendLine($"# Employee: {employee.LastName} {employee.FirstName} ({employee.Email})");
        sb.AppendLine($"# Department: {employee.Department}");
        sb.AppendLine($"# Contractual Weekly Hours: {employee.ContractualWeeklyHours}h | Statutory Ceiling: {(int)employee.StatutoryWeeklyLimit}h (Art. 9 LL)");
        sb.AppendLine($"# OLL 1 Regime: {employee.Oll1Regime}");
        sb.AppendLine($"# Period: {startUtc:yyyy-MM-dd} to {endUtc:yyyy-MM-dd}");
        sb.AppendLine();

        sb.AppendLine(string.Join(";", headers));

        double totalNet = 0;
        double totalNight = 0;
        double totalSunday = 0;
        int totalBreaks = 0;

        foreach (var entry in entries.OrderBy(e => e.ClockInUtc))
        {
            var clockInSwiss = _dateTimeProvider.ToSwissTime(entry.ClockInUtc);
            var clockOutSwiss = entry.ClockOutUtc.HasValue ? _dateTimeProvider.ToSwissTime(entry.ClockOutUtc.Value) : (DateTime?)null;

            var endUtcVal = entry.ClockOutUtc ?? _dateTimeProvider.UtcNow;
            var durationMinutes = (endUtcVal - entry.ClockInUtc).TotalMinutes;
            var netMinutes = Math.Max(0, durationMinutes - entry.BreakDurationMinutes);
            var netHours = Math.Round(netMinutes / 60.0, 2);

            var nightHours = _policy.CalculateNightHours(entry.ClockInUtc, endUtcVal);
            var sundayHours = _policy.CalculateSundayHours(entry.ClockInUtc, endUtcVal);

            totalNet += netHours;
            totalNight += nightHours;
            totalSunday += sundayHours;
            totalBreaks += entry.BreakDurationMinutes;

            var violationsList = new List<string>();
            if (entry.DailyRestPeriodViolated)
            {
                violationsList.Add("Rest <11h (Art. 15a LL)");
            }

            if (entry.DailyAmplitudeExceeded)
            {
                violationsList.Add("Amplitude >14h (Art. 10 LL)");
            }

            if (entry.Violations.HasFlag(ViolationType.InsufficientBreak))
            {
                violationsList.Add("Break insufficient (Art. 15 LL)");
            }

            var violationsStr = violationsList.Count > 0 ? string.Join("|", violationsList) : "OK";

            var line = string.Join(";",
                clockInSwiss.ToString("yyyy-MM-dd"),
                clockInSwiss.ToString("HH:mm:ss"),
                clockOutSwiss?.ToString("HH:mm:ss") ?? "IN PROGRESS",
                entry.BreakDurationMinutes.ToString(),
                netHours.ToString("F2", CultureInfo.InvariantCulture),
                nightHours.ToString("F2", CultureInfo.InvariantCulture),
                sundayHours.ToString("F2", CultureInfo.InvariantCulture),
                violationsStr,
                entry.AuditTrail.Count.ToString(),
                entry.Notes?.Replace(";", " ") ?? string.Empty);

            sb.AppendLine(line);
        }

        sb.AppendLine();
        sb.AppendLine($"# TOTALS;Net Hours: {totalNet:F2};Breaks (min): {totalBreaks};Night Hours: {totalNight:F2};Sunday Hours: {totalSunday:F2}");

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public Task<byte[]> GenerateInspectionSummaryPdfAsync(
        Employee employee,
        List<TimeEntry> entries,
        DateTime startUtc,
        DateTime endUtc,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        // Genera un documento testuale formattato conforme ai requisiti dell'ispettorato SECO
        var sb = new StringBuilder();
        sb.AppendLine("================================================================================");
        sb.AppendLine("   SCHEDA DI REGISTRAZIONE DELL'ORARIO DI LAVORO (SECO - Art. 73 OLL 1)");
        sb.AppendLine("   RAPPORT DE CONTRÔLE DU TEMPS DE TRAVAIL / ARBEITSZEITERFASSUNG");
        sb.AppendLine("================================================================================");
        sb.AppendLine($"Collaboratore / Employé / Arbeitnehmer : {employee.LastName} {employee.FirstName}");
        sb.AppendLine($"Email: {employee.Email} | Reparto: {employee.Department}");
        sb.AppendLine($"Orario contrattuale: {employee.ContractualWeeklyHours}h | Limite max LL art. 9: {(int)employee.StatutoryWeeklyLimit}h");
        sb.AppendLine($"Regime OLL 1 applicato: {employee.Oll1Regime}");
        sb.AppendLine($"Periodo di rilevamento: {startUtc:dd.MM.yyyy} - {endUtc:dd.MM.yyyy}");
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine(string.Format("{0,-12} | {1,-8} | {2,-8} | {3,-6} | {4,-8} | {5,-10} | {6,-15}",
            "Data", "Inizio", "Fine", "Pausa", "Netto (h)", "Notturno", "Stato Legale"));
        sb.AppendLine("--------------------------------------------------------------------------------");

        double totalNet = 0;
        int violationsCount = 0;

        foreach (var e in entries.OrderBy(x => x.ClockInUtc))
        {
            var inSwiss = _dateTimeProvider.ToSwissTime(e.ClockInUtc);
            var outSwiss = e.ClockOutUtc.HasValue ? _dateTimeProvider.ToSwissTime(e.ClockOutUtc.Value) : (DateTime?)null;
            var endUtcVal = e.ClockOutUtc ?? _dateTimeProvider.UtcNow;
            var netHours = Math.Max(0, (endUtcVal - e.ClockInUtc).TotalHours - (e.BreakDurationMinutes / 60.0));
            var night = _policy.CalculateNightHours(e.ClockInUtc, endUtcVal);

            var status = "CONFORME";
            if (e.DailyRestPeriodViolated || e.DailyAmplitudeExceeded)
            {
                status = "NON CONFORME";
                violationsCount++;
            }

            totalNet += netHours;

            sb.AppendLine(string.Format("{0,-12} | {1,-8} | {2,-8} | {3,-6} | {4,-8:F2} | {5,-10:F2} | {6,-15}",
                inSwiss.ToString("dd.MM.yyyy"),
                inSwiss.ToString("HH:mm"),
                outSwiss?.ToString("HH:mm") ?? "--:--",
                $"{e.BreakDurationMinutes}m",
                netHours,
                night,
                status));
        }

        sb.AppendLine("================================================================================");
        sb.AppendLine($"Totale ore lavorate nel periodo: {totalNet:F2} ore");
        sb.AppendLine($"Totale anomalie legali rilevate: {violationsCount}");
        sb.AppendLine("================================================================================");
        sb.AppendLine("Dichiarazione di conformità SECO:");
        sb.AppendLine("Il presente documento attesta la registrazione delle presenze ai sensi degli artt. 46 LL");
        sb.AppendLine("e 73 dell'Ordinanza 1 concernente la legge sul lavoro. I dati sono conservati per 5 anni.");
        sb.AppendLine();
        sb.AppendLine("Firma del Datore di Lavoro: _______________________    Data: _______________");
        sb.AppendLine("Firma del Collaboratore:    _______________________    Data: _______________");

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string[] GetHeaders(string lang) => lang switch
    {
        "de" => ["Datum", "Beginn", "Ende", "Pause_Min", "Netto_Stunden", "Nachtarbeit_Stunden", "Sonntagsarbeit_Stunden", "Gesetzesverstoss", "Korrekturen", "Notizen"],
        "fr" => ["Date", "Debut", "Fin", "Pause_Min", "Heures_Nettes", "Heures_Nuit", "Heures_Dimanche", "Infractions_Legales", "Corrections", "Notes"],
        "en" => ["Date", "Start_Time", "End_Time", "Break_Min", "Net_Hours", "Night_Hours", "Sunday_Hours", "Violations", "Corrections", "Notes"],
        _ => ["Data", "Ora_Inizio", "Ora_Fine", "Pausa_Min", "Ore_Nette", "Ore_Notturne", "Ore_Domenicali", "Anomalie_Legge", "Rettifiche_Audit", "Note"]
    };
}
