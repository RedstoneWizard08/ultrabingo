using Cake.Common;
using Cake.Core;
using Cake.Frosting;

namespace Build;

// ReSharper disable once ClassNeverInstantiated.Global
public class BuildContext(ICakeContext context) : FrostingContext(context) {
    public string GamePath { get; set; } = context.Argument("game-path",
        "/home/redstone/.steam/steam/steamapps/common/ULTRAKILL/ULTRAKILL.exe");

    public string ProfilePath { get; set; } =
        context.Argument("profile-path", "/home/redstone/.local/share/com.kesomannen.gale/ultrakill/profiles/Dev");

    public string MsBuildConfiguration { get; set; } = context.Argument("configuration", "Release");

    public string ProjectName { get; set; } = context.Argument("project-name", "UltraBINGO");

    public string ModName { get; set; } = context.Argument("mod-name", "Clearwater-BaphometsBingo");

    public int AppId { get; set; } = context.Argument("app-id", 1229490);
}