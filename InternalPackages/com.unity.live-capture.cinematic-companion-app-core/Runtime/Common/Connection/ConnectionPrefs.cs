using UnityEngine;

namespace Unity.CompanionAppCommon
{
    static class ConnectionPrefs
    {
        const string k_Ip = "connection-ip";
        const string k_Port = "connection-port";

        static readonly ConnectionModel s_Default = new ConnectionModel()
        {
            Ip = "192.168.0.1",
            Port = 9000
        };

        public static void Load(this ConnectionModel connection)
        {
            connection.Ip = PlayerPrefs.GetString(k_Ip, s_Default.Ip);
            connection.Port = PlayerPrefs.GetInt(k_Port, s_Default.Port);
        }

        public static void Save(this ConnectionModel connection)
        {
            PlayerPrefs.SetString(k_Ip, connection.Ip);
            PlayerPrefs.SetInt(k_Port, connection.Port);

            PlayerPrefs.Save();
        }

        public static void RestoreDefaults(this ConnectionModel connection)
        {
            s_Default.Save();
            connection.Load();
        }
    }
}
