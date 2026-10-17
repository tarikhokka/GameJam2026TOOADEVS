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
        private const string AudioFolder = "Assets/_Project/Audio";
        private const string ArtFolder = "Assets/_Project/Art";
        private const string RuleSetPath = "Assets/_Project/Data/RuleSet.asset";

        // Oda yerleşimi (desk_bg 1920x1080 üstünde px, EEE görseli): koltuk desk_bg'nin içinde (x 650-1310, üstü ~355),
        // masanın üst kenarı ~750 (desk_front bu kenardan aşağısıdır). Portre 600x600, karakter alta ortalı ve belden
        // yukarı: üstü 260'ta olunca kafa koltuğun üst kenarına denk gelir, masa göğüs/bel arasını keser.
        private const float DeskTopY = 750f;
        private const float PortraitSize = 600f;
        private const float PortraitTopY = 260f;
        private const float HeadTopY = PortraitTopY + 40f; // portrede saçın üstü ~40 px
        private const float BalloonShiftX = 0f; // balon başın tam üstünde
        private static readonly Color BalloonColor = new Color(1f, 0.99f, 0.96f);

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
            cameraGo.AddComponent<AudioListener>();

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
            WirePortraits(bootstrap);
            Wire(bootstrap, "rules", AssetDatabase.LoadMainAssetAtPath(RuleSetPath)); // el kitabı oyundakiyle aynı olsun

            EnsureFolder(SceneFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Desk] Sandbox sahnesi kuruldu: {ScenePath}. Play'e bas.");
        }

        // Sandbox'taki sahte müşterilere EEE'nin portrelerini verir (Art/portrait_*.png).
        private static void WirePortraits(DeskSandboxBootstrap bootstrap)
        {
            var so = new SerializedObject(bootstrap);
            var list = so.FindProperty("portraits");
            var guids = AssetDatabase.FindAssets("portrait_ t:Sprite", new[] { ArtFolder });
            list.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[i]));
            so.ApplyModifiedPropertiesWithoutUndo();
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

            AddImage(Stretch(NewRect("Background", root)), DeskColor); // 16:9 dışı ekranlarda odanın kenarları

            // Çizim sırası: sahne (oda, müteahhit, masa, masadaki eşyalar) → üst bar → açık dosya → pencereler.
            Wire(screen, "scene", BuildScene(root, out var room, out var handoff));
            Wire(screen, "handoff", handoff);
            Wire(screen, "hud", BuildHud(root, out var clientTag));
            Wire(screen, "props", BuildProps(room, clientTag, out var handbookBook));
            var dossier = BuildDossier(root, out var documentView, out var actionPanel, out var dossierHandbook);
            Wire(screen, "dossier", dossier);
            Wire(screen, "documentView", documentView);
            Wire(screen, "actionPanel", actionPanel);
            var rulebook = BuildRulebook(root, handbookBook, dossierHandbook); // açık dosyanın da üstünde
            Wire(screen, "rulebook", rulebook);
            LinkSheets(dossier.transform, rulebook.transform);
            Wire(screen, "tutorial", BuildTutorial(root));
            Wire(screen, "eventCard", BuildEventCard(root));
            Wire(screen, "deskAudio", BuildAudio(root));

            EnsureFolder(PrefabFolder);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);
            Object.DestroyImmediate(root.gameObject);
            return prefab;
        }

        // ---------- Bölümler ----------

        // Sesler EEE'nin Audio klasöründen ada göre bulunur (uzantıya güvenilmez, bkz. Audio/README.md).
        private static DeskAudio BuildAudio(Transform parent)
        {
            var go = new GameObject("Audio");
            go.transform.SetParent(parent, false);
            var audio = go.AddComponent<DeskAudio>();

            var sfx = new GameObject("SfxSource").AddComponent<AudioSource>();
            sfx.transform.SetParent(go.transform, false);
            sfx.playOnAwake = false;

            var music = new GameObject("MusicSource").AddComponent<AudioSource>();
            music.transform.SetParent(go.transform, false);
            music.playOnAwake = false;
            music.loop = true;

            Wire(audio, "sfxSource", sfx);
            Wire(audio, "musicSource", music);
            Wire(audio, "click", FindClip("sfx_click"));
            Wire(audio, "paper", FindClip("sfx_paper"));
            Wire(audio, "stamp", FindClip("sfx_stamp"));
            Wire(audio, "glassBreak", FindClip("sfx_glass_break"));
            Wire(audio, "officeMusic", FindClip("mus_office_ambient"));
            return audio;
        }

        private static AudioClip FindClip(string name)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{name} t:AudioClip", new[] { AudioFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name)
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            Debug.LogWarning($"[Desk] Ses bulunamadı: {AudioFolder}/{name}. Prefab sessiz kurulacak.");
            return null;
        }

        private static DeskHud BuildHud(Transform parent, out TMP_Text clientTag)
        {
            var hud = Anchor(NewRect("Hud", parent), new Vector2(0, 1), Vector2.one, new Vector2(0, -80), Vector2.zero);
            AddImage(hud, HudColor);
            var layout = HLayout(hud, 20, new RectOffset(30, 30, 10, 10));
            layout.childForceExpandHeight = true;

            var title = NewText("LevelTitle", hud, "Ay", 36, LightText, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            Layout(title, prefW: 200);
            // Masadaki müteahhit / proje adı (DeskPropsView yazar); masa eşyalarla dolu olduğu için üst barda.
            clientTag = NewText("ClientTag", hud, "Müteahhit · Proje", 28, LightText, TextAlignmentOptions.Center, FontStyles.Bold);
            Layout(clientTag, flexW: 1);
            var money = NewText("Money", hud, "Kasa", 30, LightText, TextAlignmentOptions.MidlineRight);
            Layout(money, prefW: 420);
            var quota = NewText("Quota", hud, "Onay", 28, LightText, TextAlignmentOptions.MidlineRight);
            Layout(quota, prefW: 440);

            var view = hud.gameObject.AddComponent<DeskHud>();
            Wire(view, "levelTitleText", title);
            Wire(view, "moneyText", money);
            Wire(view, "quotaText", quota);
            return view;
        }

        // Kağıdı sürüklenebilir/animasyonlu yapar: saydamlık için CanvasGroup, arkasında gölge (ilk çocuk, yerleşim dışı).
        private static DraggableSheet AddSheet(RectTransform rt)
        {
            var group = rt.gameObject.AddComponent<CanvasGroup>();
            var shadow = Stretch(NewRect("Shadow", rt));
            shadow.SetAsFirstSibling();
            AddImage(shadow, new Color(0, 0, 0, 0.35f)).raycastTarget = false;
            shadow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var sheet = rt.gameObject.AddComponent<DraggableSheet>();
            Wire(sheet, "shadow", shadow);
            Wire(sheet, "group", group);
            return sheet;
        }

        // Sürüklenen iki kağıt (dosya ve El Kitabı) basılınca birbirinin önüne geçer: kökleri kardeş, sadece ikisi yer değiştirir.
        private static void LinkSheets(Transform dossierRoot, Transform rulebookRoot)
        {
            var dossierSheet = dossierRoot.GetComponentInChildren<DraggableSheet>(true);
            var book = rulebookRoot.GetComponentInChildren<DraggableSheet>(true);
            Wire(dossierSheet, "layer", dossierRoot);
            Wire(dossierSheet, "otherLayer", rulebookRoot);
            Wire(book, "layer", rulebookRoot);
            Wire(book, "otherLayer", dossierRoot);
        }

        // El kitabı penceresi. Kök ve "Panel" grafiksiz (tıklamayı engellemez); perde YOK: kitap açıkken dosya ve masa
        // tıklanabilir kalır. Dosya da açıksa kitap sağa yaslanır, dosya küçülüp sola kayar (DeskScreen.ArrangeSheets).
        // Kitap 760 genişlikte: yan yana modda ekranın sağ kısmına sığar.
        private static RulebookView BuildRulebook(Transform parent, Button openButton, Button dossierOpenButton)
        {
            var root = Stretch(NewRect("Rulebook", parent));
            var panel = Stretch(NewRect("Panel", root));

            var book = NewRect("Book", panel);
            book.anchorMin = book.anchorMax = book.pivot = new Vector2(0.5f, 0.5f);
            book.sizeDelta = new Vector2(760, 900);
            book.anchoredPosition = new Vector2(0, -40);
            AddImage(book, PaperColor);
            VLayout(book, 14, new RectOffset(30, 30, 30, 30));
            var bookSheet = AddSheet(book);

            var title = NewText("Title", book, "Denetçi El Kitabı", 40, Ink, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            title.enableAutoSizing = true;
            title.fontSizeMin = 28;
            title.fontSizeMax = 40;
            Layout(title, prefH: 56);
            var body = NewText("Body", book, "Kurallar", 26, Ink, TextAlignmentOptions.TopLeft);
            body.enableAutoSizing = true;
            body.fontSizeMin = 16;
            body.fontSizeMax = 26;
            body.overflowMode = TextOverflowModes.Truncate;
            Layout(body, prefH: 0, flexH: 1);

            // Alt bar: < Önceki · sayfa no · Sonraki > · Kapat
            var footer = NewRect("Footer", book);
            HLayout(footer, 12, new RectOffset(0, 0, 0, 0));
            Layout(footer, prefH: 64);
            var prev = NewButton("PrevButton", footer, "< Önceki", NeutralButton, Color.white, 24);
            Layout(prev, prefW: 170, prefH: 64);
            var pageLabel = NewText("Page", footer, "1 / 1", 24, Ink, TextAlignmentOptions.Center);
            Layout(pageLabel, prefH: 64, flexW: 1);
            var next = NewButton("NextButton", footer, "Sonraki >", NeutralButton, Color.white, 24);
            Layout(next, prefW: 170, prefH: 64);
            var close = NewButton("CloseButton", footer, "Kapat", NeutralButton, Color.white, 24);
            Layout(close, prefW: 130, prefH: 64);

            panel.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<RulebookView>();
            Wire(view, "openButton", openButton);
            Wire(view, "dossierOpenButton", dossierOpenButton);
            Wire(view, "panelRoot", panel.gameObject);
            Wire(view, "book", bookSheet);
            Wire(view, "titleText", title);
            Wire(view, "bodyText", body);
            Wire(view, "pageText", pageLabel);
            Wire(view, "prevButton", prev);
            Wire(view, "nextButton", next);
            Wire(view, "closeButton", close);
            return view;
        }

        // 1. gün öğretici kartı: perde + ortada kart (başlık, metin, sayfa, Atla / İleri). Kart metinleri TutorialView'da.
        private static TutorialView BuildTutorial(Transform parent)
        {
            var panel = Stretch(NewRect("Tutorial", parent));
            AddImage(panel, new Color(0, 0, 0, 0.6f)); // perde: arkadaki masaya tıklanmasın

            var card = NewRect("Card", panel);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(940, 560);
            AddImage(card, PaperColor);
            VLayout(card, 20, new RectOffset(56, 56, 44, 40));

            var title = NewText("Title", card, "Başlık", 42, Ink, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            Layout(title, prefH: 60);
            var body = NewText("Body", card, "Metin", 30, Ink, TextAlignmentOptions.TopLeft);
            body.enableAutoSizing = true;
            body.fontSizeMin = 18;
            body.fontSizeMax = 30;
            Layout(body, prefH: 0, flexH: 1);

            var footer = NewRect("Footer", card);
            var footerLayout = HLayout(footer, 16, new RectOffset(0, 0, 0, 0));
            footerLayout.childForceExpandHeight = true;
            Layout(footer, prefH: 70);
            var page = NewText("Page", footer, "1 / 4", 26, Ink, TextAlignmentOptions.MidlineLeft);
            Layout(page, flexW: 1);
            var skip = NewButton("SkipButton", footer, "Atla", NeutralButton, Color.white, 26);
            Layout(skip, prefW: 180);
            var next = NewButton("NextButton", footer, "İleri", ApproveGreen, Color.white, 30);
            Layout(next, prefW: 240);

            panel.gameObject.SetActive(false);

            // Bileşen panele değil, hep aktif ayrı bir objeye konur: pasif objede Awake çalışmaz.
            var holder = Stretch(NewRect("TutorialController", parent));
            var view = holder.gameObject.AddComponent<TutorialView>();
            Wire(view, "panelRoot", panel.gameObject);
            Wire(view, "titleText", title);
            Wire(view, "bodyText", body);
            Wire(view, "pageText", page);
            Wire(view, "nextButton", next);
            Wire(view, "nextLabel", next.GetComponentInChildren<TMP_Text>());
            Wire(view, "skipButton", skip);
            return view;
        }

        // Olay kartı (kabadayı vb.): perde + ortada kart (başlık, metin, Tamam). Metni DeskScreen verir.
        private static EventCardView BuildEventCard(Transform parent)
        {
            var panel = Stretch(NewRect("EventCard", parent));
            AddImage(panel, new Color(0, 0, 0, 0.6f)); // perde: arkadaki masaya tıklanmasın

            var card = NewRect("Card", panel);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(820, 440);
            card.localRotation = Quaternion.Euler(0, 0, -2);
            AddImage(card, PaperColor);
            VLayout(card, 20, new RectOffset(56, 56, 44, 40));

            var title = NewText("Title", card, "Olay", 46, StampRed, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            Layout(title, prefH: 64);
            var body = NewText("Body", card, "Metin", 30, Ink, TextAlignmentOptions.TopLeft);
            body.enableAutoSizing = true;
            body.fontSizeMin = 18;
            body.fontSizeMax = 30;
            Layout(body, prefH: 0, flexH: 1);
            var ok = NewButton("OkButton", card, "Tamam", NeutralButton, Color.white, 30);
            Layout(ok, prefH: 70);

            panel.gameObject.SetActive(false);

            // Bileşen panele değil, hep aktif ayrı bir objeye konur: pasif objede Awake çalışmaz.
            var holder = Stretch(NewRect("EventCardController", parent));
            var view = holder.gameObject.AddComponent<EventCardView>();
            Wire(view, "panelRoot", panel.gameObject);
            Wire(view, "titleText", title);
            Wire(view, "bodyText", body);
            Wire(view, "okButton", ok);
            return view;
        }

        // Karar sonrası müteahhide giden kapalı klasör: kapak + sekme + proje adı + eğik kaşe. Hiçbiri tıklamayı engellemez.
        private static FolderHandoffView BuildHandoff(RectTransform room)
        {
            var root = Stretch(NewRect("Handoff", room));

            var folder = NewRect("Folder", root);
            folder.anchorMin = folder.anchorMax = folder.pivot = new Vector2(0.5f, 0.5f);
            folder.sizeDelta = new Vector2(1000, 940);
            AddImage(folder, FolderColor).raycastTarget = false;

            var tab = NewRect("Tab", folder);
            tab.anchorMin = tab.anchorMax = new Vector2(0f, 1f);
            tab.pivot = new Vector2(0f, 0f);
            tab.sizeDelta = new Vector2(280, 60);
            tab.anchoredPosition = new Vector2(40, 0);
            AddImage(tab, FolderColor * 0.9f + new Color(0, 0, 0, 0.1f)).raycastTarget = false;

            var title = NewText("Title", folder, "Proje", 60, Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            Anchor(title.rectTransform, new Vector2(0, 1), Vector2.one, new Vector2(60, -260), new Vector2(-60, -80));
            title.enableAutoSizing = true;
            title.fontSizeMin = 30;
            title.fontSizeMax = 60;

            var stamp = NewRect("Stamp", folder);
            stamp.anchorMin = stamp.anchorMax = stamp.pivot = new Vector2(0.5f, 0.5f);
            stamp.sizeDelta = new Vector2(680, 180);
            stamp.anchoredPosition = new Vector2(0, -80);
            stamp.localRotation = Quaternion.Euler(0, 0, 12);
            var stampImage = AddImage(stamp, new Color(0, 0, 0, 0.15f));
            stampImage.raycastTarget = false;
            var stampText = NewText("Text", stamp, "ONAYLANDI", 96, Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(stampText.rectTransform);
            stampText.enableAutoSizing = true;
            stampText.fontSizeMin = 40;
            stampText.fontSizeMax = 96;

            var view = root.gameObject.AddComponent<FolderHandoffView>();
            Wire(view, "folder", folder);
            Wire(view, "titleText", title);
            Wire(view, "stampText", stampText);
            Wire(view, "stampImage", stampImage);
            folder.gameObject.SetActive(false);
            return view;
        }

        // Ofis sahnesi. Oda 1920x1080 sabit ve ortada (Canvas referansıyla birebir); çizim sırası (EEE):
        // desk_bg (oda + koltuk + masa) → portre → desk_front (sadece masa, şeffaf) → masadaki eşyalar → konuşma balonu.
        private static ClientSceneView BuildScene(Transform parent, out RectTransform room, out FolderHandoffView handoff)
        {
            var root = Stretch(NewRect("Scene", parent));
            var deskSprite = FindSprite("desk_bg");

            room = NewRect("Room", root);
            room.anchorMin = room.anchorMax = room.pivot = new Vector2(0.5f, 0.5f);
            room.sizeDelta = new Vector2(1920, 1080);
            SpriteImage(room, deskSprite, DeskColor);

            var portrait = TopCenter(NewRect("Portrait", room), new Vector2(PortraitSize, PortraitSize), PortraitTopY);
            var portraitImage = SpriteImage(portrait, null, Color.white);
            portraitImage.preserveAspect = true;

            handoff = BuildHandoff(room); // portrenin önünde, masanın arkasında: klasör masa kenarının arkasına girer

            // Masa önü portrenin önünde (müteahhit oturuyor gibi görünür). EEE'nin desk_front'u yoksa
            // desk_bg'nin masa kenarından aşağısı maskeyle kullanılır.
            var frontSprite = FindSprite("desk_front");
            if (frontSprite != null)
            {
                SpriteImage(Stretch(NewRect("DeskFront", room)), frontSprite, Color.white);
            }
            else
            {
                var front = Anchor(NewRect("DeskFront", room), Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 1080 - DeskTopY));
                front.gameObject.AddComponent<RectMask2D>();
                var frontImage = NewRect("Image", front);
                frontImage.anchorMin = frontImage.anchorMax = frontImage.pivot = new Vector2(0.5f, 0f);
                frontImage.sizeDelta = new Vector2(1920, 1080);
                SpriteImage(frontImage, deskSprite, DeskColor);
            }

            // Klasörün yolculuk katmanı: masa önünün üstünde (büyükken masanın kenarı kesmesin).
            Wire(handoff, "frontLayer", Stretch(NewRect("HandoffFront", room)));

            // Konuşma balonu: başın üstünde, alttan yukarı büyür; kuyruk portrenin başını gösterir.
            var balloon = NewRect("Balloon", room);
            balloon.anchorMin = balloon.anchorMax = new Vector2(0.5f, 1f);
            balloon.pivot = new Vector2(0.5f, 0f);
            balloon.anchoredPosition = new Vector2(BalloonShiftX, -(HeadTopY - 16));
            balloon.sizeDelta = new Vector2(520, 0);
            AddImage(balloon, BalloonColor).raycastTarget = false;
            VLayout(balloon, 0, new RectOffset(28, 28, 18, 18));
            balloon.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var balloonText = NewText("Text", balloon, "Söz", 28, Ink, TextAlignmentOptions.Center, FontStyles.Italic);
            var tail = NewRect("Tail", balloon);
            tail.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            tail.anchorMin = tail.anchorMax = new Vector2(0.5f, 0f);
            tail.sizeDelta = new Vector2(28, 28);
            tail.anchoredPosition = new Vector2(-BalloonShiftX, 0);
            tail.localRotation = Quaternion.Euler(0, 0, 45);
            AddImage(tail, BalloonColor).raycastTarget = false;
            balloon.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<ClientSceneView>();
            Wire(view, "portraitRoot", portrait);
            Wire(view, "portraitImage", portraitImage);
            Wire(view, "balloonRoot", balloon.gameObject);
            Wire(view, "balloonText", balloonText);
            return view;
        }

        // Görseller EEE'nin Art klasöründen ada göre bulunur; yoksa düz renkli yer tutucu kalır.
        private static Sprite FindSprite(string name)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{name} t:Sprite", new[] { ArtFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name)
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            Debug.LogWarning($"[Desk] Görsel bulunamadı: {ArtFolder}/{name}. Düz renk kullanılacak.");
            return null;
        }

        private static Image SpriteImage(Component target, Sprite sprite, Color fallback)
        {
            var image = AddImage(target, sprite != null ? Color.white : fallback);
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        // Odanın üst-ortasına göre yerleştirir (y = odanın üst kenarından aşağı px).
        private static RectTransform TopCenter(RectTransform rt, Vector2 size, float y)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(0, -y);
            return rt;
        }

        // Masadaki eşyalar (odanın içinde, desk_bg px'leriyle): sol altta LAB RAPORU kitabı, sağ altta EL KİTABI,
        // isimliğin sağındaki boş masa yüzeyinde zarf, gün bitince boş koltuğun önünde "Mesaiyi bitir"
        // (müteahhit / proje adı üst barda). Kitaplar köşelerdeki kupa/kağıtların bir kısmını örter (EEE planı).
        private static DeskPropsView BuildProps(RectTransform room, TMP_Text clientTag, out Button handbookBook)
        {
            var root = Stretch(NewRect("Props", room));

            var labBook = BookButton("LabBook", root, "book_lab", new Color(0.24f, 0.45f, 0.7f), 40);
            handbookBook = BookButton("HandbookBook", root, "book_rules", new Color(0.33f, 0.6f, 0.33f), 1920 - 40 - BookSize.x);

            var envelope = TopLeft(NewRect("Envelope", root), new Vector2(160, 84), 1300, 785); // isimliğin sağı, masa yüzeyi
            AddImage(envelope, EnvelopeColor).raycastTarget = false;
            var envelopeText = NewText("Text", envelope, "ZARF", 20, Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(envelopeText.rectTransform);
            envelope.gameObject.SetActive(false);

            var finish = NewButton("FinishButton", root, "Mesaiyi bitir", NeutralButton, Color.white, 30);
            TopLeft((RectTransform)finish.transform, new Vector2(360, 70), 780, 650); // boş koltuğun önü
            finish.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<DeskPropsView>();
            Wire(view, "labBook", labBook);
            Wire(view, "envelopeRoot", envelope.gameObject);
            Wire(view, "envelopeText", envelopeText);
            Wire(view, "clientTagText", clientTag);
            Wire(view, "finishButton", finish);
            return view;
        }

        private static readonly Vector2 BookSize = new Vector2(300, 200); // book_*.png 600x400 (EEE) yarı boyutta

        private static Button BookButton(string name, Transform parent, string sprite, Color fallback, float x)
        {
            var rt = TopLeft(NewRect(name, parent), BookSize, x, 1080 - 5 - BookSize.y);
            var image = SpriteImage(rt, FindSprite(sprite), fallback);
            image.raycastTarget = true;
            image.preserveAspect = true;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        // Odanın sol-üstüne göre yerleştirir (x, y = desk_bg üzerindeki px).
        private static RectTransform TopLeft(RectTransform rt, Vector2 size, float x, float y)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(x, -y);
            return rt;
        }

        // Açık dosya: perde + ortada belge (sol) ve karar sütunu (sağ). Kapalıyken sahne tamamen görünür.
        private static DossierView BuildDossier(Transform parent, out DocumentView documentView,
            out ActionPanel actionPanel, out Button handbookButton)
        {
            var root = Stretch(NewRect("Dossier", parent));
            var panel = Stretch(NewRect("Panel", root));

            var backdrop = Stretch(NewRect("Backdrop", panel));
            var backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.targetGraphic = AddImage(backdrop, new Color(0, 0, 0, 0.45f));
            backdropButton.transition = Selectable.Transition.None;

            var sheet = NewRect("Sheet", panel);
            sheet.anchorMin = sheet.anchorMax = sheet.pivot = new Vector2(0.5f, 0.5f);
            sheet.sizeDelta = new Vector2(1560, 940);
            sheet.anchoredPosition = new Vector2(0, -40);
            var draggable = AddSheet(sheet);

            documentView = BuildDocument(sheet, 1000);
            actionPanel = BuildActions(sheet, 540, out handbookButton, out var closeButton);

            panel.gameObject.SetActive(false);

            // Bileşen panele değil, hep aktif köke konur: pasif objede Awake çalışmaz.
            var view = root.gameObject.AddComponent<DossierView>();
            Wire(view, "panelRoot", panel.gameObject);
            Wire(view, "sheet", draggable);
            Wire(view, "closeButton", closeButton);
            Wire(view, "backdropButton", backdropButton);
            return view;
        }

        private static DocumentView BuildDocument(Transform parent, float width)
        {
            var area = Anchor(NewRect("Document", parent), Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(width, 0));

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
            VLayout(paper, 14, new RectOffset(50, 50, 36, 36));

            var title = NewText("Title", paper, "Belge başlığı", 40, Ink, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            Layout(title, prefH: 56);
            var meta = NewText("Meta", paper, "Künye", 26, Ink, TextAlignmentOptions.TopLeft);

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
            Wire(rowView, "reportedText", cells[1]);
            Wire(rowView, "actualText", cells[2]);
            rowTemplate.SetActive(false);

            // Kaşe (layout dışı, sağ üst köşe, hafif yamuk)
            var stamp = NewRect("Stamp", paper);
            stamp.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            stamp.anchorMin = stamp.anchorMax = stamp.pivot = Vector2.one;
            stamp.sizeDelta = new Vector2(320, 110);
            stamp.anchoredPosition = new Vector2(-40, -36);
            stamp.localRotation = Quaternion.Euler(0, 0, 10);
            AddImage(stamp, new Color(StampRed.r, StampRed.g, StampRed.b, 0.12f));
            var stampLabel = NewText("Label", stamp, "ONAYLIDIR\n<size=60%>Kaşe</size>", 34, StampRed,
                TextAlignmentOptions.Center, FontStyles.Bold);
            // Uzun lab adları ("<LAB ADI> · AKREDİTE") kutuya sığsın diye yazı otomatik küçülür.
            stampLabel.enableAutoSizing = true;
            stampLabel.fontSizeMin = 12;
            stampLabel.fontSizeMax = 32;
            Stretch(stampLabel.rectTransform).offsetMin = new Vector2(10, 6);
            stampLabel.rectTransform.offsetMax = new Vector2(-10, -6);

            // Karar kaşesi (ONAYLANDI / REDDEDİLDİ): kağıdın ortasında, en üstte, karar verilince görünür.
            var decision = NewRect("DecisionStamp", paper);
            decision.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            decision.anchorMin = decision.anchorMax = decision.pivot = new Vector2(0.5f, 0.5f);
            decision.sizeDelta = new Vector2(620, 160);
            decision.anchoredPosition = new Vector2(0, -60);
            decision.localRotation = Quaternion.Euler(0, 0, -8);
            var decisionImage = AddImage(decision, new Color(StampRed.r, StampRed.g, StampRed.b, 0.12f));
            decisionImage.raycastTarget = false;
            var decisionLabel = NewText("Label", decision, "ONAYLANDI", 90, StampRed, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(decisionLabel.rectTransform);
            decision.gameObject.SetActive(false);

            var empty = NewText("EmptyState", area, "", 36, LightText, TextAlignmentOptions.Center); // müşteri yokken boş
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
            Wire(view, "decisionStamp", decision);
            Wire(view, "decisionStampImage", decisionImage);
            Wire(view, "decisionStampText", decisionLabel);
            return view;
        }

        private static GameObject BuildParamRow(string name, Transform parent, bool isHeader)
        {
            var row = NewRect(name, parent);
            var layout = HLayout(row, 10, new RectOffset(0, 0, 0, 0));
            layout.childForceExpandHeight = true;
            Layout(row, prefH: 44);

            var style = isHeader ? FontStyles.Bold : FontStyles.Normal;
            // Gerekli değer sütunu yok (Ç1): oyuncu El Kitabı'na bakar.
            string[] labels = isHeader
                ? new[] { "Parametre", "Beyan", "Ek test" }
                : new[] { "Parametre", "0", "?" };
            float[] widths = { 2, 1, 1 };

            for (int i = 0; i < labels.Length; i++)
            {
                var cell = NewText("Cell" + i, row, labels[i], 26, Ink,
                    i == 0 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Midline, style);
                Layout(cell, prefW: 0, flexW: widths[i]);
            }
            return row.gameObject;
        }

        // Açık dosyanın sağ sütunu: müteahhit, zarf, El Kitabı (belgeyle karşılaştırmak için), ek test, kararlar, Kapat.
        private static ActionPanel BuildActions(Transform parent, float width, out Button handbookButton, out Button closeButton)
        {
            var panel = Anchor(NewRect("Actions", parent), new Vector2(1, 0), Vector2.one, new Vector2(-width, 0), Vector2.zero);
            AddImage(panel, PanelColor);
            VLayout(panel, 12, new RectOffset(24, 24, 24, 24));

            var clientName = NewText("ClientName", panel, "Başvuru sahibi", 34, LightText, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            var clientLine = NewText("ClientLine", panel, "Proje", 26, LightText, TextAlignmentOptions.TopLeft);

            var bribe = NewRect("Bribe", panel);
            AddImage(bribe, EnvelopeColor);
            VLayout(bribe, 10, new RectOffset(16, 16, 16, 16));
            var bribeText = NewText("BribeText", bribe, "Zarf", 24, Ink, TextAlignmentOptions.TopLeft);
            var takeBribe = NewButton("TakeBribeButton", bribe, "Zarfı al ve imzala", BribeButton, Color.white, 26);
            Layout(takeBribe, prefH: 56);

            Layout(NewRect("Spacer", panel), flexH: 1);

            handbookButton = NewButton("HandbookButton", panel, "El Kitabı'na bak", new Color(0.33f, 0.5f, 0.33f), Color.white, 26);
            Layout(handbookButton, prefH: 60);
            var test = NewButton("ExtraTestButton", panel, "Ek test iste", NeutralButton, Color.white, 26);
            Layout(test, prefH: 64);
            var approve = NewButton("ApproveButton", panel, "ONAYLA", ApproveGreen, Color.white, 34);
            Layout(approve, prefH: 78);
            var reject = NewButton("RejectButton", panel, "REDDET", RejectRed, Color.white, 34);
            Layout(reject, prefH: 78);

            var verdict = NewText("Verdict", panel, "", 36, LightText, TextAlignmentOptions.Center, FontStyles.Bold);
            Layout(verdict, prefH: 50);
            closeButton = NewButton("CloseButton", panel, "Kapat (masaya bırak)", NeutralButton, Color.white, 26);
            Layout(closeButton, prefH: 60);

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
