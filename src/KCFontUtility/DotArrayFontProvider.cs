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
            Size16 = 16, Size24 = 24, Size32 = 32, Size48 = 48
        }
        //不同字型檔轉為byte[]的實作方式不同
        public abstract byte[] GetFontData(FontSize sz, bool halfWidth);

        //寬度所需位元數
        protected abstract int GetWidthBytes(FontSize fontSize, bool halfWidth);

        //
        protected abstract int GetHeightBytes(FontSize fontSize, bool halfWidth);

        Encoding big5Enc = Encoding.GetEncoding(950);

        public virtual bool IsHalfWidth(char ch)
        {
            return big5Enc.GetBytes(ch.ToString()).Length == 1;
        }

        //取得特定字元的點陣資料(byte[])
        public byte[] GetCharData(char ch, FontSize sz = FontSize.Size24)
        {
            byte[] b = Encoding.GetEncoding(950).GetBytes(new char[] { ch });

            //ASCII 1-254採半形
            bool halfWidth = IsHalfWidth(ch);
            int offset = -1;

            var wBytes = GetWidthBytes(sz, halfWidth);
            var hBytes = GetHeightBytes(sz, halfWidth);

            int arraySize = wBytes * hBytes;
            byte[] result = new byte[arraySize];
            //半形時依ASCII碼決定資料起始位址
            if (halfWidth)
            {
                offset = arraySize * b[0];
            }
            else
            {
                //全形文字依倚天字型檔的存放規則
                //http://www.cnblogs.com/armstrong-cn/archive/2011/09/01/2161567.html
                byte hi = b[0], lo = b[1];
                int serCode = (hi - 161) * 157 + (lo >= 161 ? lo - 161 + 1 + 63 : lo - 64 + 1);
                if (serCode >= 472 && serCode < 5872)
                    offset = (serCode - 472) * arraySize;
                else if (serCode >= 6281 && serCode <= 13973)
                    offset = (serCode - 6281) * arraySize + 5401 * arraySize;
            }
            if (offset < 0) return null;
            Buffer.BlockCopy(GetFontData(sz, halfWidth), offset, result, 0, arraySize);
            return result;

        }
        //將點陣內容由byte[]轉為長*寬的二維byte[,]，1表示該點要顯示, 0表示該點留白
        public byte[,] GetDotArray(byte[] data, int w, int h, FontSize sz)
        {
            //偵測是否為半形字
            bool halfWidth = data.Length == GetWidthBytes(sz, true) * GetHeightBytes(sz, true);
            if (halfWidth) w = w / 2;
            var wBytes = GetWidthBytes(sz, halfWidth);
            var hBytes = GetHeightBytes(sz, halfWidth);

            //如為半形字，寬度減半
            //宣告二維陣列以存放點陣資料
            byte[,] dotArray = new byte[h, w];
            byte b = 0;
            for (int y = 0; y < h; y++)
            {
                int offset = wBytes * y;
                for (int x = 0; x < w; x++)
                {
                    if (x > wBytes * 8 - 1)
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
