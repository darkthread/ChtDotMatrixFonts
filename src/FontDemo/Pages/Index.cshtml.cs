using KCFontUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FontDemo.Pages
{
    //REF: https://blog.darkthread.net/blog/hello-razor-pages/
    //

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly KCFontProvider kcfp;

        public IndexModel(ILogger<IndexModel> logger, KCFontProvider kcfp)
        {
            _logger = logger;
            this.kcfp = kcfp;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPostDisplay(string message, int fontSize)
        {
            var fs = (DotArrayFontProvider.FontSize)fontSize;
            var sz = fontSize;

            var totalWidth = message.ToCharArray().Select(o => kcfp.IsHalfWidth(o) ? fontSize / 2 : fontSize).Sum();


            Bitmap bmp = new Bitmap(totalWidth * 4, sz * 4);
            SolidBrush bb = new SolidBrush(Color.Black);
            SolidBrush yb = new SolidBrush(Color.Orange);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(bb, 0, 0, bmp.Width, bmp.Height);
            int offset = 0;
            foreach (char ch in message)
            {
                var fontData = kcfp.GetFontData(ch, fs);
                byte[,] d = kcfp.GetDotArray(fontData,                   
                    sz, sz);
                var w = kcfp.IsHalfWidth(ch) ? sz / 2 : sz;
                for (int y = 0; y < sz; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (d[y, x] == 1)
                        {
                            g.FillRectangle(yb, offset + x * 4, y * 4, 3, 3);
                        }
                    }
                }
                offset += w * 4;
            }
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return new FileContentResult(ms.ToArray(), "image/png");
            }
        }
    }
}
