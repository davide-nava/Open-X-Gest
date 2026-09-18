using OpenX.Gest.Domain.ValueObjects;

namespace OpenX.Gest.Application.UnitTests.Domain;

public class GpsCoordinateTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_ShouldCreateInstance()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var gps = new GpsCoordinate(46.0037, 8.9511, 12.5, timestamp);

        // Assert
        gps.Latitude.Should().Be(46.0037);
        gps.Longitude.Should().Be(8.9511);
        gps.AccuracyMeters.Should().Be(12.5);
        gps.TimestampUtc.Should().Be(timestamp);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Constructor_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException(double invalidLat)
    {
        var act = () => new GpsCoordinate(invalidLat, 8.9511);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("latitude");
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void Constructor_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException(double invalidLon)
    {
        var act = () => new GpsCoordinate(46.0037, invalidLon);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("longitude");
    }

    [Fact]
    public void DistanceToInMeters_ShouldCalculateApproximateDistanceCorrectly()
    {
        // Lugano to Zurich is approx 155-160 km
        var lugano = new GpsCoordinate(46.0037, 8.9511);
        var zurich = new GpsCoordinate(47.3769, 8.5417);

        var distance = lugano.DistanceToInMeters(zurich);

        distance.Should().BeInRange(150000, 165000);
    }

    [Fact]
    public void ToString_ShouldFormatProperly()
    {
        var gps = new GpsCoordinate(46.0037, 8.9511, 10.0);
        var str = gps.ToString();
        str.Should().Contain("46.0037").And.Contain("8.9511");
    }
}
