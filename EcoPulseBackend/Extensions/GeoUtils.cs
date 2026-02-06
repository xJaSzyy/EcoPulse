using EcoPulseBackend.Models;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Extensions;

public static class GeoUtils
{
    public static double CalculateHaversineLength(Coordinate[] coords)
    {
        double length = 0;
        const double R = 6371000; 

        for (int i = 1; i < coords.Length; i++)
        {
            var p1 = coords[i - 1];
            var p2 = coords[i];

            double dLat = (p2.Y - p1.Y) * Math.PI / 180;
            double dLon = (p2.X - p1.X) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(p1.Y * Math.PI / 180) * Math.Cos(p2.Y * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            length += R * c;
        }
        return length;
    }
}