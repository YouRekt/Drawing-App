using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DrawingAppCG.Models
{
    public class Pill : ShapeBase
    {
        public (int x, int y) CenterA { get; set; }
        public (int x, int y) CenterB { get; set; }
        public int Radius { get; set; }
        public override bool ContainsPoint(int x, int y)
        {
            return Distance((x, y), CenterA) <= Radius || Distance((x, y), CenterB) <= Radius || Line.DistanceToLine(x, y, CenterA.x - P.x, CenterA.y + P.y, CenterB.x - P.x, CenterB.y + P.y) <= 5 || Line.DistanceToLine(x, y, CenterA.x + P.x, CenterA.y - P.y, CenterB.x + P.x, CenterB.y - P.y) <= 5;

            static double Distance((int x, int y) point1, (int x, int y) point2)
            {
                return Math.Sqrt(Math.Pow(point1.x - point2.x, 2) + Math.Pow(point1.y - point2.y, 2));
            }
        }
        private void DrawHalfCircles(WriteableBitmap bitmap, int x, int y)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    if (DotProduct((x, y), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x + x, CenterB.y + y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + x, CenterA.y + y, Color);
                    }
                    if (DotProduct((-x, y), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x - x, CenterB.y + y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - x, CenterA.y + y, Color);
                    }
                    if (DotProduct((x, -y), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x + x, CenterB.y - y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + x, CenterA.y - y, Color);
                    }
                    if (DotProduct((-x, -y), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x - x, CenterB.y - y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - x, CenterA.y - y, Color);
                    }
                    if (DotProduct((y, x), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x + y, CenterB.y + x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + y, CenterA.y + x, Color);
                    }
                    if (DotProduct((-y, x), (dx, dy)) > 0)
                    {

                        fb.SetPixel(CenterB.x - y, CenterB.y + x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - y, CenterA.y + x, Color);
                    }
                    if (DotProduct((y, -x), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x + y, CenterB.y - x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + y, CenterA.y - x, Color);
                    }
                    if (DotProduct((-y, -x), (dx, dy)) > 0)
                    {
                        fb.SetPixel(CenterB.x - y, CenterB.y - x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - y, CenterA.y - x, Color);
                    }
                }
            }
        }
        private static float DotProduct((int x, int y) vector1, (int x, int y) vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y;
        }
        [JsonIgnore]
        private int dx => CenterB.x - CenterA.x;
        [JsonIgnore]
        private int dy => CenterB.y - CenterA.y;
        [JsonIgnore]
        private (int x, int y) P => ((int)(dy / Math.Sqrt(dx * dx + dy * dy) * Radius), (int)(dx / Math.Sqrt(dx * dx + dy * dy) * Radius));
        public override void Draw(WriteableBitmap bitmap)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    var line1 = new Line
                    {
                        Start = (CenterA.x - P.x, CenterA.y + P.y),
                        End = (CenterB.x - P.x, CenterB.y + P.y),
                        Color = Color,
                    };
                    var line2 = new Line
                    {
                        Start = (CenterA.x + P.x, CenterA.y - P.y),
                        End = (CenterB.x + P.x, CenterB.y - P.y),
                        Color = Color,
                    };
                    line1.Draw(bitmap);
                    line2.Draw(bitmap);

                    int dE = 3;
                    int dSE = 5 - 2 * Radius;
                    int d = 1 - Radius;
                    int x = 0;
                    int y = Radius;

                    DrawHalfCircles(bitmap, x, y);
                    while (y > x)
                    {
                        if (d < 0)
                        {
                            d += dE;
                            dE += 2;
                            dSE += 2;
                        }
                        else
                        {
                            d += dSE;
                            dE += 2;
                            dSE += 4;
                            --y;
                        }
                        ++x;
                        DrawHalfCircles(bitmap, x, y);
                    }
                }
            }
        }
        public override List<(int x, int y)> GetControlPoints() => [CenterA, CenterB, (CenterA.x + dx / 2 - P.x, CenterA.y + dy / 2 + P.y)];
        public override void Move(int deltaX, int deltaY)
        {
            CenterA = (CenterA.x + deltaX, CenterA.y + deltaY);
            CenterB = (CenterB.x + deltaX, CenterB.y + deltaY);
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            switch (pointIndex)
            {
                case 0:
                    CenterA = (newX, newY);
                    break;
                case 1:
                    CenterB = (newX, newY);
                    break;
                case 2:
                    (int x, int y) v = ((int)(newX - (CenterA.x + dx / 2)), (int)(newY - (CenterA.y + dy / 2)));

                    Radius = (int)Math.Sqrt(v.x * v.x + v.y * v.y);
                    break;
            }
        }
    }
}
