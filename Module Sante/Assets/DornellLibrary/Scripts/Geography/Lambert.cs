using System;
using System.Collections.Generic;
using System.Text;

namespace Dornell.Geography {
    using FloatValue = System.Double;
    using MathLib = System.Math;

    public class LambertCCProjection {
        public struct Constants {
            public FloatValue n;  // exposant de la projection
            public FloatValue c;  // constante de la projection
            public FloatValue e;  // premiere excentricite de l'ellipsoide
            public FloatValue lonC;    // longitude de l'origine par rapport au meridien origine
            public FloatValue Xs; // coordonnee en projection du pole
            public FloatValue Ys; // coordonnee en projection du pole
            public FloatValue epsilon;  // tolerance de convergence
        }
        // from https://geodesie.ign.fr/contenu/fichiers/documentation/algorithmes/notice/NTG_71.pdf

        protected Constants C;

        protected const FloatValue Rad2Deg = 180 / MathLib.PI;
        protected const FloatValue Deg2Rad = MathLib.PI / 180;

        public void SetConstants(Constants constants) {
            C = constants;
        }

        public FloatValue LatToIsoLat(FloatValue lat) {
            FloatValue eSinLat = C.e * MathLib.Sin(lat);
            return MathLib.Log(MathLib.Tan(MathLib.PI / 4 + lat / 2) * MathLib.Pow((1 - eSinLat) / (1 + eSinLat), C.e / 2));
        }
        public FloatValue IsoLatToLat(FloatValue L) {
            FloatValue expL = MathLib.Exp(L);
            FloatValue lat0, lat1;
            lat1 = 2 * MathLib.Atan(expL) - MathLib.PI / 2;
            int i = 0;
            do {
                lat0 = lat1;
                FloatValue eSinLat0 = C.e * MathLib.Sin(lat0);
                lat1 = 2 * MathLib.Atan(MathLib.Pow((1 + eSinLat0) / (1 - eSinLat0), C.e / 2) * expL) - MathLib.PI / 2;
                ++i;
            } while (MathLib.Abs(lat1 - lat0) > C.epsilon);
            return lat1;
        }

        public (FloatValue x, FloatValue y) GeographicToCartesian(FloatValue latDeg, FloatValue lonDeg) {
            FloatValue lat = latDeg * Deg2Rad;
            FloatValue lon = lonDeg * Deg2Rad;
            FloatValue L = LatToIsoLat(lat);
            FloatValue cExpnL = C.c * MathLib.Exp(-C.n * L);
            FloatValue x = C.Xs + cExpnL * MathLib.Sin(C.n * (lon - C.lonC));
            FloatValue y = C.Ys - cExpnL * MathLib.Cos(C.n * (lon - C.lonC));
            return (x, y);
        }

        public (FloatValue lat, FloatValue lon) CartesianToGeographic(FloatValue x, FloatValue y) {
            FloatValue R = MathLib.Sqrt((x - C.Xs) * (x - C.Xs) + (y - C.Ys) * (y - C.Ys));
            FloatValue gamma = MathLib.Atan((x - C.Xs) / (C.Ys - y));
            FloatValue lon = C.lonC + gamma / C.n;
            FloatValue L = -1 / C.n * MathLib.Log(MathLib.Abs(R / C.c));
            FloatValue lat = IsoLatToLat(L);
            return (lat * Rad2Deg, lon * Rad2Deg);
        }
    }

    public class Lambert93 : LambertCCProjection {
        public Lambert93() {
            C.n = 0.7256077650;  // exposant de la projection
            C.c = 11754255.426;  // constante de la projection
            C.e = 0.08181919106;    // premiere excentricite de l'ellipsoide
            C.lonC = 3.0 * Deg2Rad;    // meridien central de la projection : 3 degres Est
            C.Xs = 700000.0; // coordonnee en projection du pole
            C.Ys = 12655612.050; // coordonnee en projection du pole
            C.epsilon = 1e-11;  // tolerance de convergence
        }
    }


}
