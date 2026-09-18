namespace OpenX.Gest.Domain.Enums;

/// <summary>
/// Regimi di registrazione della durata del lavoro previsti dall'Ordinanza 1 concernente la legge sul lavoro (OLL 1).
/// </summary>
public enum Oll1Regime
{
    /// <summary>
    /// Registrazione ordinaria completa di inizio, pause e fine del lavoro (Art. 73 OLL 1).
    /// </summary>
    StandardRecord = 1,

    /// <summary>
    /// Registrazione semplificata della sola durata complessiva del lavoro giornaliero (Art. 73a OLL 1).
    /// Applicabile a collaboratori con autonomia nella fissazione del proprio orario di lavoro.
    /// </summary>
    SimplifiedRecord = 2,

    /// <summary>
    /// Rinuncia alla registrazione della durata del lavoro (Art. 73b OLL 1).
    /// Riservata a quadri dirigenti e specialisti con retribuzione annua lorda > CHF 120'000.
    /// </summary>
    OptOut = 3
}
