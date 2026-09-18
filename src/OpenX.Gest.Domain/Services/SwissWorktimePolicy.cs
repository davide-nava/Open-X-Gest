using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Domain.Services;

/// <summary>
/// Domain Service che incapsula le regole legali e le formule di calcolo del Diritto del Lavoro Svizzero
/// (LL art. 9, 10, 12, 13, 15, 15a, 16, 17, 18, 19 e relative ordinanze OLL 1 e OLL 3).
/// </summary>
public class SwissWorktimePolicy
{
    private static readonly TimeZoneInfo SwissTimeZone = GetSwissTimeZone();

    private static TimeZoneInfo GetSwissTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); // Windows ID
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich"); // IANA / Linux ID
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }

    /// <summary>
    /// Calcola la pausa minima obbligatoria secondo l'art. 15 LL.
    /// - più di 5.5 ore: almeno 15 minuti
    /// - più di 7.0 ore: almeno 30 minuti
    /// - più di 9.0 ore: almeno 60 minuti
    /// Le pause inferiori a 15 minuti non interrompono l'orario di lavoro (art. 18 OLL 1).
    /// </summary>
    public int CalculateStatutoryBreakMinutes(double workedHours)
    {
        if (workedHours > 9.0)
            return 60;
        if (workedHours > 7.0)
            return 30;
        if (workedHours > 5.5)
            return 15;
        return 0;
    }

    /// <summary>
    /// Verifica se l'ampiezza giornaliera (intervallo dal primo inizio all'ultima fine, comprese pause)
    /// rispetta il limite massimo di 14 ore consecutive (art. 10 LL / art. 13 OLL 1).
    /// </summary>
    public (double AmplitudeHours, bool Exceeded) EvaluateDailyAmplitude(DateTime startUtc, DateTime endUtc)
    {
        var duration = (endUtc - startUtc).TotalHours;
        var exceeded = duration > 14.0;
        return (Math.Round(duration, 2), exceeded);
    }

    /// <summary>
    /// Verifica se è stato rispettato il riposo giornaliero minimo di 11 ore consecutive (art. 15a LL / art. 19 OLL 1)
    /// tra la fine del turno precedente e l'inizio del presente turno.
    /// </summary>
    public (double RestHours, bool Violated) EvaluateDailyRestPeriod(DateTime previousShiftEndUtc, DateTime currentShiftStartUtc)
    {
        var restHours = (currentShiftStartUtc - previousShiftEndUtc).TotalHours;
        var violated = restHours < 11.0;
        return (Math.Round(restHours, 2), violated);
    }

    /// <summary>
    /// Calcola le ore di lavoro notturno svolte nell'intervallo 23:00 - 06:00 (fuso svizzero Europe/Zurich)
    /// soggette a supplemento del 25% (art. 16/17 LL).
    /// </summary>
    public double CalculateNightHours(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
        {
            return 0;
        }

        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, SwissTimeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, SwissTimeZone);

        double nightMinutes = 0;
        var current = startLocal;

        while (current < endLocal)
        {
            var next = current.AddMinutes(1);
            var hour = current.Hour;
            // Intervallo notturno: dalle 23:00 alle 06:00
            if (hour >= 23 || hour < 6)
            {
                nightMinutes += 1;
            }
            current = next;
        }

        return Math.Round(nightMinutes / 60.0, 2);
    }

    /// <summary>
    /// Calcola le ore di lavoro domenicale svolte tra le 23:00 del sabato e le 23:00 della domenica (fuso svizzero)
    /// soggette a supplemento salariale del 50% (art. 18/19 LL).
    /// </summary>
    public double CalculateSundayHours(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
        {
            return 0;
        }

        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, SwissTimeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, SwissTimeZone);

        double sundayMinutes = 0;
        var current = startLocal;

        while (current < endLocal)
        {
            var next = current.AddMinutes(1);
            var dayOfWeek = current.DayOfWeek;
            var hour = current.Hour;

            // Sabato dopo le 23:00
            bool isSaturdayNight = dayOfWeek == DayOfWeek.Saturday && hour >= 23;
            // Domenica fino alle 23:00
            bool isSundayDay = dayOfWeek == DayOfWeek.Sunday && hour < 23;

            if (isSaturdayNight || isSundayDay)
            {
                sundayMinutes += 1;
            }

            current = next;
        }

        return Math.Round(sundayMinutes / 60.0, 2);
    }

    /// <summary>
    /// Ripartisce il totale delle ore settimanali lavorate tra:
    /// - Ore ordinarie (fino all'orario contrattuale, es. 40h o 42h)
    /// - Lavoro supplementare (*Überstunden*, art. 321c CO): ore tra il contrattuale e il massimo di legge (45h/50h)
    /// - Lavoro straordinario di legge (*Überzeit*, art. 12/13 LL): ore eccedenti le 45h/50h massime legali (+25% o riposo compensativo)
    /// </summary>
    public WorktimeBreakdown SplitWorkHours(
        decimal totalWorkedHours,
        decimal contractualWeeklyHours,
        StatutoryWeeklyLimit statutoryLimit)
    {
        decimal legalCeiling = (int)statutoryLimit;
        decimal contractHours = contractualWeeklyHours > 0 ? contractualWeeklyHours : 40.0m;

        decimal ordinaryHours = Math.Min(totalWorkedHours, contractHours);
        decimal remaining = Math.Max(0, totalWorkedHours - ordinaryHours);

        decimal supplementaryHours = 0; // Überstunden
        decimal statutoryOvertimeHours = 0; // Überzeit

        if (contractHours < legalCeiling)
        {
            decimal maxSupplementary = legalCeiling - contractHours;
            supplementaryHours = Math.Min(remaining, maxSupplementary);
            statutoryOvertimeHours = Math.Max(0, remaining - supplementaryHours);
        }
        else
        {
            statutoryOvertimeHours = remaining;
        }

        return new WorktimeBreakdown(
            Math.Round(totalWorkedHours, 2),
            Math.Round(ordinaryHours, 2),
            Math.Round(supplementaryHours, 2),
            Math.Round(statutoryOvertimeHours, 2)
        );
    }
}

public record WorktimeBreakdown(
    decimal TotalWorkedHours,
    decimal OrdinaryHours,
    decimal SupplementaryHours, // Überstunden (CO art. 321c)
    decimal StatutoryOvertimeHours // Überzeit (LL art. 12/13)
);
