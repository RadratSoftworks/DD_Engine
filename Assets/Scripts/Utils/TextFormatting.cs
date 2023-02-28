namespace DDEngine.Utils
{
    public static class TextFormatting
    {
        public static bool IsTextFullItalic(string text)
        {
            return text.StartsWith("<i>") && text.EndsWith("</i>");
        }

        public static bool IsTextFullMiddle(string text)
        {
            return text.StartsWith("<m>") && text.EndsWith("</m>");
        }

        public static string PostTransform(string preTransformedText)
        {
            return preTransformedText.Replace("<m>", "").Replace("</m>", "");
        }
    }
}
