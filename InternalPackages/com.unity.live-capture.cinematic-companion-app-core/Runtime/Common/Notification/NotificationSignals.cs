using System;
using Unity.TouchFramework;
using ModalPopupData = Unity.TouchFramework.ModalPopup.ModalPopupData;

namespace Unity.CompanionAppCommon
{
    class ShowNotificationSignal : Signal<string> {}

    class ShowModalSignal
    {
        public string Title = "Alert";
        public string Text = String.Empty;
        public string PositiveText = "Done";
        public string NegativeText = String.Empty;
        public Action PositiveCallback = delegate {};
        public Action NegativeCallback = delegate {};
        public bool ClosePopupOnConfirm = true;
        public float FadeDuration = UIConfig.dialogFadeTime;
        public float BackgroundOpacity = 0.5f;

        public ModalPopupData ToModalPopupData()
        {
            return new ModalPopupData()
            {
                title = Title,
                text = Text,
                positiveText = PositiveText,
                negativeText = NegativeText,
                positiveCallback = PositiveCallback,
                negativeCallback = NegativeCallback,
                closePopupOnConfirm = ClosePopupOnConfirm,
                fadeDuration = FadeDuration,
                backgroundOpacity = BackgroundOpacity
            };
        }
    }
}
