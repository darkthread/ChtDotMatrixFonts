using System;
using System.Collections.Generic;
using System.Text;

namespace KCFontUtility
{
    public enum CharCategories
    {
        Unknown,
        Ascii,
        Symbol,
        Chinese
    }

    public class CharData
    {
        public char Char { get; set; }
        public CharCategories Category { get; set; } = CharCategories.Unknown;
        public int RelativePos { get; set; }
        public FontData Font { get; set; }

        static Encoding big5Enc = Encoding.GetEncoding(950);

        public CharData(char ch)
        {
            byte[] b = big5Enc.GetBytes(ch.ToString());
            if (b.Length == 1)
            {
                Category = CharCategories.Ascii;
                RelativePos = b[0];
            }
            else if (b.Length == 2 &&
                b[0] >= 0xA1 && b[0] <= 0xA3 &&
                b[1] >= 0x40 && b[1] < 0xFF)
            {
                Category = CharCategories.Symbol;
                byte hi = b[0], lo = b[1];
                RelativePos = (hi - 0xA1) * 157 + (lo >= 0xA1 ? lo - 0xA1 + 63 : lo - 0x40);
            }
            else
            {
                Category = CharCategories.Chinese;
                //全形文字依倚天字型檔的存放規則
                //http://www.cnblogs.com/armstrong-cn/archive/2011/09/01/2161567.html
                byte hi = b[0], lo = b[1];
                int serCode = (hi - 161) * 157 + (lo >= 161 ? lo - 161 + 1 + 63 : lo - 64 + 1);
                if (serCode >= 472 && serCode < 5872)
                    RelativePos = (serCode - 472);
                else if (serCode >= 6281 && serCode <= 13973)
                    RelativePos = (serCode - 6281) + 5401;
            }
        }

    }
}
