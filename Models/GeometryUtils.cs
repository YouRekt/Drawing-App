using System;
using System.Collections.Generic;

namespace DrawingAppCG.Models
{
    public static class GeometryUtils
    {
        public static double DotProduct((double x, double y) v1, (double x, double y) v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }
        public static bool IsConvex(List<(int x, int y)> points)
        {
            int n = points.Count;
            if (n < 3)
                return false;

            double angleSum = 0;
            // start with last edge → first vertex
            var (oldX, oldY) = points[n - 2];
            var (newX, newY) = points[n - 1];
            double newDir = Math.Atan2(newY - oldY, newX - oldX);
            double orientation = 1;

            for (int i = 0; i < n; i++)
            {
                var (px, py) = points[i];
                // advance edge
                oldX = newX; oldY = newY;
                double oldDir = newDir;

                newX = px; newY = py;
                newDir = Math.Atan2(newY - oldY, newX - oldX);

                // zero‐length edge?
                if (oldX == newX && oldY == newY)
                    return false;

                double angle = newDir - oldDir;
                // normalize into (-π, π]
                if (angle <= -Math.PI) angle += 2 * Math.PI;
                else if (angle > Math.PI) angle -= 2 * Math.PI;

                if (i == 0)
                {

                    if (angle == 0) return false;
                    orientation = angle > 0 ? 1 : -1;
                }
                else
                {
                    if (orientation * angle <= 0)
                        return false;
                }

                angleSum += angle;
            }

            // total turn should be ±2π
            return Math.Abs(Math.Round(angleSum / (Math.PI * 2))) == 1;
        }
        public static int GetWindingDirection(List<(int x, int y)> points)
        {
            int area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var (x0, y0) = points[i];
                var (x1, y1) = points[(i + 1) % points.Count];
                area += (x1 - x0) * (y1 + y0);
            }

            return area < 0 ? -1 : 1; // CCW = -1, CW = 1
        }
    }
}
