using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestsSystems_HardnessTester
{
    internal static class ImageConverter
    {
        public static BitmapImage Bitmap2BitmappImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image.Clone();

        }
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public static System.Drawing.Bitmap BitmapSource2Bitmap(System.Windows.Media.Imaging.BitmapSource bitmapSource)
        {
            var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var data = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}
