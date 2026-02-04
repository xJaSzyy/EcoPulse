namespace EcoPulseBackend.Extensions;

public static class DangerZoneUtils
{
    public static string GetColorByConcentration(float concentration)
    {
        var color = ColorMap[225.4];
        foreach (var pair in ColorMap.Where(pair => concentration <= pair.Key))
        {
            color = pair.Value;
            break;
        }

        return color;
    }
    
    private static readonly SortedDictionary<double, string> ColorMap = new()
    {
        { 9999, "rgba(138, 79, 163, 1)"},
        { 225.4, "rgba(164, 125, 184, 1)" },
        { 125.4, "rgba(246, 104, 106, 1)" },
        { 55.4, "rgba(251, 153, 86, 1)" },
        { 35.4, "rgba(248, 212, 97, 1)" },
        { 9.0, "rgba(171, 209, 98, 1)" }
    };
}