using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace DrawingAppCG.Models
{
    public class Circle : ShapeBase
    {
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Radius { get; set; }
        private void DrawOctants(WriteableBitmap bitmap, int x, int y)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    fb.SetPixel(CenterX + x, CenterY + y, Color, Thickness,isMoreHorizontal(CenterX + x, CenterY + y));
                    fb.SetPixel(CenterX - x, CenterY + y, Color, Thickness,isMoreHorizontal(CenterX - x, CenterY + y));
                    fb.SetPixel(CenterX + x, CenterY - y, Color, Thickness, isMoreHorizontal(CenterX + x, CenterY - y));
                    fb.SetPixel(CenterX - x, CenterY - y, Color, Thickness, isMoreHorizontal(CenterX - x, CenterY - y));
                    fb.SetPixel(CenterX + y, CenterY + x, Color, Thickness, isMoreHorizontal(CenterX + y, CenterY + x));
                    fb.SetPixel(CenterX - y, CenterY + x, Color, Thickness, isMoreHorizontal(CenterX - y, CenterY + x));
                    fb.SetPixel(CenterX + y, CenterY - x, Color, Thickness, isMoreHorizontal(CenterX + y, CenterY - x));
                    fb.SetPixel(CenterX - y, CenterY - x, Color, Thickness, isMoreHorizontal(CenterX - y, CenterY - x));
                }
            }

            bool isMoreHorizontal(int x, int y) => (y == 0 ? int.MaxValue : Math.Abs(-x/y)) >= 1;
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
            double distance = Math.Sqrt(Math.Pow(x - CenterX, 2) + Math.Pow(y - CenterY, 2));
            return Math.Abs(distance - Radius) <= 5;
        }
        public override void Move(int deltaX, int deltaY)
        {
            CenterX += deltaX;
            CenterY += deltaY;
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            if (pointIndex == 0) // Center point
            {
                CenterX = newX;
                CenterY = newY;
            }
            else // Radius handle
            {
                Radius = (int)Math.Sqrt(Math.Pow(newX - CenterX, 2) + Math.Pow(newY - CenterY, 2));
            }
        }
        public override List<(int x, int y)> GetControlPoints() => [(CenterX, CenterY), (CenterX + Radius, CenterY)];
    }
}
