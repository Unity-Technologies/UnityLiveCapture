using System;
using System.Collections.Generic;
using Unity.CompanionAppCommon;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    abstract class BaseTakeGalleryController :
        IInitializable,
        IDisposable,
        IPreviewReceiver
    {
        protected abstract ITakeGalleryView GetGalleryView();
        protected abstract IPreviewManager GetPreviewManager();
        protected abstract ITakeLibraryModel GetTakeLibraryModel();

        protected abstract SignalBus GetSignalBus();

        readonly List<TakeThumbnailData> m_TmpTakeThumbnailDatas = new List<TakeThumbnailData>();
        readonly Dictionary<Guid, Guid> m_PreviewToTakeDict = new Dictionary<Guid, Guid>();
        readonly TakeNameFormatter m_TakeNameFormatter = new TakeNameFormatter();
        // Must track locally the list of awaited assets for cleanup.
        readonly List<Guid> m_Previews = new List<Guid>();

        public virtual void Initialize()
        {
            var signalBus = GetSignalBus();
            signalBus.Subscribe<TakeDescriptorsChangedSignal>(UpdateTakeDescriptors);
            signalBus.Subscribe<TakeMetadataDescriptorsChangedSignal>(UpdateMetadataDescriptors);
        }

        public virtual void Dispose()
        {
            var signalBus = GetSignalBus();
            signalBus.Unsubscribe<TakeDescriptorsChangedSignal>(UpdateTakeDescriptors);
            signalBus.Unsubscribe<TakeMetadataDescriptorsChangedSignal>(UpdateMetadataDescriptors);

            RemovePreviewReceivers();

            m_TmpTakeThumbnailDatas.Clear();
            m_PreviewToTakeDict.Clear();
            m_Previews.Clear();
        }

        public virtual void Receive(Guid previewGuid, Texture2D preview)
        {
            if (m_PreviewToTakeDict.TryGetValue(previewGuid, out var takeGuid))
            {
                GetGalleryView().SetPreview(takeGuid, preview);
            }
        }

        protected virtual void UpdateTakeDescriptors()
        {
            RemovePreviewReceivers();

            m_PreviewToTakeDict.Clear();

            foreach (var descriptor in GetTakeLibraryModel().TakeDescriptors)
            {
                if (descriptor.Screenshot != Guid.Empty)
                {
                    m_PreviewToTakeDict.Add(descriptor.Screenshot, descriptor.Guid);
                }
            }

            // Update the gallery before handling previews, otherwise the gallery won't be able to assign previews.
            UpdateGalleryThumbnails();

            AddPreviewReceivers();
        }

        protected virtual void UpdateMetadataDescriptors()
        {
            // Full gallery refresh since thumbnail description is inferred from metadata.
            RemovePreviewReceivers();
            UpdateGalleryThumbnails();
            AddPreviewReceivers();
        }

        protected void UpdateGalleryThumbnails()
        {
            m_TmpTakeThumbnailDatas.Clear();

            foreach (var descriptor in GetTakeLibraryModel().TakeDescriptors)
            {
                m_TmpTakeThumbnailDatas.Add(FromDescriptor(descriptor));
            }

            GetGalleryView().SetThumbnails(m_TmpTakeThumbnailDatas);
        }

        void AddPreviewReceivers()
        {
            m_Previews.Clear();

            var previewManager = GetPreviewManager();

            foreach (var descriptor in GetTakeLibraryModel().TakeDescriptors)
            {
                var preview = descriptor.Screenshot;
                m_Previews.Add(preview);
                previewManager.AddReceiver(preview, this);
            }
        }

        void RemovePreviewReceivers()
        {
            if (SceneState.IsBeingDestroyed)
                return;

            var previewManager = GetPreviewManager();

            foreach (var preview in m_Previews)
            {
                previewManager.RemoveReceiver(preview, this);
            }
        }

        protected string GetTakeName(TakeDescriptor descriptor)
        {
            m_TakeNameFormatter.ConfigureTake(descriptor.SceneNumber, descriptor.ShotName, descriptor.TakeNumber);
            return m_TakeNameFormatter.GetTakeName();
        }

        TakeThumbnailData FromDescriptor(TakeDescriptor descriptor)
        {
            var metadata = String.Empty;

            if (GetTakeLibraryModel().TryGetMetadata(descriptor.Guid, out var metadataDescriptor))
            {
                var sensorSize = metadataDescriptor.SensorSize;
                var focalLength = metadataDescriptor.FocalLength;
                metadata = $"{focalLength.ToString("F0")}mm - {sensorSize.x.ToString("F0")}x{sensorSize.x.ToString("F0")}mm";
            }

            return new TakeThumbnailData
            {
                guid = descriptor.Guid,
                name = GetTakeName(descriptor),
                metadata = metadata,
                isStarred = descriptor.Rating > 0
            };
        }
    }
}
