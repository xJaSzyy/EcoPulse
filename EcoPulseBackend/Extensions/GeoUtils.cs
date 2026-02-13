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
    
    public static double Distance(Point p1, Point p2)
    {
        const double R = 6371000; 
        
        double lat1 = p1.Y * Math.PI / 180;
        double lon1 = p1.X * Math.PI / 180;
        double lat2 = p2.Y * Math.PI / 180;
        double lon2 = p2.X * Math.PI / 180;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    public static double Distance(Coordinate c1, Coordinate c2)
    {
        return Distance(new Point(c1.X, c1.Y), new Point(c2.X, c2.Y));
    }
}