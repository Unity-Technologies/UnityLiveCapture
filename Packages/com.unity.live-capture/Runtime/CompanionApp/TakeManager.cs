using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.CompanionApp
{
    interface IAssetManager
    {
        void Save(Object obj);
        T Load<T>(Guid guid) where T : Object;
        void Delete<T>(Guid guid) where T : Object;
        Texture2D GetPreview<T>(Guid guid) where T : UnityEngine.Object;
    }

    interface IAssetPreview
    {
        Texture2D GetAssetPreview<T>(Guid guid) where T : UnityEngine.Object;
    }

    class EditorAssetPreview : IAssetPreview
    {
        public Texture2D GetAssetPreview<T>(Guid guid) where T : UnityEngine.Object
        {
            var texture = default(Texture2D);
#if UNITY_EDITOR
            var asset = AssetDatabaseUtility.LoadAssetWithGuid<T>(guid);

            if (asset != null)
            {
                texture = AssetPreview.GetAssetPreview(asset);
            }
#endif
            return texture;
        }
    }

    class AssetManager : IAssetManager
    {
        public static AssetManager Instance { get; } = new AssetManager();

        IAssetPreview m_AssetPreview;

        public AssetManager() : this(new EditorAssetPreview()) { }

        public AssetManager(IAssetPreview assetPreview)
        {
            m_AssetPreview = assetPreview;
        }

        public void Save(Object asset)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
#endif
        }

        public T Load<T>(Guid guid) where T : Object
        {
            var asset = default(T);
#if UNITY_EDITOR
            asset = AssetDatabaseUtility.LoadAssetWithGuid<T>(guid);
#endif
            return asset;
        }

        public void Delete<T>(Guid guid) where T : Object
        {
#if UNITY_EDITOR
            var path = AssetDatabaseUtility.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
#endif
        }

        public Texture2D GetPreview<T>(Guid guid) where T : UnityEngine.Object
        {
            return m_AssetPreview.GetAssetPreview<T>(guid);
        }
    }
}
