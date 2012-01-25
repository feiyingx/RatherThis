using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RatherThis.Code
{
    public class Constants
    {
        public enum Gender
        {
            M,
            F
        }

        public enum QuestionSort
        {
            LATEST,
            TOP_VIEWED
        }

        public enum Filter
        {
            QUESTION,
            ANSWER
        }

        public static string BaseUrl
        {
            get
            {
                string url = "http://www.ratherthis.com";
                if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                    url = "";

                return url;
            }
        }
    }
}