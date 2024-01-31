using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace TeensyRom.Core.Common
{
    public static class BitmapExtensions
    {
        public const int BITS_PER_PIXEL = 32;
        public const int BITS_PER_BYTE = 8;

        /// <summary>
        /// Gets a byte array representation of the  specified bitmap
        /// </summary>
        public static byte[] GetBytes(this Bitmap bmp)
        {
            MemoryStream ms = null!;
            try
            {
                ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Bmp);
                var imageBytes = new byte[ms.Length];
                imageBytes = ms.ToArray();
                return imageBytes;
            }
            finally
            {
                ms.Close();
            }
        }

        public static void Display(this byte[] bytes, string filename)
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            using var bitmap = new Bitmap(img);
            bitmap.Display(filename);
        }


        public static Process Display(this Bitmap bmp, string filename)
        {
            bmp.Save(filename);
            return Process.Start(@"cmd.exe ", @"/c " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filename)));
        }
    }
}
