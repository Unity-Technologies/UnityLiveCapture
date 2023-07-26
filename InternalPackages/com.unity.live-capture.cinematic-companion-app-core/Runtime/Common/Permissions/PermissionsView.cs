using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionAppCommon
{
    class PermissionsView : DialogView, IPermissionsView
    {
        public event Action OpenSettingsClicked = delegate {};
        public event Action CancelClicked = delegate {};

        [SerializeField]
        Button m_OpenSettingsButton;

        [SerializeField]
        Button m_CancelButton;

        [SerializeField]
        TextMeshProUGUI m_Message;

        void Awake()
        {
            m_OpenSettingsButton.onClick.AddListener(OnOpenSettingsClicked);
            m_CancelButton.onClick.AddListener(OnCancelClicked);
        }

        void OnDestroy()
        {
            m_OpenSettingsButton.onClick.RemoveListener(OnOpenSettingsClicked);
            m_CancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        void OnOpenSettingsClicked()
        {
            OpenSettingsClicked.Invoke();
        }

        void OnCancelClicked()
        {
            CancelClicked.Invoke();
        }

        public void Show(string message, bool cancelable)
        {
            m_Message.text = message;
            m_CancelButton.gameObject.SetActive(cancelable);

            Show();
        }

        /*
        public void Display(string message, bool hideAutomatically, bool cancelable = false)
        {
            m_Message.text = message;
            m_CancelButton.gameObject.SetActive(cancelable);

            if (hideAutomatically)
            {
                StartAnimation(AnimationInOut(m_DefaultDisplayDuration, m_DefaultFadeDuration));
            }
            else
            {
                StartAnimation(AnimationIn(m_DefaultFadeDuration));
            }
        }
        */

        /*
        public IEnumerator CheckVideoPermission(
            string appName,
            Action onPermissionGranted,
            Action onPermissionDenied)
        {
            if (IOSHelper.HasVideoPermission())
            {
                // We have permission already, abort.
                HideImmediate();
                onPermissionGranted.Invoke();
                yield break;
            }

            // Display permission dialog.
            var message = $"Camera permission is needed for \"{appName}\" to work." +
                $" Allow access in Settings > {appName} > Camera";
            Display(message, false);
            onPermissionDenied.Invoke();

            // Wait for permission to be granted.
            while (!IOSHelper.HasVideoPermission())
            {
                yield return null;
            }

            // Permission was granted. Hide dialog, exit.
            HideImmediate();
            onPermissionGranted.Invoke();
        }
        */
    }
}
