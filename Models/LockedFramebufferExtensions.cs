using Avalonia.Media;
using Avalonia.Platform;
using System;

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
                if (x < 0 || x >= framebuffer.Size.Width || y < 0 || y >= framebuffer.Size.Height)
                    throw new ArgumentOutOfRangeException("Trying to get pixel outside the image.");

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
            pixel[3] = (byte)(color.A);
        }
        public static void SetPixel(this ILockedFramebuffer framebuffer, int x, int y, Color color, int thickness, bool isMoreHorizontal)
        {
            if (thickness == 1)
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
        private static float Coverage(float w, float d, float r)
        {
            if (float.IsNaN(d) || float.IsInfinity(d))
                return 0;

            if (w >= r)
            {
                if (w <= d)
                    return cov(d - w, r);
                else if (0 <= d && d <= w)
                    return 1 - cov(w - d, r);
            }
            else
            {
                if (0 <= d && d <= w)
                    return 1 - cov(w - d, r) - cov(w + d, r);
                else if (w <= d && d <= r - w)
                    return cov(d - w, r) - cov(d + w, r);
                else if (r - w <= d && d <= r + w)
                    return cov(d - w, r);
            }
            return 1;

            static float cov(float d, float r) => d <= r ? (float)((1 / Math.PI) * Math.Acos(d / r) - (d / (Math.PI * r * r)) * Math.Sqrt(r * r - d * d)) : 0;
        }
        public static float IntensifyPixel(this ILockedFramebuffer framebuffer, int x, int y, Color color, float thickness, float distance, Color? background = null)
        {
            var bgPixel = framebuffer.GetPixel(x, y);

            background = Color.FromArgb(bgPixel[3], bgPixel[2], bgPixel[1], bgPixel[0]);
            if (bgPixel[3] == 0)
                background = Colors.White;

            float r = 0.5f;
            float cov = Coverage(thickness, distance, r);
            if (cov > 0)
                framebuffer.SetPixel(x, y, lerp(background.Value, color, cov));
            //System.Diagnostics.Debug.WriteLine($"x: {x}, y: {y}, color: {lerp(background.Value, color, cov)}, thickness: {thickness}, distance: {distance}, cov: {cov}");
            return cov;

            Color lerp(Color background, Color line, float t)
            {
                byte R = (byte)(background.R + (line.R - background.R) * t);
                byte G = (byte)(background.G + (line.G - background.G) * t);
                byte B = (byte)(background.B + (line.B - background.B) * t);
                return Color.FromRgb(R, G, B);
            }
        }
    }
}
