namespace OpenX.Gest.Domain.Enums;

/// <summary>
/// Durata massima settimanale di lavoro stabilita dall'Art. 9 della Legge federale sul lavoro (LL).
/// </summary>
public enum StatutoryWeeklyLimit
{
    /// <summary>
    /// 45 ore settimanali per i lavoratori nelle aziende industriali, per il personale d'ufficio,
    /// per il personale tecnico e per altro personale, compreso il personale di vendita nelle grandi aziende del commercio al dettaglio.
    /// </summary>
    Hours45 = 45,

    /// <summary>
    /// 50 ore settimanali per tutti gli altri lavoratori.
    /// </summary>
    Hours50 = 50
}
