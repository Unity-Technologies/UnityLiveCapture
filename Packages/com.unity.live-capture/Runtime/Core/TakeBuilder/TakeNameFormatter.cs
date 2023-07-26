namespace Unity.LiveCapture
{
    class TakeNameFormatter : WildcardFormatter
    {
        public static TakeNameFormatter Instance { get; } = new TakeNameFormatter();

        internal static class Wildcards
        {
            public const string Scene = "<Scene>";
            public const string Name = "<Name>";
            public const string Shot = "<Shot>";
            public const string Take = "<Take>";
            public const string Timecode = "<Timecode>";
        }

        public void ConfigureTake(int sceneNumber, string shotName, int takeNumber)
        {
            m_Replacements[Wildcards.Scene] = sceneNumber.ToString("D3");
            m_Replacements[Wildcards.Shot] = shotName;
            m_Replacements[Wildcards.Take] = takeNumber.ToString("D3");
        }

        public void ConfigureAsset(string name)
        {
            m_Replacements[Wildcards.Name] = name;
        }

        public string GetTakeName()
        {
            var name = Format(LiveCaptureSettings.Instance.TakeNameFormat);
            return FileNameFormatter.Instance.Format(name);
        }

        public string GetAssetName()
        {
            var name = Format(LiveCaptureSettings.Instance.AssetNameFormat);
            return FileNameFormatter.Instance.Format(name);
        }
    }
}
