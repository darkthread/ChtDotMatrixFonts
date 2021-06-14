using System;
using System.Collections.Generic;
using System.Text;

namespace KCFontUtility
{
    //允許以不同廠商字型檔作為來源的點陣字型物件
    public abstract class DotArrayFontProvider
    {
        //宣告不同的尺寸規格
        public enum FontSize
        {
            Size16 = 16, Size24 = 24
        }

        //不同字型檔轉為byte[]的實作方式不同
        public abstract void FillFont(CharData cd, FontSize sz);

        Encoding big5Enc = Encoding.GetEncoding(950);

        public virtual bool IsHalfWidth(char ch)
        {
            return big5Enc.GetBytes(ch.ToString()).Length == 1;
        }

        //取得特定字元的點陣資料(byte[])
        public FontData GetFontData(char ch, FontSize sz = FontSize.Size24)
        {
            var cd = new CharData(ch);
            FillFont(cd, sz);
            return cd.Font;

        }
        //將點陣內容由byte[]轉為長*寬的二維byte[,]，1表示該點要顯示, 0表示該點留白
        public byte[,] GetDotArray(FontData fontData, int w, int h)
        {
            var wb = fontData.WidthBytes;
            var hb = fontData.HeightBytes;
            var data = fontData.Binary;

            //如為半形字，寬度減半
            //宣告二維陣列以存放點陣資料
            byte[,] dotArray = new byte[h, w];
            byte b = 0;
            for (int y = 0; y < h; y++)
            {
                int offset = wb * y;
                for (int x = 0; x < w; x++)
                {
                    if (x > wb * 8 - 1)
                    {
                        b = 0;
                    }
                    if (x % 8 == 0)
                    {
                        b = offset >= data.Length ? (byte)0 : data[offset];
                        offset++;
                    }
                    dotArray[y, x] =                     
                        (byte)(((b << (x % 8)) & 0x80) != 0 ? 1 : 0);
                }
            }
            return dotArray;
        }
    }
}
