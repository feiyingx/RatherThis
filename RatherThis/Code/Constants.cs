using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;

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
            Latest,
            Top_Rated
        }

        public enum Filter
        {
            QUESTION,
            ANSWER
        }


        private static List<CategoryItem> _catItems = null;
        public static List<CategoryItem> QuestionCategories
        {
            get
            {
                if (_catItems == null)
                {
                    _catItems = new List<CategoryItem>();
                    _catItems.Add(new CategoryItem(1, "Love & Sex"));
                    _catItems.Add(new CategoryItem(2, "Sports"));
                    _catItems.Add(new CategoryItem(3, "Tech & Games"));
                    _catItems.Add(new CategoryItem(4, "News & Politics"));
                    _catItems.Add(new CategoryItem(5, "People & Life"));
                    _catItems.Add(new CategoryItem(6, "School & Work"));
                    _catItems.Add(new CategoryItem(0, "Random"));
                }
                return _catItems;
            }
        }


        public static string BaseUrl
        {
            get
            {
                string url = "http://www.ratherthis.com";
                if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                    url = "http://localhost:26034";

                return url;
            }
        }
    }

    public class CategoryItem
    {
        public CategoryItem(int id, string name)
        {
            CategoryID = id;
            Name = name;
        }
        public int CategoryID { get; set; }
        public string Name { get; set; }
    }
}