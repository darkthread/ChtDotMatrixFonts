using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
            switch(catg)
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
            switch(cd.Category)
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
            //Graphics.FromImage(bmp).FillRectangle(new SolidBrush(Color.White), 0, 0, bmp.Width, bmp.Height);
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
                //var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);
                //var monochrome = new Bitmap(bmp.Width, bmp.Height, bmpData.Stride, PixelFormat.Format4bppIndexed, bmpData.Scan0);
                //bmp.UnlockBits(bmpData);
                //monochrome.Save(ms, ImageFormat.Bmp);
                //bmp.Save(ms, ImageFormat.Gif);
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
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
