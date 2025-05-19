using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingAppCG.Models
{
    public static class FillRegion
    {
        public static void SmithScanline(WriteableBitmap bitmap, (int x, int y) seed, IFillSource fillSource)
        {
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    //Queue<(int x, int y)> queue = new();
                    Stack<(int x, int y)> stack = new();
                    stack.Push(seed);
                    //HashSet<(int x, int y)> visited = new();
                    //queue.Enqueue(seed);
                    var pixel = fb.GetPixel(seed.x, seed.y);
                    Color bg = Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
                    if (bg == fillSource.GetColor(seed.x, seed.y)) return;

                    while (stack.Count > 0)
                    {
                        var (x, y) = stack.Pop();

                        if (y < 0 || y >= fb.Size.Height) continue;

                        int xl = x;
                        int xr = x;

                        pixel = fb.GetPixel(x, y);

                        while (xl >= 0 && ColorsEqual(bg, fb.GetPixel(xl, y)))
                        {
                            xl--;
                        }
                        xl++;

                        while (xr < fb.Size.Width && ColorsEqual(bg, fb.GetPixel(xr, y)))
                        {
                            xr++;
                        }
                        xr--;

                        for (int i = xl; i <= xr; i++)
                        {
                            if (y > 0 && ColorsEqual(bg, fb.GetPixel(i, y - 1)))
                                stack.Push((i, y - 1));
                            if (y < fb.Size.Height - 1 && ColorsEqual(bg, fb.GetPixel(i, y + 1)))
                                stack.Push((i, y + 1));
                            fb.SetPixel(i, y, fillSource.GetColor(i, y));
                        }
                    }
                }
            }
            static bool ColorsEqual(Color bg, Span<Byte> pixel)
            {
                return bg.B == pixel[0] && bg.G == pixel[1] && bg.R == pixel[2];
            }
        }
    }
}
