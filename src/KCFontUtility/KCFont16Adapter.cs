using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace KCFontUtility
{
    /// <summary>
    /// 國喬字型檔介面
    /// </summary>
    public class KCFont16Adapter
    {
        Stream KCCHIN16;
        Stream KCTEXT16;
        public KCFont16Adapter(Stream kcChin16, Stream kcText16)
        {
            KCCHIN16 = kcChin16;
            KCTEXT16 = kcText16;
        }

        const int KCHeaderOffset = 256;
        const int SymbolDataOffset = 765 * 2 * 16;
        public byte[] GetByteArrayForCategory(CharCategories catg)
        {
            switch (catg)
            {
                case CharCategories.Ascii: return new byte[15];
                case CharCategories.Symbol: return new byte[2 * 16];
                case CharCategories.Chinese: return new byte[2 * 14];
                default: throw new NotSupportedException();
            }
        }

        public byte[] ReadFont(char ch)
        {

            var cd = new CharData(ch);
            var b = GetByteArrayForCategory(cd.Category);
            switch (cd.Category)
            {
                case CharCategories.Ascii:
                    lock (KCTEXT16)
                    {
                        KCTEXT16.Seek(KCHeaderOffset + cd.RelativePos * b.Length, SeekOrigin.Begin);
                        KCTEXT16.Read(b, 0, b.Length);
                    }
                    break;
                case CharCategories.Symbol:
                    lock (KCCHIN16)
                    {
                        KCCHIN16.Seek(KCHeaderOffset + cd.RelativePos * b.Length, SeekOrigin.Begin);
                        KCCHIN16.Read(b, 0, b.Length);
                    }
                    break;
                case CharCategories.Chinese:
                    lock (KCCHIN16)
                    {
                        KCCHIN16.Seek(KCHeaderOffset + SymbolDataOffset + cd.RelativePos * b.Length, SeekOrigin.Begin);
                        KCCHIN16.Read(b, 0, b.Length);
                    }
                    break;
                default: throw new NotSupportedException();
            }
            return b;
        }

        public byte[] GetCharPng(char ch)
        {
            var cd = new CharData(ch);
            var fontData = ReadFont(ch);
            var bmp = new Bitmap(cd.Category == CharCategories.Ascii ? 8 : 16, 16);
            var h = cd.Category == CharCategories.Symbol ? 16 : (cd.Category == CharCategories.Chinese ? 14 : 15);
            var w = cd.Category == CharCategories.Ascii ? 8 : 16;
            var offset = 0;
            for (var y = 0; y < h; y++)
            {
                var b = fontData[offset];
                for (var x = 0; x < w; x++)
                {
                    if ((b & 0x80) != 0)
                        bmp.SetPixel(x, y, Color.Black);
                    if (w > 8 && x == 7)
                        b = fontData[++offset];
                    else
                        b = (byte)(b << 1);
                }
                offset++;
            }
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        static Bitmap Bmp4Ascii = new Bitmap(8, 16, PixelFormat.Format1bppIndexed);
        static Bitmap Bmp4Chinese = new Bitmap(16, 16, PixelFormat.Format1bppIndexed);

        public byte[] GetCharBmp(char ch)
        {
            var cd = new CharData(ch);
            var fontData = ReadFont(ch);
            var w = cd.Category == CharCategories.Ascii ? 8 : 16;
            var fontDataStride = w / 8;
            var h = 16;
            var bmp = cd.Category == CharCategories.Ascii ? Bmp4Ascii : Bmp4Chinese;
            lock (bmp)
            {
                var bmd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
                var idx = 0;
                unsafe
                {
                    byte* p = (byte*)bmd.Scan0;
                    for (var y = 0; y < h; y++)
                    {
                        for (var s = 0; s < bmd.Stride; s++)
                        {
                            if (s < fontDataStride && idx < fontData.Length)
                            {
                                p[0] = fontData[idx++];
                            }
                            p++;
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Bmp);
                    return ms.ToArray();
                }
            }
        }

        static Brush Black = new SolidBrush(Color.Black);
        static Dictionary<string, Point> PosOffset = new Dictionary<string, Point>()
        {
            ["細明體"] = new Point(1, 0),
            ["Noto Sans CJK TC Light"] = new Point(0, -3)
        };
        public Bitmap DrawCharBmp(char ch, string fontName)
        {
            var cd = new CharData(ch);
            var bmp = new Bitmap(cd.Category == CharCategories.Ascii ? 8 : 16, 16);
            var h = cd.Category == CharCategories.Symbol ? 16 : (cd.Category == CharCategories.Chinese ? 14 : 15);
            var w = cd.Category == CharCategories.Ascii ? 8 : 16;
            var g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            var offset = PosOffset[fontName];
            g.DrawString(ch.ToString(), new Font(fontName, 10), new SolidBrush(Color.White), offset.X, offset.Y);
            Bitmap monochrome = new Bitmap(w, 16, PixelFormat.Format1bppIndexed);
            Convert(bmp, monochrome);
            return monochrome;
        }

        public byte[] ExtractFontData(Bitmap bmp, int rowCount = 14)
        {
            var bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                         bmp.PixelFormat);
            var fontStride = bmp.Width / 8;
            var fontData = new byte[rowCount * fontStride];
            var idx = 0;
            unsafe
            {
                byte* p = (byte*)bmd.Scan0;
                for (var y = 0; y < rowCount; y++)
                {
                    for (var s = 0; s < bmd.Stride; s++)
                    {
                        if (s < fontStride && idx < fontData.Length)
                        {
                            fontData[idx++] = p[0];
                        }
                        p++;
                    }
                }
            }
            return fontData;
        }


        //REF: https://stackoverflow.com/a/22256055/288936
        private static unsafe void Convert(Bitmap src, Bitmap conv)
        {

            // Lock source and destination in memory for unsafe access
            var bmbo = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly,
                                     src.PixelFormat);
            var bmdn = conv.LockBits(new Rectangle(0, 0, conv.Width, conv.Height), ImageLockMode.ReadWrite,
                                     conv.PixelFormat);

            var srcScan0 = bmbo.Scan0;
            var convScan0 = bmdn.Scan0;

            var srcStride = bmbo.Stride;
            var convStride = bmdn.Stride;

            byte* sourcePixels = (byte*)(void*)srcScan0;
            byte* destPixels = (byte*)(void*)convScan0;

            var srcLineIdx = 0;
            var convLineIdx = 0;
            var hmax = src.Height - 1;
            var wmax = src.Width - 1;
            for (int y = 0; y < hmax; y++)
            {
                // find indexes for source/destination lines

                // use addition, not multiplication?
                srcLineIdx += srcStride;
                convLineIdx += convStride;

                var srcIdx = srcLineIdx;
                for (int x = 0; x < wmax; x++)
                {
                    // index for source pixel (32bbp, rgba format)
                    srcIdx += 4;
                    //var r = pixel[2];
                    //var g = pixel[1];
                    //var b = pixel[0];

                    // could just check directly?
                    //if (Color.FromArgb(r,g,b).GetBrightness() > 0.01f)
                    if (!(sourcePixels[srcIdx] == 0 && sourcePixels[srcIdx + 1] == 0 && sourcePixels[srcIdx + 2] == 0))
                    {
                        // destination byte for pixel (1bpp, ie 8pixels per byte)
                        var idx = convLineIdx + (x >> 3);
                        // mask out pixel bit in destination byte
                        destPixels[idx] |= (byte)(0x80 >> (x & 0x7));
                    }
                }
            }
            src.UnlockBits(bmbo);
            conv.UnlockBits(bmdn);
        }



        public void WriteFont(char ch, byte[] data)
        {
            var cd = new CharData(ch);
            var b = GetByteArrayForCategory(cd.Category);
            if (b.Length != data.Length)
            {
                throw new ArgumentException($"data length dismatch!");
            }
            switch (cd.Category)
            {
                case CharCategories.Ascii:
                    lock (KCTEXT16)
                    {
                        KCTEXT16.Seek(KCHeaderOffset + cd.RelativePos * b.Length, SeekOrigin.Begin);
                        KCTEXT16.Write(data, 0, data.Length);
                    }
                    break;
                case CharCategories.Symbol:
                    lock (KCCHIN16)
                    {
                        KCCHIN16.Seek(KCHeaderOffset + cd.RelativePos * b.Length, SeekOrigin.Begin);
                        KCCHIN16.Write(data, 0, data.Length);
                    }
                    break;
                case CharCategories.Chinese:
                    lock (KCCHIN16)
                    {
                        KCCHIN16.Seek(KCHeaderOffset + SymbolDataOffset + cd.RelativePos * b.Length, SeekOrigin.Begin);
                        KCCHIN16.Read(data, 0, data.Length);
                    }
                    break;
                default: throw new NotSupportedException();
            }
        }



    }
}
