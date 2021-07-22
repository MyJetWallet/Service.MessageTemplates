using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.MessageTemplates.Settings
{
    public class SettingsModel
    {
        [YamlProperty("MessageTemplates.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("MessageTemplates.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("MessageTemplates.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
