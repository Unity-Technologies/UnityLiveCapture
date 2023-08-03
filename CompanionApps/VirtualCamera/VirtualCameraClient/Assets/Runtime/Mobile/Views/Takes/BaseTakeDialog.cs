using UnityEngine;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    // TODO should move higher we need a generic way of displaying dialogs,
    // maybe generalize BasePopup?
    public class BaseTakeDialog : MonoBehaviour, IPresentable
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
