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

namespace LunarROMCorruptor.Modules
{
    internal class ByteView
    {
        public static Bitmap? ConvertByteToImage(byte[] data, bool enableColour)
        {
            TraceLogger.Log("Converting byte array to image...");
            if (data == null) return null; //Ignore invalid null data

            int bytesPerPixel = 3; // Assuming 3 bytes for RGB

            int imageSize = (int)Math.Sqrt(data.Length / bytesPerPixel);
            int width = imageSize;
            int height = data.Length / (width * bytesPerPixel);

            Bitmap image = new(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int byteIndex = (i * width + j) * bytesPerPixel;

                    byte red, green, blue;

                    if (byteIndex + 2 < data.Length)
                    {
                        red = data[byteIndex];
                        green = data[byteIndex + 1];
                        blue = data[byteIndex + 2];
                    }
                    else
                    {
                        // Handle the case where there are not enough bytes left
                        // For example, you could set a default color or skip the pixel
                        red = green = blue = 0; // Default to black
                    }

                    Color color;

                    if (enableColour)
                    {
                        color = Color.FromArgb(red, green, blue);
                    }
                    else
                    {
                        byte grayscaleValue = (byte)((red + green + blue) / 3);
                        color = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                    }

                    image.SetPixel(j, i, color);
                }
            }
            TraceLogger.Log("Byte array successfully converted to image.");
            return image;
        }

        public static Bitmap FlipImage(Bitmap original, bool horizontal, bool vertical)
        {
            // Check if the original image is null, if so return bitmap of 1x1 pixel to avoid null reference exceptions
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
