using System.Text.RegularExpressions;

namespace Unity.LiveCapture
{
    class FileNameFormatter
    {
        public static FileNameFormatter Instance { get; } = new FileNameFormatter();

        static readonly string s_InvalidFilenameChars = Regex.Escape("/?<>\\:*|\"");
        static readonly string s_InvalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", s_InvalidFilenameChars);

        public string Format(string name)
        {
            return Regex.Replace(name, s_InvalidRegStr, "_");
        }
    }
}
