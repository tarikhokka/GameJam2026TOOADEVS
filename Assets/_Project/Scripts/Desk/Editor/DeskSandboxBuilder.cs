using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CivilJam.Desk.Editor
{
    // DeskScreen prefab'ını ve Sandbox_Desk sahnesini uGUI ile sıfırdan kurar.
    // Üretilen prefab bir başlangıç iskeleti; sonrasında Editor'de elle düzenlenebilir.
    public static class DeskSandboxBuilder
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Desk";
        private const string PrefabPath = PrefabFolder + "/DeskScreen.prefab";
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/Sandbox_Desk.unity";

        private static readonly Color DeskColor = new Color(0.36f, 0.27f, 0.21f);
        private static readonly Color HudColor = new Color(0.12f, 0.11f, 0.1f, 0.92f);
        private static readonly Color PanelColor = new Color(0.16f, 0.14f, 0.12f, 0.85f);
        private static readonly Color PaperColor = new Color(0.95f, 0.92f, 0.85f);
        private static readonly Color FolderColor = new Color(0.86f, 0.78f, 0.6f);
        private static readonly Color EnvelopeColor = new Color(0.9f, 0.82f, 0.55f);
        private static readonly Color Ink = new Color(0.17f, 0.17f, 0.17f);
        private static readonly Color LightText = new Color(0.93f, 0.9f, 0.84f);
        private static readonly Color StampRed = new Color(0.7f, 0.15f, 0.15f);
        private static readonly Color ApproveGreen = new Color(0.22f, 0.5f, 0.27f);
        private static readonly Color RejectRed = new Color(0.66f, 0.2f, 0.18f);
        private static readonly Color NeutralButton = new Color(0.33f, 0.36f, 0.42f);
        private static readonly Color BribeButton = new Color(0.55f, 0.42f, 0.12f);

        private static TMP_FontAsset font;

        [MenuItem("CivilJam/Desk/Build DeskScreen Prefab")]
        public static void BuildPrefabMenu()
        {
            if (!ConfirmOverwrite()) return;
            if (BuildPrefab() != null)
                Debug.Log($"[Desk] Prefab kuruldu: {PrefabPath}");
        }

        // Prefab'ı ve sandbox sahnesini birlikte yeniden kurar (prefab yapısı değişince sahnedeki örnek de tazelenir).
        [MenuItem("CivilJam/Desk/Build Sandbox_Desk Scene")]
        public static void BuildSandboxScene()
        {
            if (!ConfirmOverwrite()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var prefab = BuildPrefab();
            if (prefab == null) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cameraGo.transform.position = new Vector3(0, 0, -10);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            var screenGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasGo.transform);
            Stretch((RectTransform)screenGo.transform);

            var bootstrap = new GameObject("DeskSandboxBootstrap").AddComponent<DeskSandboxBootstrap>();
            Wire(bootstrap, "screen", screenGo.GetComponent<DeskScreen>());

            EnsureFolder(SceneFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Desk] Sandbox sahnesi kuruldu: {ScenePath}. Play'e bas.");
        }

        private static bool ConfirmOverwrite()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null ||
                   EditorUtility.DisplayDialog("DeskScreen prefab'ı yeniden kurulacak",
                       "Prefab'da elle yaptığın değişiklikler kaybolur. Devam edilsin mi?",
                       "Yeniden kur", "Vazgeç");
        }

        private static GameObject BuildPrefab()
        {
            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
            {
                EditorUtility.DisplayDialog("TextMeshPro eksik",
                    "Önce Window > TextMeshPro > Import TMP Essential Resources ile TMP kaynaklarını içe aktar, sonra tekrar dene.",
                    "Tamam");
                return null;
            }
            font = TMP_Settings.defaultFontAsset;

            var root = NewRect("DeskScreen", null);
            Stretch(root);
            var screen = root.gameObject.AddComponent<DeskScreen>();

            AddImage(Stretch(NewRect("Background", root)), DeskColor);

            Wire(screen, "hud", BuildHud(root));
            Wire(screen, "queue", BuildQueue(root));
            Wire(screen, "documentView", BuildDocument(root));
            Wire(screen, "actionPanel", BuildActions(root));

            EnsureFolder(PrefabFolder);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);
            Object.DestroyImmediate(root.gameObject);
            return prefab;
        }

        // ---------- Bölümler ----------

        private static DeskHud BuildHud(Transform parent)
        {
            var hud = Anchor(NewRect("Hud", parent), new Vector2(0, 1), Vector2.one, new Vector2(0, -80), Vector2.zero);
            AddImage(hud, HudColor);
            var layout = HLayout(hud, 20, new RectOffset(30, 30, 0, 0));
            layout.childForceExpandHeight = true;

            var title = NewText("LevelTitle", hud, "Ay", 36, LightText, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            Layout(title, flexW: 1);
            var money = NewText("Money", hud, "Kasa", 30, LightText, TextAlignmentOptions.MidlineRight);
            Layout(money, prefW: 420);
            var quota = NewText("Quota", hud, "Onay", 30, LightText, TextAlignmentOptions.MidlineRight);
            Layout(quota, prefW: 260);

            var view = hud.gameObject.AddComponent<DeskHud>();
            Wire(view, "levelTitleText", title);
            Wire(view, "moneyText", money);
            Wire(view, "quotaText", quota);
            return view;
        }

        private static ProjectQueueView BuildQueue(Transform parent)
        {
            var panel = Anchor(NewRect("Queue", parent), Vector2.zero, new Vector2(0, 1), new Vector2(20, 20), new Vector2(400, -100));
            AddImage(panel, PanelColor);
            VLayout(panel, 12, new RectOffset(16, 16, 16, 16));

            Layout(NewText("Header", panel, "Gelen Dosyalar", 30, LightText, TextAlignmentOptions.MidlineLeft, FontStyles.Bold), prefH: 50);

            var content = NewRect("Content", panel);
            VLayout(content, 10, new RectOffset(0, 0, 0, 0));
            Layout(content, flexH: 1);

            var item = NewRect("ItemTemplate", content);
            var bg = AddImage(item, FolderColor);
            var button = item.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            VLayout(item, 4, new RectOffset(16, 16, 12, 12));
            Layout(item, prefH: 110);

            var name = NewText("Name", item, "Proje", 28, Ink, TextAlignmentOptions.TopLeft);
            var status = NewText("Status", item, "Bekliyor", 22, Ink, TextAlignmentOptions.TopLeft, FontStyles.Bold);

            var itemView = item.gameObject.AddComponent<ProjectQueueItem>();
            Wire(itemView, "button", button);
            Wire(itemView, "background", bg);
            Wire(itemView, "nameText", name);
            Wire(itemView, "statusText", status);
            item.gameObject.SetActive(false);

            var view = panel.gameObject.AddComponent<ProjectQueueView>();
            Wire(view, "content", content);
            Wire(view, "itemTemplate", itemView);
            return view;
        }

        private static DocumentView BuildDocument(Transform parent)
        {
            var area = Anchor(NewRect("Document", parent), Vector2.zero, Vector2.one, new Vector2(420, 20), new Vector2(-500, -100));

            // Sekmeler
            var tabs = Anchor(NewRect("Tabs", area), new Vector2(0, 1), Vector2.one, new Vector2(0, -60), Vector2.zero);
            var tabsLayout = HLayout(tabs, 8, new RectOffset(0, 0, 0, 0));
            tabsLayout.childForceExpandHeight = true;
            var tab = NewButton("TabTemplate", tabs, "Belge", FolderColor, Ink, 24);
            Layout(tab, prefW: 380);
            tab.gameObject.SetActive(false);

            // Kağıt
            var paper = Anchor(NewRect("Paper", area), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -64));
            AddImage(paper, PaperColor);
            VLayout(paper, 16, new RectOffset(50, 50, 40, 40));

            var title = NewText("Title", paper, "Belge başlığı", 42, Ink, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            Layout(title, prefH: 60);
            var meta = NewText("Meta", paper, "Künye", 28, Ink, TextAlignmentOptions.TopLeft);

            var divider = NewRect("Divider", paper);
            AddImage(divider, new Color(Ink.r, Ink.g, Ink.b, 0.4f));
            Layout(divider, prefH: 3);

            var header = BuildParamRow("ParamHeader", paper, true);
            var list = NewRect("ParamList", paper);
            VLayout(list, 6, new RectOffset(0, 0, 0, 0));
            var rowTemplate = BuildParamRow("ParamRowTemplate", list, false);
            var rowView = rowTemplate.AddComponent<ParamRowView>();
            var cells = rowTemplate.GetComponentsInChildren<TMP_Text>();
            Wire(rowView, "nameText", cells[0]);
            Wire(rowView, "requiredText", cells[1]);
            Wire(rowView, "reportedText", cells[2]);
            Wire(rowView, "actualText", cells[3]);
            rowTemplate.SetActive(false);

            // Kaşe (layout dışı, sağ üst köşe, hafif yamuk)
            var stamp = NewRect("Stamp", paper);
            stamp.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            stamp.anchorMin = stamp.anchorMax = stamp.pivot = Vector2.one;
            stamp.sizeDelta = new Vector2(300, 110);
            stamp.anchoredPosition = new Vector2(-50, -40);
            stamp.localRotation = Quaternion.Euler(0, 0, 10);
            AddImage(stamp, new Color(StampRed.r, StampRed.g, StampRed.b, 0.12f));
            var stampLabel = NewText("Label", stamp, "ONAYLIDIR\n<size=60%>Kaşe</size>", 34, StampRed,
                TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(stampLabel.rectTransform);

            var empty = NewText("EmptyState", area, "Soldan bir dosya seç.", 36, LightText, TextAlignmentOptions.Center);
            Stretch(empty.rectTransform);

            var view = area.gameObject.AddComponent<DocumentView>();
            Wire(view, "tabsContainer", tabs);
            Wire(view, "tabTemplate", tab);
            Wire(view, "paperRoot", paper.gameObject);
            Wire(view, "emptyState", empty.gameObject);
            Wire(view, "titleText", title);
            Wire(view, "metaText", meta);
            Wire(view, "stampRoot", stamp.gameObject);
            Wire(view, "stampText", stampLabel);
            Wire(view, "paramHeader", header);
            Wire(view, "paramContainer", list);
            Wire(view, "paramRowTemplate", rowView);
            return view;
        }

        private static GameObject BuildParamRow(string name, Transform parent, bool isHeader)
        {
            var row = NewRect(name, parent);
            var layout = HLayout(row, 10, new RectOffset(0, 0, 0, 0));
            layout.childForceExpandHeight = true;
            Layout(row, prefH: 44);

            var style = isHeader ? FontStyles.Bold : FontStyles.Normal;
            string[] labels = isHeader
                ? new[] { "Parametre", "Gerekli", "Beyan", "Ek test" }
                : new[] { "Parametre", "min. 0", "0", "?" };
            float[] widths = { 2, 1, 1, 1 };

            for (int i = 0; i < labels.Length; i++)
            {
                var cell = NewText("Cell" + i, row, labels[i], 26, Ink,
                    i == 0 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Midline, style);
                Layout(cell, prefW: 0, flexW: widths[i]);
            }
            return row.gameObject;
        }

        private static ActionPanel BuildActions(Transform parent)
        {
            var panel = Anchor(NewRect("Actions", parent), new Vector2(1, 0), Vector2.one, new Vector2(-480, 20), new Vector2(-20, -100));
            AddImage(panel, PanelColor);
            VLayout(panel, 14, new RectOffset(24, 24, 24, 24));

            var clientName = NewText("ClientName", panel, "Başvuru sahibi", 34, LightText, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            var clientLine = NewText("ClientLine", panel, "Söz", 26, LightText, TextAlignmentOptions.TopLeft, FontStyles.Italic);

            var bribe = NewRect("Bribe", panel);
            AddImage(bribe, EnvelopeColor);
            VLayout(bribe, 10, new RectOffset(16, 16, 16, 16));
            var bribeText = NewText("BribeText", bribe, "Zarf", 26, Ink, TextAlignmentOptions.TopLeft);
            var takeBribe = NewButton("TakeBribeButton", bribe, "Zarfı al ve imzala", BribeButton, Color.white, 26);
            Layout(takeBribe, prefH: 60);

            Layout(NewRect("Spacer", panel), flexH: 1);

            var test = NewButton("ExtraTestButton", panel, "Ek test iste", NeutralButton, Color.white, 28);
            Layout(test, prefH: 70);
            var approve = NewButton("ApproveButton", panel, "ONAYLA", ApproveGreen, Color.white, 34);
            Layout(approve, prefH: 80);
            var reject = NewButton("RejectButton", panel, "REDDET", RejectRed, Color.white, 34);
            Layout(reject, prefH: 80);

            var verdict = NewText("Verdict", panel, "", 40, LightText, TextAlignmentOptions.Center, FontStyles.Bold);
            Layout(verdict, prefH: 60);
            var finish = NewButton("FinishButton", panel, "Mesaiyi bitir", NeutralButton, Color.white, 30);
            Layout(finish, prefH: 80);

            var view = panel.gameObject.AddComponent<ActionPanel>();
            Wire(view, "clientNameText", clientName);
            Wire(view, "clientLineText", clientLine);
            Wire(view, "bribeRoot", bribe.gameObject);
            Wire(view, "bribeText", bribeText);
            Wire(view, "takeBribeButton", takeBribe);
            Wire(view, "extraTestButton", test);
            Wire(view, "extraTestLabel", test.GetComponentInChildren<TMP_Text>());
            Wire(view, "approveButton", approve);
            Wire(view, "rejectButton", reject);
            Wire(view, "verdictText", verdict);
            Wire(view, "finishButton", finish);
            return view;
        }

        // ---------- Yardımcılar ----------

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static RectTransform Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        private static RectTransform Stretch(RectTransform rt) => Anchor(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        private static Image AddImage(Component target, Color color)
        {
            var image = target.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            var tmp = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button NewButton(string name, Transform parent, string label, Color background, Color textColor, float size)
        {
            var rt = NewRect(name, parent);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = AddImage(rt, background);
            Stretch(NewText("Label", rt, label, size, textColor, TextAlignmentOptions.Center, FontStyles.Bold).rectTransform);
            return button;
        }

        private static VerticalLayoutGroup VLayout(Component target, float spacing, RectOffset padding)
        {
            var layout = target.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private static HorizontalLayoutGroup HLayout(Component target, float spacing, RectOffset padding)
        {
            var layout = target.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private static void Layout(Component target, float prefW = -1, float prefH = -1, float flexW = -1, float flexH = -1)
        {
            // Unity'nin "fake null"u yüzünden ?? kullanılmaz.
            var element = target.GetComponent<LayoutElement>();
            if (element == null) element = target.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = prefW;
            element.preferredHeight = prefH;
            element.flexibleWidth = flexW;
            element.flexibleHeight = flexH;
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[Desk] {target.GetType().Name}.{field} alanı bulunamadı.");
                return;
            }
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
