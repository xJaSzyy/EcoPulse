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

    public static string GetPollutionLevelByConcentration(float concentration)
    {
        var color = PollutionLevelMap[225.4];
        foreach (var pair in PollutionLevelMap.Where(pair => concentration <= pair.Key))
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
    
    private static readonly SortedDictionary<double, string> PollutionLevelMap = new()
    {
        { 9999, "экстремальный"},
        { 225.4, "очень высокий" },
        { 125.4, "высокий" },
        { 55.4, "средний" },
        { 35.4, "низкий" },
        { 9.0, "очень низкий" }
    };
}