namespace OpenX.Gest.Domain.Enums;

public enum TimeEntryStatus
{
    Open = 1,
    Completed = 2,
    PendingCorrection = 3,
    Approved = 4
}

public enum LanguageCode
{
    It = 1,
    De = 2,
    Fr = 3,
    En = 4
}

public static class LanguageCodeExtensions
{
    public static string ToLocaleCode(this LanguageCode lang) => lang switch
    {
        LanguageCode.It => "it-CH",
        LanguageCode.De => "de-CH",
        LanguageCode.Fr => "fr-CH",
        LanguageCode.En => "en-US",
        _ => "it-CH"
    };

    public static LanguageCode FromCode(string? code) => (code?.ToLowerInvariant()) switch
    {
        "de" or "de-ch" or "de-de" => LanguageCode.De,
        "fr" or "fr-ch" or "fr-fr" => LanguageCode.Fr,
        "en" or "en-us" or "en-gb" => LanguageCode.En,
        _ => LanguageCode.It
    };
}

[Flags]
public enum ViolationType
{
    None = 0,
    DailyRestPeriodViolated = 1, // < 11h riposo consecutivo (Art. 15a LL)
    DailyAmplitudeExceeded = 2,  // > 14h ampiezza massima (Art. 10 LL)
    InsufficientBreak = 4,       // Pausa obbligatoria non rispettata (Art. 15 LL)
    StatutoryOvertime = 8        // Superamento ore massime di legge Überzeit (Art. 12 LL)
}
