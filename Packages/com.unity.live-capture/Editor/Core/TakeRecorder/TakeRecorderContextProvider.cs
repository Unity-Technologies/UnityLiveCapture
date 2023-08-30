using System;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    abstract class TakeRecorderContextProvider
    {
        public string DisplayName { get; private set; }

        public TakeRecorderContextProvider(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            DisplayName = displayName;
        }

        public abstract ITakeRecorderContext GetContext();

        public virtual void OnNoContextGUI()
        {

        }

        public virtual void OnToolbarGUI(Rect rect)
        {

        }

        public virtual void Update()
        {

        }
    }
}
