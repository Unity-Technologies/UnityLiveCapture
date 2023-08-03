using System;
using System.Collections;
using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IPreviewManager : IAssetManager<Guid, Texture2D>
    {
    }

    interface IPreviewReceiver : IAssetReceiver<Guid, Texture2D>
    {
    }

    // Used for Take thumbnails only at the moment.
    // If we introduced other texture preview usages we'd add new PreviewManager types,
    // and register as ITexturePreviewListener.
    class PreviewManager : AssetManager<Guid, Texture2D>, IPreviewManager, ITexturePreviewListener, IInitializable
    {
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        ICoroutineRunner m_CoroutineRunner;

        Coroutine m_Coroutine;

        readonly CappedSet<Guid> m_ExpectedGuids = new CappedSet<Guid>(128);

        public PreviewManager(uint cacheSize) : base(new LruCache<Guid, Texture2D>(cacheSize)) {}

        public void Initialize()
        {
            m_Coroutine = m_CoroutineRunner.StartCoroutine(RequestAssetCoroutine());
        }

        public override void Dispose()
        {
            m_ExpectedGuids.Dispose();

            base.Dispose();
        }

        public void SetTexturePreview(Guid guid, Texture2D texture)
        {
            // We may have multiple asset managers,
            // they're only interested in the data they individually requested.
            if (m_ExpectedGuids.Contains(guid))
            {
                if (texture != null)
                {
                    AddAsset(guid, texture);
                }
                else
                {
#if !UNITY_EDITOR
                    // When running in editor we usually can't interpret the transmitted textures because of
                    // missing format support. This is why we don't log it in editor.
                    Debug.LogError($"Couldn't interpret transmitted texture with {nameof(guid)}: {guid}");
#endif
                    RemovePendingAsset(guid);
                }
            }
        }

        IEnumerator RequestAssetCoroutine()
        {
            for (;;)
            {
                yield return new WaitForSeconds(.2f);
                foreach (var guid in PendingGuids)
                {
                    m_ExpectedGuids.Add(guid);
                    m_CompanionApp.RequestTexturePreview(guid);
                }
            }
        }
    }
}
