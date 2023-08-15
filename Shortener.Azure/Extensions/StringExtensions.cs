using System.Text.RegularExpressions;

namespace Shortener.AzureServices.Extensions
{
    public static class StringExtensions
    {
        public static readonly Regex DisallowedCharsInTableKeys = new Regex(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F\.]", RegexOptions.Compiled);

        public static string SanitiseForTableKey(this string input)
        {
            return DisallowedCharsInTableKeys.Replace(input, "_");
        }
    }
}
