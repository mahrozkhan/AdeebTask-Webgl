#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AdeebTask.Views;
using System.IO;
using AdeebTask.Controllers;

namespace AdeebTask.EditorScripts
{
    public class SelectionFrameGenerator
    {
        [MenuItem("AdeebTask/Generate Selection Frame")]
        public static void GenerateFrame()
        {
            // 0. Ensure Sprites Exist
            Sprite borderSprite = GetOrCreateBorderSprite();
            Sprite scaleSprite = GetOrCreateCircleSprite("ScaleHandle", new Color(0.2f, 0.5f, 1f)); // Blue
            Sprite rotateSprite = GetOrCreateCircleSprite("RotateHandle", new Color(1f, 0.8f, 0.1f)); // Yellow

            // 1. Create Root
            GameObject root = new GameObject("SelectionFrame");

            // 2. Create Border
            GameObject border = new GameObject("Border");
            border.transform.SetParent(root.transform);
            border.transform.localPosition = Vector3.zero;
            
            var sr = border.AddComponent<SpriteRenderer>();
            sr.sprite = borderSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sortingOrder = 100;

            // 3. Create 4 Scale Handles
            CreateHandle(root, "Handle_TopLeft", HandleType.Scale, new Vector2(-0.5f, 0.5f), scaleSprite);
            CreateHandle(root, "Handle_TopRight", HandleType.Scale, new Vector2(0.5f, 0.5f), scaleSprite);
            CreateHandle(root, "Handle_BottomLeft", HandleType.Scale, new Vector2(-0.5f, -0.5f), scaleSprite);
            CreateHandle(root, "Handle_BottomRight", HandleType.Scale, new Vector2(0.5f, -0.5f), scaleSprite);

            // 4. Create Rotate Handle
            CreateHandle(root, "Handle_Rotate", HandleType.Rotate, new Vector2(0f, 0.8f), rotateSprite);

            // Auto-assign to SelectionController if it exists in the scene
            SelectionController controller = Object.FindObjectOfType<SelectionController>();
            if (controller != null)
            {
                controller.SetSelectionFrameData(root, border.GetComponent<SpriteRenderer>(),
                    root.transform.Find("Handle_TopLeft"), root.transform.Find("Handle_TopRight"),
                    root.transform.Find("Handle_BottomLeft"), root.transform.Find("Handle_BottomRight"),
                    root.transform.Find("Handle_Rotate"));
                EditorUtility.SetDirty(controller);
            }

            // Select the newly created root in the editor so the user can easily see it
            Selection.activeGameObject = root;

            Debug.Log("Selection Frame generated successfully! Link the 'SelectionFrame' to your 'SelectionController'.");
        }

        private static void CreateHandle(GameObject parent, string name, HandleType type, Vector2 localPos, Sprite sprite)
        {
            GameObject handle = new GameObject(name);
            handle.transform.SetParent(parent.transform);
            handle.transform.localPosition = localPos;
            handle.transform.localScale = new Vector3(0.3f, 0.3f, 1f); // Visually smaller
            
            var sr = handle.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 101; // Render on top of the border
            
            var col = handle.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;

            var selHandle = handle.AddComponent<SelectionHandle>();
            selHandle.HandleType = type;
        }

        private static Sprite GetOrCreateBorderSprite()
        {
            string dirPath = "Assets/_Project/Sprites";
            string filePath = dirPath + "/SelectionBorder.png";

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!File.Exists(filePath))
            {
                int size = 64;
                int borderThickness = 4;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                
                Color transparent = new Color(0, 0, 0, 0);
                Color black = Color.black;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (x < borderThickness || x >= size - borderThickness || 
                            y < borderThickness || y >= size - borderThickness)
                        {
                            tex.SetPixel(x, y, black);
                        }
                        else
                        {
                            tex.SetPixel(x, y, transparent);
                        }
                    }
                }
                tex.Apply();
                
                File.WriteAllBytes(filePath, tex.EncodeToPNG());
                AssetDatabase.Refresh();

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(filePath);
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100;
                    // Set 9-slice borders (left, bottom, right, top)
                    importer.spriteBorder = new Vector4(borderThickness, borderThickness, borderThickness, borderThickness);
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
        }

        private static Sprite GetOrCreateCircleSprite(string fileName, Color color)
        {
            string dirPath = "Assets/_Project/Sprites";
            string filePath = dirPath + "/" + fileName + ".png";

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!File.Exists(filePath))
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                Color transparent = new Color(0, 0, 0, 0);
                float center = size / 2f;
                float radius = size / 2f - 2f; // Slight padding

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                        if (dist < radius) tex.SetPixel(x, y, color);
                        else tex.SetPixel(x, y, transparent);
                    }
                }
                tex.Apply();

                File.WriteAllBytes(filePath, tex.EncodeToPNG());
                AssetDatabase.Refresh();

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(filePath);
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100;
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
        }
    }
}
#endif
