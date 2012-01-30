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

        static List<String> greetings = new List<string>(){
            "<span>{0}</span> is in the house!", "How are you today <span>{0}</span>?", "What's up <span>{0}</span>!", "Good to see you <span>{0}</span>!", "It's always a pleasure to see you <span>{0}</span>!"
        };
        public static string GetGreetingMessage(string username)
        {
            int size = greetings.Count;
            int randomIndex = DateTime.Now.Second % size;

            return string.Format(greetings.ElementAt(randomIndex), username);
        }
    }
}