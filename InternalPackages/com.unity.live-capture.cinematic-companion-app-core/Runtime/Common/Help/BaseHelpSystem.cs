using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Unity.TouchFramework;
using Zenject;

namespace Unity.CompanionAppCommon
{
    public struct HelpTooltipData
    {
        [XmlAttribute("id")]
        public string id;
        [XmlElement("title")]
        public string title;
        [XmlElement("content")]
        public string content;
    }

    [XmlRoot("help")]
    public class HelpData
    {
        [XmlArray("tooltips")]
        [XmlArrayItem("tooltip")]
        public List<HelpTooltipData> tooltips = new List<HelpTooltipData>();

        public static HelpData Load(string xmlString)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(HelpData));
                using (var reader = new System.IO.StringReader(xmlString))
                {
                    return serializer.Deserialize(reader) as HelpData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception loading help file: {e}");
                return null;
            }
        }
    }

    interface IHelpModeListener
    {
        void SetHelpMode(bool value);
    }

    class BaseHelpSystem<T, TArgs> : IInitializable
        where T : struct, Enum
        where TArgs : BaseHelpTooltipArgs<T>
    {
        [Inject]
        HelpTooltip m_HelpTooltip;

        [Inject(Id = "Help")]
        TextAsset m_HelpXmlAsset;

        [Inject]
        IHelpView m_HelpView;

        [Inject]
        List<IHelpModeListener> m_Listeners;

        readonly Dictionary<T, HelpTooltipData> m_TooltipsData = new Dictionary<T, HelpTooltipData>();
        readonly Dictionary<T, TArgs> m_TooltipsArgs = new Dictionary<T, TArgs>();

        const float k_Width = 330;

        static void ParseXml(string xmlString, Dictionary<T, HelpTooltipData> data)
        {
            var deserialized = HelpData.Load(xmlString);

            foreach (var entry in deserialized.tooltips)
            {
                if (Enum.TryParse<T>(entry.id, out var id))
                {
                    data.Add(id, entry);
                }
                else
                {
                    Debug.LogError($"Help xml parsing error: id \"{entry.id}\" is invalid.");
                }
            }
        }

        public void Initialize()
        {
            m_TooltipsData.Clear();

            ParseXml(m_HelpXmlAsset.text, m_TooltipsData);

            m_TooltipsArgs.Clear();

            foreach (var args in Resources.FindObjectsOfTypeAll<TArgs>())
            {
                if (IsArgsValid(args))
                {
                    if (m_TooltipsArgs.ContainsKey(args.id))
                    {
                        Debug.LogError($"Help System already registered args for {args.id}.", args.gameObject);
                        continue;
                    }

                    m_TooltipsArgs.Add(args.id, args);
                }
            }
        }

        protected virtual bool IsArgsValid(TArgs args) => args.gameObject.scene.isLoaded;

        public virtual void Toggle(bool value)
        {
            if (value)
            {
                m_HelpView.Show();
            }
            else
            {
                m_HelpTooltip.HideIfNeeded();
                m_HelpView.Hide();
            }

            foreach (var listener in m_Listeners)
            {
                listener.SetHelpMode(value);
            }
        }

        public void OpenTooltip(T id)
        {
            var hasEntry = m_TooltipsData.TryGetValue(id, out var entry);
            var hasArgs = m_TooltipsArgs.TryGetValue(id, out var args);

            if (hasEntry && hasArgs)
            {
                var width = args.overrideWidth ? args.width : k_Width;

                var data = new Unity.TouchFramework.HelpTooltip.HelpTooltipData
                {
                    title = entry.title,
                    text = entry.content,
                    pointedAtTransform = args.pointedAtTransform,
                    targetTransform = args.targetTransform,
                    verticalAlign = args.verticalAlign,
                    horizontalAlign = args.horizontalAlign,
                    width = width
                };

                m_HelpTooltip.Display(data);
            }
            else
            {
                if (!hasEntry)
                {
                    Debug.LogError(
                        $"Could not display help for {id}. No corresponding entry in {nameof(BaseHelpDialogData<T>)}.");
                }

                if (!hasArgs)
                {
                    Debug.LogError(
                        $"Could not display help for {id}. No corresponding {nameof(TArgs)} component found.");
                }
            }
        }

        public void CloseTooltip()
        {
            m_HelpTooltip.HideIfNeeded();
        }
    }
}
