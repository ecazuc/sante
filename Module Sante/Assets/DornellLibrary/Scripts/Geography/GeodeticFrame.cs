using System;

namespace Dornell.Geography {

    public interface IGeodeticFrame {
        (float x, float y) GeographicToCartesian(float lat, float lon);
        (float lat, float lon) CartesianToGeographic(float x, float y);
    }

    public class DistanceGeodeticFrame : IGeodeticFrame {
        private float geographicToCartesianRatioX;
        private float geographicToCartesianRatioY;
        private float referenceLatitude;
        private float referenceLongitude;

        public DistanceGeodeticFrame(float lat, float lon) {
            referenceLatitude = lat;
            referenceLongitude = lon;

            (geographicToCartesianRatioX, geographicToCartesianRatioY) = GetLatLonDegreeToMetersRatio(lat);
        }

        public (float x, float y) GeographicToCartesian(float lat, float lon) {
            return ((lon - referenceLongitude) * geographicToCartesianRatioX, (lat - referenceLatitude) * geographicToCartesianRatioY);
        }

        public (float lat, float lon) CartesianToGeographic(float x, float y) {
            return (y / geographicToCartesianRatioY + referenceLatitude, x / geographicToCartesianRatioX + referenceLongitude);
        }

        public static (float rx, float ry) GetLatLonDegreeToMetersRatio(float lat) {
            // computations taken from https://en.wikipedia.org/wiki/Geographic_coordinate_system#Length_of_a_degree
#if NETSTANDARD2_1
            lat *= MathF.PI / 180.0f;
            float rx = 111412.84f * MathF.Cos(lat) - 93.5f * MathF.Cos(3 * lat) + 0.118f * MathF.Cos(5 * lat);
            float ry = 111132.92f - 559.82f * MathF.Cos(2 * lat) + 1.175f * MathF.Cos(4 * lat) - 0.0023f * MathF.Cos(6 * lat);
#else
            lat *= Convert.ToSingle(Math.PI) / 180.0f;
            float rx = Convert.ToSingle(111412.84f * Math.Cos(lat) - 93.5f * Math.Cos(3 * lat) + 0.118f * Math.Cos(5 * lat));
            float ry = Convert.ToSingle(111132.92f - 559.82f * Math.Cos(2 * lat) + 1.175f * Math.Cos(4 * lat) - 0.0023f * Math.Cos(6 * lat));
#endif
            return (rx, ry);
        }
    }

    public class GeodeticFrameLambert93 : IGeodeticFrame {
        private double referenceX;
        private double referenceY;
        private Lambert93 lambert93;

        public GeodeticFrameLambert93(float lat, float lon) {
            lambert93 = new Lambert93();
            (referenceX, referenceY) = lambert93.GeographicToCartesian(lat, lon);
        }

        public (float x, float y) GeographicToCartesian(float lat, float lon) {
            (double x, double y) = lambert93.GeographicToCartesian(lat, lon);
            return (Convert.ToSingle(x - referenceX), Convert.ToSingle(y - referenceY));
        }

        public (float lat, float lon) CartesianToGeographic(float x, float y) {
            (double lat, double lon) = lambert93.CartesianToGeographic(x + referenceX, y + referenceY);
            return (Convert.ToSingle(lat), Convert.ToSingle(lon));
        }

    }
}
