namespace OpenX.Gest.Domain.ValueObjects;

/// <summary>
/// Rappresenta le coordinate geografiche puntuali rilevate all'atto di una timbratura.
/// </summary>
/// <remarks>
/// CONFORMITÀ LEGALE SVIZZERA (OLL 3 art. 26 e nLPD):
/// In ossequio all'art. 26 dell'Ordinanza 3 concernente la legge sul lavoro (OLL 3) e ai principi
/// della nuova Legge federale sulla protezione dei dati (nLPD), è severamente vietato qualsiasi
/// sistema di sorveglianza continuativa del comportamento del lavoratore.
/// Pertanto, la geolocalizzazione in Open-X Gest è rigorosamente PUNTUALE: viene acquisita unicamente
/// come istantanea al momento esatto del click di timbratura (entrata o uscita) per validare la presenza,
/// e non viene mai eseguito alcun tracciamento periodico, continuativo o in background.
/// </remarks>
public record GpsCoordinate
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? AccuracyMeters { get; init; }
    public DateTime TimestampUtc { get; init; }

    // Costruttore per EF Core
    protected GpsCoordinate() { }

    public GpsCoordinate(double latitude, double longitude, double? accuracyMeters = null, DateTime? timestampUtc = null)
    {
        if (latitude is < -90.0 or > 90.0)
            throw new ArgumentOutOfRangeException(nameof(latitude), "La latitudine deve essere compresa tra -90 e 90 gradi.");

        if (longitude is < -180.0 or > 180.0)
            throw new ArgumentOutOfRangeException(nameof(longitude), "La longitudine deve essere compresa tra -180 e 180 gradi.");

        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        TimestampUtc = timestampUtc ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Calcola la distanza geodetica approssimativa in metri rispetto ad un'altra coordinata GPS via formula dell'emisenoverso (Haversine).
    /// </summary>
    /// <param name="other">Coordinata geografica di destinazione.</param>
    /// <returns>Distanza calcolata in metri.</returns>
    public double DistanceToInMeters(GpsCoordinate other)
    {
        ArgumentNullException.ThrowIfNull(other);

        const double earthRadiusMeters = 6371000.0;
        double dLat = (other.Latitude - Latitude) * Math.PI / 180.0;
        double dLon = (other.Longitude - Longitude) * Math.PI / 180.0;

        double lat1Rad = Latitude * Math.PI / 180.0;
        double lat2Rad = other.Latitude * Math.PI / 180.0;

        double a = (Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0)) +
                   (Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0) * Math.Cos(lat1Rad) * Math.Cos(lat2Rad));
        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        return earthRadiusMeters * c;
    }
}
