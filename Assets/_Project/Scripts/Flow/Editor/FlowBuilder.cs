using System.Collections.Generic;
using CivilJam.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow.Editor
{
    /// <summary>
    /// Flow ekran prefab'larını, MainMenu sahnesini ve Sandbox_Flow'u kurar (YAML'ı elle yazmamak için).
    /// Ekran UI'ı çalışma anında kodla kurulur; prefab sadece bileşeni + sprite referanslarını taşır.
    /// Game.unity'ye ekleme A'nın işi: CivilJam > Core > Install Screens Into Game Scene.
    /// </summary>
    public static class FlowBuilder
    {
        const string Art = "Assets/_Project/Art/";
        const string PrefabDir = "Assets/_Project/Prefabs/Flow";
        const string MainMenuScene = "Assets/_Project/Scenes/MainMenu.unity";
        const string SandboxScene = "Assets/_Project/Scenes/Sandbox_Flow.unity";

        [MenuItem("CivilJam/Flow/1 - Build Flow Prefabs")]
        public static void BuildPrefabs()
        {
            EnsureFolder(PrefabDir);
            BuildScreenPrefab<ResultScreen>("ResultScreen", ("background", "newspaper_bg.png"));
            BuildScreenPrefab<DaySummaryScreen>("DaySummaryScreen", ("background", "daysummary_bg.png"));
            BuildScreenPrefab<DailyExpensesScreen>("DailyExpensesScreen", ("background", "payment_bg.png"));
            BuildScreenPrefab<EndingScreen>("EndingScreen",
                ("goodImage", "ending_good.png"), ("badImage", "ending_bad.png"), ("closedImage", "ending_closed.png"));
            AssetDatabase.SaveAssets();
            Debug.Log("[FlowBuilder] Prefab'lar hazır: " + PrefabDir);
        }

        [MenuItem("CivilJam/Flow/2 - Build MainMenu Scene")]
        public static void BuildMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = new GameObject("Main Camera", typeof(Camera));
            cam.tag = "MainCamera";
            var camera = cam.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            var canvas = CreateCanvas();
            var menu = new GameObject("MainMenu", typeof(RectTransform), typeof(MainMenuScreen));
            menu.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)menu.transform);
            AssignSprites(menu.GetComponent<MainMenuScreen>(), ("background", "menu_bg.png"), ("logo", "logo.png"));
            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, MainMenuScene);
            AddToBuildSettings(MainMenuScene, first: true);
            Debug.Log("[FlowBuilder] MainMenu sahnesi kaydedildi ve Build Settings'te 1. sıraya eklendi.");
        }

        [MenuItem("CivilJam/Flow/3 - Build Sandbox_Flow Scene")]
        public static void BuildSandbox()
        {
            BuildPrefabs();
            var scene = EditorSceneManager.OpenScene(SandboxScene, OpenSceneMode.Single);

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) canvas = CreateCanvas();
            EnsureEventSystem();

            // Eski boş ResultScreen paneli ve önceki kurulumlar temizlenir
            foreach (Transform child in new List<Transform>(GetChildren(canvas.transform)))
                Object.DestroyImmediate(child.gameObject);
            foreach (var old in Object.FindObjectsByType<FlowSandboxBootstrap>(FindObjectsInactive.Include))
                Object.DestroyImmediate(old.gameObject);

            var result = Place<ResultScreen>(canvas, "ResultScreen");
            var daySummary = Place<DaySummaryScreen>(canvas, "DaySummaryScreen");
            var expenses = Place<DailyExpensesScreen>(canvas, "DailyExpensesScreen");
            var ending = Place<EndingScreen>(canvas, "EndingScreen");

            var boot = new GameObject("FlowSandbox", typeof(FlowSandboxBootstrap));
            var so = new SerializedObject(boot.GetComponent<FlowSandboxBootstrap>());
            so.FindProperty("result").objectReferenceValue = result;
            so.FindProperty("daySummary").objectReferenceValue = daySummary;
            so.FindProperty("dailyExpenses").objectReferenceValue = expenses;
            so.FindProperty("ending").objectReferenceValue = ending;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FlowBuilder] Sandbox_Flow kuruldu. Play'e bas; sol üstteki düğmelerle ekranları dene.");
        }

        // ---------- yardımcılar ----------

        static T Place<T>(Canvas canvas, string prefabName) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{prefabName}.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            return instance.GetComponent<T>();
        }

        static IEnumerable<Transform> GetChildren(Transform t)
        {
            foreach (Transform c in t) yield return c;
        }

        static void BuildScreenPrefab<T>(string name, params (string field, string file)[] sprites) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(T));
            Stretch((RectTransform)go.transform);
            AssignSprites(go.GetComponent<T>(), sprites);
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
            Object.DestroyImmediate(go);
        }

        static void AssignSprites(Object target, params (string field, string file)[] sprites)
        {
            var so = new SerializedObject(target);
            foreach (var (field, file) in sprites)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + file);
                if (sprite == null)
                {
                    Debug.LogWarning($"[FlowBuilder] Sprite bulunamadı: {Art + file}");
                    continue;
                }
                so.FindProperty(field).objectReferenceValue = sprite;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        static void AddToBuildSettings(string scenePath, bool first)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == scenePath);
            var entry = new EditorBuildSettingsScene(scenePath, true);
            if (first) list.Insert(0, entry); else list.Add(entry);
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
