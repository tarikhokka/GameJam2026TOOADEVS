using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk.Editor
{
    // Kağıt pencereler (Yapı Denetim Formu, Lab Raporu, Denetçi El Kitabı): Ahmet'in piksel tasarımı
    // (Docs/Tasarim/*_GORUNUM.html): klasör rengi + 3 px koyu çerçeve, üstte sekme, içinde krem kağıt ve düz gölge,
    // noktalı satırlar, kutulu bölüm harfleri, kesik çizgili karar kutuları (burada düz çerçeve). İçerik El Kitabı v2.
    // Fontlar: başlık/etiket Pixelify Sans, değerler VT323 (EEE ekleyince adlarıyla bulunur; yoksa varsayılan font).
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

        private static TMP_FontAsset titleFont, valueFont;
        private static bool paperFontsResolved;

        private static TMP_FontAsset TitleFont { get { ResolvePaperFonts(); return titleFont; } }
        private static TMP_FontAsset ValueFont { get { ResolvePaperFonts(); return valueFont; } }

        // ---------- Pencereler ----------

        private static PaperWindow BuildFormWindow(Transform papers, out FormView formView)
        {
            var window = BuildPaperWindow(papers, "FormWindow", FormFolderColor, FormShadowColor, "DOSYA · 2026-0000",
                new Vector2(40, -240), out var content, out var paper, out var fileTab); // masadaki form klasörü (isimliğin arkası)

            var date = Header(content, "AFET KONTROL BÜROSU", "YAPI DENETİM FORMU");

            var wide = NewRect("WideRows", content);
            VLayout(wide, 6, new RectOffset(0, 0, 0, 0));
            var wideTemplate = DottedRow("WideRowTemplate", wide);

            var grid = NewRect("GridRows", content);
            var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2((ContentW - 20) * 0.5f, RowH);
            gridLayout.spacing = new Vector2(20, 6);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            var gridTemplate = DottedRow("GridRowTemplate", grid);

            var sections = NewRect("Sections", content);
            VLayout(sections, 14, new RectOffset(0, 0, 4, 0));
            var sectionTemplate = SectionBlock("SectionTemplate", sections, DottedRow("RowTemplate", null));

            Layout(NewRect("Spacer", content), flexH: 1);

            // İmza + belediye kaşesi (kaşe yerleşim dışı, satırın sağında, hafif yamuk)
            var signRow = NewRect("SignatureRow", content);
            Layout(signRow, prefH: 70);
            var signature = NewText("Signature", signRow, "İmza", 24, PaperInk, TextAlignmentOptions.BottomLeft);
            signature.font = ValueFont;
            Stretch(signature.rectTransform);
            var stamp = InkStamp("MunicipalStamp", signRow, new Vector2(250, 76), new Vector2(-10, 4), 8f, out var stampText);

            // ONAY / RED kutuları ve zarf
            var decisions = NewRect("Decisions", content);
            var decisionsLayout = HLayout(decisions, 14, new RectOffset(0, 0, 0, 0));
            decisionsLayout.childForceExpandWidth = true;
            Layout(decisions, prefH: 76);
            var approve = DecisionBox("ApproveBox", decisions, "ONAY", new Color(0.82f, 1f, 0.82f));
            var reject = DecisionBox("RejectBox", decisions, "RED", new Color(1f, 0.82f, 0.82f));

            var bribe = BorderedButton("TakeBribeButton", content, EnvelopeColor, "ZARFI AL VE ONAYLA", 20, out var bribeLabel);
            Layout(bribe, prefH: 46);

            // Karar kaşesi: kağıdın ortasında, en üstte (yerleşim dışı)
            var decisionStamp = BuildDecisionStamp(paper);

            formView = window.gameObject.AddComponent<FormView>();
            Wire(formView, "fileTabText", fileTab);
            Wire(formView, "dateText", date);
            Wire(formView, "wideRowsContainer", wide);
            Wire(formView, "wideRowTemplate", wideTemplate);
            Wire(formView, "gridRowsContainer", grid);
            Wire(formView, "gridRowTemplate", gridTemplate);
            Wire(formView, "sectionsContainer", sections);
            Wire(formView, "sectionTemplate", sectionTemplate);
            Wire(formView, "signatureText", signature);
            Wire(formView, "municipalStamp", stamp.gameObject);
            Wire(formView, "municipalStampText", stampText);
            Wire(formView, "approveButton", approve);
            Wire(formView, "rejectButton", reject);
            Wire(formView, "bribeRoot", bribe.gameObject);
            Wire(formView, "takeBribeButton", bribe);
            Wire(formView, "bribeLabel", bribeLabel);
            Wire(formView, "decisionStamp", decisionStamp);
            return window;
        }

        private static PaperWindow BuildLabWindow(Transform papers, out LabReportView labView)
        {
            var window = BuildPaperWindow(papers, "LabWindow", LabFolderColor, LabShadowColor, "LAB RAPORU",
                new Vector2(-480, -380), out var content, out _, out _); // LAB RAPORU kitabı (sol)

            var date = Header(content, "LABORATUVAR", "LABORATUVAR RAPORU", out var labName);

            var info = NewRect("Info", content);
            VLayout(info, 6, new RectOffset(0, 0, 0, 0));
            var infoTemplate = DottedRow("InfoTemplate", info);

            // Tablo: Parametre | Beyan | Ek test
            var header = ParamRow("TableHeader", content, true);
            Divider(content, 2);
            var list = NewRect("Rows", content);
            VLayout(list, 4, new RectOffset(0, 0, 0, 0));
            var rowTemplate = ParamRow("RowTemplate", list, false);
            var cells = rowTemplate.GetComponentsInChildren<TMP_Text>();
            var rowView = rowTemplate.AddComponent<ParamRowView>();
            Wire(rowView, "nameText", cells[0]);
            Wire(rowView, "reportedText", cells[1]);
            Wire(rowView, "actualText", cells[2]);
            rowTemplate.SetActive(false);
            header.SetActive(true);

            Layout(NewRect("Spacer", content), flexH: 1);

            var signRow = NewRect("SignatureRow", content);
            Layout(signRow, prefH: 70);
            var signature = NewText("Signature", signRow, "Raporu düzenleyen", 22, PaperInk, TextAlignmentOptions.BottomLeft);
            signature.font = ValueFont;
            Stretch(signature.rectTransform);
            var stamp = InkStamp("LabStamp", signRow, new Vector2(270, 76), new Vector2(-10, 4), -7f, out var stampText);

            var test = BorderedButton("ExtraTestButton", content, LabFolderColor, "EK TEST İSTE", 22, out var testLabel);
            Layout(test, prefH: 56);

            labView = window.gameObject.AddComponent<LabReportView>();
            Wire(labView, "labNameText", labName);
            Wire(labView, "dateText", date);
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

        private static RulebookView BuildRulebook(Transform papers, Button openButton)
        {
            var window = BuildPaperWindow(papers, "RulebookWindow", HandbookFolderColor, HandbookShadowColor, "EL KİTABI · BASKI 3",
                new Vector2(600, -380), out var content, out _, out _); // EL KİTABI (sağ)

            var titles = NewRect("Titles", content);
            VLayout(titles, 2, new RectOffset(0, 0, 0, 8));
            Small(titles, "AFET KONTROL BÜROSU");
            Title(titles, "DENETÇİ EL KİTABI");
            var subtitle = NewText("Subtitle", titles, "Formdaki değerleri bu kurallarla karşılaştır.", 24, PaperInk, TextAlignmentOptions.TopLeft);
            subtitle.font = ValueFont;
            Divider(content, Border);

            // Sayfa 1: bölümler + GENEL KURAL
            var rulesPage = NewRect("RulesPage", content);
            VLayout(rulesPage, 16, new RectOffset(0, 0, 0, 0));
            Layout(rulesPage, flexH: 1);
            var sections = NewRect("Sections", rulesPage);
            VLayout(sections, 18, new RectOffset(0, 0, 0, 0));
            var sectionTemplate = SectionBlock("SectionTemplate", sections, RuleRow("RuleTemplate", null));
            Layout(NewRect("Spacer", rulesPage), flexH: 1);
            var general = NewRect("GeneralRule", rulesPage);
            var generalInner = Bordered(general, PaperCream);
            VLayout(generalInner, 2, new RectOffset(16, 16, 10, 12));
            Layout(general, prefH: 84);
            var generalTitle = NewText("Title", generalInner, "GENEL KURAL", 18, PaperInk, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            generalTitle.font = TitleFont;
            generalTitle.characterSpacing = 6;
            var generalBody = NewText("Body", generalInner, "Tek bir kurala uymayan bina REDDEDİLİR.", 26, PaperInk, TextAlignmentOptions.TopLeft);
            generalBody.font = ValueFont;

            // Sayfa 2: nasıl kullanılır + belge kontrolü + akredite laboratuvarlar
            var notesPage = NewRect("NotesPage", content);
            VLayout(notesPage, 0, new RectOffset(0, 0, 0, 0));
            Layout(notesPage, flexH: 1);
            var notes = NewText("Notes", notesPage, "Notlar", 22, PaperInk, TextAlignmentOptions.TopLeft);
            notes.font = TitleFont;
            notes.enableAutoSizing = true;
            notes.fontSizeMin = 14;
            notes.fontSizeMax = 22;
            Layout(notes, flexH: 1);
            notesPage.gameObject.SetActive(false);

            // Sayfa çevirme
            var footer = NewRect("Footer", content);
            var footerLayout = HLayout(footer, 12, new RectOffset(0, 0, 0, 0));
            footerLayout.childAlignment = TextAnchor.MiddleCenter;
            Layout(footer, prefH: 44);
            var prev = BorderedButton("PrevButton", footer, PaperCream, "<", 24, out _);
            Layout(prev, prefW: 64, prefH: 44);
            var page = NewText("Page", footer, "1 / 2", 24, PaperInk, TextAlignmentOptions.Center);
            page.font = ValueFont;
            Layout(page, prefW: 120, prefH: 44);
            var next = BorderedButton("NextButton", footer, PaperCream, ">", 24, out _);
            Layout(next, prefW: 64, prefH: 44);

            var view = window.gameObject.AddComponent<RulebookView>();
            Wire(view, "openButton", openButton);
            Wire(view, "window", window);
            Wire(view, "rulesPage", rulesPage.gameObject);
            Wire(view, "sectionsContainer", sections);
            Wire(view, "sectionTemplate", sectionTemplate);
            Wire(view, "notesPage", notesPage.gameObject);
            Wire(view, "notesText", notes);
            Wire(view, "pageText", page);
            Wire(view, "prevButton", prev);
            Wire(view, "nextButton", next);
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
            var note = NewText("Note", text, "Not", 22, new Color(PaperInk.r, PaperInk.g, PaperInk.b, 0.8f), TextAlignmentOptions.TopLeft);
            note.font = ValueFont;
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

        // Kök (PaperWindow, hep aktif) → Panel (açılıp kapanır) → Sheet (680x990, sürüklenir) → Klasör + Sekme + Kapat + Kağıt.
        // content: kağıdın içi (VLayout); paper: kağıdın kendisi (yerleşim dışı katmanlar için); tabText: sekme yazısı.
        private static PaperWindow BuildPaperWindow(Transform papers, string name, Color folderColor, Color shadowColor,
            string tab, Vector2 fromOffset, out RectTransform content, out RectTransform paper, out TMP_Text tabText)
        {
            var root = Stretch(NewRect(name, papers));
            var panel = Stretch(NewRect("Panel", root));

            var sheet = NewRect("Sheet", panel);
            sheet.anchorMin = sheet.anchorMax = sheet.pivot = new Vector2(0.5f, 0.5f);
            sheet.sizeDelta = new Vector2(SheetW, SheetH);
            sheet.anchoredPosition = new Vector2(0, -40);
            var draggable = AddSheet(sheet, root);

            // Klasör gövdesi (sekmenin altından başlar), içinde düz gölgeli krem kağıt
            var folder = Anchor(NewRect("Folder", sheet), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -(TabH - Border)));
            var folderInner = Bordered(folder, folderColor);
            var paperShadow = Anchor(NewRect("PaperShadow", folderInner), Vector2.zero, Vector2.one,
                new Vector2(FolderPad + 6, FolderPad - 6), new Vector2(-FolderPad + 6, -FolderPad - 6));
            AddImage(paperShadow, shadowColor);
            paper = Anchor(NewRect("Paper", folderInner), Vector2.zero, Vector2.one,
                new Vector2(FolderPad, FolderPad), new Vector2(-FolderPad, -FolderPad));
            var paperInner = Bordered(paper, PaperCream);
            content = Anchor(NewRect("Content", paperInner), Vector2.zero, Vector2.one,
                new Vector2(PaperPad, PaperPad), new Vector2(-PaperPad, -PaperPad));
            VLayout(content, 14, new RectOffset(0, 0, 0, 0));

            // Sekme: alt çerçevesi yok (klasörün üst çizgisini örter)
            var tabRect = NewRect("Tab", sheet);
            tabRect.anchorMin = tabRect.anchorMax = tabRect.pivot = new Vector2(0f, 1f);
            tabRect.sizeDelta = new Vector2(330, TabH);
            AddImage(tabRect, PaperInk);
            var tabFill = Anchor(NewRect("Fill", tabRect), Vector2.zero, Vector2.one, new Vector2(Border, -Border), new Vector2(-Border, -Border));
            AddImage(tabFill, folderColor);
            tabText = NewText("Text", tabFill, tab, 18, PaperInk, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            tabText.font = TitleFont;
            tabText.characterSpacing = 3;
            tabText.textWrappingMode = TextWrappingModes.NoWrap;
            Stretch(tabText.rectTransform).offsetMin = new Vector2(16, 0);

            // Kapat: sağ üstte, sekme yüksekliğinde kare
            var close = BorderedButton("CloseButton", sheet, folderColor, "X", 22, out _);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(TabH, TabH);

            panel.gameObject.SetActive(false);

            // Bileşen hep aktif köke konur: pasif objede Awake çalışmaz.
            var window = root.gameObject.AddComponent<PaperWindow>();
            Wire(window, "panelRoot", panel.gameObject);
            Wire(window, "sheet", draggable);
            Wire(window, "closeButton", close);
            var so = new SerializedObject(window);
            so.FindProperty("fromOffset").vector2Value = fromOffset;
            so.ApplyModifiedPropertiesWithoutUndo();
            return window;
        }

        // Kağıdı sürüklenebilir/animasyonlu yapar: saydamlık için CanvasGroup, arkasında yumuşak gölge, öne alınacak kök.
        private static DraggableSheet AddSheet(RectTransform rt, Transform layer)
        {
            var group = rt.gameObject.AddComponent<CanvasGroup>();
            var shadow = Stretch(NewRect("Shadow", rt));
            shadow.SetAsFirstSibling();
            AddImage(shadow, new Color(0, 0, 0, 0.35f)).raycastTarget = false;
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

        // "1.1  Kural metni" satırı (metin sarar). parent null ise şablon.
        private static RuleRowView RuleRow(string name, Transform parent)
        {
            var row = NewRect(name, parent);
            var layout = HLayout(row, 10, new RectOffset(0, 0, 0, 0));
            layout.childAlignment = TextAnchor.UpperLeft;

            var number = NewText("Number", row, "1.1", 24, PaperInk, TextAlignmentOptions.TopLeft);
            number.font = ValueFont;
            Layout(number, prefW: 44);
            var body = NewText("Body", row, "Kural", 23, PaperInk, TextAlignmentOptions.TopLeft);
            body.font = ValueFont;
            body.lineSpacing = -8;
            Layout(body, prefW: 0, flexW: 1);

            var view = row.gameObject.AddComponent<RuleRowView>();
            Wire(view, "numberText", number);
            Wire(view, "bodyText", body);
            return view;
        }

        // Bölüm: [harf kutusu] BAŞLIK + satırlar. rowTemplate bölümün satır listesine taşınır.
        private static SectionBlockView SectionBlock(string name, Transform parent, RowView rowTemplate)
        {
            var block = NewRect(name, parent);
            VLayout(block, 6, new RectOffset(0, 0, 0, 0));

            var head = NewRect("Head", block);
            var headLayout = HLayout(head, 10, new RectOffset(0, 0, 0, 0));
            headLayout.childAlignment = TextAnchor.MiddleLeft;
            Layout(head, prefH: 34);
            var box = NewRect("Box", head);
            Layout(box, prefW: 34, prefH: 34);
            var boxInner = Bordered(box, PaperCream);
            var letter = NewText("Letter", boxInner, "A", 20, PaperInk, TextAlignmentOptions.Center, FontStyles.Bold);
            letter.font = TitleFont;
            Stretch(letter.rectTransform);
            var title = NewText("Title", head, "YAPI", 20, PaperInk, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            title.font = TitleFont;
            title.characterSpacing = 6;
            title.textWrappingMode = TextWrappingModes.NoWrap;

            var rows = NewRect("Rows", block);
            VLayout(rows, 6, new RectOffset(0, 0, 0, 0));
            rowTemplate.transform.SetParent(rows, false);
            rowTemplate.gameObject.SetActive(false);

            var view = block.gameObject.AddComponent<SectionBlockView>();
            Wire(view, "letterText", letter);
            Wire(view, "titleText", title);
            Wire(view, "rowsContainer", rows);
            Wire(view, "rowTemplate", rowTemplate);
            block.gameObject.SetActive(false);
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

        // EEE'nin TMP font asset'leri (Pixelify Sans, VT323) adlarıyla bulunur; yoksa varsayılan font.
        private static void ResolvePaperFonts()
        {
            if (paperFontsResolved) return;
            paperFontsResolved = true;
            titleFont = FindFont("Pixelify") ?? font;
            valueFont = FindFont("VT323") ?? font;
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
