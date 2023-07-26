using System;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class TakeIterationController : BaseTakeGalleryController,
        ISlateIterationBaseGuidListener, IChannelFlagsListener
    {
        const string k_NoIterationBaseLabel = "None";

        [Inject]
        ITakeLibraryModel m_TakeLibraryModel;
        [Inject]
        ITakeSelectionView m_SelectionView;
        [Inject]
        ITakeIterationView m_IterationView;
        [Inject]
        IPreviewManager m_PreviewManager;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        SignalBus m_SignalBus;

        public override void Initialize()
        {
            base.Initialize();

            CloseSelectionView();

            // TODO where do we manage iteration view display? Main view?

            m_IterationView.onChannelFlagsChanged += OnChannelFlagsChanged;
            m_IterationView.onClearIterationBase += OnClearIterationBase;
            m_IterationView.onSetIterationBase += OnSetIterationView;
            m_IterationView.onCloseClicked += OnCloseView;

            m_SelectionView.CancelClicked += OnSelectionCancelClicked;
            m_SelectionView.DoneClicked += OnSelectionDoneClicked;
            m_SelectionView.TakeSelected += OnTakeSelected;
        }

        public override void Dispose()
        {
            m_IterationView.onChannelFlagsChanged -= OnChannelFlagsChanged;
            m_IterationView.onClearIterationBase -= OnClearIterationBase;
            m_IterationView.onSetIterationBase -= OnSetIterationView;
            m_IterationView.onCloseClicked -= OnCloseView;

            m_SelectionView.CancelClicked -= OnSelectionCancelClicked;
            m_SelectionView.DoneClicked -= OnSelectionDoneClicked;
            m_SelectionView.TakeSelected -= OnTakeSelected;

            base.Dispose();
        }

        protected override ITakeGalleryView GetGalleryView() => m_SelectionView;
        protected override IPreviewManager GetPreviewManager() => m_PreviewManager;
        protected override ITakeLibraryModel GetTakeLibraryModel() => m_TakeLibraryModel;
        protected override SignalBus GetSignalBus() => m_SignalBus;

        public void SetSlateIterationBase(Guid value)
        {
            m_SelectionView.SetSelectedThumbnail(value);
            UpdateSelectionViewButtons();

            if (m_TakeLibraryModel.TryGetDescriptor(value, out var descriptor))
            {
                m_IterationView.IterationBaseName = descriptor.ShotName;
                m_IterationView.CanClearIterationBase = true;
            }
            else
            {
                m_IterationView.IterationBaseName = k_NoIterationBaseLabel;
                m_IterationView.CanClearIterationBase = false;
            }
        }

        public void SetChannelFlags(VirtualCameraChannelFlags channelFlags)
        {
            m_IterationView.SetChannelFlags(channelFlags);
        }

        void OnChannelFlagsChanged(VirtualCameraChannelFlags channelFlags)
        {
            m_SignalBus.Fire(new SendChannelFlagsSignal(){value = channelFlags});
        }

        // TODO Should not be done here
        void OnCloseView()
        {
            m_SignalBus.Fire(new ShowTakeIterationSignal() { value = false });
        }

        void OnClearIterationBase() => m_CompanionApp.ClearIterationBase();

        void OnSetIterationView()
        {
            m_SelectionView.SetSelectedThumbnail(m_TakeLibraryModel.SlateIterationBase);
            UpdateSelectionViewButtons();
            OpenSelectionView();
        }

        void OnSelectionCancelClicked() => CloseSelectionView();

        void OnSelectionDoneClicked()
        {
            CloseSelectionView();

            var iterationBaseGuid = m_SelectionView.GetSelectedThumbnail();
            if (iterationBaseGuid != Guid.Empty)
            {
                m_CompanionApp.SetIterationBase(iterationBaseGuid);
            }
            else
            {
                Debug.LogError("Selection Done clicked while no thumbnail is selected, " +
                    "Done button should have been disabled.");
            }
        }

        void OnTakeSelected(Guid guid)
        {
            // Selected take will always be updated in response to the server.
            // But the selection can be locally modified by the user and a server update
            // will only be sent if the user presses "Done"
            m_SelectionView.SetSelectedThumbnail(guid);
            UpdateSelectionViewButtons();
        }

        // TODO Could be handled internally in the selection view.
        void UpdateSelectionViewButtons()
        {
            var selectedThumbnail = m_SelectionView.GetSelectedThumbnail();
            m_SelectionView.DoneButtonEnabled = selectedThumbnail != Guid.Empty;
        }

        void OpenSelectionView() => m_SelectionView.Show();

        void CloseSelectionView() => m_SelectionView.Hide();
    }
}
