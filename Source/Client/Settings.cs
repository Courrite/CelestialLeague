using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using CelestialLeague.Client.Services;
using System;
using System.Threading.Tasks;

namespace CelestialLeague.Client
{
    public class CelestialLeagueSettings : EverestModuleSettings
    {
        [SettingName("Auto Connect")]
        [SettingSubText("Automatically connect to server on game start")]
        public bool AutoConnect { get; set; } = true;

        [SettingName("Auto Reconnect")]
        [SettingSubText("Automatically reconnect if connection is lost")]
        public bool AutoReconnect { get; set; } = true;

        [SettingName("Server Host")]
        [SettingSubText("Server hostname or IP address")]
        public string ServerHost { get; set; } = "127.0.0.1";

        [SettingName("Server Port")]
        [SettingSubText("Server port number")]
        public int ServerPort { get; set; } = 7777;

        [SettingName("Connection Timeout (seconds)")]
        [SettingSubText("How long to wait for connection")]
        [SettingRange(5, 60)]
        public int ConnectionTimeout { get; set; } = 10;

        [SettingName("Test Username")]
        [SettingSubText("Username for testing")]
        public string TestUsername { get; set; } = "testuser";

        [SettingName("Test Password")]
        [SettingSubText("Password for testing")]
        public string TestPassword { get; set; } = "testpass";
    }
}

