using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk.Editor
{
    // Kağıt pencereler (Yapı Denetim Formu, Lab Raporu, Denetçi El Kitabı): Ahmet'in piksel tasarımı
    // (Docs/Tasarim/*_GORUNUM.html): klasör rengi + 3 px koyu çerçeve, üstte sekme, içinde krem kağıt ve düz gölge,
    // noktalı satırlar, kutulu bölüm harfleri, kesik çizgili karar kutuları (burada düz çerçeve). İçerik El Kitabı v2.
    // Fontlar: başlık Paytone One, metin Fira Sans (EEE, adlarıyla bulunur; yoksa varsayılan font).
    public static partial class DeskSandboxBuilder
    {
        // Tasarım renkleri
        private static readonly Color PaperInk = Hex(0x3b2f22);
        private static readonly Color PaperCream = Hex(0xf1ead8);
        private static readonly Color FormFolderColor = Hex(0xd8b878);
        private static readonly Color FormShadowColor = Hex(0xa88a4e);
        private static readonly Color LabFolderColor = Hex(0xaebdcc);
        private static readonly Color LabShadowColor = Hex(0x7d8fa3);
        private static readonly Color HandbookFolderColor = Hex(0xb9c4a6);
        private static readonly Color HandbookShadowColor = Hex(0x8a9776);

        // Pencere ölçüleri (DeskScreen.SheetWidth ile aynı genişlik)
        private const float SheetW = 680f, SheetH = 990f, Border = 3f, FolderPad = 22f, PaperPad = 24f, TabH = 46f;
        private const float ContentW = SheetW - 2 * (Border + FolderPad + Border + PaperPad); // 576
        private const float RowH = 30f;
        private const float HandbookTabW = 44f; // El Kitabı sekme şeridi (sekme 40 px + pay; EEE tab_* 120x300 oranı)

        private static TMP_FontAsset titleFont, valueFont, boldFont;
        private static bool paperFontsResolved;

        private static TMP_FontAsset TitleFont { get { ResolvePaperFonts(); return titleFont; } }
        private static TMP_FontAsset ValueFont { get { ResolvePaperFonts(); return valueFont; } }
        private static TMP_FontAsset BoldFont { get { ResolvePaperFonts(); return boldFont; } }

        // ---------- Pencereler ----------

        // Yapı Denetim Formu (yeni hedef arayüz): başlık, PROJE BİLGİLERİ (+ bina resmi), TEKNİK BİLGİLER tablosu, belediye kaşesi,
        // zarf ve EEE'nin ONAY / RED düğmeleri. Tasarım boyutu 576x820, doc_form'un yazı alanına ölçeklenir.
        private static PaperWindow BuildFormWindow(Transform papers, out FormView formView)
        {
            var window = BuildPaperWindow(papers, "FormWindow", "doc_form", DocHomes().form, new Vector2(0.11f, 0.06f),
                new Vector2(0.92f, 0.94f), new Vector2(DocDesignW, DocDesignH), null, out var content, out _, out _);
            content.GetComponent<VerticalLayoutGroup>().spacing = 10;

            DocHeader(content, "icon_building", "YAPI DENETİM FORMU", out var fileNo, out var date);

            SectionBar(content, "PROJE BİLGİLERİ");
            var infoRow = NewRect("ProjectInfo", content);
            var infoLayout = HLayout(infoRow, 12, new RectOffset(4, 0, 0, 0));
            infoLayout.childAlignment = TextAnchor.UpperLeft;
            var info = NewRect("Rows", infoRow);
            VLayout(info, 2, new RectOffset(0, 0, 0, 0));
            Layout(info, prefW: 0, flexW: 1);
            var infoTemplate = InfoRow("InfoTemplate", info);
            var building = NewRect("Building", infoRow);
            Layout(building, prefW: 150, prefH: 190);
            var buildingImage = AddImage(building, Color.white);
            buildingImage.preserveAspect = true;
            buildingImage.raycastTarget = false;

            SectionBar(content, "TEKNİK BİLGİLER");
            TableRow("TechHeader", content, true, "Özellik", "Beyan değeri");
            var tech = NewRect("TechRows", content);
            VLayout(tech, 0, new RectOffset(0, 0, 0, 0));
            var techTemplate = TableRow("TechTemplate", tech, false, "Özellik", "Değer").GetComponent<DottedRowView>();
            techTemplate.gameObject.SetActive(false);

            Layout(NewRect("Spacer", content), flexH: 1);

            var bribe = BorderedButton("TakeBribeButton", content, EnvelopeColor, "ZARFI AL VE ONAYLA", 20, out var bribeLabel);
            bribeLabel.font = BoldFont;
            Layout(bribe, prefH: 44);

            var decisions = NewRect("Decisions", content);
            var decisionsLayout = HLayout(decisions, 18, new RectOffset(0, 0, 0, 0));
            decisionsLayout.childForceExpandWidth = true;
            decisionsLayout.childAlignment = TextAnchor.MiddleCenter;
            Layout(decisions, prefH: 92);
            var approve = SpriteButton("ApproveButton", decisions, "btn_approve", "ONAY", ApproveGreen);
            var reject = SpriteButton("RejectButton", decisions, "btn_reject", "RED", RejectRed);

            // Belediye kaşesi: sağ üstte, bina resminin üstüne hafif yamuk basılmış (yerleşim dışı).
            var stamp = ArtStamp("MunicipalStamp", content, "stamp_municipal", new Vector2(120, 120), new Vector2(-6, -86), -10f,
                new Color(0.2f, 0.3f, 0.6f), out var stampText);

            var decisionStamp = BuildDecisionStamp(content); // içerikle birlikte ölçeklenir

            formView = window.gameObject.AddComponent<FormView>();
            Wire(formView, "fileTabText", fileNo);
            Wire(formView, "dateText", date);
            Wire(formView, "infoContainer", info);
            Wire(formView, "infoTemplate", infoTemplate);
            Wire(formView, "buildingImage", buildingImage);
            Wire(formView, "techContainer", tech);
            Wire(formView, "techTemplate", techTemplate);
            Wire(formView, "municipalStamp", stamp.gameObject);
            Wire(formView, "municipalStampText", stampText);
            Wire(formView, "approveButton", approve);
            Wire(formView, "rejectButton", reject);
            Wire(formView, "bribeRoot", bribe.gameObject);
            Wire(formView, "takeBribeButton", bribe);
            Wire(formView, "bribeLabel", bribeLabel);
            Wire(formView, "decisionStamp", decisionStamp);

            var so = new SerializedObject(formView);
            var sprites = so.FindProperty("buildingSprites");
            string[] names = { "bld_low", "bld_mid", "bld_high" };
            sprites.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++) sprites.GetArrayElementAtIndex(i).objectReferenceValue = FindSprite(names[i]);
            so.ApplyModifiedPropertiesWithoutUndo();
            return window;
        }

        // Laboratuvar Raporu (yeni hedef arayüz): başlık, künye, TEST SONUÇLARI tablosu, DEĞERLENDİRME (karar vermez), yetkili + imza,
        // lab kaşesi (EEE görseli) ve altında kaşedeki lab adı, EK TEST düğmesi (EEE görseli) + ücreti.
        private static PaperWindow BuildLabWindow(Transform papers, out LabReportView labView)
        {
            var window = BuildPaperWindow(papers, "LabWindow", "doc_lab", DocHomes().lab, new Vector2(0.08f, 0.06f),
                new Vector2(0.93f, 0.95f), new Vector2(DocDesignW, DocDesignH), null, out var content, out _, out _);
            content.GetComponent<VerticalLayoutGroup>().spacing = 10;

            DocHeader(content, "icon_tube", "LABORATUVAR RAPORU", out var topRight, out var topRight2);
            topRight.text = "";
            topRight2.text = "";

            var info = NewRect("Info", content);
            VLayout(info, 2, new RectOffset(4, 0, 0, 0));
            var infoTemplate = InfoRow("InfoTemplate", info);

            SectionBar(content, "TEST SONUÇLARI");
            var header = ParamRow("TableHeader", content, true);
            header.SetActive(true);
            var list = NewRect("Rows", content);
            VLayout(list, 2, new RectOffset(0, 0, 0, 0));
            var rowTemplate = ParamRow("RowTemplate", list, false);
            var cells = rowTemplate.GetComponentsInChildren<TMP_Text>();
            var rowView = rowTemplate.AddComponent<ParamRowView>();
            Wire(rowView, "nameText", cells[0]);
            Wire(rowView, "reportedText", cells[1]);
            Wire(rowView, "actualText", cells[2]);
            rowTemplate.SetActive(false);

            SectionBar(content, "DEĞERLENDİRME");
            // Karar vermez (Ahmet): her laboratuvar bunu yazar; uygunluğu denetçi el kitabıyla kendisi kontrol eder.
            var evaluation = NewText("Evaluation", content,
                "Numuneler üzerinde yapılan deneylerin sonuçları yukarıda beyan edilmiştir. Yönetmeliğe uygunluğun kontrolü denetçiye aittir.",
                18, PaperInk, TextAlignmentOptions.TopLeft);
            evaluation.font = ValueFont;
            evaluation.textWrappingMode = TextWrappingModes.Normal; // kodla kurulan TMP kaydırmasız gelebiliyor (tek satırda taşıyordu): açıkça aç

            Layout(NewRect("Spacer", content), flexH: 1);

            // Yetkili + imza (solda), kaşe ve altında lab adı (sağda)
            var sign = NewRect("SignatureRow", content);
            var signLayout = HLayout(sign, 10, new RectOffset(4, 0, 0, 0));
            signLayout.childAlignment = TextAnchor.MiddleLeft;
            Layout(sign, prefH: 150);
            var signer = NewRect("Signer", sign);
            VLayout(signer, 4, new RectOffset(0, 0, 0, 0));
            Layout(signer, prefW: 0, flexW: 1);
            var who = NewText("Title", signer, "Mühendis", 18, PaperInk, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            who.font = BoldFont;
            var signature = NewText("Signature", signer, "İmza", 22, PaperInk, TextAlignmentOptions.MidlineLeft);
            signature.font = ValueFont;
            var stampHolder = NewRect("StampHolder", sign);
            Layout(stampHolder, prefW: 200, prefH: 150);
            var stamp = ArtStamp("LabStamp", stampHolder, "stamp_lab", new Vector2(120, 120), new Vector2(-40, 0), 8f, StampRed, out var stampText);

            // EK TEST düğmesi + ücret
            var testRow = NewRect("TestRow", content);
            var testLayout = HLayout(testRow, 14, new RectOffset(0, 0, 0, 0));
            testLayout.childAlignment = TextAnchor.MiddleCenter;
            Layout(testRow, prefH: 76);
            var test = SpriteButton("ExtraTestButton", testRow, "btn_test", "EK TEST", new Color(0.24f, 0.45f, 0.7f));
            Layout(test, prefW: 266, prefH: 76);
            var testLabel = NewText("Cost", testRow, "300 TL", 22, PaperInk, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            testLabel.font = BoldFont;
            Layout(testLabel, prefW: 120);

            labView = window.gameObject.AddComponent<LabReportView>();
            Wire(labView, "infoContainer", info);
            Wire(labView, "infoTemplate", infoTemplate);
            Wire(labView, "paramContainer", list);
            Wire(labView, "paramRowTemplate", rowView);
            Wire(labView, "stampRoot", stamp.gameObject);
            Wire(labView, "stampText", stampText);
            Wire(labView, "signatureText", signature);
            Wire(labView, "extraTestButton", test);
            Wire(labView, "extraTestLabel", testLabel);
            return window;
        }

        // ---------- Belge parçaları (yeni hedef arayüz) ----------

        private const float DocDesignW = 576f, DocDesignH = 820f; // form ve lab raporu iç tasarım boyutu (belgeye ölçeklenir)

        // Belge başlığı: solda ikon, büyük başlık; sağda iki satır künye (ör. "Proje : YD-…" / "Tarih : …"); altında çizgi.
        private static void DocHeader(Transform content, string icon, string title, out TMP_Text right1, out TMP_Text right2)
        {
            var row = NewRect("Header", content);
            var layout = HLayout(row, 10, new RectOffset(0, 0, 0, 4));
            layout.childAlignment = TextAnchor.MiddleLeft;
            Layout(row, prefH: 70);
            var iconRect = NewRect("Icon", row);
            Layout(iconRect, prefW: 60, prefH: 60);
            var iconImage = SpriteImage(iconRect, FindSprite(icon), Color.clear);
            iconImage.preserveAspect = true;
            var t = NewText("Title", row, title, 30, PaperInk, TextAlignmentOptions.MidlineLeft);
            t.font = TitleFont;
            t.enableAutoSizing = true;
            t.fontSizeMin = 20;
            t.fontSizeMax = 30;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            Layout(t, prefW: 0, flexW: 1);
            var right = NewRect("Right", row);
            VLayout(right, 0, new RectOffset(0, 0, 0, 0));
            Layout(right, prefW: 190);
            right1 = NewText("Line1", right, "Proje", 17, PaperInk, TextAlignmentOptions.MidlineRight);
            right1.font = ValueFont;
            right2 = NewText("Line2", right, "Tarih", 17, PaperInk, TextAlignmentOptions.MidlineRight);
            right2.font = ValueFont;
            Divider(content, 2);
        }

        // Bölüm başlığı şeridi: "PROJE BİLGİLERİ" (açık kahve zemin, kalın).
        private static void SectionBar(Transform content, string title)
        {
            var bar = NewRect("Bar_" + title, content);
            AddImage(bar, new Color(PaperInk.r, PaperInk.g, PaperInk.b, 0.12f));
            Layout(bar, prefH: 28);
            var t = NewText("Text", bar, title, 17, PaperInk, TextAlignmentOptions.MidlineLeft);
            t.font = BoldFont;
            t.characterSpacing = 3;
            Stretch(t.rectTransform).offsetMin = new Vector2(8, 0);
        }

        // "Etiket  :  değer" satırı (DottedRowView: labelText, valueText). parent verilirse şablon olarak gizlenir.
        private static DottedRowView InfoRow(string name, Transform parent)
        {
            var row = NewRect(name, parent);
            var layout = HLayout(row, 4, new RectOffset(0, 0, 0, 0));
            layout.childForceExpandHeight = true;
            Layout(row, prefH: 24);
            var label = NewText("Label", row, "Etiket", 17, PaperInk, TextAlignmentOptions.MidlineLeft);
            label.font = BoldFont;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            Layout(label, prefW: 150);
            var colon = NewText("Colon", row, ":", 17, PaperInk, TextAlignmentOptions.Midline);
            colon.font = ValueFont;
            Layout(colon, prefW: 10);
            var value = NewText("Value", row, "Değer", 17, PaperInk, TextAlignmentOptions.MidlineLeft);
            value.font = ValueFont;
            value.textWrappingMode = TextWrappingModes.NoWrap;
            value.overflowMode = TextOverflowModes.Ellipsis;
            Layout(value, prefW: 0, flexW: 1);

            var view = row.gameObject.AddComponent<DottedRowView>();
            Wire(view, "labelText", label);
            Wire(view, "valueText", value);
            row.gameObject.SetActive(false);
            return view;
        }

        // İki sütunlu tablo satırı (başlıkta zemin rengi; her satırın altında ince çizgi). Satır şablonu DottedRowView taşır.
        private static GameObject TableRow(string name, Transform parent, bool header, string a, string b)
        {
            var row = NewRect(name, parent);
            if (header) AddImage(row, new Color(PaperInk.r, PaperInk.g, PaperInk.b, 0.08f));
            var layout = HLayout(row, 8, new RectOffset(8, 8, 0, 0));
            layout.childForceExpandHeight = true;
            Layout(row, prefH: header ? 28 : 30);
            var left = NewText("A", row, a, header ? 16 : 18, PaperInk, TextAlignmentOptions.MidlineLeft);
            left.font = header ? BoldFont : ValueFont;
            Layout(left, prefW: 0, flexW: 1.4f);
            var right = NewText("B", row, b, header ? 16 : 18, PaperInk, TextAlignmentOptions.Midline);
            right.font = BoldFont;
            Layout(right, prefW: 0, flexW: 1);
            var line = NewRect("Line", row);
            line.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            line.anchorMin = Vector2.zero;
            line.anchorMax = new Vector2(1, 0);
            line.sizeDelta = new Vector2(0, 1);
            line.pivot = new Vector2(0.5f, 0);
            AddImage(line, new Color(PaperInk.r, PaperInk.g, PaperInk.b, 0.35f)).raycastTarget = false;
            if (!header)
            {
                var view = row.gameObject.AddComponent<DottedRowView>();
                Wire(view, "labelText", left);
                Wire(view, "valueText", right);
            }
            return row.gameObject;
        }

        // EEE'nin görsel düğmesi (yazı görselde). Görsel yoksa aynı yazıyla renkli düğme.
        private static Button SpriteButton(string name, Transform parent, string sprite, string fallbackLabel, Color fallback)
        {
            var art = FindSprite(sprite);
            var rt = NewRect(name, parent);
            Layout(rt, prefW: 0, flexW: 1, prefH: 84);
            var image = AddImage(rt, art != null ? Color.white : fallback);
            image.sprite = art;
            image.preserveAspect = true;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.55f);
            button.colors = colors;
            if (art == null)
            {
                var t = NewText("Label", rt, fallbackLabel, 28, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
                t.font = TitleFont;
                Stretch(t.rectTransform);
            }
            return button;
        }

        // Görsel kaşe (EEE: stamp_lab / stamp_municipal) + altında kaşedeki metin (StampId; sahte belge ipucu lab adı burada).
        // Yerleşim dışı, ebeveynin sağ üstüne göre offset; hafif yamuk.
        private static RectTransform ArtStamp(string name, Transform parent, string sprite, Vector2 size, Vector2 offset, float angle,
            Color ink, out TMP_Text text)
        {
            var stamp = NewRect(name, parent);
            stamp.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            stamp.anchorMin = stamp.anchorMax = stamp.pivot = new Vector2(1f, 1f);
            stamp.sizeDelta = size;
            stamp.anchoredPosition = offset;
            stamp.localRotation = Quaternion.Euler(0, 0, angle);
            var image = SpriteImage(stamp, FindSprite(sprite), new Color(ink.r, ink.g, ink.b, 0.15f));
            image.preserveAspect = true;
            text = NewText("Label", stamp, "KAŞE", 13, ink, TextAlignmentOptions.Top, FontStyles.Bold);
            text.font = BoldFont;
            text.enableAutoSizing = true;
            text.fontSizeMin = 8;
            text.fontSizeMax = 13;
            var tr = text.rectTransform;
            tr.anchorMin = new Vector2(0.5f, 0f);
            tr.anchorMax = new Vector2(0.5f, 0f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(size.x * 1.5f, 30);
            tr.anchoredPosition = new Vector2(0, -2);
            return stamp;
        }

        // Denetçi El Kitabı ("Hızlı Bakış", Ahmet onayı): sayfalar ve sekmeler çalışırken kurallardan kurulur (RulebookView + HandbookUi).
        // Builder sadece kabuğu kurar: kağıt pencere, sayfa alanı, sağ kenarda sekme şeridi; fontlar ve bina simgeleri bağlanır.
        private static RulebookView BuildRulebook(Transform papers, Button openButton)
        {
            // doc_handbook: spiralin sağındaki sayfa; sekmeler defterin sağ kenarından dışarı çıkar.
            var window = BuildPaperWindow(papers, "RulebookWindow", "doc_handbook", DocHomes().handbook, new Vector2(0.13f, 0.05f),
                new Vector2(0.93f, 0.96f), new Vector2(540, 720), null, out var content, out var paper, out _);

            // Sekmeler: defterin sağ kenarından dışarı taşar, sol kenarları defterin altına girer (EEE) → görselin arkasında.
            var sheet = (RectTransform)paper.parent;
            var tabs = NewRect("Tabs", sheet);
            tabs.SetSiblingIndex(1); // gölgenin önü, defter görselinin arkası
            tabs.anchorMin = tabs.anchorMax = new Vector2(1f, 0.5f);
            tabs.pivot = new Vector2(0f, 0.5f);
            tabs.sizeDelta = new Vector2(HandbookTabW, DocH - 60);
            tabs.anchoredPosition = new Vector2(-(HandbookTabW - DocTabOut), 0);
            var tabsLayout = tabs.gameObject.AddComponent<VerticalLayoutGroup>();
            tabsLayout.spacing = 6;
            tabsLayout.childAlignment = TextAnchor.UpperCenter;
            tabsLayout.childControlWidth = tabsLayout.childControlHeight = true;
            tabsLayout.childForceExpandWidth = true;
            tabsLayout.childForceExpandHeight = false;

            var view = window.gameObject.AddComponent<RulebookView>();
            Wire(view, "openButton", openButton);
            Wire(view, "window", window);
            Wire(view, "pageRoot", content);
            Wire(view, "tabsRoot", tabs);
            Wire(view, "titleFont", TitleFont);
            Wire(view, "textFont", ValueFont);
            Wire(view, "boldFont", BoldFont);

            var so = new SerializedObject(view);
            var icons = so.FindProperty("floorIcons");
            string[] names = { "bld_low", "bld_mid", "bld_high" };
            icons.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++) icons.GetArrayElementAtIndex(i).objectReferenceValue = FindSprite(names[i]);
            var tabSprites = so.FindProperty("tabSprites");
            string[] tabNames = { "tab_beton", "tab_donati", "tab_yapi", "tab_zemin" };
            tabSprites.arraySize = tabNames.Length;
            for (int i = 0; i < tabNames.Length; i++) tabSprites.GetArrayElementAtIndex(i).objectReferenceValue = FindSprite(tabNames[i]);
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        // Hediye kartı (kağıtlarla aynı piksel tasarım): hafif perde + ortada kart; solda hediyenin görseli, sağda
        // "HEDİYE", "<müteahhit> size bir <eşya> bıraktı.", notu ve sadece KABUL ET. Açılış/kapanış PopupMotion ("Card").
        private static GiftOfferView BuildGiftOffer(Transform parent)
        {
            var panel = Stretch(NewRect("GiftOffer", parent));
            AddImage(panel, new Color(0, 0, 0, 0.35f)); // perde: kabul edilene kadar masaya tıklanmasın

            var card = NewRect("Card", panel);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(660, 300);
            card.anchoredPosition = new Vector2(0, 60);
            card.localRotation = Quaternion.Euler(0, 0, -1.5f);
            var inner = Bordered(card, PaperCream);
            var row = HLayout(inner, 24, new RectOffset(26, 26, 24, 24));
            row.childAlignment = TextAnchor.MiddleLeft;

            var picture = NewRect("Picture", inner);
            Layout(picture, prefW: 210, prefH: 210);
            var image = AddImage(picture, Color.white);
            image.preserveAspect = true;
            image.raycastTarget = false;

            var text = NewRect("Text", inner);
            VLayout(text, 10, new RectOffset(0, 0, 0, 0));
            Layout(text, prefW: 0, flexW: 1);
            Small(text, "HEDİYE");
            var body = NewText("Body", text, "Usta size bir hediye bıraktı.", 26, PaperInk, TextAlignmentOptions.TopLeft);
            body.font = TitleFont;
            body.textWrappingMode = TextWrappingModes.Normal;
            var note = NewText("Note", text, "Not", 22, new Color(PaperInk.r, PaperInk.g, PaperInk.b, 0.8f), TextAlignmentOptions.TopLeft);
            note.font = ValueFont;
            note.textWrappingMode = TextWrappingModes.Normal;
            Layout(NewRect("Spacer", text), flexH: 1);
            var accept = BorderedButton("AcceptButton", text, FormFolderColor, "KABUL ET", 24, out _);
            Layout(accept, prefH: 52);

            panel.gameObject.SetActive(false);

            // Bileşen panele değil, hep aktif ayrı bir objeye konur: pasif objede Awake çalışmaz.
            var holder = Stretch(NewRect("GiftOfferController", parent));
            var view = holder.gameObject.AddComponent<GiftOfferView>();
            Wire(view, "panelRoot", panel.gameObject);
            Wire(view, "giftImage", image);
            Wire(view, "bodyText", body);
            Wire(view, "noteText", note);
            Wire(view, "acceptButton", accept);
            return view;
        }

        // ---------- Yeni hedef arayüz: masadaki üç belge ----------
        // Belgeler masada hep açık, düz ve yan yana (soldan form, lab raporu, El Kitabı); masa kenarının hemen altından başlar.
        // Kabuk EEE'nin boş belge görseli (doc_form / doc_lab / doc_handbook); yazılar kodla, kağıdın yazı alanına ölçeklenerek oturur.
        private const float DocH = 640f;     // belge yüksekliği (genişlik görselin oranından)
        private const float DocY = -200f;    // belgelerin ortası (tuval ortasına göre): üst kenar ~y 420
        private const float DocGap = 40f;
        private const float DocTabOut = 38f; // El Kitabı sekmelerinin defterden dışarı taşan kısmı

        private static float DocWidth(string sprite)
        {
            var art = FindSprite(sprite);
            return art != null ? DocH * art.rect.width / art.rect.height : DocH * 0.82f;
        }

        // Üç belgenin masadaki yerleri (x), toplam genişliğe göre ortalanmış.
        private static (float form, float lab, float handbook) DocHomes()
        {
            float wForm = DocWidth("doc_form"), wLab = DocWidth("doc_lab"), wBook = DocWidth("doc_handbook");
            float total = wForm + wLab + wBook + DocTabOut + 2 * DocGap;
            float left = -total * 0.5f;
            return (left + wForm * 0.5f,
                    left + wForm + DocGap + wLab * 0.5f,
                    left + wForm + DocGap + wLab + DocGap + wBook * 0.5f);
        }

        // Kök (PaperWindow, hep aktif) → Panel (açılıp kapanır) → Sheet (belge görseli; masadaki yerinde açılır, sürüklenebilir).
        // inner: görselde yazı alanı (0-1 oran). content: o alana ölçeklenmiş tasarım boyutlu iç alan (VLayout);
        // paper: yazı alanının kendisi (yerleşim dışı katmanlar için); tab verilirse içeriğin en üstünde küçük künye yazısı.
        private static PaperWindow BuildPaperWindow(Transform papers, string name, string sprite, float homeX, Vector2 innerMin,
            Vector2 innerMax, Vector2 design, string tab, out RectTransform content, out RectTransform paper, out TMP_Text tabText)
        {
            var root = Stretch(NewRect(name, papers));
            var panel = Stretch(NewRect("Panel", root));

            var size = new Vector2(DocWidth(sprite), DocH);
            var sheet = NewRect("Sheet", panel);
            sheet.anchorMin = sheet.anchorMax = sheet.pivot = new Vector2(0.5f, 0.5f);
            sheet.sizeDelta = size;
            sheet.anchoredPosition = new Vector2(homeX, DocY);
            var draggable = AddSheet(sheet, root, 0.18f);

            var doc = Stretch(NewRect("Doc", sheet));
            SpriteImage(doc, FindSprite(sprite), PaperCream).raycastTarget = true;

            paper = Anchor(NewRect("Paper", sheet), innerMin, innerMax, Vector2.zero, Vector2.zero);
            var area = new Vector2(size.x * (innerMax.x - innerMin.x), size.y * (innerMax.y - innerMin.y));
            float scale = Mathf.Min(area.x / design.x, area.y / design.y);
            content = NewRect("Content", paper);
            content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = design;
            content.localScale = Vector3.one * scale;
            VLayout(content, 14, new RectOffset(0, 0, 0, 0));

            tabText = null;
            if (tab != null)
            {
                tabText = NewText("FileTab", content, tab, 17, PaperInk, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
                tabText.font = BoldFont;
                tabText.characterSpacing = 2;
                Layout(tabText, prefH: 20);
            }

            panel.gameObject.SetActive(false);

            // Bileşen hep aktif köke konur: pasif objede Awake çalışmaz.
            var window = root.gameObject.AddComponent<PaperWindow>();
            Wire(window, "panelRoot", panel.gameObject);
            Wire(window, "sheet", draggable);
            var so = new SerializedObject(window);
            so.FindProperty("fromOffset").vector2Value = new Vector2(0, -760); // aşağıdan kayarak gelir
            so.FindProperty("home").vector2Value = new Vector2(homeX, DocY);
            so.ApplyModifiedPropertiesWithoutUndo();
            return window;
        }

        // Kağıdı sürüklenebilir/animasyonlu yapar: saydamlık için CanvasGroup, arkasında yumuşak gölge, öne alınacak kök.
        private static DraggableSheet AddSheet(RectTransform rt, Transform layer, float shadowAlpha = 0.35f)
        {
            var group = rt.gameObject.AddComponent<CanvasGroup>();
            var shadow = Stretch(NewRect("Shadow", rt));
            shadow.SetAsFirstSibling();
            AddImage(shadow, new Color(0, 0, 0, shadowAlpha)).raycastTarget = false;
            shadow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var sheet = rt.gameObject.AddComponent<DraggableSheet>();
            Wire(sheet, "layer", layer);
            Wire(sheet, "shadow", shadow);
            Wire(sheet, "group", group);
            return sheet;
        }

        // ---------- Parçalar ----------

        // "AFET KONTROL BÜROSU / YAPI DENETİM FORMU" + sağda tarih; altında 3 px çizgi. Tarih yazısını döner.
        private static TMP_Text Header(Transform content, string small, string title) => Header(content, small, title, out _);

        private static TMP_Text Header(Transform content, string small, string title, out TMP_Text smallText)
        {
            var row = NewRect("Header", content);
            var layout = HLayout(row, 10, new RectOffset(0, 0, 0, 8));
            layout.childAlignment = TextAnchor.LowerLeft;
            var titles = NewRect("Titles", row);
            VLayout(titles, 0, new RectOffset(0, 0, 0, 0));
            Layout(titles, flexW: 1);
            smallText = Small(titles, small);
            Title(titles, title);
            var date = NewText("Date", row, "01.01.2026", 26, PaperInk, TextAlignmentOptions.BottomRight);
            date.font = ValueFont;
            date.textWrappingMode = TextWrappingModes.NoWrap;
            Divider(content, Border);
            return date;
        }

        private static TMP_Text Small(Transform parent, string text)
        {
            var t = NewText("Small", parent, text, 15, PaperInk, TextAlignmentOptions.TopLeft);
            t.font = TitleFont;
            t.characterSpacing = 6;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            return t;
        }

        private static TMP_Text Title(Transform parent, string text)
        {
            var t = NewText("Title", parent, text, 34, PaperInk, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            t.font = TitleFont;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.enableAutoSizing = true;
            t.fontSizeMin = 22;
            t.fontSizeMax = 34;
            return t;
        }

        private static void Divider(Transform parent, float thickness)
        {
            var line = NewRect("Divider", parent);
            AddImage(line, PaperInk).raycastTarget = true; // sürükleme kağıttan da başlayabilsin
            Layout(line, prefH: thickness);
        }

        // "Etiket ............ DEĞER" satırı. parent null ise şablon olarak döner (çağıran yerleştirir).
        private static DottedRowView DottedRow(string name, Transform parent)
        {
            var row = NewRect(name, parent);
            var layout = HLayout(row, 6, new RectOffset(0, 0, 0, 0));
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childForceExpandHeight = true;
            Layout(row, prefH: RowH);

            var label = NewText("Label", row, "Etiket", 19, PaperInk, TextAlignmentOptions.BottomLeft);
            label.font = TitleFont;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            var dots = NewText("Dots", row, new string('.', 160), 19, PaperInk, TextAlignmentOptions.BottomLeft);
            dots.font = TitleFont;
            dots.textWrappingMode = TextWrappingModes.NoWrap;
            dots.overflowMode = TextOverflowModes.Truncate;
            Layout(dots, prefW: 0, flexW: 1);
            var value = NewText("Value", row, "DEĞER", 25, PaperInk, TextAlignmentOptions.BottomRight);
            value.font = ValueFont;
            value.textWrappingMode = TextWrappingModes.NoWrap;

            var view = row.gameObject.AddComponent<DottedRowView>();
            Wire(view, "labelText", label);
            Wire(view, "valueText", value);
            if (parent != null) row.gameObject.SetActive(false);
            return view;
        }

        // Lab raporu tablo satırı: Parametre | Beyan | Ek test.
        private static GameObject ParamRow(string name, Transform parent, bool isHeader)
        {
            var row = NewRect(name, parent);
            var layout = HLayout(row, 10, new RectOffset(0, 0, 0, 0));
            layout.childForceExpandHeight = true;
            Layout(row, prefH: isHeader ? 30 : 34);

            string[] labels = isHeader ? new[] { "PARAMETRE", "BEYAN", "EK TEST" } : new[] { "Parametre", "0", "?" };
            float[] widths = { 2, 1, 1 };
            for (int i = 0; i < labels.Length; i++)
            {
                bool value = !isHeader && i > 0;
                var cell = NewText("Cell" + i, row, labels[i], isHeader ? 16 : (value ? 25 : 20), PaperInk,
                    i == 0 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Midline, isHeader ? FontStyles.Bold : FontStyles.Normal);
                cell.font = value ? ValueFont : TitleFont;
                if (isHeader) cell.characterSpacing = 4;
                cell.textWrappingMode = TextWrappingModes.NoWrap;
                Layout(cell, prefW: 0, flexW: widths[i]);
            }
            return row.gameObject;
        }

        // ONAY / RED: çerçeveli büyük kutu; üzerine gelince hafif yeşil/kırmızı.
        private static Button DecisionBox(string name, Transform parent, string label, Color highlight)
        {
            var button = BorderedButton(name, parent, PaperCream, label, 28, out var text);
            text.characterSpacing = 10;
            var colors = button.colors;
            colors.highlightedColor = highlight;
            colors.pressedColor = highlight * 0.9f;
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.6f);
            button.colors = colors;
            return button;
        }

        // Çerçeveli düğme: dış koyu çizgi, iç dolgu (hedef grafik), ortada başlık fontuyla yazı.
        private static Button BorderedButton(string name, Transform parent, Color fill, string label, float size, out TMP_Text text)
        {
            var rt = NewRect(name, parent);
            var inner = Bordered(rt, fill);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = inner.GetComponent<Image>();
            text = NewText("Label", inner, label, size, PaperInk, TextAlignmentOptions.Center, FontStyles.Bold);
            text.font = TitleFont;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            Stretch(text.rectTransform);
            return button;
        }

        // Mürekkep kaşesi (belediye / laboratuvar): kırmızı çerçeve + soluk dolgu, yazı kutuya sığar. Yerleşim dışı, satırın sağında.
        private static RectTransform InkStamp(string name, Transform parent, Vector2 size, Vector2 offset, float angle, out TMP_Text text)
        {
            var stamp = NewRect(name, parent);
            stamp.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            stamp.anchorMin = stamp.anchorMax = stamp.pivot = new Vector2(1f, 0f);
            stamp.sizeDelta = size;
            stamp.anchoredPosition = offset;
            stamp.localRotation = Quaternion.Euler(0, 0, angle);
            var image = AddImage(stamp, new Color(StampRed.r, StampRed.g, StampRed.b, 0.1f));
            image.raycastTarget = false;
            var outline = stamp.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(StampRed.r, StampRed.g, StampRed.b, 0.8f);
            outline.effectDistance = new Vector2(3, -3);
            text = NewText("Label", stamp, "KAŞE", 24, StampRed, TextAlignmentOptions.Center, FontStyles.Bold);
            text.font = TitleFont;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = 24;
            Stretch(text.rectTransform).offsetMin = new Vector2(10, 6);
            text.rectTransform.offsetMax = new Vector2(-10, -6);
            return stamp;
        }

        // ONAYLANDI / REDDEDİLDİ: kağıdın ortasında, en üstte, karar verilince görünür (DecisionStampView).
        private static DecisionStampView BuildDecisionStamp(RectTransform paper)
        {
            var holder = Stretch(NewRect("DecisionStampLayer", paper));
            holder.gameObject.AddComponent<LayoutElement>().ignoreLayout = true; // içerik VLayout'unda yer kaplamaz
            var stamp = NewRect("DecisionStamp", holder);
            stamp.anchorMin = stamp.anchorMax = stamp.pivot = new Vector2(0.5f, 0.5f);
            stamp.sizeDelta = new Vector2(520, 140);
            stamp.anchoredPosition = new Vector2(0, -40);
            stamp.localRotation = Quaternion.Euler(0, 0, -8);
            var image = AddImage(stamp, new Color(StampRed.r, StampRed.g, StampRed.b, 0.12f));
            image.raycastTarget = false;
            var label = NewText("Label", stamp, "ONAYLANDI", 80, StampRed, TextAlignmentOptions.Center, FontStyles.Bold);
            label.font = TitleFont;
            label.enableAutoSizing = true;
            label.fontSizeMin = 30;
            label.fontSizeMax = 80;
            Stretch(label.rectTransform);
            stamp.gameObject.SetActive(false);

            var view = holder.gameObject.AddComponent<DecisionStampView>();
            Wire(view, "stamp", stamp);
            Wire(view, "stampImage", image);
            Wire(view, "stampText", label);
            return view;
        }

        // Dış koyu çizgi (3 px) + iç dolgu. Dolgunun RectTransform'unu döner.
        private static RectTransform Bordered(RectTransform rt, Color fill)
        {
            AddImage(rt, PaperInk);
            var inner = Anchor(NewRect("Fill", rt), Vector2.zero, Vector2.one, new Vector2(Border, Border), new Vector2(-Border, -Border));
            AddImage(inner, fill);
            return inner;
        }

        // EEE'nin TMP font asset'leri adlarıyla bulunur (17 Ekim: başlık Paytone One, metin Fira Sans; Pixelify/VT323 iptal); yoksa varsayılan font.
        private static void ResolvePaperFonts()
        {
            if (paperFontsResolved) return;
            paperFontsResolved = true;
            titleFont = FindFont("PaytoneOne") ?? font;
            valueFont = FindFont("FiraSans-Regular") ?? font;
            boldFont = FindFont("FiraSans-Bold") ?? valueFont;
        }

        private static TMP_FontAsset FindFont(string namePart)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path).IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
            return null;
        }

        private static Color Hex(int rgb) =>
            new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f);
    }
}
