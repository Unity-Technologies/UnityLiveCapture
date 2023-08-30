using System.Collections.Generic;
using System.Text;

namespace Unity.LiveCapture
{
    abstract class WildcardFormatter
    {
        protected readonly Dictionary<string, string> m_Replacements = new Dictionary<string, string>();
        readonly StringBuilder m_StringBuilder = new StringBuilder(256);

        protected string Format(string str)
        {
            m_StringBuilder.Clear();
            m_StringBuilder.Append(str);

            foreach (var pair in m_Replacements)
            {
                if (pair.Value != null)
                {
                    m_StringBuilder.Replace(pair.Key, pair.Value);
                }
            }

            return m_StringBuilder.ToString();
        }
    }
}
