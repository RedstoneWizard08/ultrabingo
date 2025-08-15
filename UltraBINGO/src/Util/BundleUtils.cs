using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using UnityEngine;

namespace UltraBINGO.Util;

/// <summary>
///     Utility methods for working with asset bundles.
/// </summary>
public static class BundleUtils {
    private static MemoryStream CopyToMemoryStream(Stream stream) {
        using var reader = new StreamReader(stream);
        var ms = new MemoryStream();

        reader.BaseStream.CopyTo(ms);

        return ms;
    }

    /// <summary>
    ///     Loads an asset bundle from a file path.
    /// </summary>
    /// <param name="path">The path to load the asset bundle from.</param>
    /// <param name="gzipped">Whether the stream is gzipped.</param>
    /// <returns>The loaded asset bundle.</returns>
    public static AssetBundle? LoadAssetBundle(string path, bool gzipped = false) {
        return LoadAssetBundle(File.OpenRead(path), gzipped);
    }

    /// <summary>
    ///     Loads an asset bundle from an embedded resource.
    /// </summary>
    /// <param name="assembly">The assembly to load the asset bundle from.</param>
    /// <param name="path">The path to load the asset bundle from.</param>
    /// <param name="gzipped">Whether the stream is gzipped.</param>
    /// <returns>The loaded asset bundle.</returns>
    public static AssetBundle? LoadEmbeddedAssetBundle(Assembly assembly, string path, bool gzipped = false) {
        var stream = assembly.GetManifestResourceStream(path) ??
                     throw new Exception($"Failed to load embedded resource '{path}'.");

        return LoadAssetBundle(stream, gzipped);
    }

    /// <summary>
    ///     Loads an asset bundle from a stream.
    /// </summary>
    /// <param name="stream">The stream to load the asset bundle from.</param>
    /// <param name="gzipped">Whether the stream is gzipped.</param>
    /// <returns>The loaded asset bundle.</returns>
    public static AssetBundle? LoadAssetBundle(Stream stream, bool gzipped = false) {
        if (!gzipped) return AssetBundle.LoadFromStream(CopyToMemoryStream(stream));

        using var gzStream = new GZipStream(stream, CompressionMode.Decompress);
        using var ms = CopyToMemoryStream(gzStream);

        return AssetBundle.LoadFromStream(ms);
    }
}
