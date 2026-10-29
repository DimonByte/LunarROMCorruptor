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


namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    public class MergeEngine
    {
        public sealed class Settings
        {
            public string MergeFilePath = "";
            public CorruptionOptions Option = CorruptionOptions.NONE; //NONE or RANGE
            public bool SamePosition = true; //Use the merge file byte at the same position, otherwise a random merge file byte
            public bool Mod256; //Use wrapping (mod 256) arithmetic instead of copying (NONE) / absolute difference (RANGE)
        }

        public static FileBuffer OpenMergeFile(Settings s)
        {
            if (string.IsNullOrWhiteSpace(s.MergeFilePath) || !File.Exists(s.MergeFilePath))
                throw new FileNotFoundException("The Merge Engine file couldn't be found. Please select a merge file.", s.MergeFilePath);
            var data = FileBuffer.OpenRead(s.MergeFilePath, 256);
            if (data.Length == 0)
            {
                data.Dispose();
                throw new InvalidDataException("The Merge Engine file is empty.");
            }
            return data;
        }

        public static void CorruptByte(CorruptionContext ctx, Settings s, FileBuffer merge, long i)
        {
            //Same position wraps around if the merge file is smaller than the file being corrupted (previously this crashed).
            long mergeIndex = s.SamePosition ? i % merge.Length : ctx.Rnd.NextInt64(0, merge.Length);
            byte mergeByte = merge[mergeIndex];
            byte current = ctx.Get(i);

            switch (s.Option)
            {
                case CorruptionOptions.NONE:
                    ctx.Set(i, s.Mod256 ? (byte)((current + mergeByte) & 0xFF) : mergeByte);
                    break;

                case CorruptionOptions.RANGE:
                    ctx.Set(i, s.Mod256 ? (byte)((current - mergeByte) & 0xFF) : (byte)Math.Abs(current - mergeByte));
                    break;

                default:
                    throw new InvalidOperationException($"The Merge Engine doesn't support the corruption type {s.Option}.");
            }
        }
    }
}
