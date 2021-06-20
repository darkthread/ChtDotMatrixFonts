using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KCFontUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FontDemo.Pages
{
    public class Big5ListModel : PageModel
    {
        private readonly KCFont16Adapter kcfa;

        public Big5ListModel(KCFontUtility.KCFont16Adapter kcfa)
        {
            this.kcfa = kcfa;
        }
        public void OnGet()
        {
        }

        char ParseBig5Code(string big5Code)
        {
            return Encoding.GetEncoding(950).GetString(new byte[] {
                Convert.ToByte(big5Code.Substring(0, 2), 16),
                Convert.ToByte(big5Code.Substring(2, 2), 16)
            })[0];
        }
        
        public IActionResult OnGetKCFont(string big5Code)
        {
            return Content(BitConverter.ToString(kcfa.ReadFont(ParseBig5Code(big5Code))));
        }
        public IActionResult OnGetKCFontBmp(string big5Code)
        {
            return File(kcfa.GetCharBmp(ParseBig5Code(big5Code)), "image/bmp");
        }

        public IActionResult OnGetDiyFontBmp(string big5Code, string fontName)
        {
            using (var ms = new MemoryStream())
            {
                var bmp = kcfa.DrawCharBmp(ParseBig5Code(big5Code), fontName);
                bmp.Save("d:\\TestSaveFile.bmp", ImageFormat.Bmp);
                bmp.Save(ms, ImageFormat.Bmp);
                System.IO.File.WriteAllBytes("d:\\TestSaveBytes.bmp", ms.ToArray());
                return File(ms.ToArray(), "image/bmp");
            }
        }

        public IActionResult OnGetDiyFont(string big5Code, string fontName)
        {
            var bmp = kcfa.DrawCharBmp(ParseBig5Code(big5Code), fontName);
            bmp.Save("d:\\TestGetData.bmp", ImageFormat.Bmp);
            return Content(BitConverter.ToString(kcfa.ExtractFontData(bmp, big5Code.CompareTo("A4") < 0 ? 16 : 14)));
        }
    }
}
