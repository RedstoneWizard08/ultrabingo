using System;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Path = System.IO.Path;

namespace Build.Tasks;

[TaskName("Launch")]
[IsDependentOn(typeof(BuildTask))]
public sealed class LaunchTask : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        var config = context.MsBuildConfiguration;
        var gamePath = context.GamePath;
        var profilePath = context.ProfilePath;
        var outPath = $"../{context.ProjectName}/bin/{config}/netstandard2.1/{context.ProjectName}.dll";
        var pluginPath = Path.Join(profilePath, $"BepInEx/plugins/{context.ModName}");
        var args = $"--doorstop-enable true --doorstop-target {profilePath}/BepInEx/core/BepInEx.Preloader.dll";

        if (!Path.Exists(Path.Join(outPath, ".."))) {
            context.CreateDirectory(Path.Join(outPath, ".."));
        }

        context.CreateDirectory(pluginPath);

        context.Log.Information("Copying files...");

        context.CopyFile(outPath, $"{pluginPath}/{context.ProjectName}.dll");

        context.Log.Information("Launching game...");

        if (OperatingSystem.IsWindows())
            context.StartAndReturnProcess(
                gamePath,
                new ProcessSettings {
                    Arguments = args,
                    RedirectStandardOutput = true,
                }
            ).WaitForExit();
        else context.Log.Information("Done! Now launch the game with your mod manager.");
    }
}