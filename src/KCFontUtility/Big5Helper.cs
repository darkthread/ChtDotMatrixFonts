using System;
using System.Collections.Generic;
using System.Text;

namespace KCFontUtility
{
    public class Big5Helper
    {

        public static Dictionary<int, char> AllChars = new Dictionary<int, char>();
        static Encoding big5 = Encoding.GetEncoding(950);

        public Big5Helper()
        {
            for (byte hi = 0xa1; hi <= 0xf9; hi++)
            {
                for (byte lo = 0x40; lo <0x7f; lo++)
                {
                    AllChars.Add(hi * 256 + lo, big5.GetString(new byte[] { hi, lo })[0]);
                }
                for (byte lo = 0xa1; lo < 0xff; lo++)
                {
                    AllChars.Add(hi * 256 + lo, big5.GetString(new byte[] { hi, lo })[0]);
                }
            }
        }

    }
}
