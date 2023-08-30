using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A formatter that ensures redundant string entries are given a unique name.
    /// </summary>
    class UniqueNameFormatter
    {
        readonly HashSet<string> m_Names = new HashSet<string>();

        public string Format(string text)
        {
            var name = ObjectNames.GetUniqueName(m_Names.ToArray(), text);
            m_Names.Add(name);
            return name;
        }
    }
}
