using UnityEngine;
using UnityEngine.Events;

namespace Unity.TouchFramework
{
    public interface IDialog
    {
        /// <summary>
        /// Is the dialog visible?
        /// </summary>
        bool open { get; }

        /// <summary>
        /// Open the dialog window with optional transition.
        /// </summary>
        /// <param name="instant">If true, opens instantly without transition.</param>
        /// <param name="setInteractable">If true, set canvas group to be interactable.</param>
        void Open(bool instant = false, bool setInteractable = true, float fadeTime = -1f);
        /// <summary>
        /// Close the dialog window with optional transition.
        /// </summary>
        /// <param name="instant">If true, opens instantly without transition.</param>
        void Close(bool instant = false, float fadeTime = -1f);
        /// <summary>
        /// Invoked when the dialog is opened
        /// </summary>
        UnityEvent dialogClose { get; }
        /// <summary>
        /// Invoked when the dialog is closed
        /// </summary>
        UnityEvent dialogOpen { get; }
    }
}
