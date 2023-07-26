using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.LiveCapture.CompanionApp;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class TakeLibraryController : BaseTakeGalleryController,
        IDeviceModeListener,
        ITakeSelectionGuidListener
    {
        const string k_DateFormat = "yyyy_MM_dd";
        const string k_TimeFormat = "HH:mm:ss:ff";
        const string k_TimeSpanFormat = @"hh\:mm\:ss\:ff";
        const string k_NotAvailable = "N/A";

        [Inject]
        ITakeLibraryModel m_TakeLibraryModel;
        [Inject]
        IPreviewManager m_PreviewManager;
        [Inject]
        ITakeLibraryView m_LibraryView;
        [Inject]
        TakeMetadataView m_MetadataView;
        [Inject]
        TakeRenameView m_RenameView;
        [Inject]
        TakeDeleteView m_DeleteView;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        SignalBus m_SignalBus;

        Guid m_InteractedWithTake = Guid.Empty;
        Guid m_SelectedTake = Guid.Empty;

        readonly List<IPresentable> m_Dialogs = new List<IPresentable>();
        readonly TakeDescriptor m_TmpDescriptor = new TakeDescriptor();

        public override void Initialize()
        {
            base.Initialize();

            m_Dialogs.Clear();
            m_Dialogs.Add(m_MetadataView);
            m_Dialogs.Add(m_RenameView);
            m_Dialogs.Add(m_DeleteView);

            ExitDialog();

            m_MetadataView.doneClicked += ExitDialog;

            m_RenameView.cancelClicked += ExitDialog;
            m_RenameView.renameClicked += RenameTakeFromView;

            m_DeleteView.canceled += ExitDialog;
            m_DeleteView.validate += DeleteTakeFromView;

            m_LibraryView.EditTakeClicked += OpenEditTakeView;
            m_LibraryView.DisplayTakeMetadataClicked += OpenMetadataView;
            m_LibraryView.DeleteTakeClicked += OpenDeleteView;
            m_LibraryView.TakeSelected += SendTakeSelected;
            m_LibraryView.CloseClicked += OnCloseView;
            m_LibraryView.TakeRatingChanged += OnTakeRatingChanged;
        }

        public override void Dispose()
        {
            // We'll see if we keep the "FromView" naming,
            // whether we respond to UI or network is a source of confusion.
            m_MetadataView.doneClicked -= ExitDialog;

            m_RenameView.cancelClicked -= ExitDialog;
            m_RenameView.renameClicked -= RenameTakeFromView;

            m_DeleteView.canceled -= ExitDialog;
            m_DeleteView.validate -= DeleteTakeFromView;

            m_LibraryView.EditTakeClicked -= OpenEditTakeView;
            m_LibraryView.DisplayTakeMetadataClicked -= OpenMetadataView;
            m_LibraryView.DeleteTakeClicked -= OpenDeleteView;
            m_LibraryView.TakeSelected -= SendTakeSelected;
            m_LibraryView.CloseClicked -= OnCloseView;
            m_LibraryView.TakeRatingChanged += OnTakeRatingChanged;

            if (!SceneState.IsBeingDestroyed)
            {
                ExitDialog();
            }

            base.Dispose();
        }

        protected override ITakeGalleryView GetGalleryView() => m_LibraryView;
        protected override IPreviewManager GetPreviewManager() => m_PreviewManager;
        protected override ITakeLibraryModel GetTakeLibraryModel() => m_TakeLibraryModel;
        protected override SignalBus GetSignalBus() => m_SignalBus;

        public void SetSelectedTake(Guid value)
        {
            m_SelectedTake = value;
            m_LibraryView.SetSelectedThumbnail(value);
            ExitDialogIfInteractedWithTakeIsNotSelected();
        }

        public void SetDeviceMode(DeviceMode mode)
        {
            if (mode != DeviceMode.Playback)
            {
                m_LibraryView.Hide();
            }

            ExitDialog();
        }

        protected override void UpdateTakeDescriptors()
        {
            base.UpdateTakeDescriptors();
            ExitDialogIfInteractedWithTakeIsNotSelected();
        }

        void SendTakeSelected(Guid guid)
        {
            m_SelectedTake = guid;
            m_CompanionApp.SelectTake(guid);
        }

        void OnTakeRatingChanged(Guid guid, bool value)
        {
            if (GetTakeLibraryModel().TryGetDescriptor(guid, out var descriptor))
            {
                // We use a copy to enforce the model update only in response to the server.
                m_TmpDescriptor.CopyFrom(descriptor);
                m_TmpDescriptor.Rating = value ? 1 : 0;
                m_CompanionApp.UpdateTake(m_TmpDescriptor);
            }
        }

        void OnCloseView()
        {
            m_SignalBus.Fire(new ShowTakeLibrarySignal() {value = false});
            ExitDialog();
        }

        void OpenEditTakeView(Guid guid)
        {
            if (GetTakeLibraryModel().TryGetDescriptor(guid, out var descriptor))
            {
                m_InteractedWithTake = guid;
                SendTakeSelected(guid);
                Show(m_RenameView);

                m_RenameView.SceneNumber = descriptor.SceneNumber;
                m_RenameView.TakeNumber = descriptor.TakeNumber;
                m_RenameView.ShotName = descriptor.ShotName;
                m_RenameView.Description = descriptor.Description;
            }
        }

        void OpenMetadataView(Guid guid)
        {
            if (GetTakeLibraryModel().TryGetDescriptor(guid, out var descriptor))
            {
                string ToStr(float value) => value.ToString("F");

                m_InteractedWithTake = guid;
                SendTakeSelected(guid);

                m_MetadataView.TakeName = GetTakeName(descriptor);
                m_MetadataView.Description = descriptor.Description;
                m_MetadataView.Framerate = $"{descriptor.FrameRate.ToString()} FPS";
                var dateTime = DateTime.FromBinary(descriptor.CreationTime);
                m_MetadataView.Date = dateTime.ToString(k_DateFormat);
                m_MetadataView.Time = dateTime.ToString(k_TimeFormat);
                m_MetadataView.Timeline = descriptor.TimelineName;

                var lengthTimeSpan = TimeSpan.FromSeconds(descriptor.TimelineDuration);
                var numFrames = Mathf.FloorToInt((float) descriptor.TimelineDuration * descriptor.FrameRate.AsFloat());
                m_MetadataView.Length = $"{lengthTimeSpan.ToString(k_TimeSpanFormat)} | {numFrames} frames";

                if (GetTakeLibraryModel().TryGetMetadata(guid, out var metadata))
                {
                    m_MetadataView.FocalLength = $"{ToStr(metadata.FocalLength)} mm";
                    m_MetadataView.FocusDistance = $"{ToStr(metadata.FocusDistance)} m";
                    m_MetadataView.Aperture = $"f/{ToStr(metadata.Aperture)}";

                    if (String.IsNullOrEmpty(metadata.SensorPresetName))
                    {
                        var sensorSize = metadata.SensorSize;
                        m_MetadataView.SensorSize = $"{ToStr(sensorSize.x)} x {ToStr(sensorSize.y)} mm";
                    }
                    else
                    {
                        m_MetadataView.SensorSize = metadata.SensorPresetName;
                    }

                    m_MetadataView.Iso = ToStr(metadata.Iso);
                    m_MetadataView.ShutterSpeed = $"{ToStr(metadata.ShutterSpeed)} s";
                    m_MetadataView.AspectRatio = ToStr(metadata.AspectRatio);
                }
                else
                {
                    m_MetadataView.FocalLength = k_NotAvailable;
                    m_MetadataView.FocusDistance = k_NotAvailable;
                    m_MetadataView.Aperture = k_NotAvailable;
                    m_MetadataView.SensorSize = k_NotAvailable;
                    m_MetadataView.Iso = k_NotAvailable;
                    m_MetadataView.ShutterSpeed = k_NotAvailable;
                    m_MetadataView.AspectRatio = k_NotAvailable;
                }

                Show(m_MetadataView);
            }
        }

        void OpenDeleteView(Guid guid)
        {
            if (GetTakeLibraryModel().TryGetDescriptor(guid, out var descriptor))
            {
                m_InteractedWithTake = guid;
                SendTakeSelected(guid);
                m_DeleteView.takeName = GetTakeName(descriptor);
                Show(m_DeleteView);
            }
        }

        void RenameTakeFromView()
        {
            if (GetTakeLibraryModel().TryGetDescriptor(m_InteractedWithTake, out var descriptor))
            {
                // We use a copy to enforce the model update only in response to the server.
                m_TmpDescriptor.CopyFrom(descriptor);
                m_TmpDescriptor.SceneNumber = m_RenameView.SceneNumber;
                m_TmpDescriptor.TakeNumber = m_RenameView.TakeNumber;
                m_TmpDescriptor.ShotName = m_RenameView.ShotName;
                m_TmpDescriptor.Description = m_RenameView.Description;
                m_CompanionApp.UpdateTake(m_TmpDescriptor);
            }

            ExitDialog();
        }

        void DeleteTakeFromView()
        {
            Assert.IsFalse(m_InteractedWithTake == Guid.Empty);
            m_CompanionApp.DeleteTake(m_InteractedWithTake);
            ExitDialog();
        }

        void ExitDialogIfInteractedWithTakeIsNotSelected()
        {
            if (!(m_SelectedTake != Guid.Empty && m_SelectedTake == m_InteractedWithTake))
            {
                ExitDialog();
            }
        }

        void ExitDialog()
        {
            Show(null);
            m_InteractedWithTake = Guid.Empty;
        }

        void Show(IPresentable presentable)
        {
            // Could take advantage of the fact that at most one dialog will be visible at a time.
            foreach (var dialog in m_Dialogs)
            {
                if (dialog == presentable)
                {
                    dialog.Show();
                }
                else
                {
                    dialog.Hide();
                }
            }
        }
    }
}
