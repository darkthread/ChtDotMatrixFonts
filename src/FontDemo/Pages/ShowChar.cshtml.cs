using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using KCFontUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FontDemo.Pages
{
    public class ShowCharModel : PageModel
    {
        private readonly KCFont16Adapter kcfa;

        public ShowCharModel(KCFont16Adapter kcfa)
        {
            this.kcfa = kcfa;

        }
        public IActionResult OnGetX()
        {
            var sw = new Stopwatch();
            var s = "我達達的馬蹄是美麗的錯誤";
            var ary = s.ToCharArray();
            int times = 1000;
            for (int j = 0; j < 3; j++)
            {
                sw.Restart();
                for (var i = 0; i < times; i++)
                    s.ToCharArray().Select(ch => kcfa.GetCharPng(ch)).ToList();
                sw.Stop();
                Debug.WriteLine($"PNG: {sw.ElapsedTicks:n0}");
                sw.Restart();
                for (var i = 0; i < times; i++)
                    s.ToCharArray().Select(ch => kcfa.GetCharBmp(ch)).ToList();
                sw.Stop();
                Debug.WriteLine($"BMP: {sw.ElapsedTicks:n0}");
            }
            return Content("OK");
        }


        public IActionResult OnGet(string ch)
        {
            if (string.IsNullOrEmpty(ch)) return Content("invalid argument");
            //return File(kcfa.GetCharPng(ch[0]), "image/png");
            kcfa.GetCharPng(ch[0]);
            return File(kcfa.GetCharBmp(ch[0]), "image/bmp");
        }
    }
}
