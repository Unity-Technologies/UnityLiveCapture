using System;
using UnityEngine;

namespace Unity.CompanionAppCommon
{
    public class BaseHelpDialogData<T> : ScriptableObject where T: struct, Enum
    {
        [Serializable]
        public struct Entry
        {
            public T id;
            public string title;
            public string content;
        }

#pragma warning disable CS0649
        [SerializeField] Entry[] m_Entries;
#pragma warning restore CS0649

        public Entry[] entries => m_Entries;
    }
}
