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

using PaintDotNet;
using PaintDotNet.PropertySystem;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

// ReSharper disable once CheckNamespace
namespace WicDecoder
{
    public sealed class DdsFileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new FileType[] { new WicDecoderType() };
        }
    }

    internal class WicDecoderType : PropertyBasedFileType
    {
        public WicDecoderType() :
            base(
                "WIC Decoder",
                new FileTypeOptions()
                {
                    LoadExtensions = WicCodecs.GetSupportedExtensions(),
                    SupportsCancellation = false,
                    SupportsLayers = false
                })
        {
        }

        protected override Document OnLoad(Stream input)
        {
            int numberOfFrames = GetNumberOfFrames(input);

            if (numberOfFrames == 1)
            {
                using (var image = LoadFrameAsBitmap(input))
                {
                    return Document.FromImage(image);
                }
            }

            if (numberOfFrames > 1)
            {
                Size size;
                Document document;

                using (var frame = LoadFrameAsBitmap(input))
                {
                    size = new Size(frame.Width, frame.Height);
                    document = new Document(size);
                }

                for (int i = 0; i < numberOfFrames; i++)
                {
                    using (var frame = LoadFrameAsBitmap(input, i))
                    using (var surface = Surface.CopyFromBitmap(frame))
                    {
                        if (surface.Size == size)
                        {
                            var bitmapLayer = new BitmapLayer(surface)
                            {
                                Name = "Frame " + (i + 1)
                            };
                            document.Layers.Insert(0, bitmapLayer);
                        }
                    }
                }

                return document;
            }

            throw new System.ArgumentException();
        }

        protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface,
            ProgressEventHandler progressCallback)
        {
            throw new System.NotImplementedException();
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            throw new System.NotImplementedException();
        }

        private static Bitmap LoadFrameAsBitmap(Stream input, int frameIndex = 0)
        {
            input.Seek(0, SeekOrigin.Begin);
            var bitmapSource = LoadFrameAsBitmapSource(input, frameIndex);
            return BitmapFromSource(bitmapSource);
        }

        private static BitmapSource LoadFrameAsBitmapSource(Stream input, int frameIndex)
        {
            input.Seek(0, SeekOrigin.Begin);
            var decoder = BitmapDecoder.Create(input, BitmapCreateOptions.None, BitmapCacheOption.None);
            return Convert(decoder.Frames[frameIndex]);
        }

        private static int GetNumberOfFrames(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);
            var decoder = BitmapDecoder.Create(input, BitmapCreateOptions.None, BitmapCacheOption.None);
            return decoder.Frames.Count;
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
            using (var outStream = new MemoryStream())
            {
                // Use a PNG encoder to support transparency
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                return new Bitmap(outStream);
            }
        }
    }
}
