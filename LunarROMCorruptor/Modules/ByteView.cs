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
    public enum ByteViewMode
    {
        Normal, //3 bytes = 1 RGB (or grey) pixel
        Diff, //Grey image with every changed byte highlighted in red
        Entropy //Heatmap of how random each block of the file is (padding = dark, text/tables = blue/green, code = yellow, compressed = red)
    }

    /// <summary>
    /// A rendered ByteView image plus everything needed to map a pixel back to a file offset (used for drag-to-target).
    /// </summary>
    public sealed class ByteViewImage : IDisposable
    {
        public required Bitmap Image;
        public int Width;
        public int Height;
        public long BytesPerPixel; //How many file bytes one pixel represents
        public long FileLength;
        public bool FlipH;
        public bool FlipV;
        public ByteViewMode Mode;
        public long ChangedBytes = -1; //Diff mode only

        /// <summary>
        /// File offset of the first byte shown by an image pixel (flips are taken into account). Returns -1 outside of the file.
        /// </summary>
        public long OffsetOf(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return -1;
            if (FlipH) x = Width - 1 - x;
            if (FlipV) y = Height - 1 - y;
            long offset = ((long)y * Width + x) * BytesPerPixel;
            return offset < FileLength ? offset : -1;
        }

        public void Dispose() => Image.Dispose();
    }

    internal class ByteView
    {
        public const int MaxPixels = 2048 * 2048; //Larger files are sampled so the bitmap stays a sensible size
        private const int BytesPerRgbPixel = 3;

        /// <summary>
        /// Renders a file in the given mode. Safe to call from a background thread.
        /// Diff mode compares against <paramref name="originalPath"/> when it's a different file of the same size, otherwise it uses
        /// <paramref name="changedOffsets"/> (the stash) to find the changed bytes.
        /// </summary>
        public static ByteViewImage? Render(string path, ByteViewMode mode, bool enableColour, bool flipH, bool flipV, string? originalPath = null, IEnumerable<long>? changedOffsets = null, CancellationToken token = default)
        {
            TraceLogger.Log($"Rendering ByteView ({mode}) for: {path}");
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            long length = new FileInfo(path).Length;
            if (length < BytesPerRgbPixel)
            {
                TraceLogger.Log("The file is too small to show in ByteView (less than 3 bytes).", StatusSeverityType.Warning);
                return null;
            }

            ByteViewImage result = mode switch
            {
                ByteViewMode.Entropy => RenderEntropy(path, length, enableColour, token),
                ByteViewMode.Diff => RenderDiff(path, length, originalPath, changedOffsets, token),
                _ => RenderNormal(path, length, enableColour, token)
            };
            result.FileLength = length;
            result.Mode = mode;
            if (flipH || flipV)
            {
                var flipped = FlipImage(result.Image, flipH, flipV);
                result.Image.Dispose();
                result.Image = flipped;
                result.FlipH = flipH;
                result.FlipV = flipV;
            }
            TraceLogger.Log($"ByteView rendered: {result.Width}x{result.Height}, 1 pixel = {result.BytesPerPixel} byte(s).");
            return result;
        }

        /// <summary>
        /// Old entry point, kept for compatibility: normal mode, no flips.
        /// </summary>
        public static Bitmap? ConvertFileToImage(string path, bool enableColour, CancellationToken token = default)
        {
            var image = Render(path, ByteViewMode.Normal, enableColour, false, false, token: token);
            return image?.Image;
        }

        private static (int Width, int Height, long Step) RgbLayout(long length)
        {
            long totalPixels = length / BytesPerRgbPixel;
            long step = Math.Max(1, (totalPixels + MaxPixels - 1) / MaxPixels); //Pixel step for sampling
            long pixels = Math.Max(1, totalPixels / step);
            int width = Math.Max(1, (int)Math.Sqrt(pixels));
            int height = (int)Math.Max(1, pixels / width);
            return (width, height, step);
        }

        private static ByteViewImage RenderNormal(string path, long length, bool enableColour, CancellationToken token)
        {
            var (width, height, step) = RgbLayout(length);
            if (step > 1)
                TraceLogger.Log($"File is large, ByteView is showing 1 out of every {step} pixels ({width}x{height}).");
            var image = DrawRgb(path, width, height, step, (r, g, b, _) => enableColour ? (r, g, b) : Grey(r, g, b), token);
            return new ByteViewImage { Image = image, Width = width, Height = height, BytesPerPixel = step * BytesPerRgbPixel };
        }

        private static (byte, byte, byte) Grey(byte r, byte g, byte b)
        {
            byte grey = (byte)((r + g + b) / 3);
            return (grey, grey, grey);
        }

        private static ByteViewImage RenderDiff(string path, long length, string? originalPath, IEnumerable<long>? changedOffsets, CancellationToken token)
        {
            var (width, height, step) = RgbLayout(length);
            long bytesPerPixel = step * BytesPerRgbPixel;
            var changed = new bool[(long)width * height];
            long changedBytes = 0;

            bool compareFiles = originalPath != null && File.Exists(originalPath)
                && !string.Equals(Path.GetFullPath(originalPath), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase)
                && new FileInfo(originalPath).Length == length;
            if (compareFiles)
            {
                const int chunk = 1 << 20;
                byte[] a = new byte[chunk], b = new byte[chunk];
                using var fa = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.SequentialScan);
                using var fb = new FileStream(originalPath!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.SequentialScan);
                for (long pos = 0; pos < length; pos += chunk)
                {
                    token.ThrowIfCancellationRequested();
                    int n = (int)Math.Min(chunk, length - pos);
                    fa.ReadExactly(a, 0, n);
                    fb.ReadExactly(b, 0, n);
                    if (a.AsSpan(0, n).SequenceEqual(b.AsSpan(0, n))) continue;
                    for (int k = 0; k < n; k++)
                    {
                        if (a[k] == b[k]) continue;
                        changedBytes++;
                        long pixel = (pos + k) / bytesPerPixel;
                        if (pixel < changed.Length) changed[pixel] = true;
                    }
                }
            }
            else if (changedOffsets != null)
            {
                var seen = new HashSet<long>();
                foreach (long offset in changedOffsets)
                {
                    if (offset < 0 || offset >= length) continue;
                    if (seen.Add(offset)) changedBytes++;
                    long pixel = offset / bytesPerPixel;
                    if (pixel < changed.Length) changed[pixel] = true;
                }
            }
            else
            {
                TraceLogger.Log("ByteView Diff: nothing to compare with (corrupt to a different file, or enable stash saves for in-place corruption).", StatusSeverityType.Warning, true);
            }

            var image = DrawRgb(path, width, height, step, (r, g, bl, index) =>
            {
                if (changed[index]) return (255, 40, 40);
                byte grey = (byte)((r + g + bl) / 3 * 0.35 + 20); //Dimmed so the changes stand out
                return (grey, grey, grey);
            }, token);
            return new ByteViewImage { Image = image, Width = width, Height = height, BytesPerPixel = bytesPerPixel, ChangedBytes = changedBytes };
        }

        private static ByteViewImage RenderEntropy(string path, long length, bool enableColour, CancellationToken token)
        {
            long blockBytes = Math.Max(256, (length + MaxPixels - 1) / MaxPixels);
            long pixels = (length + blockBytes - 1) / blockBytes;
            int width = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(pixels)));
            int height = (int)Math.Max(1, (pixels + width - 1) / width);
            const int maxSample = 64 * 1024; //Huge blocks are sampled (first 64 KB of each block)
            int sample = (int)Math.Min(blockBytes, maxSample);
            byte[] buffer = new byte[sample];
            int[] histogram = new int[256];

            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16, FileOptions.SequentialScan);
                byte[] row = new byte[data.Stride];
                for (int y = 0; y < height; y++)
                {
                    token.ThrowIfCancellationRequested();
                    Array.Clear(row);
                    for (int x = 0; x < width; x++)
                    {
                        long block = (long)y * width + x;
                        long offset = block * blockBytes;
                        if (offset >= length) break;
                        stream.Position = offset;
                        int read = stream.ReadAtLeast(buffer, (int)Math.Min(sample, length - offset), false);
                        Array.Clear(histogram);
                        for (int k = 0; k < read; k++) histogram[buffer[k]]++;
                        double entropy = 0;
                        foreach (int count in histogram)
                        {
                            if (count == 0) continue;
                            double p = (double)count / read;
                            entropy -= p * Math.Log2(p);
                        }
                        //Small samples can't reach 8 bits even for random data, so scale by the entropy expected from random data of this size
                        double expectedMax = Math.Max(1.0, 8.0 - 255.0 / (2.0 * read * Math.Log(2)));
                        var (r, g, b) = EntropyColour(entropy / expectedMax, enableColour);
                        row[x * 3] = b;
                        row[x * 3 + 1] = g;
                        row[x * 3 + 2] = r;
                    }
                    Marshal.Copy(row, 0, data.Scan0 + y * data.Stride, data.Stride);
                }
            }
            finally
            {
                image.UnlockBits(data);
            }
            return new ByteViewImage { Image = image, Width = width, Height = height, BytesPerPixel = blockBytes };
        }

        private static readonly (double T, byte R, byte G, byte B)[] EntropyStops =
        [
            (0.00, 0, 0, 30), //Empty / padding
            (0.30, 20, 70, 200), //Tables, simple data
            (0.55, 0, 180, 140), //Text
            (0.75, 240, 210, 0), //Code / mixed data
            (0.92, 240, 90, 20), //Dense data
            (1.00, 255, 30, 60), //Compressed / encrypted / random
        ];

        public static (byte R, byte G, byte B) EntropyColour(double t, bool colour)
        {
            t = Math.Clamp(t, 0, 1);
            if (!colour)
            {
                byte grey = (byte)(t * 255);
                return (grey, grey, grey);
            }
            for (int k = 1; k < EntropyStops.Length; k++)
            {
                var (t1, r1, g1, b1) = EntropyStops[k];
                if (t > t1) continue;
                var (t0, r0, g0, b0) = EntropyStops[k - 1];
                double f = (t - t0) / (t1 - t0);
                return ((byte)(r0 + (r1 - r0) * f), (byte)(g0 + (g1 - g0) * f), (byte)(b0 + (b1 - b0) * f));
            }
            return (EntropyStops[^1].R, EntropyStops[^1].G, EntropyStops[^1].B);
        }

        /// <summary>
        /// Draws an RGB layout image (3 bytes per pixel, sampled every <paramref name="step"/> pixels) using a colour function.
        /// </summary>
        private static Bitmap DrawRgb(string path, int width, int height, long step, Func<byte, byte, byte, long, (byte, byte, byte)> colourOf, CancellationToken token)
        {
            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16, FileOptions.SequentialScan);
                byte[] row = new byte[data.Stride];
                byte[] input = new byte[step == 1 ? width * BytesPerRgbPixel : BytesPerRgbPixel];
                for (int y = 0; y < height; y++)
                {
                    token.ThrowIfCancellationRequested();
                    if (step == 1)
                    {
                        int read = ReadFully(stream, input, (long)y * width * BytesPerRgbPixel);
                        Array.Clear(input, read, input.Length - read);
                        for (int x = 0; x < width; x++)
                            WritePixel(row, x, colourOf(input[x * 3], input[x * 3 + 1], input[x * 3 + 2], (long)y * width + x));
                    }
                    else
                    {
                        for (int x = 0; x < width; x++)
                        {
                            long pixelIndex = (long)y * width + x;
                            int read = ReadFully(stream, input, pixelIndex * step * BytesPerRgbPixel);
                            if (read < 3) Array.Clear(input, read, 3 - read);
                            WritePixel(row, x, colourOf(input[0], input[1], input[2], pixelIndex));
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

        private static void WritePixel(byte[] row, int x, (byte R, byte G, byte B) colour)
        {
            int o = x * 3; //24bpp bitmaps are stored as BGR
            row[o] = colour.B;
            row[o + 1] = colour.G;
            row[o + 2] = colour.R;
        }

        public static Bitmap FlipImage(Bitmap original, bool horizontal, bool vertical)
        {
            if (original == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap flippedImage = (Bitmap)original.Clone();

            if (horizontal)
                flippedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

            if (vertical)
                flippedImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return flippedImage;
        }
    }
}
