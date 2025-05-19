using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace DrawingAppCG.Models
{
    public interface IFillSource
    {
        Color GetColor(int x, int y);
    }
    public class SolidColorFill(Color color) : IFillSource
    {
        public Color FillColor { get; set; } = color;

        public Color GetColor(int x, int y) => FillColor;
    }
    public static class ColorExtensions
    {
        public static IFillSource AsFillSource(this Color color)
        {
            return new SolidColorFill(color);
        }
    }
    public class ImageFill(WriteableBitmap bitmap, string path) : IFillSource
    {
        public string Path { get; set; } = path;
        public WriteableBitmap Bitmap { get; set; } = bitmap;

        public Color GetColor(int x, int y)
        {
            using (var fb = Bitmap.Lock())
            {
                var px = x % fb.Size.Width;
                var py = y % fb.Size.Height;
                var pixel = fb.GetPixel(                                            // BGRA
                    px < 0 ? 0 : px >= fb.Size.Width ? fb.Size.Width - 1 : px,      // This abomination is here because when a polygon that is filled with
                    py < 0 ? 0 : py >= fb.Size.Height ? fb.Size.Height - 1 : py);   // an image is moved around really fast the modulo calculation above
                return new Color(pixel[3], pixel[2], pixel[1], pixel[0]);           // fails for some reason and returns negative values??
            }
        }
    }
}
