using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingAppCG.Models
{
    public static class LockedFramebufferExtensions
    {
        public static Span<byte> GetPixels(this ILockedFramebuffer framebuffer)
        {
            unsafe
            {
                return new Span<byte>((byte*)framebuffer.Address, framebuffer.RowBytes * framebuffer.Size.Height);
            }
        }
        public static Span<byte> GetPixel(this ILockedFramebuffer framebuffer, int x, int y)
        {
            unsafe
            {
                int offset = framebuffer.RowBytes * y + 4 * x;
                return new Span<byte>((byte*)framebuffer.Address + offset, 4);
            }
        }
        public static void SetPixel(this ILockedFramebuffer framebuffer, int x, int y, Color color)
        {
            if (x < 0 || x >= framebuffer.Size.Width || y < 0 || y >= framebuffer.Size.Height)
                return;

            var pixel = framebuffer.GetPixel(x, y);

            pixel[0] = (byte)(color.B);
            pixel[1] = (byte)(color.G);
            pixel[2] = (byte)(color.R);
        }
        public static void SetPixel(this ILockedFramebuffer framebuffer, int x, int y, Color color, int thickness, bool isMoreHorizontal)
        {
            if(thickness == 1)
            {
                SetPixel(framebuffer, x, y, color);
                return;
            }

            int spread = thickness / 2;

            if (isMoreHorizontal)
            {
                // Spread thickness vertically
                for (int dy = -spread; dy <= spread; dy++)
                {
                    framebuffer.SetPixel(x, y + dy, color);
                }
            }
            else
            {
                // Spread thickness horizontally
                for (int dx = -spread; dx <= spread; dx++)
                {
                    framebuffer.SetPixel(x + dx, y, color);
                }
            }
        }
    }
}
