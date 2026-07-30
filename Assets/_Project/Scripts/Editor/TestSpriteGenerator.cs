using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace AdeebTask.Editor
{
    /// <summary>
    /// One-click generator for test placeholder sprites.
    /// Creates simple colored 128x128 PNG sprites and marks them as Addressables
    /// with keys matching the asset_catalogue.json addressableKey values.
    /// Menu: AdeebTask > Generate Test Addressable Sprites
    /// </summary>
    public static class TestSpriteGenerator
    {
        private struct SpriteEntry
        {
            public string addressableKey;
            public string fileName;
            public Color color;
        }

        [MenuItem("AdeebTask/Generate Test Addressable Sprites")]
        public static void Generate()
        {
            // These keys MUST match asset_catalogue.json and FirebaseService mock data
            var entries = new SpriteEntry[]
            {
                new SpriteEntry { addressableKey = "Chair_Wooden",  fileName = "Chair_Wooden",  color = new Color(0.55f, 0.35f, 0.17f) }, // Brown
                new SpriteEntry { addressableKey = "Sofa_Leather",  fileName = "Sofa_Leather",  color = new Color(0.40f, 0.15f, 0.10f) }, // Dark Red
                new SpriteEntry { addressableKey = "Table_Round",   fileName = "Table_Round",   color = new Color(0.75f, 0.60f, 0.35f) }, // Tan
                new SpriteEntry { addressableKey = "Lamp_Desk",     fileName = "Lamp_Desk",     color = new Color(0.95f, 0.85f, 0.30f) }, // Yellow
                new SpriteEntry { addressableKey = "Plant_Potted",  fileName = "Plant_Potted",  color = new Color(0.20f, 0.65f, 0.25f) }, // Green
            };

            string folderPath = "Assets/_Project/Art/TestSprites";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                // Create parent folders as needed
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Art"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Art");
                AssetDatabase.CreateFolder("Assets/_Project/Art", "TestSprites");
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[TestSpriteGenerator] Addressable settings not found! Initialize Addressables first.");
                return;
            }

            // Use the default group
            var group = settings.DefaultGroup;

            foreach (var entry in entries)
            {
                string assetPath = $"{folderPath}/{entry.fileName}.png";

                // Create a simple 128x128 colored texture
                var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                var pixels = new Color[128 * 128];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = entry.color;
                tex.SetPixels(pixels);
                tex.Apply();

                // Save as PNG
                byte[] pngData = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);

                System.IO.File.WriteAllBytes(assetPath, pngData);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                // Set texture import settings to Sprite
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.maxTextureSize = 128;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                // Mark as Addressable with the correct key
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                var addressableEntry = settings.CreateOrMoveEntry(guid, group, false, false);
                addressableEntry.address = entry.addressableKey;

                Debug.Log($"[TestSpriteGenerator] Created: {assetPath} -> Addressable Key: {entry.addressableKey}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TestSpriteGenerator] Done! {entries.Length} test sprites created and marked as Addressables in the Default Group.");
        }
    }
}
