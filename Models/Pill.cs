using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return Distance((x, y), CenterA) <= Radius || Distance((x, y), CenterB) <= Radius;

            double Distance((int x, int y) point1, (int x, int y) point2)
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
                    (int dx, int dy) = (CenterB.x - CenterA.x,  CenterB.y - CenterA.y);

                    //Octant 2
                    if(DotProduct((x, y),(dx,dy)) > 0)
                    {
                        //fb.SetPixel(CenterA.x + x + dx, CenterA.y + y + dy, Color);
                        fb.SetPixel(CenterB.x + x, CenterB.y + y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + x, CenterA.y + y, Color);
                    }
                    //Octant 3
                    if(DotProduct((-x, y),(dx,dy)) > 0)
                    {
                        //fb.SetPixel(CenterA.x - x + dx, CenterA.y + y + dy, Color);
                        fb.SetPixel(CenterB.x - x, CenterB.y + y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - x, CenterA.y + y, Color);
                    }
                    //Octant 7
                    if (DotProduct((x, -y), (dx, dy)) > 0)
                    {
                        //fb.SetPixel(CenterB.x + x + dx, CenterB.y - y + dy, Color);
                        fb.SetPixel(CenterB.x + x, CenterB.y - y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + x, CenterA.y - y, Color);
                    }
                    //Octant 6
                    if (DotProduct((-x, -y), (dx, dy)) > 0)
                    {
                        //fb.SetPixel(CenterB.x - x + dx, CenterB.y - y + dy, Color);
                        fb.SetPixel(CenterB.x - x, CenterB.y - y, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - x, CenterA.y - y, Color);
                    }
                    //Octant 4
                    if (DotProduct((y, x), (dx, dy)) > 0)
                    {
                        //fb.SetPixel(CenterA.x + y + dx, CenterA.y + x + dy, Color);
                        fb.SetPixel(CenterB.x + y, CenterB.y + x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + y, CenterA.y + x, Color);
                    }
                    //Octant 1
                    if (DotProduct((-y, x), (dx, dy)) > 0)
                    {
                        //fb.SetPixel(CenterB.x - y + dx, CenterB.y + x + dy, Color);
                        fb.SetPixel(CenterB.x - y, CenterB.y + x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - y, CenterA.y + x, Color);
                    }
                    if (DotProduct((y, -x), (dx, dy)) > 0)
                    {
                        //fb.SetPixel(CenterA.x + y + dx, CenterA.y - x + dy, Color);
                        fb.SetPixel(CenterB.x + y, CenterB.y - x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x + y, CenterA.y - x, Color);
                    }
                    if (DotProduct((-y, -x), (dx, dy)) > 0)
                    {
                        //fb.SetPixel(CenterA.x - y + dx, CenterA.y - x + dy, Color);
                        fb.SetPixel(CenterB.x - y, CenterB.y - x, Color);
                    }
                    else
                    {
                        fb.SetPixel(CenterA.x - y, CenterA.y - x, Color);
                    }
                }
            }
            static float DotProduct((int x, int y) vector1, (int x, int y) vector2)
            {
                return vector1.x * vector2.x + vector1.y * vector2.y;
            }
        }

        public override void Draw(WriteableBitmap bitmap)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    (int dx, int dy) = (CenterA.x - CenterB.x, CenterA.y - CenterB.y);
                    System.Diagnostics.Debug.WriteLine($"dx: {dx}, dy: {dy}");
                    double length = Math.Sqrt(dx * dx + dy * dy);
                    (int x, int y) p1 = ((int)(-dy / length * Radius),(int)(dx / length * Radius));
                    (int x, int y) p2 = ((int)(dy / length * Radius),(int)(-dx / length * Radius));
                    //(int x, int y) p1 = ((int)(-dy / Math.Sqrt(dy * dy + dx * dx) * Radius), (int)(dx / Math.Sqrt(dy * dy + dx * dx) * Radius));
                    //(int x, int y) p2 = ((int)(dy / Math.Sqrt(dy * dy + dx * dx) * Radius), (int)(-dx / Math.Sqrt(dy * dy + dx * dx) * Radius));

                    var line1 = new Line
                    {
                        Start = (CenterA.x + p1.x, CenterA.y + p1.y),
                        End = (CenterB.x + p1.x, CenterB.y + p1.y),
                        Color = Color,
                    };
                    var line2 = new Line
                    {
                        Start = (CenterA.x + p2.x, CenterA.y + p2.y),
                        End = (CenterB.x + p2.x, CenterB.y + p2.y),
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

        public override List<(int x, int y)> GetControlPoints() => [CenterA, CenterB, (CenterA.x + Radius, CenterA.y + Radius), (CenterB.x + Radius, CenterB.y + Radius)];

        public override void Move(int deltaX, int deltaY)
        {
            CenterA = (CenterA.x + deltaX, CenterA.y + deltaY);
            CenterB = (CenterB.x + deltaX, CenterB.y + deltaY);
        }

        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            switch(pointIndex)
            {
                case 0:
                    CenterA = (newX, newY);
                    break;
                case 1:
                    CenterB = (newX, newY);
                    break;
                case 2:
                    Radius = (int)Math.Sqrt(Math.Pow(newX - CenterA.x, 2) + Math.Pow(newY - CenterA.y, 2));
                    break;
                case 3:
                    Radius = (int)Math.Sqrt(Math.Pow(newX - CenterA.x, 2) + Math.Pow(newY - CenterA.y, 2));
                    break;
            }
        }
    }
}
