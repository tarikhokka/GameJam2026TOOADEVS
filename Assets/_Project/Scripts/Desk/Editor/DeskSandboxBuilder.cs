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
    public static partial class DeskSandboxBuilder
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Desk";
        private const string PrefabPath = PrefabFolder + "/DeskScreen.prefab";
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/Sandbox_Desk.unity";
        private const string AudioFolder = "Assets/_Project/Audio";
        private const string ArtFolder = "Assets/_Project/Art";
        private const string RuleSetPath = "Assets/_Project/Data/RuleSet.asset";
        private const string GiftsFolder = "Assets/_Project/Data/Gifts";

        // Oda yerleşimi (yeni hedef arayüz, 1920x1080 px, yukarıdan): office_bg 0-540; portre PortraitSize kare (EEE: portrait_* 1000x1000),
        // üstü PortraitTopY'de (üst barın altında; şapkadan bele görünür, Onur'un referansı gibi); masa kenarı DeskEdgeY'de
        // (portrenin önünde, belden aşağısını keser), masa yüzeyi DeskSurfaceY'den ekranın altına. Belgeler kenarın hemen altından başlar.
        private const float PortraitSize = 460f;  // referanstaki gibi şapkadan bele (cep hizası) görünsün
        private const float PortraitTopY = 58f;   // şapkanın üstü ~y 90: üst barın (80 px) hemen altı
        private const float DeskEdgeY = 380f;     // müteahhidi bel hizasından keser
        private const float DeskSurfaceY = 440f;
        private const float BalloonX = 1080f;     // balon başın sağında (sol kenarı)
        private const float BalloonTopY = 130f;
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
            WireGifts(bootstrap);

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

        // Sandbox'ta hediye denensin diye EEE'nin GiftSO'ları (Data/Gifts). Editor assembly'si Core'u görmez: tip adıyla aranır.
        private static void WireGifts(DeskSandboxBootstrap bootstrap)
        {
            var so = new SerializedObject(bootstrap);
            var list = so.FindProperty("gifts");
            var guids = AssetDatabase.IsValidFolder(GiftsFolder) ? AssetDatabase.FindAssets("t:GiftSO", new[] { GiftsFolder }) : new string[0];
            list.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]));
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
            paperFontsResolved = false; // EEE yeni font eklediyse bu kurulumda bulunsun

            var root = NewRect("DeskScreen", null);
            Stretch(root);
            var screen = root.gameObject.AddComponent<DeskScreen>();

            AddImage(Stretch(NewRect("Background", root)), DeskColor); // 16:9 dışı ekranlarda odanın kenarları

            // Çizim sırası: sahne (oda, müteahhit, masa, masadaki eşyalar) → üst bar → kağıt pencereler → kartlar.
            Wire(screen, "scene", BuildScene(root, out var room));
            Wire(screen, "hud", BuildHud(root, out var clientTag));
            Wire(screen, "props", BuildProps(room, clientTag, out var handbookBook, out var officeGifts));
            Wire(screen, "officeGifts", officeGifts);
            room.Find("Balloon").SetAsLastSibling(); // konuşma balonu masadakilerin (zarf, hediyeler) önünde

            // Kağıt pencereler ortak katmanda: basılan en öne gelir (DraggableSheet.BringToFront). Ayrıntı: DeskSandboxBuilder.Papers.cs
            var papers = Stretch(NewRect("Papers", root));
            Wire(screen, "formWindow", BuildFormWindow(papers, out var formView));
            Wire(screen, "formView", formView);
            Wire(screen, "labWindow", BuildLabWindow(papers, out var labView));
            Wire(screen, "labView", labView);
            Wire(screen, "rulebook", BuildRulebook(papers, handbookBook));
            Wire(screen, "tutorial", BuildTutorial(root));
            Wire(screen, "eventCard", BuildEventCard(root));
            Wire(screen, "giftOffer", BuildGiftOffer(root));
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

            var typing = new GameObject("TypingSource").AddComponent<AudioSource>(); // konuşma balonu yazı sesi (perdesi değişir)
            typing.transform.SetParent(go.transform, false);
            typing.playOnAwake = false;

            Wire(audio, "sfxSource", sfx);
            Wire(audio, "musicSource", music);
            Wire(audio, "typingSource", typing);
            Wire(audio, "typing", FindClip("sfx_typing"));
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
            Wire(view, "titleFont", TitleFont); // EEE: kart fontları ve yerleşimi TutorialView.StyleCard'da
            Wire(view, "bodyFont", ValueFont);
            Wire(view, "boldFont", BoldFont);
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

        // Ofis sahnesi, yeni hedef arayüz (Ahmet kararı; Onur'un Docs/Tasarim/hedef_arayuz.png'si, EEE görselleri). Oda 1920x1080:
        // office_bg (üstte 0-540) → portre (masanın arkasında; baş ve göğüs görünür) → desk_surface (masa yüzeyi, DeskSurfaceY'den aşağı)
        // → desk_edge (masa kenarı, DeskEdgeY'de, portrenin önünde) → masadakiler → konuşma balonu (başın sağında).
        // Üç belge masanın üstünde, kenarın hemen altından başlar (kağıt pencereler, ayrı katman).
        private static ClientSceneView BuildScene(Transform parent, out RectTransform room)
        {
            var root = Stretch(NewRect("Scene", parent));

            room = NewRect("Room", root);
            room.anchorMin = room.anchorMax = room.pivot = new Vector2(0.5f, 0.5f);
            room.sizeDelta = new Vector2(1920, 1080);
            AddImage(room, DeskColor).raycastTarget = false; // görseller yoksa düz renk

            var office = TopLeft(NewRect("Office", room), new Vector2(1920, 540), 0, 0);
            SpriteImage(office, FindSprite("office_bg"), DeskColor);

            var portrait = TopCenter(NewRect("Portrait", room), new Vector2(PortraitSize, PortraitSize), PortraitTopY);
            var portraitImage = SpriteImage(portrait, null, Color.white);
            portraitImage.preserveAspect = true;


            // Masa: yüzey portrenin önünde (belden aşağısını örter), kenar onun da önünde.
            var surface = TopLeft(NewRect("DeskSurface", room), new Vector2(1920, 1080 - DeskSurfaceY), 0, DeskSurfaceY);
            SpriteImage(surface, FindSprite("desk_surface"), DeskColor);
            var edge = TopLeft(NewRect("DeskEdge", room), new Vector2(1920, 110), 0, DeskEdgeY);
            SpriteImage(edge, FindSprite("desk_edge"), DeskColor * 0.8f);


            // Konuşma balonu: başın sağında, kuyruğu sola (müteahhide) bakar; üst kenarından aşağı büyür.
            var balloon = NewRect("Balloon", room);
            balloon.anchorMin = balloon.anchorMax = new Vector2(0f, 1f);
            balloon.pivot = new Vector2(0f, 1f);
            balloon.anchoredPosition = new Vector2(BalloonX, -BalloonTopY);
            balloon.sizeDelta = new Vector2(460, 0);
            AddImage(balloon, BalloonColor).raycastTarget = true; // tıklanınca yazı anında tamamlanır (ClientSceneView)
            VLayout(balloon, 0, new RectOffset(26, 26, 16, 16));
            balloon.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var balloonText = NewText("Text", balloon, "Söz", 26, Ink, TextAlignmentOptions.Center, FontStyles.Normal);
            balloonText.font = ValueFont;
            var tail = NewRect("Tail", balloon);
            tail.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            tail.anchorMin = tail.anchorMax = new Vector2(0f, 1f);
            tail.sizeDelta = new Vector2(26, 26);
            tail.anchoredPosition = new Vector2(0, -44);
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

        // Masadaki eşyalar (yeni hedef arayüz): hediyeler, zarf (4. adımda yeni yerine), gün bitince "Mesaiyi bitir"
        // (müteahhit / proje adı üst barda). Belgeler burada değil: Papers katmanında, masada hep açık.
        private static DeskPropsView BuildProps(RectTransform room, TMP_Text clientTag, out Button handbookBook,
            out OfficeGiftsView officeGifts)
        {
            var root = Stretch(NewRect("Props", room));

            // Hediyeler: masa kenarının üstünde, müteahhidin iki yanında (balonun altında kalmayan yerlerde).
            officeGifts = BuildOfficeGifts(root);

            // Yeni hedef arayüz: masada kitap / klasör yok; üç belge masada hep açık (kağıt pencereler, Papers katmanı).
            handbookBook = null;

            // Zarf: müteahhidin sağında, masa kenarının üstünde (balonun altı, belgelerin üstü), hafif yamuk.
            var envelope = TopLeft(NewRect("Envelope", root), new Vector2(140, 76), 1200, DeskEdgeY - 64);
            envelope.localRotation = Quaternion.Euler(0, 0, -6);
            AddImage(envelope, EnvelopeColor).raycastTarget = false;
            var envelopeText = NewText("Text", envelope, "ZARF", 20, Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(envelopeText.rectTransform);
            envelope.gameObject.SetActive(false);

            var finish = NewButton("FinishButton", root, "Mesaiyi bitir", NeutralButton, Color.white, 30);
            TopLeft((RectTransform)finish.transform, new Vector2(360, 70), 780, 650); // boş koltuğun önü
            finish.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<DeskPropsView>();
            Wire(view, "envelopeRoot", envelope.gameObject);
            Wire(view, "envelopeText", envelopeText);
            Wire(view, "clientTagText", clientTag);
            Wire(view, "finishButton", finish);
            return view;
        }

        // Masadaki nesne: kök (konum + eğim, DeskBookProp) → Shadow (yatay yumuşak elips) + Contact (dar, alt kenarda)
        // + Book (görsel + Button, pivot alt kenarda). x, top: sol-üst köşe (desk_bg px); yükseklik görselin oranından.
        // artBottom: görselde nesnenin alt kenarı, yüksekliğin bu oranı kadar yukarıda (EEE kitaplarında altta şeffaf boşluk ~%17).
        // Görsel yoksa düz renk; fallbackLabel verilirse çerçeve + yazı eklenir.
        private static Button BookButton(string name, Transform parent, string sprite, Color fallback, float x, float tilt,
            out GameObject bookRoot, float width, float top, float fallbackAspect = 0.52f, string fallbackLabel = null,
            float artBottom = 0.17f)
        {
            var art = FindSprite(sprite);
            float height = art != null ? width * art.rect.height / art.rect.width : width * fallbackAspect;
            var size = new Vector2(width, height);
            if (art == null) artBottom = 0f;

            var root = NewRect(name, parent);
            root.anchorMin = root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0.5f, 0.5f); // eğim ortadan
            root.sizeDelta = size;
            root.anchoredPosition = new Vector2(x + size.x * 0.5f, -(top + size.y * 0.5f));
            root.localRotation = Quaternion.Euler(0, 0, tilt);

            // EEE: gölge kitabın altında, alt kenara yakın, yatay yumuşak elips (~%30 koyu); ışık soldan → hafif sağa.
            float baseY = size.y * artBottom;
            var shadow = BookShadow("Shadow", root, new Vector2(size.x * 0.98f, size.y * 0.3f), new Vector2(size.x * 0.03f, baseY));
            var contact = BookShadow("Contact", root, new Vector2(size.x * 0.88f, size.y * 0.09f), new Vector2(size.x * 0.01f, baseY + size.y * 0.01f));

            var book = Stretch(NewRect("Book", root));
            book.pivot = new Vector2(0.5f, 0f); // basma/kalkma alt kenardan: kitap masada oturur
            var image = SpriteImage(book, art, fallback);
            image.raycastTarget = true;
            image.preserveAspect = true;
            var button = book.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None; // geri bildirim DeskBookProp'ta (kalkma)
            if (art == null && fallbackLabel != null)
            {
                var outline = book.gameObject.AddComponent<Outline>(); // piksel tasarımdaki 3 px koyu çerçeve
                outline.effectColor = PaperInk;
                outline.effectDistance = new Vector2(3, -3);
                var label = NewText("Label", book, fallbackLabel, 26, PaperInk, TextAlignmentOptions.Center, FontStyles.Bold);
                Stretch(label.rectTransform);
                label.font = TitleFont;
                label.characterSpacing = 4;
                label.enableAutoSizing = true; // küçük klasörde taşmasın
                label.fontSizeMin = 12;
                label.fontSizeMax = 26;
                label.margin = new Vector4(8, 6, 8, 6);
            }

            var prop = root.gameObject.AddComponent<DeskBookProp>();
            Wire(prop, "book", book);
            Wire(prop, "bookImage", image);
            Wire(prop, "shadow", shadow);
            Wire(prop, "shadowImage", shadow.GetComponent<Image>());
            Wire(prop, "contact", contact);
            Wire(prop, "contactImage", contact.GetComponent<Image>());

            bookRoot = root.gameObject;
            return button;
        }

        // Hediye yerleri (yeni hedef arayüz): masa kenarının üstünde, müteahhidin iki yanında, ofisin önünde (alt kenar y = GiftBottomY).
        // Müteahhit (x ~730-1190), balon (x 1080-1540) ve zarf açıkta kalır; belgeler kenarın altından başladığı için örtmez.
        // Kabul sırasıyla dolar; sadece dürüst ustalar verdiği için pratikte 1-2 tanesi kullanılır.
        private static readonly float[] GiftSlotCenterX = { 460, 1640, 280, 1820, 640, 100 };
        private const float GiftSize = 160f;      // EEE: ~140-180 px (gift_*.png 300x300, nesne alta oturur)
        private const float GiftBottomY = DeskEdgeY + 14f; // masa kenarının üst yüzeyi
        private const float GiftArtBottom = 0.09f; // görselde nesnenin altındaki şeffaf boşluk

        private static OfficeGiftsView BuildOfficeGifts(RectTransform props)
        {
            var root = Stretch(NewRect("Gifts", props));
            var slots = new GameObject[GiftSlotCenterX.Length];
            var images = new Image[GiftSlotCenterX.Length];
            var size = new Vector2(GiftSize, GiftSize);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = NewRect("Gift" + i, root);
                slot.anchorMin = slot.anchorMax = new Vector2(0f, 1f);
                slot.pivot = new Vector2(0.5f, 0f); // masaya konma animasyonu alt kenardan büyür
                slot.sizeDelta = size;
                slot.anchoredPosition = new Vector2(GiftSlotCenterX[i], -GiftBottomY);

                float baseY = size.y * GiftArtBottom;
                var shadow = BookShadow("Shadow", slot, new Vector2(size.x * 0.75f, size.y * 0.16f), new Vector2(size.x * 0.05f, baseY));
                var contact = BookShadow("Contact", slot, new Vector2(size.x * 0.6f, size.y * 0.05f), new Vector2(size.x * 0.01f, baseY + 1));

                var item = Stretch(NewRect("Item", slot));
                item.pivot = new Vector2(0.5f, 0f);
                var image = AddImage(item, Color.white);
                image.preserveAspect = true; // görsel çalışırken GiftSO'dan gelir

                var prop = slot.gameObject.AddComponent<DeskBookProp>();
                Wire(prop, "book", item);
                Wire(prop, "bookImage", image);
                Wire(prop, "shadow", shadow);
                Wire(prop, "shadowImage", shadow.GetComponent<Image>());
                Wire(prop, "contact", contact);
                Wire(prop, "contactImage", contact.GetComponent<Image>());

                slot.gameObject.SetActive(false);
                slots[i] = slot.gameObject;
                images[i] = image;
            }

            var view = root.gameObject.AddComponent<OfficeGiftsView>();
            var so = new SerializedObject(view);
            var slotsProp = so.FindProperty("slots");
            var imagesProp = so.FindProperty("slotImages");
            slotsProp.arraySize = imagesProp.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
                imagesProp.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        // Kitabın alt kenarına ortalı gölge elipsi (sprite'ı DeskBookProp kodla üretir).
        private static RectTransform BookShadow(string name, RectTransform root, Vector2 size, Vector2 offset)
        {
            var shadow = NewRect(name, root);
            shadow.anchorMin = shadow.anchorMax = new Vector2(0.5f, 0f);
            shadow.pivot = new Vector2(0.5f, 0.5f);
            shadow.sizeDelta = size;
            shadow.anchoredPosition = offset;
            AddImage(shadow, new Color(0, 0, 0, 0.4f)).raycastTarget = false;
            return shadow;
        }

        // Odanın sol-üstüne göre yerleştirir (x, y = desk_bg üzerindeki px).
        private static RectTransform TopLeft(RectTransform rt, Vector2 size, float x, float y)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(x, -y);
            return rt;
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
