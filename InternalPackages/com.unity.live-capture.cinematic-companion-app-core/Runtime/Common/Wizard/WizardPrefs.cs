using UnityEngine;

namespace Unity.CompanionAppCommon
{
    static class WizardPrefs
    {
        const string k_WasShown = "wizard-was-shown";

        static bool s_WasShown;

        static bool s_IsLoaded;

        public static bool WasShown
        {
            get
            {
                if (!s_IsLoaded)
                {
                    Load();
                    s_IsLoaded = true;
                }
                return s_WasShown;
            }

            set
            {
                s_WasShown = value;
                Save();
            }
        }

        static void Load()
        {
            s_WasShown = PlayerPrefs.GetInt(k_WasShown, 0) == 1;
        }

        static void Save()
        {
            PlayerPrefs.SetInt(k_WasShown, s_WasShown ? 1 : 0);

            PlayerPrefs.Save();
        }
    }
}
