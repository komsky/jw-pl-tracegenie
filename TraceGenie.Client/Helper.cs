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
    }
}
