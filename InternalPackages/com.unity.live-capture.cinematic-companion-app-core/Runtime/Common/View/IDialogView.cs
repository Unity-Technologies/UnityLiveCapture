using UnityEngine;

namespace Unity.CompanionAppCommon
{
    interface IDialogView : IPresentable
    {
        bool IsShown { get; }
        bool IsInteractable { get; }
        Vector3 Position { set; }
        Vector2 Size { set; }
        Vector2 Pivot { set; }
    }
}
