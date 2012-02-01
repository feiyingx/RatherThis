using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RatherThis.Code
{
    public static class Extensions
    {
        //extension for relative date time format
        public static string TimeAgo(this DateTime date)
        {
            TimeSpan timeSince = DateTime.Now.Subtract(date);
            if (timeSince.TotalMinutes < 1) return "just now";
            if (timeSince.TotalMinutes < 2) return "a minute ago";
            if (timeSince.TotalMinutes < 60) return string.Format("{0} minutes ago", timeSince.Minutes);
            if (timeSince.TotalMinutes < 120) return "an hour ago";
            if (timeSince.TotalHours < 24) return string.Format("{0} hours ago", timeSince.Hours);
            if (timeSince.TotalDays < 2) return "yesterday";
            if (timeSince.TotalDays < 7) return string.Format("{0} days ago", timeSince.Days);
            if (timeSince.TotalDays < 14) return "last week";
            if (timeSince.TotalDays < 21) return "2 weeks ago";
            if (timeSince.TotalDays < 28) return "3 weeks ago";
            if (timeSince.TotalDays < 60) return "last month";
            if (timeSince.TotalDays < 365) return string.Format("{0} months ago", Math.Round(timeSince.TotalDays / 30));
            if (timeSince.TotalDays < 730) return "last year";
            return string.Format("{0} years ago", Math.Round(timeSince.TotalDays / 365));
        }

        public static string LimitString(this string str, int limit)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= limit)
                return str;

            return string.Format("{0}...", str.Substring(0, limit));
        }

        public static string UppercaseFirst(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            char[] letters = str.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        public static string ReverseString(this string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }
}