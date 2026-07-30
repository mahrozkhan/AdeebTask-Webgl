#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AdeebTask.Core;
using AdeebTask.UI;
using AdeebTask.UI.Screens;
using AdeebTask.Services.Assets;

namespace AdeebTask.EditorScripts
{
    public class SceneBuilder
    {
        [MenuItem("AdeebTask/Generate Main Scene Setup")]
        public static void BuildScene()
        {
            // 1. App Bootstrapper
            GameObject bootstrapperObj = new GameObject("[APP_BOOTSTRAPPER]");
            var bootstrapper = bootstrapperObj.AddComponent<AppBootstrapper>();
            var uiManager = bootstrapperObj.AddComponent<UIManager>();
            
            var soBoot = new SerializedObject(bootstrapper);
            soBoot.FindProperty("_uiManager").objectReferenceValue = uiManager;
            soBoot.ApplyModifiedProperties();

            // 1.5 Event System (CRITICAL FOR UI CLICKS)
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("[EVENT_SYSTEM]");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 2. UI Root
            GameObject uiRoot = new GameObject("[UI_ROOT]");
            var canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiRoot.AddComponent<CanvasScaler>();
            uiRoot.AddComponent<GraphicRaycaster>();

            var cardList = CreateScreen<CardListScreen>("CardListScreen", uiRoot.transform);
            var setupScreen = CreateScreen<ProjectSetupScreen>("ProjectSetupScreen", uiRoot.transform);
            var editorScreen = CreateScreen<EditorScreen>("EditorScreen", uiRoot.transform);
            var navScreen = CreateScreen<PageNavigationScreen>("PageNavigationScreen", uiRoot.transform);
            var popupScreen = CreateScreen<ConfirmationPopupScreen>("ConfirmationPopupScreen", uiRoot.transform);

            var soUI = new SerializedObject(uiManager);
            var screensProp = soUI.FindProperty("_preRegisteredScreens");
            screensProp.arraySize = 5;
            screensProp.GetArrayElementAtIndex(0).objectReferenceValue = cardList;
            screensProp.GetArrayElementAtIndex(1).objectReferenceValue = setupScreen;
            screensProp.GetArrayElementAtIndex(2).objectReferenceValue = editorScreen;
            screensProp.GetArrayElementAtIndex(3).objectReferenceValue = navScreen;
            screensProp.GetArrayElementAtIndex(4).objectReferenceValue = popupScreen;
            soUI.ApplyModifiedProperties();

            // 3. Controllers
            GameObject controllersObj = new GameObject("[CONTROLLERS]");
            
            // Use Reflection to completely bypass any MSBuild cache desync issues
            AddController(controllersObj, "MainMenuController");
            var editorController = AddController(controllersObj, "EditorController");
            AddController(controllersObj, "PageController");
            AddController(controllersObj, "PlaybackController");
            AddController(controllersObj, "ObjectPlacementController");
            var selectionController = AddController(controllersObj, "SelectionController");

            // Wire up controllers if found
            if (editorController != null && selectionController != null)
            {
                var soEditor = new SerializedObject(editorController);
                var selProp = soEditor.FindProperty("_selectionController");
                if (selProp != null)
                {
                    selProp.objectReferenceValue = selectionController;
                    soEditor.ApplyModifiedProperties();
                }
            }

            // 4. Object Pool
            GameObject poolObj = new GameObject("[OBJECT_POOL]");
            poolObj.AddComponent<PlacedObjectPool>();

            Debug.Log("<color=green><b>[AdeebTask] Scene hierarchy generated successfully!</b></color>");
        }

        private static Component AddController(GameObject obj, string className)
        {
            System.Type type = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                // Safely try to find it by full name first
                type = assembly.GetType($"AdeebTask.Controllers.{className}");
                if (type != null) break;
                
                // Fallback: search by short name if namespace/assembly shifted
                try
                {
                    foreach (var t in assembly.GetTypes())
                    {
                        if (t.Name == className)
                        {
                            type = t;
                            break;
                        }
                    }
                }
                catch { /* Ignore assembly load exceptions */ }
                
                if (type != null) break;
            }

            if (type != null)
            {
                return obj.AddComponent(type);
            }
            else
            {
                Debug.LogError($"[SceneBuilder] Failed to find {className} in ANY loaded assembly. This means the script has a compilation error in Unity, preventing it from loading.");
                return null;
            }
        }

        private static T CreateScreen<T>(string name, Transform parent) where T : AppScreen
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            
            // Sub-canvas for performance (prevents full UI redraws)
            var canvas = obj.AddComponent<Canvas>();
            canvas.overrideSorting = true; // Crucial for nested canvases to handle their own rendering
            
            obj.AddComponent<GraphicRaycaster>();
            
            return obj.AddComponent<T>();
        }
    }
}
#endif
