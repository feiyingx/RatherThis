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

        public static string GetMetaKeywords(int questionCategory)
        {
            string metaKeywords = "";
            //categories are defined inside config
            switch (questionCategory)
            {
                case 1:
                    //love and sex
                    metaKeywords = "sexy would you rather questions, dirty would you rather questions, love would you rather questions, would you rather dirty, would you rather questions about love, would you rather for adults, would you rather adult game, would you rather adult quiz, relationship would you rather questions, rather questions about sex, questions about love, " +
                               "sex questions, fun sex questions, weird sex questions";
                    break;
                case 2:
                    //sports
                    metaKeywords = "would you rather sports questions, would you rather sports, would you rather questions about sports, would you rather play basketball or football";
                    break;
                case 3:
                    //tech games
                    metaKeywords = "would you rather play dota, would you rather get dslr, would you rather get iphone, would you rather get android, would you rather tech questions, would you rather technology, would you rather questions for games" +
                               "would you rather play d3, would you rather play diablo 3";
                    break;
                case 4:
                    //news politics
                    metaKeywords = "who would you rather be president, would you rather questions about politics, would you rather have obama, would you rather keep obama, would you rather stop the war, would you rather raise taxes";
                    break;
                case 5:
                    //people life
                    metaKeywords = "would you rather questions about life, would you rather date a celebrity, deep would you rather questions, philosophical questions, questions that make you think";
                    break;
                case 6:
                    //school and work
                    metaKeywords = "would you rather questions about work, would you rather questions about school, would you rather work questions, would you rather study questions";
                    break;
                default:
                    metaKeywords = "random would you rather questions, fun random questions to ask your friends, questions to ask your friends, gross would you rather questions, would you rather questions gross";
                    break;
            }

            return metaKeywords;
        }

        public static string GetMetaDescription(int questionCategory)
        {
            string metaDesc = "";
            switch (questionCategory)
            {
                case 1:
                    //love sex
                    metaDesc = "Find lots of fun and dirty questions about sex, love, relationships, and others. Would you rather have sex with a coworker or a complete stranger? Would you rather have sex only with the love of your life or with anyone but your love? See what other people have answered and find out what kind of person you are!";
                    break;
                case 2:
                    //sports
                    metaDesc = "Find tons of fun and random questions about sports and atheletes. Would you rather play football or basketball? Would you rather be the fastest person in the world or the strongest person in the world? See what other people have answered!";
                    break;
                case 3:
                    //tech games
                    metaDesc = "Find lots of fun and random questions about technology and games. Would you rather play DOTA 2 or Diablo 3? See what others have answered and join the fun!";
                    break;
                case 4:
                    //news politics
                    metaDesc = "Find lots of interesting questions about politics and current events. Who would you rather have as our next president? What would you rather do about taxes? Come and join the discussion and see what others say!";
                    break;
                case 5:
                    //people life
                    metaDesc = "Find lots of interesting questions about life. Would you rather be successful financially at the cost of your family or have no money but have a happy and healthy family? Think about it and let us know what your answer is!";
                    break;
                case 6:
                    //school and work
                    metaDesc = "Find lots of random and funny questions about work and school. Would you rather work for the rest of your life or stay in school for the rest of your life? Find out what others answered!";
                    break;
                default:
                    metaDesc = "Find tons of dirty, sexy, random, fun, and gross Would You Rather questions. Would you rather always have an Angry face or a Gloomy face? Think about it and let us know your answer!";
                    break;
            }

            return metaDesc;
        }
    }

    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}