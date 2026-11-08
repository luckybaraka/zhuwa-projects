namespace CalculatorAndConverter.Core;

public static class DistanceConverter
{
    private const double MilesPerKilometre = 0.621371;

    public static double KilometresToMiles(double kilometres) => kilometres * MilesPerKilometre;

    public static double MilesToKilometres(double miles) => miles / MilesPerKilometre;
}
