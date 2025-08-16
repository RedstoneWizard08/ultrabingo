using BepInEx.Configuration;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using Resources = UltraBINGO.Util.Resources;

namespace UltraBINGO;

public class ModConfig {
    private const string DefaultHost = "clearwaterbirb.uk";
    private const int DefaultPort = 2052;
    private const string LogoPath = "SquareLogo.png";

    public readonly ConfigEntry<string> ServerHost;
    public readonly ConfigEntry<int> ServerPort;
    public readonly ConfigEntry<string> LastRankUsed;

    private readonly PluginConfigurator _config;

    public ModConfig(ConfigFile config) {
        _config = PluginConfigurator.Create(Main.PluginName, Main.PluginId);

        _config.image = Resources.LoadEmbeddedSprite(
            typeof(Main).Assembly,
            typeof(Main).Namespace + "." + LogoPath
        );

        _ = new ConfigHeader(_config.rootPanel, "General");

        ServerHost = CreateWrappedField(
            config.Bind(
                "General",
                nameof(ServerHost),
                DefaultHost,
                "Server URL"
            )
        );

        ServerPort = CreateWrappedField(
            config.Bind(
                "General",
                nameof(ServerPort),
                DefaultPort,
                "Server Port"
            )
        );

        LastRankUsed = CreateWrappedField(
            config.Bind(
                "General",
                nameof(LastRankUsed),
                "None",
                "Last Rank Used (Only works if your SteamID has access to this rank)"
            )
        );
    }

    private ConfigEntry<string> CreateWrappedField(ConfigEntry<string> actual) {
        var field = new StringField(
            _config.rootPanel,
            actual.Description.Description,
            actual.Definition.ToString(),
            actual.Value
        );

        actual.SettingChanged += (_, _) => {
            if (field.value != actual.Value) field.value = actual.Value;
        };

        field.onValueChange += ev => {
            if (actual.Value != ev.value) actual.Value = ev.value;
        };

        return actual;
    }

    private ConfigEntry<int> CreateWrappedField(ConfigEntry<int> actual) {
        var field = new IntField(
            _config.rootPanel,
            actual.Description.Description,
            actual.Definition.ToString(),
            actual.Value
        );

        actual.SettingChanged += (_, _) => {
            if (field.value != actual.Value) field.value = actual.Value;
        };

        field.onValueChange += ev => {
            if (actual.Value != ev.value) actual.Value = ev.value;
        };

        return actual;
    }
}