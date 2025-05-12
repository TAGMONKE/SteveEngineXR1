using System;
using System.Collections.Generic;
using System.IO;

namespace SteveEngine
{
    public class AssetBundle
    {
        private Dictionary<string, object> assets = new Dictionary<string, object>();

        public AssetBundle(string bundlePath)
        {
            LoadBundle(bundlePath);
        }

        private void LoadBundle(string bundlePath)
        {
            if (!File.Exists(bundlePath))
            {
                throw new FileNotFoundException($"Asset bundle not found: {bundlePath}");
            }

            // Example: Deserialize the bundle (adjust based on the format used by the external project)
            var bundleData = File.ReadAllBytes(bundlePath);
            DeserializeBundle(bundleData);
        }

        private void DeserializeBundle(byte[] bundleData)
        {
            // Example: Deserialize assets (adjust based on the bundle format)
            // This could involve JSON, binary deserialization, or a custom format
            // For simplicity, assume JSON format here
            var bundleJson = System.Text.Encoding.UTF8.GetString(bundleData);
            var deserializedAssets = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(bundleJson);

            foreach (var asset in deserializedAssets)
            {
                assets[asset.Key] = asset.Value;
            }
        }

        public T GetAsset<T>(string assetName) where T : class
        {
            if (assets.TryGetValue(assetName, out var asset))
            {
                return asset as T;
            }

            throw new KeyNotFoundException($"Asset not found in bundle: {assetName}");
        }
    }
}
