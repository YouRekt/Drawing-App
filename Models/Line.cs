using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DrawingAppCG.Models
{
    public class Line : AntiAliasedShapeBase
    {
        public (int x, int y) Start { get; set; }
        public (int x, int y) End { get; set; }
        private void DrawAntialiased(WriteableBitmap bitmap)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    int x1 = Start.x, y1 = Start.y;
                    int x2 = End.x, y2 = End.y;

                    int dx = Math.Abs(x2 - x1);
                    int dy = Math.Abs(y2 - y1);
                    bool steep = dy > dx;

                    if (steep)
                    {
                        (x1, y1) = (y1, x1);
                        (x2, y2) = (y2, x2);
                        (dx, dy) = (dy, dx);
                    }

                    bool inverted = x1 > x2;
                    if (inverted)
                    {
                        (x1, x2) = (x2, x1);
                        (y1, y2) = (y2, y1);
                    }

                    int d = 2 * dy - dx;
                    int dE = 2 * dy;
                    int dNE = 2 * (dy - dx);
                    int x = x1, y = y1;
                    int s = y1 < y2 ? 1 : -1;

                    int two_v_dx = 0;
                    float invDenom = (float)(1 / (2 * Math.Sqrt(dx * dx + dy * dy)));
                    float two_dx_invDenom = 2 * dx * invDenom;

                    int i;

                    if (steep)
                    {
                        fb.IntensifyPixel(y, x, Color, Thickness, 0);
                        for (i = 1; fb.IntensifyPixel(y + i, x, Color, Thickness, i * two_dx_invDenom) > 0; ++i) ;
                        for (i = 1; fb.IntensifyPixel(y - i, x, Color, Thickness, i * two_dx_invDenom) > 0; ++i) ;
                    }
                    else
                    {
                        fb.IntensifyPixel(x, y, Color, Thickness, 0);
                        for (i = 1; fb.IntensifyPixel(x, y + i, Color, Thickness, i * two_dx_invDenom) > 0; ++i) ;
                        for (i = 1; fb.IntensifyPixel(x, y - i, Color, Thickness, i * two_dx_invDenom) > 0; ++i) ;
                    }

                    while (x < x2)
                    {
                        ++x;
                        if (d < 0)
                        {
                            two_v_dx = d + dx;
                            d += dE;
                        }
                        else
                        {
                            two_v_dx = d - dx;
                            d += dNE;
                            y += s;
                        }

                        if (steep)
                        {
                            fb.IntensifyPixel(y, x, Color, Thickness, two_v_dx * invDenom);
                            for (i = 1; fb.IntensifyPixel(y + i, x, Color, Thickness, i * two_dx_invDenom - s * two_v_dx * invDenom) > 0; ++i) ;
                            for (i = 1; fb.IntensifyPixel(y - i, x, Color, Thickness, i * two_dx_invDenom + s * two_v_dx * invDenom) > 0; ++i) ;
                        }
                        else
                        {
                            fb.IntensifyPixel(x, y, Color, Thickness, 0);
                            for (i = 1; fb.IntensifyPixel(x, y + i, Color, Thickness, i * two_dx_invDenom - s * two_v_dx * invDenom) > 0; ++i) ;
                            for (i = 1; fb.IntensifyPixel(x, y - i, Color, Thickness, i * two_dx_invDenom + s * two_v_dx * invDenom) > 0; ++i) ;
                        }
                    }
                }
            }
        }
        private void DrawNormal(WriteableBitmap bitmap)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    int x1 = Start.x, y1 = Start.y;
                    int x2 = End.x, y2 = End.y;

                    void DrawPixel(int x, int y, bool isMoreHorizontal) => fb.SetPixel(x, y, Color, Thickness, isMoreHorizontal);

                    //horizontal/vertical lines (wikipedia suggested to optimize for these) 
                    if (y1 == y2 || x1 == x2)
                    {
                        bool horizontal = y1 == y2;
                        int start = horizontal ? Math.Min(x1, x2) : Math.Min(y1, y2);
                        int end = horizontal ? Math.Max(x1, x2) : Math.Max(y1, y2);
                        for (int i = start; i <= end; i++)
                            DrawPixel(horizontal ? i : x1, horizontal ? y1 : i, horizontal);
                        return;
                    }

                    int dx = Math.Abs(x2 - x1);
                    int dy = Math.Abs(y2 - y1);
                    bool steep = dy > dx;

                    // swap x and y for steep lines  
                    if (steep)
                    {
                        (x1, y1) = (y1, x1);
                        (x2, y2) = (y2, x2);
                        (dx, dy) = (dy, dx);
                    }

                    // ensure left-to-right drawing  
                    if (x1 > x2)
                    {
                        (x1, x2) = (x2, x1);
                        (y1, y2) = (y2, y1);
                    }

                    int d = 2 * dy - dx;
                    int dE = 2 * dy;
                    int dNE = 2 * (dy - dx);
                    int xf = x1, yf = y1;
                    int xb = x2, yb = y2;
                    int s = y1 < y2 ? 1 : -1;

                    if (steep)
                    {
                        DrawPixel(yf, xf, !steep);
                        DrawPixel(yb, xb, !steep);
                    }
                    else
                    {
                        DrawPixel(xf, yf, !steep);
                        DrawPixel(xb, yb, !steep);
                    }
                    while (xf < xb)
                    {
                        ++xf; --xb;
                        if (d < 0)
                        {
                            d += dE;
                        }
                        else
                        {
                            d += dNE;
                            yf += s;
                            yb -= s;
                        }

                        if (steep)
                        {
                            DrawPixel(yf, xf, !steep);
                            DrawPixel(yb, xb, !steep);
                        }
                        else
                        {
                            DrawPixel(xf, yf, !steep);
                            DrawPixel(xb, yb, !steep);
                        }
                    }
                }
            }
        }
        public override void Draw(WriteableBitmap bitmap)
        {
            if (IsAntialiased)
            {
                DrawAntialiased(bitmap);
            }
            else
            {
                DrawNormal(bitmap);
            }
        }
        public override bool ContainsPoint(int x, int y)
        {
            return DistanceToLine(x, y, Start.x, Start.y, End.x, End.y) <= 5;
        }
        public override void Move(int deltaX, int deltaY)
        {
            Start = (Start.x + deltaX, Start.y + deltaY);
            End = (End.x + deltaX, End.y + deltaY);
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            if (pointIndex == 0) Start = (newX, newY);
            else End = (newX, newY);
        }
        public override List<(int x, int y)> GetControlPoints() => [Start, End];
        public static double DistanceToLine(int x, int y, int x1, int y1, int x2, int y2)
        {
            double A = x - x1;
            double B = y - y1;
            double C = x2 - x1;
            double D = y2 - y1;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = (len_sq != 0) ? dot / len_sq : -1;

            double xx, yy;

            if (param < 0) { xx = x1; yy = y1; }
            else if (param > 1) { xx = x2; yy = y2; }
            else { xx = x1 + param * C; yy = y1 + param * D; }

            return Math.Sqrt(Math.Pow(x - xx, 2) + Math.Pow(y - yy, 2));
        }
    }
}
