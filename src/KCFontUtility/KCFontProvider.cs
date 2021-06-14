using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KCFontUtility
{
    public class KCFontProvider : DotArrayFontProvider
    {
        Dictionary<string, byte[]> dataPool =
            new Dictionary<string, byte[]>();
        //http://bbs.unix-like.org:8080/boards/FB_chinese/M.1023317709.A
        public KCFontProvider(string fontPath = ".", 
            string chineseFont16FileName = "KCCHIN16.F00",
            string asciiFont16FileName = "KCTEXT16.F00",
            string chineseFont24FileName = "KCCHIN24.F00",
            string asciiFont24FileName = "KCTEXT24.F00")
        {
            byte[] buff = File.ReadAllBytes(
                Path.Combine(fontPath, chineseFont16FileName));

            int offset = 256 + 765 * 2 * 16;
            const int charCount = 13195;
            byte[] data = new byte[charCount * 2 * 15];
            for (int i = 0; i < charCount; i++)
            {
                Buffer.BlockCopy(buff, offset + i * 2 * 14, data, i * 2 * 15, 2 * 14);
            }
            dataPool.Add("C" + FontSize.Size16, data);

            //16x16 全形英數字及符號
            data = new byte[765 * 2 * 16];
            Buffer.BlockCopy(buff, 256, data, 0, data.Length);
            dataPool.Add("S" + FontSize.Size16, data);

            buff = File.ReadAllBytes(
                Path.Combine(fontPath, asciiFont16FileName));
            data = new byte[256 * 16];
            offset = 256;
            for (int i = 0; i < 255; i++)
                Buffer.BlockCopy(buff, offset + i * 16, data, i * 16, 16);
            dataPool.Add("A" + FontSize.Size16, data);

            buff = File.ReadAllBytes(Path.Combine(fontPath, chineseFont24FileName));
            data = new byte[charCount * 72];
            offset = 256 + 765 * 72;
            for (int i = 0; i < charCount; i++)
            {
                Buffer.BlockCopy(buff, offset + i * 72, data, i * 72, 72);
            }
            dataPool.Add("C" + FontSize.Size24, data);

            //24x24 全形英數字及符號
            data = new byte[765 * 72];
            Buffer.BlockCopy(buff, 256, data, 0, data.Length);
            dataPool.Add("S" + FontSize.Size24, data);


            buff = File.ReadAllBytes(Path.Combine(fontPath, asciiFont24FileName));
            data = new byte[256 * 48];
            offset = 256;
            for (int i = 0; i < 255; i++)
                Buffer.BlockCopy(buff, offset + i * 48, data, i * 48, 48);
            dataPool.Add("A" + FontSize.Size24, data);
        }

        public override void FillFont(CharData ch, FontSize sz)
        {
            string key = ch.Category.ToString().Substring(0, 1) + sz;
            if (!dataPool.ContainsKey(key))
                throw new NotImplementedException();
            var data = dataPool[key];

            int wb = (int)sz / 8;
            int hb = (int)sz;
            var offset = 0;
            if (sz == FontSize.Size24)
            {
                switch (ch.Category)
                {
                    case CharCategories.Ascii:
                        wb = 2;
                        break;
                }
            }
            else
            {
                switch (ch.Category)
                {
                    case CharCategories.Ascii:
                        wb = 1;
                        hb = 15;
                        break;
                    case CharCategories.Symbol:
                        hb = 16;
                        break;
                    case CharCategories.Chinese:
                        hb = 15;
                        break;
                }
            }
            var arraySize = wb * hb;
            var buff = new byte[arraySize];
            Array.Copy(data, offset + ch.RelativePos * arraySize, buff, 0, arraySize);
            ch.Font = new FontData
            {
                Binary = buff,
                HeightBytes = hb,
                WidthBytes = wb
            };
        }

        int GetWidthBytes(FontSize fontSize, bool halfWidth)
        {
            switch (fontSize)
            {
                case FontSize.Size16:
                    return halfWidth ? 1 : 2;
                case FontSize.Size24:
                    return halfWidth ? 2 : 3;
                default:
                    throw new NotImplementedException();
            }
        }

        int GetHeightBytes(FontSize fontSize, bool halfWidth)
        {
            switch (fontSize)
            {
                case FontSize.Size16:
                    return 15;
                case FontSize.Size24:
                    return 24;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
