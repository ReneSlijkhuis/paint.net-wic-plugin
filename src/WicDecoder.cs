///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//    MIT License
//
//    Copyright(c) 2017 René Slijkhuis
//
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
//
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PaintDotNet.WicDecoder
{
    public sealed class WicDecoderTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new[] { new WicDecoderType() };
        }
    }

    internal class WicDecoderType : FileType
    {
        internal WicDecoderType() :
            base("WIC Decoder", FileTypeFlags.SupportsLoading, WicCodecs.GetSupportedExtensions())
        {
        }

        protected override Document OnLoad(Stream input)
        {
            FieldInfo fi = input.GetType().GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance);
            FileStream fs = fi.GetValue(input) as FileStream;

            using (var image = LoadRAW(fs.Name))
            {
                var document = Document.FromImage(image);
                var numberOfFrames = GetNumberOfFrames(fs.Name);
                var size = new Size(image.Width, image.Height);

                if (numberOfFrames <= 1)
                {
                    return document;
                }
                else
                {
                    document.Layers.Clear();
                    for (int i = 0; i < numberOfFrames; i++)
                    {
                        using (var layer = LoadRAW(fs.Name, i))
                        using (var surface = Surface.CopyFromBitmap(layer))
                        {
                            if (surface.Size == size)
                            {
                                var bitmapLayer = new BitmapLayer(surface);
                                bitmapLayer.Name = "Frame " + (i + 1);
                                document.Layers.Insert(0, bitmapLayer);
                            }
                        }
                    }
                    return document;
                }
            }
        }

        private static Bitmap LoadRAW(string filename, int frameIndex = 0)
        {
            var bitmapSource = LoadImage(filename, frameIndex);
            return BitmapFromSource(bitmapSource);
        }

        private static BitmapSource LoadImage(string filename, int frameIndex)
        {
            using (var inFile = File.OpenRead(filename))
            {
                var decoder = BitmapDecoder.Create(inFile, BitmapCreateOptions.None, BitmapCacheOption.None);
                return Convert(decoder.Frames[frameIndex]);
            }
        }

        private static int GetNumberOfFrames(string filename)
        {
            using (var inFile = File.OpenRead(filename))
            {
                var decoder = BitmapDecoder.Create(inFile, BitmapCreateOptions.None, BitmapCacheOption.None);
                return decoder.Frames.Count;
            }
        }

        private static BitmapSource Convert(BitmapFrame frame)
        {
            int stride = frame.PixelWidth * (frame.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[frame.PixelHeight * stride];

            frame.CopyPixels(pixels, stride, 0);

            var bmpSource = BitmapSource.Create(frame.PixelWidth, frame.PixelHeight,
                frame.DpiX, frame.DpiY, frame.Format, frame.Palette, pixels, stride);

            return bmpSource;
        }

        private static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // Use a PNG encoder to support transparency
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
    }
}
