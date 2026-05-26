using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace Stereoscopia_2.Utility
{
    public static class ImageLoader
    {
        public static BitmapSource Load(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == ".jp2" || ext == ".j2k" || ext == ".jpx")
            {
                using var image = new MagickImage(path);
                // Converti in PNG in memoria, poi carica come BitmapImage WPF
                using var ms = new MemoryStream();
                image.Format = MagickFormat.Png;
                image.Write(ms);
                ms.Position = 0;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new System.Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
    }
}