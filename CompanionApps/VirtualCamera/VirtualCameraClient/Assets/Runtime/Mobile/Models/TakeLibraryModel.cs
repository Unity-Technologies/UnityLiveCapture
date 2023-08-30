using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    // TODO poor name, a place to handle take collections, selection, more of a model
    // Note: not involved in previews
    class TakeLibraryModel :
        ITakeLibraryModel,
        ITakeDescriptorsListener,
        ITakeSelectionListener,
        ISlateIterationBaseListener,
        IDisposable
    {
        [Inject]
        SignalBus m_SignalBus;
        [Inject]
        List<ITakeSelectionGuidListener> m_TakeSelectionListeners;
        [Inject]
        List<ISlateIterationBaseGuidListener> m_SlateIterationBaseListeners;

        readonly List<TakeDescriptor> m_TakeDescriptors = new List<TakeDescriptor>();
        readonly Dictionary<Guid, VcamTrackMetadataDescriptor> m_MetadataDescriptors = new Dictionary<Guid, VcamTrackMetadataDescriptor>();
        readonly Dictionary<Guid, TakeDescriptor> m_TakeDescriptorsDict = new Dictionary<Guid, TakeDescriptor>();

        Guid m_SelectedTake = Guid.Empty;
        Guid m_SlateIterationBase = Guid.Empty;
        int m_CachedSlateIterationBaseIndex = -1;
        bool m_PendingIterationBaseUpdate;

        public ReadOnlyCollection<TakeDescriptor> TakeDescriptors => m_TakeDescriptors.AsReadOnly();

        public Guid SelectedTake => m_SelectedTake;

        public Guid SlateIterationBase => m_SlateIterationBase;

        public void Dispose()
        {
            m_TakeDescriptors.Clear();
            m_MetadataDescriptors.Clear();
            m_TakeDescriptorsDict.Clear();
        }

        public void SetTakeDescriptors(TakeDescriptor[] descriptors)
        {
            m_TakeDescriptors.Clear();
            m_TakeDescriptors.AddRange(descriptors);

            m_TakeDescriptorsDict.Clear();

            foreach (var descriptor in m_TakeDescriptors)
            {
                m_TakeDescriptorsDict.Add(descriptor.Guid, descriptor);
            }

            m_SignalBus.Fire(new TakeDescriptorsChangedSignal());

            if (m_PendingIterationBaseUpdate)
            {
                m_PendingIterationBaseUpdate = false;
                SetSlateIterationBase(m_CachedSlateIterationBaseIndex);
            }
        }

        public void SetSelectedTake(int index)
        {
            m_SelectedTake = GetTakeGuid(index);

            foreach (var listener in m_TakeSelectionListeners)
            {
                listener.SetSelectedTake(m_SelectedTake);
            }
        }

        public void SetSlateIterationBase(int index)
        {
            // We may receive iteration base index before take data.
            if (index != -1 && m_TakeDescriptors.Count == 0)
            {
                m_PendingIterationBaseUpdate = true;
                m_CachedSlateIterationBaseIndex = index;
            }

            m_SlateIterationBase = GetTakeGuid(index);

            foreach (var listener in m_SlateIterationBaseListeners)
            {
                listener.SetSlateIterationBase(m_SlateIterationBase);
            }
        }

        public void SetVcamTrackMetadataDescriptors(VcamTrackMetadataDescriptor[] descriptors)
        {
            m_MetadataDescriptors.Clear();
            foreach (var descriptor in descriptors)
            {
                // TODO we may receive multiple metadata entry per take.
                // For now, we'll display the last one received.
                m_MetadataDescriptors[descriptor.TakeGuid] = descriptor;
            }

            m_SignalBus.Fire(new TakeMetadataDescriptorsChangedSignal());
        }

        public bool TryGetMetadata(Guid takeGuid, out VcamTrackMetadataDescriptor descriptor)
        {
            return m_MetadataDescriptors.TryGetValue(takeGuid, out descriptor);
        }

        public bool TryGetDescriptor(Guid takeGuid, out TakeDescriptor descriptor)
        {
            return m_TakeDescriptorsDict.TryGetValue(takeGuid, out descriptor);
        }

        Guid GetTakeGuid(int index)
        {
            if (TryGetTakeDescriptor(index, out var descriptor))
            {
                return descriptor.Guid;
            }

            return Guid.Empty;
        }

        bool TryGetTakeDescriptor(int index, out TakeDescriptor descriptor)
        {
            if (index < 0 || index > m_TakeDescriptors.Count - 1)
            {
                descriptor = null;
                return false;
            }

            descriptor = m_TakeDescriptors[index];
            return true;
        }
    }
}
