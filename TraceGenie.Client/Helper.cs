using System;
using System.Collections.Generic;
using TraceGenie.Client.Models;

namespace TraceGenie.Client
{
    public static class Helper
    {
        public static string ClearFromNbsps(this string text)
        {
            return text.Replace("&nbsp;", "");
        }
        public static string ClearFromDoubleSpaces(this string text)
        {
            return text.Replace("     ", " ");
        }
        public static string ClearFromTags(this string text)
        {
            return text.Replace("<h4>", "").Replace("</h4>", "").Replace("</br>", " ").Replace("<br>", " ");
        }


        public static List<TraceGenieEntry> GetFakeActiveEntries()
        {
            return new List<Models.TraceGenieEntry> {
            new Models.TraceGenieEntry { FullName = "Jan Niezbędny", Address = "lolo" },
            new Models.TraceGenieEntry { FullName = "Tomasz Kowalski", Address = "lolo" },
            new Models.TraceGenieEntry { FullName = "Niezbędny Jan", Address = "lolo" },
            new Models.TraceGenieEntry { FullName = "Jeremy Irons", Address = "lolo" },
            new Models.TraceGenieEntry { FullName = "Samuel L. Jackson", Address = "lolo" },
            new Models.TraceGenieEntry { FullName = "Byłeś serca biciem", Address = "lolo" },

            };
        }
    }
}
