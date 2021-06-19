using System;
using System.Collections.Generic;
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
        public IActionResult OnGet(string ch)
        {
            if (string.IsNullOrEmpty(ch)) return Content("invalid argument");
            return File(kcfa.GetCharPng(ch[0]), "image/png");
        }
    }
}
