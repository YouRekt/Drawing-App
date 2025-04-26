using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace DrawingAppCG.Models
{
    public class Line : ShapeBase
    {
        public (int x, int y) Start { get; set; }
        public (int x, int y) End { get; set; }
        private void Draw1(WriteableBitmap bitmap)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    //int dx = X2 - X1;
                    //int dy = Y2 - Y1;
                    //int d = 2 * dy - dx;
                    //int dE = 2 * dy;
                    //int dNE = 2 * (dy - dx);
                    //int xf = X1, yf = Y1;
                    //int xb = X2, yb = Y2;

                    //fb.SetPixel(xf, yf, Color, Thickness);
                    //fb.SetPixel(xb, yb, Color, Thickness);

                    //while (xf < xb)
                    //{
                    //    ++xf; --xb;
                    //    if (d < 0)
                    //    {
                    //        d += dE;
                    //    }
                    //    else
                    //    {
                    //        d += dNE;
                    //        ++yf;
                    //        --yb;
                    //    }
                    //    fb.SetPixel(xf, yf, Color, Thickness);
                    //    fb.SetPixel(xb, yb, Color, Thickness);
                    //}
                }
            }
        }
        private void Draw2(WriteableBitmap bitmap)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    int x1 = Start.x, y1 = Start.y;
                    int x2 = End.x, y2 = End.y;

                    void DrawPixel(int x, int y, bool isMoreHorizontal) => fb.SetPixel(x, y, Color, Thickness, isMoreHorizontal);

                    // Handle horizontal/vertical lines
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

                    // Swap x and y for steep lines
                    if (steep)
                    {
                        (x1, y1) = (y1, x1);
                        (x2, y2) = (y2, x2);
                        (dx, dy) = (dy, dx);
                    }

                    // Ensure left-to-right drawing
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
            Draw2(bitmap);
        }
    }
}
