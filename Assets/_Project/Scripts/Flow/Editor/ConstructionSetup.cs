using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow.Editor
{
    /// <summary>Elle hazırlanan inşaat panelini tamamlar; diğer ekranları yeniden kurmaz.</summary>
    public static class ConstructionSetup
    {
        [MenuItem("CivilJam/Flow/4 - Setup Construction Screen")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/_Project/Scenes/Sandbox_Flow.unity")
            {
                Debug.LogError("[ConstructionSetup] Önce Sandbox_Flow sahnesini aç.");
                return;
            }
            var canvas = Object.FindAnyObjectByType<Canvas>();
            var boot = Object.FindAnyObjectByType<FlowSandboxBootstrap>();
            var panel = canvas != null ? canvas.transform.Find("ConstructionScreen") : null;
            if (panel == null || boot == null)
            {
                Debug.LogError("[ConstructionSetup] Canvas/ConstructionScreen ve FlowSandbox gerekli.");
                return;
            }

            var title = Text(panel, "ConstructionTitle", "GÜNÜN İNŞAATLARI", 48, 200, new Vector2(1000, 120));
            var info = Text(panel, "BuildingInfo", "Bugün onaylanan bina: 2\n\nOnayladığın yapıların inşaatı tamamlandı.",
                36, 20, new Vector2(1000, 180));
            var buttonRect = Rect(panel, "ContinueButton", -180, new Vector2(480, 90));
            var image = buttonRect.GetComponent<Image>() ?? buttonRect.gameObject.AddComponent<Image>();
            image.color = new Color(0.93f, 0.88f, 0.76f);
            var button = buttonRect.GetComponent<Button>() ?? buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var label = Text(buttonRect, "Text (TMP)", "Gün sonuna geç", 32, 0, new Vector2(460, 80));
            label.color = new Color(0.10f, 0.08f, 0.06f);

            var screen = panel.GetComponent<ConstructionScreen>() ?? panel.gameObject.AddComponent<ConstructionScreen>();
            var serialized = new SerializedObject(screen);
            serialized.FindProperty("constructionTitle").objectReferenceValue = title;
            serialized.FindProperty("buildingInfo").objectReferenceValue = info;
            serialized.FindProperty("continueButton").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var bootstrap = new SerializedObject(boot);
            bootstrap.FindProperty("construction").objectReferenceValue = screen;
            bootstrap.ApplyModifiedPropertiesWithoutUndo();

            const string path = "Assets/_Project/Prefabs/Flow/ConstructionScreen.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(panel.gameObject, path, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = panel.gameObject;
            Debug.Log("[ConstructionSetup] Panel, prefab ve sandbox bağlantısı kaydedildi.");
        }

        static RectTransform Rect(Transform parent, string name, float y, Vector2 size)
        {
            var child = parent.Find(name);
            var rt = child != null ? child.GetComponent<RectTransform>()
                : new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, y);
            rt.sizeDelta = size;
            return rt;
        }

        static TextMeshProUGUI Text(Transform parent, string name, string value, float size, float y, Vector2 dimensions)
        {
            var rt = Rect(parent, name, y, dimensions);
            var text = rt.GetComponent<TextMeshProUGUI>() ?? rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (text.font == null) text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }
    }
}
