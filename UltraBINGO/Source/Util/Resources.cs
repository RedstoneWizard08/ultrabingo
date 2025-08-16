using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace UltraBINGO.Util;

public static class Resources {
    [UsedImplicitly]
    public static Stream LoadEmbeddedResource(Assembly assembly, string path) =>
        assembly.GetManifestResourceStream(path) ??
        throw new Exception($"Failed to load embedded resource '{path}'.");

    [UsedImplicitly]
    public static byte[] GetEmbeddedResourceBytes(Assembly assembly, string path) {
        var stream = LoadEmbeddedResource(assembly, path);
        using MemoryStream ms = new();

        stream.CopyTo(ms);

        return ms.ToArray();
    }

    [UsedImplicitly]
    public static Sprite CreateSprite(Texture2D texture) =>
        Sprite.Create(
            texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

    [UsedImplicitly]
    public static Texture2D LoadEmbeddedTexture(Assembly assembly, string path) {
        var iconData = GetEmbeddedResourceBytes(assembly, path);
        var tex = new Texture2D(2, 2);

        tex.LoadImage(iconData);

        return tex;
    }

    [UsedImplicitly]
    public static Sprite LoadEmbeddedSprite(Assembly assembly, string path) =>
        CreateSprite(LoadEmbeddedTexture(assembly, path));
}