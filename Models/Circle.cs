using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DrawingAppCG.Models
{
    public class Circle : ShapeBase
    {
        public (int x, int y) Center { get; set; }
        public int Radius { get; set; }
        private void DrawOctants(WriteableBitmap bitmap, int x, int y)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    fb.SetPixel(Center.x + x, Center.y + y, Color, Thickness,isMoreHorizontal(Center.x + x, Center.y + y));
                    fb.SetPixel(Center.x - x, Center.y + y, Color, Thickness,isMoreHorizontal(Center.x - x, Center.y + y));
                    fb.SetPixel(Center.x + x, Center.y - y, Color, Thickness, isMoreHorizontal(Center.x + x, Center.y - y));
                    fb.SetPixel(Center.x - x, Center.y - y, Color, Thickness, isMoreHorizontal(Center.x - x, Center.y - y));
                    fb.SetPixel(Center.x + y, Center.y + x, Color, Thickness, isMoreHorizontal(Center.x + y, Center.y + x));
                    fb.SetPixel(Center.x - y, Center.y + x, Color, Thickness, isMoreHorizontal(Center.x - y, Center.y + x));
                    fb.SetPixel(Center.x + y, Center.y - x, Color, Thickness, isMoreHorizontal(Center.x + y, Center.y - x));
                    fb.SetPixel(Center.x - y, Center.y - x, Color, Thickness, isMoreHorizontal(Center.x - y, Center.y - x));
                }
            }

            static bool isMoreHorizontal(int x, int y) => (y == 0 ? int.MaxValue : Math.Abs(-x/y)) >= 1;
        }
        public override void Draw(WriteableBitmap bitmap)
        {
            int dE = 3;
            int dSE = 5 - 2 * Radius;
            int d = 1 - Radius;
            int x = 0;
            int y = Radius;

            DrawOctants(bitmap, x, y);
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
                DrawOctants(bitmap, x, y);
            }
        }
        public override bool ContainsPoint(int x, int y)
        {
            double distance = Math.Sqrt(Math.Pow(x - Center.x, 2) + Math.Pow(y - Center.y, 2));
            return Math.Abs(distance - Radius) <= 5;
        }
        public override void Move(int deltaX, int deltaY)
        {
            Center = (Center.x + deltaX, Center.y + deltaY);
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            if (pointIndex == 0) // Center point
            {
                Center = (newX, newY);
            }
            else // Radius handle
            {
                Radius = (int)Math.Sqrt(Math.Pow(newX - Center.x, 2) + Math.Pow(newY - Center.y, 2));
            }
        }
        public override List<(int x, int y)> GetControlPoints() => [Center, (Center.x + Radius, Center.y)];
    }
}
