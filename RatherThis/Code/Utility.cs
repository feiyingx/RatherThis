using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RatherThis.Code
{
    public class Utility
    {
        public static bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }
    }
}