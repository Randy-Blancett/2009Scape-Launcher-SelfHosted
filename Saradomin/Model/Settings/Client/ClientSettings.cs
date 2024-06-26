using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings.Client
{
    public class ClientSettings : SettingsBase
    {
        public const string FileName = "config.json";

        public const string DarkOwlServerAddress = "runescape.home.darkowl.org";
        public const string LiveServerAddress = "play.2009scape.org";
        public const string TestServerAddress = "test.2009scape.org";
        public const string LocalServerAddress = "localhost";

        public enum ServerProfile
        {
            [Description("DarkOwl Server;runescape.home.darkowl.org")]
            DarkOwlServer,

            [Description("Stable server;play.2009scape.org")]
            Live,
            
            [Description("Testing server;test.2009scape.org")]
            Testing,
            
            [Description("Local server;localhost")]
            Local,
            
            [Description("Unsupported server")]
            Unsupported
        }

        [JsonPropertyName("ip_management")]
        public string ManagementServerAddress { get; set; } = LiveServerAddress;

        [JsonPropertyName("ip_address")]
        public string GameServerAddress { get; set; } = LiveServerAddress;
        
        [JsonPropertyName("world")]
        public ushort World { get; set; } = 1;
        
        [JsonPropertyName("server_port")]
        public ushort GameServerPort { get; set; } = 43594;
        
        [JsonPropertyName("wl_port")]
        public ushort WorldListServerPort { get; set; } = 43595;

        [JsonPropertyName("js5_port")]
        public ushort CacheServerPort { get; set; } = 43595;

        [JsonPropertyName("ui_scale")]
        public int UiScale { get; set; } = 1;

        [JsonPropertyName("fps")]
        public int Fps { get; set; } = 0;
    }
}
