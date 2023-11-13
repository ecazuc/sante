using System.Collections.Generic;
using UnityEngine;

namespace Dornell {
    public static class Geometry {
        public static bool IsPolygonClockwise(in IList<Vector2> polygon) {
            // check polygon orientation. See https://en.wikipedia.org/wiki/Curve_orientation
            // /!\ last point in the node list is the same as the first one => we shall ignore the last node
            Vector2 minPoint = polygon[0];
            int minPointIndex = 0;

            // select a point in convex hull by taking the lowest leftmost point. 
            for (int i = 1; i < polygon.Count - 1; ++i) {
                Vector2 pos = polygon[i];
                if (pos.x < minPoint.x || (pos.x == minPoint.y && pos.y < minPoint.y)) {
                    minPoint = pos;
                    minPointIndex = i;
                }
            }
            // compute determinant of the angle at this vertex
            Vector2 previous = polygon[(minPointIndex > 0) ? (minPointIndex - 1) : (polygon.Count - 2)]; // skip last node
            Vector2 next = polygon[(minPointIndex + 1) % (polygon.Count - 1)];
            float determinant = (minPoint.x - previous.x) * (next.y - previous.y) - (next.x - previous.x) * (minPoint.y - previous.y);
            return determinant < 0;
        }

    }
}