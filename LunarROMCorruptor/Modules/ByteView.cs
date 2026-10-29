//MIT License

//Copyright (c) 2026 DimonByte

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.


using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LunarROMCorruptor.Modules
{
    internal class ByteView
    {
        public const int MaxPixels = 2048 * 2048; //Larger files are sampled so the bitmap stays a sensible size

        /// <summary>
        /// Converts a file to an image (3 bytes = 1 RGB pixel). Files with more pixels than <see cref="MaxPixels"/> are downsampled by
        /// only reading every Nth pixel, so this works with files of any size. Safe to call from a background thread.
        /// </summary>
        public static Bitmap? ConvertFileToImage(string path, bool enableColour, CancellationToken token = default)
        {
            TraceLogger.Log($"Converting file to image: {path}");
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            const int bytesPerPixel = 3;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16, FileOptions.SequentialScan);
            long length = stream.Length;
            long totalPixels = length / bytesPerPixel;
            if (totalPixels < 1)
            {
                TraceLogger.Log("The file is too small to show in ByteView (less than 3 bytes).", StatusSeverityType.Warning);
                return null;
            }

            long step = Math.Max(1, (totalPixels + MaxPixels - 1) / MaxPixels); //Pixel step for sampling
            long pixels = totalPixels / step;
            int width = Math.Max(1, (int)Math.Sqrt(pixels));
            int height = (int)Math.Max(1, pixels / width);
            if (step > 1)
                TraceLogger.Log($"File is large, ByteView is showing 1 out of every {step} pixels ({width}x{height}).");

            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte[] row = new byte[data.Stride];
                byte[] input = new byte[step == 1 ? width * bytesPerPixel : bytesPerPixel];
                for (int y = 0; y < height; y++)
                {
                    token.ThrowIfCancellationRequested();
                    if (step == 1)
                    {
                        int read = ReadFully(stream, input, (long)y * width * bytesPerPixel);
                        Array.Clear(input, read, input.Length - read);
                        for (int x = 0; x < width; x++)
                            WritePixel(row, x, input[x * 3], input[x * 3 + 1], input[x * 3 + 2], enableColour);
                    }
                    else
                    {
                        for (int x = 0; x < width; x++)
                        {
                            long pixelIndex = ((long)y * width + x) * step;
                            int read = ReadFully(stream, input, pixelIndex * bytesPerPixel);
                            if (read < 3) Array.Clear(input, read, 3 - read);
                            WritePixel(row, x, input[0], input[1], input[2], enableColour);
                        }
                    }
                    Marshal.Copy(row, 0, data.Scan0 + y * data.Stride, data.Stride);
                }
            }
            catch
            {
                image.UnlockBits(data);
                image.Dispose();
                throw;
            }
            image.UnlockBits(data);
            TraceLogger.Log("File successfully converted to image.");
            return image;
        }

        private static int ReadFully(FileStream stream, byte[] buffer, long offset)
        {
            stream.Position = offset;
            int total = 0;
            while (total < buffer.Length)
            {
                int read = stream.Read(buffer, total, buffer.Length - total);
                if (read <= 0) break;
                total += read;
            }
            return total;
        }

        private static void WritePixel(byte[] row, int x, byte red, byte green, byte blue, bool enableColour)
        {
            int o = x * 3; //24bpp bitmaps are stored as BGR
            if (enableColour)
            {
                row[o] = blue;
                row[o + 1] = green;
                row[o + 2] = red;
            }
            else
            {
                byte grey = (byte)((red + green + blue) / 3);
                row[o] = row[o + 1] = row[o + 2] = grey;
            }
        }

        public static Bitmap FlipImage(Bitmap original, bool horizontal, bool vertical)
        {
            TraceLogger.Log("Flipping image...");
            if (original == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap flippedImage = (Bitmap)original.Clone();

            if (horizontal)
                flippedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

            if (vertical)
                flippedImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            TraceLogger.Log("Image flipped successfully.");
            return flippedImage;
        }
    }
}
