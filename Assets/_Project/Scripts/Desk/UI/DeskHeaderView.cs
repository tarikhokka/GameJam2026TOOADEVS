using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    /// <summary>
    /// Masanın 80 px üst barı. Görünümünü kendisi kurar; Core durumunu yalnızca okur.
    /// Masa yöneticisi Refresh, SetClient ve SetDayDone çağrılarını yönetir.
    /// Prefab/builder bağlantısı Oğuzhan'a aittir; bu bileşen Flow'a bağımlı değildir.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DeskHeaderView : MonoBehaviour
    {
        public const float Height = 80f;

        [Header("Yazı tipleri")]
        [SerializeField] TMP_FontAsset titleFont; // Paytone One
        [SerializeField] TMP_FontAsset bodyFont;  // Fira Sans

        [Header("T1 görselleri — boşsa kahve şerit ve kağıt etiketler")]
        [SerializeField] Sprite barSprite;       // hud_bar
        [SerializeField] Sprite tagSprite;       // hud_tag (9-slice)
        [SerializeField] Sprite dayIcon;         // hud_icon_day
        [SerializeField] Sprite moneyIcon;       // hud_icon_money
        [SerializeField] Sprite countIcon;       // hud_icon_count

        static readonly Color Ink = new Color(0.23f, 0.18f, 0.13f);
        static readonly Color Paper = new Color(0.97f, 0.94f, 0.85f);
        static readonly Color Wood = new Color(0.34f, 0.22f, 0.13f);
        static readonly Color DebtRed = new Color(0.66f, 0.16f, 0.12f);

        RectTransform root, content, cashRect;
        Image bar, dayTag, clientTag, moneyTag, countTag;
        Image daySymbol, moneySymbol, countSymbol;
        TMP_Text day, client, cash, debt, counts;
        Outline dayBorder, clientBorder, moneyBorder, countBorder;

        void Awake() => EnsureBuilt();

        /// <summary>Gün, kasa, borç ve bugünkü onaylanan/bekleyen sayılarını yeniler. Müşteri etiketine dokunmaz.</summary>
        public void Refresh(GameContext ctx)
        {
            EnsureBuilt();
            if (ctx == null || ctx.State == null)
            {
                day.text = "Gün —";
                cash.text = "Kasa: —";
                counts.text = "Onaylanan: —\nBekleyen: —";
                SetDebt(0);
                return;
            }

            var state = ctx.State;
            int waiting = 0;
            if (ctx.TodayProjects != null)
                foreach (var project in ctx.TodayProjects)
                    if (project != null && project.Verdict == ProjectVerdict.Pending) waiting++;

            day.text = $"Gün {state.Day}";
            cash.text = $"Kasa: {DeskFormat.Money(state.Money)}";
            counts.text = $"Onaylanan: {state.ApprovedToday}\nBekleyen: {waiting}";
            SetDebt(state.Debt);
        }

        /// <summary>Ortadaki müşteri/proje etiketini değiştirir; önceki gün sonu mesajını da temizler.</summary>
        public void SetClient(string clientName, string projectName)
        {
            EnsureBuilt();
            string name = (clientName ?? "").Trim();
            string project = (projectName ?? "").Trim();
            client.text = name.Length == 0 ? project
                : project.Length == 0 ? name : $"{name} · {project}";
        }

        /// <summary>ShowEmpty için: bütün kararlar verilmişse gün sonu mesajı, aksi hâlde boş etiket.</summary>
        public void SetDayDone(bool allDecided)
        {
            EnsureBuilt();
            client.text = allDecided ? "Bugünkü başvurular bitti." : "";
        }

        void SetDebt(int amount)
        {
            bool visible = amount > 0;
            debt.text = visible ? $"Borç: {DeskFormat.Money(amount)}" : "";
            debt.gameObject.SetActive(visible);
            // Borç yokken kasa satırı kartın ortasına gelir; boş bir ikinci satır bırakılmaz.
            cashRect.anchorMin = new Vector2(0f, visible ? 0.45f : 0f);
            cashRect.offsetMin = cashRect.offsetMax = Vector2.zero;
        }

        void EnsureBuilt()
        {
            if (content == null)
            {
                root = (RectTransform)transform;
                root.anchorMin = new Vector2(0f, 1f);
                root.anchorMax = Vector2.one;
                root.pivot = new Vector2(0.5f, 1f);
                root.anchoredPosition = Vector2.zero;
                root.sizeDelta = new Vector2(0f, Height);
                root.localScale = Vector3.one;

                bar = NewImage("Bar", root);
                Stretch(bar.rectTransform, Vector2.zero, Vector2.zero);
                var edge = NewImage("BottomEdge", root);
                edge.color = new Color(Ink.r, Ink.g, Ink.b, 0.6f);
                edge.rectTransform.anchorMin = Vector2.zero;
                edge.rectTransform.anchorMax = new Vector2(1f, 0f);
                edge.rectTransform.pivot = new Vector2(0.5f, 0f);
                edge.rectTransform.sizeDelta = new Vector2(0f, 3f);

                content = NewRect("Content", root);
                Stretch(content, new Vector2(18f, 8f), new Vector2(-18f, -8f));
                var layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = true;

                dayTag = NewTag("Day", 174f, false, out dayBorder);
                daySymbol = NewIcon(dayTag.transform);
                var dayBody = Body(dayTag.transform, "TextArea");
                day = NewText("DayText", dayBody, 30f, 22f, TextAlignmentOptions.MidlineLeft);
                day.text = "Gün —";

                clientTag = NewTag("Client", 240f, true, out clientBorder);
                var clientBody = NewRect("TextArea", clientTag.transform);
                Stretch(clientBody, new Vector2(18f, 6f), new Vector2(-18f, -6f));
                client = NewText("ClientText", clientBody, 28f, 20f, TextAlignmentOptions.Center);
                client.textWrappingMode = TextWrappingModes.Normal;

                moneyTag = NewTag("Money", 344f, false, out moneyBorder);
                moneySymbol = NewIcon(moneyTag.transform);
                var walletBody = Body(moneyTag.transform, "TextArea");
                cash = NewText("CashText", walletBody, 27f, 21f, TextAlignmentOptions.MidlineLeft);
                cash.text = "Kasa: —";
                cashRect = cash.rectTransform;
                debt = NewText("DebtText", walletBody, 21f, 18f, TextAlignmentOptions.MidlineLeft);
                debt.rectTransform.anchorMax = new Vector2(1f, 0.45f);
                debt.rectTransform.offsetMin = debt.rectTransform.offsetMax = Vector2.zero;
                debt.color = DebtRed;
                SetDebt(0);

                countTag = NewTag("Counts", 350f, false, out countBorder);
                countSymbol = NewIcon(countTag.transform);
                var countBody = Body(countTag.transform, "TextArea");
                counts = NewText("CountsText", countBody, 23f, 20f, TextAlignmentOptions.MidlineLeft);
                counts.textWrappingMode = TextWrappingModes.Normal;
                counts.text = "Onaylanan: —\nBekleyen: —";
            }
            ApplyAppearance(); // Builder alanları Awake'ten sonra bağlarsa sonraki Refresh'te de uygulanır.
        }

        void ApplyAppearance()
        {
            bar.sprite = barSprite;
            bar.color = barSprite != null ? Color.white : Wood;
            StyleTag(dayTag, dayBorder);
            StyleTag(clientTag, clientBorder);
            StyleTag(moneyTag, moneyBorder);
            StyleTag(countTag, countBorder);
            StyleIcon(daySymbol, dayIcon);
            StyleIcon(moneySymbol, moneyIcon);
            StyleIcon(countSymbol, countIcon);
            if (titleFont != null) day.font = titleFont;
            if (bodyFont != null) client.font = cash.font = debt.font = counts.font = bodyFont;
        }

        void StyleTag(Image tag, Outline border)
        {
            tag.sprite = tagSprite;
            tag.type = tagSprite != null && tagSprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            tag.color = tagSprite != null ? Color.white : Paper;
            border.enabled = tagSprite == null;
        }

        static void StyleIcon(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            var body = (RectTransform)image.transform.parent.Find("TextArea");
            body.offsetMin = new Vector2(sprite != null ? 56f : 16f, 6f);
        }

        Image NewTag(string name, float width, bool flexible, out Outline border)
        {
            var tag = NewImage(name, content);
            border = tag.gameObject.AddComponent<Outline>();
            border.effectColor = Ink;
            border.effectDistance = new Vector2(2f, -2f);
            var size = tag.gameObject.AddComponent<LayoutElement>();
            size.minWidth = size.preferredWidth = width;
            size.flexibleWidth = flexible ? 1f : 0f;
            return tag;
        }

        static Image NewIcon(Transform parent)
        {
            var icon = NewImage("Icon", parent);
            icon.preserveAspect = true;
            var rt = icon.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(28f, 0f);
            rt.sizeDelta = new Vector2(38f, 38f);
            return icon;
        }

        static RectTransform Body(Transform parent, string name)
        {
            var body = NewRect(name, parent);
            Stretch(body, new Vector2(56f, 6f), new Vector2(-14f, -6f));
            return body;
        }

        static TMP_Text NewText(string name, Transform parent, float size, float minSize, TextAlignmentOptions align)
        {
            var rt = NewRect(name, parent);
            Stretch(rt, Vector2.zero, Vector2.zero);
            var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = text.fontSizeMax = size;
            text.fontSizeMin = minSize;
            text.enableAutoSizing = true;
            text.color = Ink;
            text.alignment = align;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.richText = false; // Müşteri/proje adındaki işaretler TMP etiketi olarak yorumlanmaz.
            text.raycastTarget = false;
            return text;
        }

        static Image NewImage(string name, Transform parent)
        {
            var image = NewRect(name, parent).gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            return rt;
        }

        static void Stretch(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = min;
            rt.offsetMax = max;
        }

#if UNITY_EDITOR
        void Reset()
        {
            const string fonts = "Assets/_Project/Fonts/";
            const string art = "Assets/_Project/Art/";
            if (titleFont == null) titleFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fonts + "PaytoneOne-Regular SDF.asset");
            if (bodyFont == null) bodyFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fonts + "FiraSans-Regular SDF.asset");
            if (barSprite == null) barSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "hud_bar.png");
            if (tagSprite == null) tagSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "hud_tag.png");
            if (dayIcon == null) dayIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "hud_icon_day.png");
            if (moneyIcon == null) moneyIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "hud_icon_money.png");
            if (countIcon == null) countIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(art + "hud_icon_count.png");
            if (content != null) ApplyAppearance();
        }

        [ContextMenu("Önizleme/Üst bar")]
        void Preview()
        {
            Reset();
            Refresh(new GameContext { State = new GameState { Day = 4, Money = 1750, Debt = 600, ApprovedToday = 2 } });
            SetClient("Mahir ve Orhan Yaprak", "Çınar Apartmanı");
        }
#endif
    }
}
