# memory.md — Projenin ortak hafızası

## 1. Proje özeti
- CIVIL GAME JAM (ODTÜ KKK, KALTEV), 48 saat: 16 Ekim 2026 18:00 → 18 Ekim 2026 18:00 (UTC+3). Tema: "Afete Hazırlık ve Farkındalık".
- Unity 6000.6.4f1, 2D, uGUI + TextMeshPro, hedef platform Windows, 1920x1080. Kontrol sadece fare, Esc menü açar.
- Oyun (çalışma adı "İmza"): masa başı karar oyunu. Oyuncu bina onaylayan inşaat mühendisi.
  - Masaya proje ve laboratuvar belgeleri gelir; oyuncu onaylar veya reddeder; rüşvet teklif eden müteahhitler olur.
  - Onaylanan binalar yapılır → gün sonu afet kontrolü → afet varsa gazete/sonuç, yoksa gün özeti (gazete yok) → günlük ödeme → gün sonu kayıt.
- Jüri ölçütleri: Oynanabilirlik 25, Temayla ilişki 25, Yaratıcılık 20, Görsel/işitsel bütünlük 15, Teknik bütünlük 15.
- Hassasiyet: Deprem Türkiye'de hassas bir konu.
  - Depremzedeler asla olumsuz gösterilmez; isimler tamamen kurgusal; gerçek olaylara doğrudan atıf yok.
  - Mizah karakterlerde; sonlar ve deprem sonrası ciddi ve duygusal.

## 2. Kesinleşen oyun kararları
- Level sistemi kaldırıldı; oyun gün sistemiyle ilerler. Aşağıdaki akış Core'da uygulandı (GameManager, 22:15).
- Akış: DayIntro → Desk → Construction → afet kontrolü → (afet varsa Result/gazete, yoksa DaySummary) → DailyExpenses (her gün) → gün sonu Save → (NextDay | Ending). Afet günü gün özeti yok, sadece gazete.
- Her gün sonunda afet olasılığı vardır. **Taban %20, afetsiz her gün +%15, tavan %100; afet olunca tabana döner** (denge turu kararı, 17 Ekim; önce %50/+%10 idi, oyun başına 5,6 deprem veriyordu → şimdi ~3,3).
- Afet, son afetten bu yana (iki afet arasında) oyuncunun onayladığı binaları değerlendirir; önceki afetleri atlatmış binalar tekrar değerlendirilmez.
- Afetsiz günün sonunda gazete gösterilmez. Afet olunca gazete/sonuçta şimdilik: her binanın yıkılıp yıkılmadığı, ölenlerin isimleri, ölü/yaralı sayısı, yıkılmasından ya da ayakta kalmasından oyuncunun sorumlu olduğu bilgisi.
- Oyun **10 gün** (karar 17 Ekim). İçerik açılışları `Data/Days`: 1. gün konut, 3. gün rüşvet, 5. gün sahte belge + kabadayı. **Fabrika/yangın kapsam dışı** (karar 17 Ekim; Day08 `AllowFactories = 0`), vakit kalırsa yangın ayrı afet olarak dönebilir. Sel ve okul/hastane kapsam dışı.
- Deprem ayrı bir sahne/animasyon olarak gösterilmez; oyuncu depremi ve sonuçlarını gazeteden öğrenir (Quake ekranı yok, deprem/yıkılma sesi yok).
- Deprem şiddeti oyuncuya gösterilmez. Bina sadece 2 durumlu: ayakta / yıkık.
- Yıkılma kuralı (EarthquakeSimulator):
  - Strength = min(Actual/Required).
  - Strength >= 1 ise ASLA yıkılmaz (dürüst yol garantisi).
  - Aksi halde Strength + gürültü(±0.15) < Intensity ise yıkılır. Intensity yapılandırılmış afet aralığından çekilir, max <= 1.0.
- Rüşvet teklif eden her proje uygunsuzdur. Bazı uygunsuz projeler rüşvetsiz, sahte belgeyle temiz görünür; ek testle yakalanır. Dürüst usta = rüşvetsiz gelen gerçekten uygun proje.
- İki katmanlı sahte belge: (1) bedava ipuçları (parsel/tarih/lab/kaşe/imza tutarsızlığı), (2) ücretli ek test (Reported vs Actual farkını açar).
  - Kaşe ve imza ipuçları yazıyla ve karşılaştırmayla çalışır (A'nın 22:50 uygulaması): kaşe = "<LAB ADI> · AKREDİTE", sahtede başlıktaki lab adıyla uyuşmaz; imza = sahtede lab raporunu başvurudakinden farklı mühendis imzalar. Ayrı sahte kaşe/imza görseli yok.
- Hapis: kalıcı JailRisk. Yıkım varsa zar atılır.
  - Önceki denge: yıkılan bina +0.15, rüşvetle onaylanan yıkılan bina +0.25. Eski temiz level -0.05 kuralının gün sistemindeki karşılığı açık; afetsiz günün riski azaltacağı varsayılmamalı.
  - İlk yıkımda bile hapis olasılığı var. Tüm sayılar BalanceSO'da.
- Eski level tekrar kuralı kaldırıldı; başarısız gün sonrası devam/tekrar davranışı henüz kesinleşmedi.
- Giderler her gün ödenir: **kira 600** + kredi 300 (zorunlu), kabadayı cam tamiri (olursa). Ek test gider değildir, anında kasadan düşer. **Sigorta kapsam dışı** (karar 17 Ekim, etkisi yoktu).
- **Borç kuralı (karar 17 Ekim):** kasada yetmeyen gider ödenemez, zorunluysa borca yazılır; kasa asla eksiye düşmez. Borç varsa isteğe bağlı "Borç ödemesi" kalemi çıkar (kısmen ödenebilir). **Borç 2.000 TL'yi aşarsa ofis kapanır → Orta son.** Başlangıç parası 2.500 TL, **onay ücreti 300-500 TL** (denge turu). Simülasyon (`CivilJam > Core > Balance Report`, 300 oyun/tip): dikkatli oyuncu %100 İyi son ama kasa 1.663 TL'ye kadar iner; tek zarf alan %49 İyi / %42 Enkazın Gölgesi / %8 hapis; her şeyi reddeden 5. gün ofisini kaybeder; her zarfı alan %86-96 hapis.
- Kota YOK (karar, 22:40). "Her şeyi reddet" stratejisine karşı baskıyı günlük giderler ve borç kuralı kurar.
- Save/Load: tek slot JSON (`persistentDataPath/save.json`), sadece `GameState`. Günlük ödemeden sonra gün +1 yapılıp kaydedilir; "Devam Et" yeni günün başını açar. **Son ekranına girince kayıt silinir** (bitmiş oyun devam ettirilemez). Gün içi manuel kayıt yok.
- **Sonlar (karar 17 Ekim)** — günlük ödemeden sonra değerlendirilir, öncelik en ağırdan:
  1. **Kötü** (`Bad`, "Hüküm"): yıkım sonrası hapis zarı tuttu (JailRisk üzerinden).
  2. **Orta / ofis kapandı** (`MiddleClosed`, "Kapanan Kapı"): borç 2.000 TL'yi aştı. Herhangi bir günde olabilir.
  3. **Orta / yıkım** (`MiddleCollapsed`, "Enkazın Gölgesi"): 10. gün bitti, en az bir imzalı bina yıkıldı, hapis çıkmadı. Metin TASLAK (A yazdı), EEE son halini yazacak.
  4. **İyi** (`Good`, "Ayakta Kalanlar"): 10. gün bitti ve **hiç bina yıkılmadı**.
- Yıkım nedeni kartları elle yazılmaz; sonuç ekranı FailedParams / Actual / Required değerlerinden otomatik üretilir (şablon cümle).
- Sadece Türkçe. **Jüri demosu gizli tuşları** (Game sahnesi): **F9** = son güne atla, **F10** = bu gece afet kesin.

## 3. Mimari
- Tek `Game` sahnesi + ekranlar prefab. Diğer sahneler: MainMenu, Sandbox_Desk, Sandbox_Flow.
- Oyun mantığı saf C# (MonoBehaviour yok): üretim, deprem, ekonomi (GameSession). Tek MonoBehaviour GameManager: faz sırası + ekran gösterme.
- Veri:
  - ScriptableObject: DayConfigSO, RuleSetSO, ClientSO, ExpenseSO, BalanceSO, HeadlineSO, EndingSO, NamePoolSO.
  - Runtime plain C#: ProjectData, DocumentData, ParamEntry, GameState, GameContext, QuakeResult (+BuildingOutcome), DailyExpenseResult.
- Ekranlar IGameScreen uygular: Phase, Show(GameContext), Hide(), event Completed. GameManager sahnedeki ekranları kendisi bulur; ekranı olmayan faz için IMGUI yedek arayüz gösterir. Ekranlar Core'a sadece `ctx.Actions` (IGameActions) ile yazar. B/C kullanım rehberi: `AHMET.md`.
- Assembly'ler: CivilJam.Core, CivilJam.Desk, CivilJam.Flow. Desk ve Flow sadece Core'a bağlı, birbirini görmez.

### Dosya haritası
- `Assets/_Project/Scenes/Sandbox_Flow.unity` (C) — Flow deneme sahnesi; menü CivilJam > Flow > 3 ile kurulur (4 Flow ekranı + sahte context).
- `Assets/_Project/Scenes/MainMenu.unity` (C) — ana menü (Yeni / Devam Et / Emeği Geçenler / Çıkış); Build Settings'te 1. sıra, Game.unity 2.

`Assets/_Project/Scripts/Core/` (asmdef `CivilJam.Core`, namespace `CivilJam.Core`):
- `Enums.cs` — GamePhase, ParamType, BuildingType, DocumentKind, ClueType, ProjectVerdict, BuildingStatus, EndingType.
- `Runtime/` — ParamEntry (Required/Reported/Actual), DocumentData, ProjectData (Strength), GameState (kayıt), GameContext (ekranlara verilen), QuakeResult + BuildingOutcome (kayıplar, sorumluluk metni), DailyExpenseResult.
- `Screens/` — IGameScreen (Phase/Show/Hide/Completed), IGameActions (ekranlardan Core'a tek giriş: ek test, onay, ret, gider seçimi).
- `Config/` — BalanceSO, DayConfigSO, RuleSetSO, ClientSO, ExpenseSO, HeadlineSO, EndingSO, NamePoolSO (kurgusal isimler).
- `Simulation/` — EarthquakeSimulator (yıkılma kuralı, son afetten beri onaylananlar, JailRisk, hapis zarı), DisasterSystem (gün sonu zar + olasılık), ProjectGenerator (seed'li günlük projeler), CasualtyGenerator, ExpenseCalculator (günlük gider v0).
- `Game/` — GameSession (saf C# mantık + IGameActions), GameManager (MonoBehaviour, faz sırası), FallbackGui (geçici IMGUI), SaveSystem (tek slot JSON), GameLaunch (MainMenu "Devam Et" bayrağı).
- `Editor/CoreSetupBuilder.cs` — menü CivilJam > Core: Create Missing Data Assets / Build Game Scene (var olanın üzerine yazmaz) / **Install Screens Into Game Scene** (Desk + 4 Flow prefab'ı Canvas altına) / **Link Data Into Game Scene** (Days, Expenses, Clients listelerini Data'dan doldurur; EEE yeni asset ekleyince çalıştırılır).
- `Tests/Editor/` — `CoreRulesTests` (yıkılma kuralı, afet kapsamı/olasılığı, üretici, kaşe metni, müteahhit rolleri, çift ödeme) + `GameSceneTests` (Game.unity: GameManager, EventSystem, veri listeleri, 5 fazın ekranı). Test Runner > EditMode; 10/10.
- `Assets/_Project/Data/` (A, içerik EEE ile) — Balance, RuleSet, NamePool (LabNames = akredite laboratuvarlar), Headlines (EEE: 20 manşet), Days/DayXX (FromDay 1/3/5/8; 8'de fabrika kapalı), Expenses/ (Rent, Loan), Clients/ (EEE), Endings/ (Good, Bad, MiddleClosed, MiddleCollapsed).
- `README.md` — jüri için kurulum, kontroller, nasıl oynanır, ekip (taslak A, son hali EEE).
- `Assets/_Project/Scenes/Game.unity` (sadece A; Editor'den KAYDETMEYİN, değişiklikler CoreSetupBuilder ile) — GameManager (veri + 6 müteahhit bağlı), Canvas altında DeskScreen + ResultScreen + DaySummaryScreen + DailyExpensesScreen + EndingScreen, EventSystem (Input System UI). Build Settings'te.
- Flow asmdef'i C oluşturur; Desk ve Flow referansı sadece `CivilJam.Core`. Ayrıntı ve Core önerileri: `AHMET.md`.
- `Assets/_Project/Scripts/Desk/` (B) — Core tiplerini doğrudan kullanır (ProjectData, GameContext, IGameActions).
  - `CivilJam.Desk.asmdef` — referans: CivilJam.Core, TMP, uGUI.
  - `UI/DeskScreen.cs` — IGameScreen (Phase = Desk). Show(GameContext); kararlar context.Actions ile; "Mesaiyi bitir" → Completed.
  - `UI/DeskAudio.cs` — masa sesleri: tık, kağıt, kaşe, cam kırılma + ofis müziği (döngü, %25); klipler prefab'da referansla bağlı.
  - `UI/EventCardView.cs` — masada olay kartı (perde + başlık/metin/Tamam). Kabadayı reddedilince "Camın kırıldı!" (Core `WindowRepairsToday`'i artırdıysa; masa kuralı kopyalamaz, sonucu okur).
  - **Üç belge, üç ayrı pencere (Ahmet öncelik 1, B 17 Ekim öğleden sonra):** masada ortada YAPI DENETİM FORMU klasörü (müteahhit oturunca), solda LAB RAPORU, sağda EL KİTABI; her biri kendi kağıt penceresini açar, sekme yok. Görünüm Ahmet'in piksel tasarımı (`Docs/Tasarim/*_GORUNUM.html`): klasör rengi + 3 px koyu çerçeve, sekme, krem kağıt + düz gölge, noktalı satırlar, kutulu bölüm harfleri. Fontlar Pixelify Sans (başlık/etiket) + VT323 (değer): EEE TMP font asset'lerini ekleyince builder adlarıyla bulur, yoksa varsayılan font. Builder kodu: `Editor/DeskSandboxBuilder.Papers.cs`.
  - `UI/PaperWindow.cs` — kağıt pencere (perde yok): masadaki nesnesinin yönünden kayarak açılır/kapanır, `HandTo` ile kaşeli klasör olarak müteahhide gider. `DeskScreen.ArrangeSheets`: bir pencere **açılınca** açık olanlar soldan sağa (form, lab, el kitabı) yan yana dizilir, üçü açıksa %90; kapanınca diğerleri yerinde kalır.
  - `UI/FormView.cs` — Yapı Denetim Formu (`Documents[0]`): "DOSYA · FileNo" sekmesi, tarih, künye (proje, adres; kullanım, parsel no, kat sayısı, yükseklik `HeightMeters`, kat başı kişi, onay ücreti), bölümler (`ParamRule.Section`) altında beyan edilen değerler, imza + belediye kaşesi, ONAY / RED kutuları, zarf varsa "ZARFI AL VE ONAYLA", karar kaşesi (`DecisionStampView`).
  - `UI/LabReportView.cs` — Laboratuvar Raporu (`Documents[1]`): lab adı, tarih, proje + parsel no, tablo Parametre | Beyan | Ek test (`ParamRowView`), lab kaşesi + imza, EK TEST İSTE.
  - `UI/RulebookView.cs` — Denetçi El Kitabı (tasarıma göre): sayfa 1 bölüm kutuları + numaralı kurallar "1.1 Kolon kalınlığı en az: 1-3 kat 30 cm · 4-6 kat 35 cm · 7+ kat 40 cm" (`FindRule(tür, parametre, kat)` + `FloorsLabel`; seçenek sırası + `Description` küçük yazıyla) + GENEL KURAL kutusu; sayfa 2 nasıl kullanılır (`HandbookIntro`), belge kontrolü, akredite laboratuvarlar.
  - Satır parçaları: `RowView` → `DottedRowView` ("etiket ..... DEĞER"), `RuleRowView` ("1.1 metin"); `SectionBlockView` ([A] YAPI + satırlar).
  - `UI/DraggableSheet.cs` — kağıtlar fareyle sürüklenir, basılan Papers katmanında en öne gelir, en az 200 px ekranda kalır; hareketler DeskMotion ile. Karar kaşesinden sonra form müteahhide verilir (`PaperWindow.HandTo` → `UI/FolderHandoffView.cs`: kağıt kart gibi kapanır → kaşeli kapalı klasör → yayla ellere gider → desk_front'un arkasına iner; tepki balonu ondan sonra); lab raporu kitabına döner. Eşleştirme (Papers, Please inceleme modu) Ahmet'le yüz yüze konuşulacak — kural etkisi olabilir.
  - `Sandbox/DeskSandboxBootstrap` gerçek `Data/RuleSet.asset`'i kullanır (builder bağlar); böylece sandbox'taki el kitabı oyundakiyle aynı. Sahte 4 proje v2 parametreleriyle (5 parametre, kat 4/5/6/8); asset yoksa yedek yönetmelik v2'nin 15 kuralının aynısı.
  - `UI/ClientSceneView.cs` — ofis sahnesi: `desk_bg` (oda + koltuk + masa) → portre (600x600) → `desk_front` (sadece masa) → masadaki eşyalar → konuşma balonu. Müteahhit sağdan gelip oturur; balonda gelişte `GreetingText` (+ rüşvet repliği), karardan sonra `ReactionText`.
  - `UI/DeskMotion.cs` — masa ekranının **ortak hareket dili** (ui-ux-pro-max §7): süreler `Fast` 0,16 / `Base` 0,26 / `Slow` 0,42 sn, çıkış = girişin %65'i; varış ease-out, çıkış ease-in; tutulan/oturan şeyler yay (spring). Yeni animasyon bunları kullansın, kendi süresini uydurmasın.
  - `UI/PopupMotion.cs` — olay kartı ve öğretici kartının açılış/kapanışı (perde kararır, kart aşağıdan yaylanarak gelir; kapanış kısa, girdiyi engellemez). Çalışırken eklenir, prefab değişmez.
  - Hareketler (B, 17 Ekim öğle): kağıtlar yayla açılır/oturur, kapanışta hızlanarak gider; sürüklemede fareyi birebir izler, hıza göre yatar, bırakınca hızıyla kayıp durur. Kaşe hızlanarak iner, çarpınca basılıp oturur. Müteahhit sağdan gelip yaylanarak oturur, sıradaki gelmeden sola çıkar (gün sonunda da). Konuşma balonu pop'la açılır/kapanır.
  - `UI/DeskPropsView.cs` — masadakiler (EEE'nin sade masası). Oyuncu tarafı: LAB RAPORU x 238, y 815, 480 geniş; EL KİTABI x 1320, y 812, 500 geniş (EEE önerisi). Müteahhit tarafı (isimliğin arkası, masanın uzak kenarı; isimliğin önünde yer yok): YAPI DENETİM FORMU klasörü x 887, y 752, 230 geniş (EEE `Art/form_folder.png` eklerse o görsel, yoksa çerçeveli klasör + yazı) ve zarf. Form, lab ve zarf müteahhit oturunca belirir. Gün sonunda "Mesaiyi bitir".
  - `UI/GiftOfferView.cs` + `UI/OfficeGiftsView.cs` — hediye kartı (sadece KABUL ET) ve kabul edilen hediyelerin masadaki yerleri (uzak kenar, 6 yer, kitapların arkasında).
  - `UI/DeskBookProp.cs` — masadaki nesnelerin "masaya konmuş" görünümü: alt kenarda yatay yumuşak gölge (~%30) + dar temas gölgesi, üzerine gelince yayla kalkma. Kitap görsellerinde altta ~%17 şeffaf boşluk var; gölge kitabın gerçek alt kenarına oturur (builder `artBottom`).
  - `UI/TutorialView.cs` — 1. gün masa açılınca 4 öğretici kart (metinler bu dosyada; taslak, EEE onayı bekliyor).
  - `UI/` diğerleri — DeskHud (gün, kasa, onaylanan · bekleyen), DecisionStampView (ONAYLANDI/REDDEDİLDİ kaşesi + animasyon), ParamRowView, UiPool, DeskFormat (parametre adı RuleSetSO'dan, değer `FormatValue` ile). (DossierView, DocumentView, ActionPanel kaldırıldı.)
  - `Sandbox/FakeGameActions.cs` — sadece sandbox için sahte IGameActions.
  - `Sandbox/SandboxDeskContext.cs` — sahte gün: 4 proje (dürüst, rüşvetçi, sahte belgeli, kabadayı), 4 müşteri, RuleSet, akredite lab listesi.
  - `Sandbox/DeskSandboxBootstrap.cs` — Sandbox_Desk'i başlatır, mesai bitince sonucu loglar ve aynı sahte günü bir sonraki gün numarasıyla açar.
  - `Editor/DeskSandboxBuilder.cs` — menü: CivilJam > Desk > Build Sandbox_Desk Scene (prefab + sahneyi yeniden kurar).
- `Assets/_Project/Scripts/Flow/` (C, asmdef `CivilJam.Flow`, sadece Core'a bağlı) — ekranlar UI'ı kodla kurar, `FlowScreen` tabanı Completed'ı bir kez tetikler.
  - `UI/ResultScreen` (Result, gazete: newspaper_bg kutularına yerleşim), `DaySummaryScreen`, `DailyExpensesScreen` (SetExpensePaid), `EndingScreen` (EndingSO yoksa yedek metin; "Ana menüye dön" MainMenu sahnesini yükler), `MainMenuScreen` (MainMenu sahnesi; Yeni/Devam/Credits/Çıkış).
  - `Sandbox/FlowSandboxBootstrap` — Sandbox_Flow'da sahte context. `Editor/FlowBuilder` — menü CivilJam > Flow > 1 Prefab, 2 MainMenu sahnesi, 3 Sandbox_Flow.
- `Assets/_Project/Prefabs/Flow/` (C) — Result/DaySummary/DailyExpenses/Ending prefab'ları; Game.unity'ye A ekler.
- `Assets/_Project/Prefabs/Desk/DeskScreen.prefab` (B) — masa ekranı; Game.unity'de Canvas altına konur (kendi içinde tam ekran stretch).
- `Assets/_Project/Art/` (EEE) — placeholder sprite'lar (Single, PPU 100). Dosya adları ve hangi ekranın kullandığı: `Art/README.md`. Final görsel aynı adla üzerine yazılır.
- `Assets/_Project/Audio/` (EEE) — sesler `.ogg` (Kenney) ve `.wav` (`sfx_paper`, `mus_office_ambient`). İsim listesi: `Audio/README.md`.
- `CREDITS_AND_AI.md` (EEE) — hazır asset + AI kullanım kaydı.
- `Assets/TextMesh Pro/` — TMP Essential Resources (A import etti, 23:15). Desk/Flow yazıları için gerekli; tekrar import etme, silme.
- `ICERIK_TASLAK.md` (EEE) — SO metin taslakları (6 müteahhit, 20 gazete manşeti, 3 son). Onaylananlar `Data/` altındaki asset dosyalarına taşınır (A ile koordineli).
- `Assets/_Project/Data/Clients/` (EEE) — 6 ClientSO (Rifat, Sevim, Kemal, Nuri, Selim, Rustem). Portreler boş. Game.unity'ye bağlı (A). Rol Core'da alanlardan çıkar: `IsHonest`, `OffersBribes` (= dürüst değil + BribeRange > 0), `IsThug`.

## 4. Takım ve klasör sahipliği
- A (Core) — Ahmet İlman: Scripts/Core, Data.
- B (Masa) — Oğuzhan: Scripts/Desk, Prefabs/Desk, Scenes/Sandbox_Desk.
- C (Akış) — Onur Kabadayılar: Scripts/Flow, Prefabs/Flow, Scenes/Sandbox_Flow, MainMenu.
- 4. kişi (EEE): Art, Audio, SO içerikleri, CREDITS_AND_AI.md.
- Scenes/Game.unity sadece A.
- Onur’un kişisel C rolü planı: `ONUR.md`. Ortak kararların kaynağı bu dosyadır; kişisel plan ortak kararları değiştirmez.
- Kural: kimse başkasının sahne/prefab dosyasını açıp kaydetmez. Saatte bir main'i çek.
- ⚠️ **ÖNEMLİ:** `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` Play'de kendiliğinden değişir (Türkçe karakterler dinamik atlasa eklenir). **Commit'lemeyin**; commit'lenirse diğerlerinin pull'u takılır. Değişiklik listesinde görürseniz dokunmayın, pull engellerse `git checkout -- "<dosya yolu>"` ile geri alın.

## 5. Jam kuralları (ihlal = diskalifiye)
- Repo PUBLIC olmalı (private kalırsa puandan bağımsız diskalifiye). Build ile repo kodu uyumlu olmalı.
- Her hazır asset ve AI kullanımı eklendiği anda CREDITS_AND_AI.md'ye yazılır (görsel/ses/kod/metin/diğer, küçük kullanım dahil). Redistribute edilemeyen asset'ler repoya konmaz.
- Jam öncesi hazırlanmış kod veya asset commit'lenmez.
- Teslim: Windows build ZIP + en fazla 1 dk video (MP4/AVI) + public repo + kurulum/kontrol talimatı.
- Sınırlar: yeni teslim başlatma 18 Ekim 18:00; Git desteği 18 Ekim 12:00; planlanan feature freeze 18 Ekim 12:00.

## 6. Yol haritası ve durum
- Cum 20-22: kurulum + veri sözleşmesi
- Cum 22-02: dikey dilim v0
- Cts 08-14: ekonomi / rüşvet / ek test / sahte belge içerikleri
- Cts 14-20: fabrika-yangın / 3 son / ses
- Cts 20-22: MVP kontrolü
- Cts gece - Paz sabah: denge + cila
- Paz 12:00: freeze. Paz 15-16:30: yükleme. 16:30-18:00: tampon.

### Durum (güncelleme: A, `a7a733c`)
- **Faz 1 KAPANDI.** Game.unity'de oyun MainMenu → masa → (gazete | gün özeti) → günlük ödeme → kayıt → sonraki gün → son ekranı akışıyla oynanıyor. Sadece DayIntro / Construction / Save fazları hâlâ mavi IMGUI yedek arayüzle (sahibi C).
- **Core (A, analiz sonrası):** Borç kuralı + ofis kapanışı, 4 son ve öncelikleri, son ekranında kayıt silme, build'de yedek arayüz kapalı, F9/F10 demo tuşları, 10 gün / 2.500 TL / ücret 400-700, fabrika ve sigorta kapsam dışı, 4 EndingSO bağlı. EditMode 13/13. **İlk Windows build alındı** (`Builds/Imza/Imza.exe`, açılış testi temiz; `Builds/` git dışında). `README.md` taslağı repoda.
- **Desk (B, 23:50):** Core tiplerini kullanıyor (IGameScreen, ctx.TodayProjects, ctx.Actions); kota yok (HUD: "Bugün onaylanan: N"); kaşe yazısı auto-size; sesler ve ONAYLANDI/REDDEDİLDİ kaşe animasyonu.
  - (B, 17 Ekim) Y1: "El Kitabı" penceresi (kontrol listesi + yönetmelik + akredite lab listesi), lab raporunda başvuru değerleri yan yana, 1. gün 4 öğretici kart. Kural değişikliği yok, Core'a dokunulmadı. Sandbox ve Game.unity'de test edildi. Kart metinleri taslak → Tarık onaylayacak.
  - (B, 17 Ekim) O4: kabadayı reddedilince kaşeden sonra cam sesi (`sfx_glass_break`) + "Camın kırıldı!" kartı (tamir ücreti `Balance.WindowRepairCost`, "bugünün giderlerine eklendi"). Sandbox (4. dosya: Rüstem) ve Game.unity'de test edildi.
  - (B, 17 Ekim) Yeni masa düzeni: dosya listesi yok, müteahhitler sırayla gelip koltuğa oturur (konuşma balonu). Masada LAB RAPORU kitabı (tıklayınca dosya ortada açılır; kararlar dosyanın içinde) ve EL KİTABI kitabı. Dosya kapalıyken sahne tamamen görünür. Test edildi. Tarık'ın yeni görsellerine (desk_bg + desk_front, 600x600 portreler, 600x400 kitaplar) göre yerleşim yapıldı (`43fc1e8`); müteahhit/proje adı üst barda; `chair_back` kullanılmıyor.
- **Flow (C, 00:17):** Gazete, gün özeti, günlük ödeme, son ekranı prefab'ları + MainMenu (Emeği Geçenler dahil) + Sandbox_Flow; manşet `{count}` düzeltildi. A `0bebf9b`'de Game.unity'ye ekledi.
- **Art/Audio/İçerik (EEE, 00:40):** 13 placeholder sprite + ending görselleri; masa sesleri (`sfx_click/stamp/paper`, `mus_office_ambient` CC BY 4.0 → credits MainMenu'de var); 6 ClientSO; Headlines.asset 20 manşet; son metinleri `ICERIK_TASLAK.md`'de. Stil: hazır asset (tercihen CC0), piksel art yok. Tarz: "İyi Pizza, Güzel Pizza" (Good Pizza, Great Pizza) benzeri flat cartoon; müteahhit masanın önüne kayarak gelir (karar 17 Ekim gece).
- **Sırada:** bkz. aşağıda "📌 Analiz sonrası işler" (kişi bazlı). Özet: C → ödeme/Save, credits ve müzik (Esc tamam); B → öğretici/denetçi notu + lab listesi + intro kararı; EEE → MiddleCollapsed son metni, README son hali, final görsel/ses; A → denge turu, 20:00 ikinci build.
- **Bilinen sorunlar:**
  - Esc menüsü C tarafından eklendi; son Windows build'de kontrol edilecek.
  - Oyuncuya kuralları anlatan öğretici yok (Oğuzhan + Tarık). — (B, 17 Ekim) Masaya eklendi (El Kitabı + 1. gün kartları); metinler Tarık'ın onayını bekliyor.
  - Unity'yi **6000.6.5f1 ile açmayın** (proje sürümünü yükseltiyor; ekip 6000.6.4f1). `ProjectVersion.txt` değişirse commit'lemeyin.
  - Game.unity Editor'den kaydedilirse ekran anchor'ları bozulabiliyor (bir kez oldu); kaydetmeyin.

### Ekibe notlar (A / Ahmet'ten, 2026-10-16 22:20)
> ⚠️ **Oğuzhan — ÖNCE BUNU OKU (A, `61353fd`):** `62165c0`'daki "yeni kural modeli" (Yapı Denetim Formu + Denetçi El Kitabı, tek ihlal = RED, ek test/lab raporu/sahte belge ipuçlarının kaldırılması, sel afeti) **Ahmet tarafından onaylanmadı**; notta yazan "Ahmet onayladı" yanlıştı. Commit geri alındı (`git revert`): memory.md eski kurallara döndü, `Docs/Tasarim/*.png` kaldırıldı (geçmişte duruyor).
> - Masa ekranını yeni forma göre değiştirmeye başladıysan **dur**; mevcut modelle devam et: başvuru + lab raporu, Reported/Actual, ücretli ek test, parsel/tarih/lab/kaşe/imza ipuçları, rüşvet zarfı. Core sözleşmesi değişmedi (`ctx.TodayProjects`, `ctx.Actions`).
> - Oyunun kural modelini değiştiren öneriler için önce Ahmet'le konuş; "Ahmet onayladı" notunu sadece Ahmet (ya da onun açık onayıyla) yazar.
> - Fikirlerden ileride yararlanmak istersek (ör. el kitabı, kaçak kat, yangın kuralları) Ahmet'le ayrıca konuşulur.
>
> ⚠️ **Tarık — (A, `cbd8587`):** `4f33750`'daki yeni modele uyarlamalar da geri alındı: Selim ("sahte belgeci" tekrar), Sevim replikleri, Headlines.asset (deprem / "Belgeler sahte, imza gerçek" tekrar), ICERIK_TASLAK.md'deki son metinleri (lab raporu / ruhsat tekrar). **Kalanlar:** credits çözüldü notu, piksel art stil kararı (Art/README; silinen `Docs/Tasarim` atfı düzeltildi), Selim "A onayladı" notu. Oyunda sel yok; manşet/sonlarda "deprem" dili geçerli.

### 🎨 Belge tasarımı: Yapı Denetim Formu + Denetçi El Kitabı (Ahmet kararı, 17 Ekim öğleden sonra) — ÖNCE BUNU OKU
Ahmet iki görsel tasarım verdi: `Docs/Tasarim/yapi_denetim_formu_GORUNUM.html` ve `Docs/Tasarim/denetci_el_kitabi_GORUNUM.html`.
- **Karar: GÖRÜNÜM tasarımdan, İÇERİK El Kitabı v2'den.** Tasarımlardaki örnek içerik (YANGIN, SEL, kaçak kat, DSİ, sabit "en az 30 cm", tek tip nervürlü demir) **kullanılmaz**; bunlar reddedilen eski modelin içeriği. Dosyaların başında bu not var.
- **Belgeler: Form + lab raporu.** "Yapı Ruhsatı Başvurusu"nun yerine **Yapı Denetim Formu** gelir (Core'da `Documents[0].Title` artık bu). **Lab raporu ayrı belge olarak kalır**: sahte belge ipuçları (iki belge arasında parsel/tarih/kaşe/imza/lab uyuşmazlığı) ve ek test aynen çalışır.

**Tasarım → Core alanları (A ekledi, push'landı)**
| Formdaki yer | Core |
|---|---|
| Sekme "DOSYA · 2026-0417" | `project.FileNo` |
| Sağ üst tarih | `project.Documents[0].Date` |
| Adres / Kullanım / Kat sayısı | `Address` / `DeskFormat.BuildingName(BuildingType)` / `Floors` |
| Yükseklik | `project.HeightMeters` (kat × 3 m) → "24,00 M" |
| Kat başı kişi | `project.PeoplePerFloor` |
| Yapım yılı | **Kullanma** → yerine "Başvuru tarihi" (`Documents[0].Date`) ya da boş bırak |
| Bölüm kutuları (A / B) ve başlıkları | `rule.Section` → "A · YAPI", "B · ZEMİN" (aynı bölümdekileri birlikte göster) |
| Bölümdeki satırlar | `project.Params` → ad `ctx.Rules.DisplayNameOf(p.Type)`, değer `ctx.Rules.FormatValue(p.Type, p.Reported)` |
| ONAY / RED kutuları | Mevcut karar akışı (`ctx.Actions.Approve / Reject`), kaşe animasyonu kutuya |

**El kitabı → Core:** başlık "DENETÇİ EL KİTABI", alt satır "Formdaki değerleri bu kurallarla karşılaştır." Bölümler `rule.Section`; kural satırı numarası (1.1, 1.2, 2.1...) masada üretilir; satır metni örneği: **"Kolon kalınlığı en az: 1-3 kat 30 cm · 4-6 kat 35 cm · 7+ kat 40 cm"** (`rule.FloorsLabel` + `FormatValue(Param, Required)`); istersen altında `Description`. Demir satırına "B220 (düz) hiçbir binada kabul edilmez." "GENEL KURAL: Tek bir kurala uymayan bina REDDEDİLİR." kutusu aynen. Akredite laboratuvarlar sayfası kalır.

- **Oğuzhan (B) — 🔴 ÖNCELİK 1 (Ahmet oynadı, 13:15):** Şu an "LAB RAPORU" kitabına tıklayınca form ve lab raporu **aynı pencerede sekme** olarak açılıyor ve piksel tasarım uygulanmamış. İstenen:
  - [ ] **İki ayrı belge, masada iki ayrı nesne, iki ayrı pencere:** (1) **Yapı Denetim Formu** (müteahhidin getirdiği dosya/klasör; `Documents[0]`), (2) **Lab Raporu** (lab raporu kitabı/kâğıdı; `Documents[1]` + parametre tablosu ve ek test). Sekme yok. Oyuncu ikisini ayrı ayrı açıp gerekirse yan yana koyar (sahte belge ipuçları iki belge karşılaştırmasıyla bulunur).
  - [ ] Her biri **kendi piksel tasarımıyla** (`Docs/Tasarim/*_GORUNUM.html`): renkler, çerçeve, noktalı satırlar, bölüm kutuları (A/B), ONAY/RED kutuları formda. Fontlar Tarık'tan (Pixelify Sans + VT323); gelene kadar mevcut fontla yerleşimi kur.
  - [ ] El kitabı da kendi tasarımına göre (aşağıda).
  - [ ] Yapı Denetim Formu'nu tasarıma göre kur (başvuru sekmesinin yerine), yukarıdaki alan eşlemesiyle.
  - [ ] Lab raporunu aynı görsel dile uydur (ayrı belge; renk/kâğıt farkı olabilir).
  - [ ] El kitabını tasarıma göre kur (bölüm kutuları, numaralı kurallar, kat grubu değerleri tek satırda, GENEL KURAL kutusu).
- **Tarık (EEE):**
  - [ ] 🔴 **LİSANS KAYDINI UNUTMA (Ahmet hatırlatması, jam kuralı = ihlal diskalifiye):** eklediğin her font, görsel ve ses **eklendiği anda** `CREDITS_AND_AI.md`'ye yazılır. Fontlar için → "Font" tablosuna: `Pixelify Sans` ve `VT323` · kaynak Google Fonts linki · **SIL Open Font License 1.1** · ekleyen · tarih. OFL lisans metni font dosyalarıyla birlikte repoda dursun (`OFL.txt`). Oyun içi "Emeği Geçenler"e de font satırı eklensin (Onur'un `MainMenuScreen` metni + `CREDITS_AND_AI.md` sonundaki blok). AI ile üretilen görseller (ChatGPT/Gemini kitapları, portreler) de "Yapay zekâ kullanımı" tablosunda olmalı; eksik varsa tamamla.
  - [ ] ⚠️ **Yazı tipi / tarz:** tasarımlar **Pixelify Sans** + **VT323** (Google Fonts, ikisi de SIL OFL) kullanıyor; bu sabahki "piksel art yok, düz çizgi film" kararınla çelişiyor. Ahmet'in kararı tasarımın görünümünü almak → bu iki fontu **TMP font asset** olarak ekle (`CREDITS_AND_AI.md` → Font, OFL), Oğuzhan'a haber ver. Masa/portre görsellerinin bu fontlarla uyumuna bak; uyumsuzsa Ahmet'le konuş.

### 🎮 Oynanış testi → yeni işler (Ahmet oynadı, 17 Ekim gece) — ÖNCE BUNU OKU
Ahmet oyunu baştan sona oynadı; 6 şikayet ve kararlaştırılan çözümler aşağıda. **Hepsi onaylı (Ahmet), sırayla yapılacak.** Kodlar: Ç1-Ç5.

**Şikayetler ve kök nedenleri (A koddan doğruladı)**
1. Müteahhitlerin karakteri hissedilmiyor → karardan sonra hiçbir tepki yok, geçmişi hatırlamıyorlar. Üstelik **isim türü ele veriyor**: Rıfat/Sevim hep uygun, Kemal/Nuri/Rüstem hep rüşvetli, Selim hep sahte belgeli; oyuncu belgeye bakmadan isimden karar verebiliyor.
2. Sahte belge "hiç denk gelmiyor" → sadece 5. günden sonra geliyor (oyun başına ~7,5) ve değerleri yönetmeliği geçtiği için temiz görünüyor; tek ipucu künyedeki küçük uyuşmazlık.
3. Sadece rüşvetçinin belgesi sorunlu → 3-4. günlerde uygunsuz projelerin **hepsi** rüşvetli; rüşvetsiz ama kötü proje sadece 1-2. günlerde var. Oyuncu "zarf varsa kötü, yoksa iyi" kuralını öğreniyor.
4. Karşılaştırma çok kolay → lab raporunda her değerin yanında "min. X" yazıyor; parsel/tarih uyuşmazlığı da başvurudaki değer yanına soluk yazılarak gösteriliyor.
5. İsimler tekrar ediyor → oyun başına ~47 proje, 12 bina adı (her ad ~4 kez); Rıfat ve Sevim oyunda ~13'er kez geliyor.
6. Parametreler az ve sabit → 4 parametre, gerekli değer her projede aynı; ezberlenince el kitabına gerek kalmıyor.

**Çözümler ve sahipleri**

**Ç1 — El kitabı v2 (Ahmet kararı, 17 Ekim öğle) — eski bölge/8 parametre modelinin YERİNE GEÇER. A ✅ Core kısmı yapıldı.**
- **Mahalle / deprem bölgesi TAMAMEN KALDIRILDI.** Adres artık sadece görsel bilgi ("Akasya Sokak No: 12"), oyun kuralını etkilemez.
- **5 parametre, her projede beşi de gelir** (2 sayısal + 3 seçenekli):

  | Parametre | 1-3 kat | 4-6 kat | 7+ kat |
  |---|---|---|---|
  | Kolon kalınlığı (sayısal) | 30 cm | 35 cm | 40 cm |
  | Temel derinliği (sayısal) | 1,5 m | 2,0 m | 2,5 m |
  | Beton sınıfı (C16 / C20 / C25 / C30 / C35) | C20 | C25 | C30 |
  | Demir tipi (B220 düz / B420 / B500 / B500S) | B420 | B500 | B500S |
  | Zemin türü (Dolgu / Yumuşak kil / Sert kil / Kaya) | Yumuşak kil | Sert kil | Kaya |

- Değerler "en az" kuralıdır (seçeneklilerde sırada sağdaki daha iyi). Gerekli değer **sadece kat sayısına** göre değişir; binalar 2-9 kat. Oyuncu: başvurudan kat sayısını okur → el kitabı tablosunda sütunu bulur → lab raporuyla karşılaştırır. Masada "min." yok.
- Veri `RuleSet.asset`'te (15 kural); yeniden üretmek: `CivilJam > Core > Rebuild Handbook Rules` (EEE metinlerini korur). Açıklamalar ve `HandbookIntro` sade dille A taslağı; EEE düzeltebilir.
- Denge (simülasyon, 300 oyun/tip): dikkatli %100 İyi; tek zarf %49 İyi / %42 Enkazın Gölgesi / %9 hapis; her zarfı alan %85 hapis; reddeci 5. gün ofisi kaybeder. Seçenekli parametrede her eksik kademe dayanımı %25 düşürür (`ParamEntry.LabelStepPenalty`).
- ✅ (Oğuzhan `255b24e` bölge kodunu kaldırdı, A uyumluluk üyelerini sildi.) **Oğuzhan — derleme notu (A):** `a9932ac` (sayfalı el kitabın) ile `c4a9bd2` (v2, bölge kaldırıldı) aynı anda push'landı ve main birkaç dakika derlenmedi. A, Core'a **uyumluluk** için `RuleSetSO.Zones` (her zaman boş), `ZoneEntry` ve 4 parametreli `FindRule(tür, parametre, bölge, kat)` (bölgeyi yok sayar) ekledi → derleniyor. Senin kodun Zones boşken tek "Yönetmelik" tablosu gösteriyor, yani **v2 için zaten doğru çalışıyor**. Uygun bir anda `RulebookView`'dan bölge/mahalle kodunu (ZonesPage, ZoneCount döngüsü) kaldır ve `FindRule(type, param, floors)` kullan; haber verince A uyumluluk üyelerini siler.
- **Oğuzhan (el kitabı ekranı):** en okunaklısı **tek tablo**: satırlar 5 parametre (`DisplayName`), sütunlar 3 kat grubu (`rule.FloorsLabel` → "1-3 kat", "4-6 kat", "7+ kat"), hücre `ctx.Rules.FormatValue(rule.Param, rule.Required)`; altında `Description`'lar. Sayfalı kitap gerekmiyorsa sayfa sayısı azalır (giriş + tablo + akredite lab'lar). Mahalle → bölge sayfası **artık yok**.
  - (B, 17 Ekim öğle) ✅ Yapıldı: `RulebookView`'dan bölge/mahalle kodu kaldırıldı (`Zones`, `ZoneEntry`, 4 parametreli `FindRule` artık kullanılmıyor; `FindRule(tür, parametre, kat)` + `FloorsLabel`). Kitap 3 sayfa: giriş → tek tablo + açıklamalar → akredite lab'lar. Sandbox projeleri v2'ye geçti. **Ahmet: uyumluluk üyelerini silebilirsin.**
- **Tarık:** mahalle adı yazmana gerek yok (özellik kalktı). El kitabı metinleri (5 parametre açıklaması + giriş) sende kalabilir ya da A'nın taslağı kalır.

**Ç2 — Uygunsuz proje karışımı (şikayet 2, 3). A ✅ yapıldı (aşağıdaki son madde B'de).**
- Sahte belge 3. günden başlar. Rüşvetsiz ama bir değeri yetersiz proje **her gün** gelir (Ç1 ile ancak el kitabıyla fark edilir). Bazı rüşvetçiler sahte belge de getirir.
- B: künyede yan yana başvuru değerini kaldır (oyuncu sekmeler arasında gidip gelsin).

**Ç3 — Gevşek roller (şikayet 1'in kökü). A ✅ yapıldı; EEE'nin yeni müteahhitleri bekleniyor.**
- Dürüst usta çoğunlukla uygun ama %15-20 ihtimalle dikkatsizlikten bir değeri eksik getirir; rüşvetçi bazen temiz proje getirir ve zarf vermez; Selim çoğunlukla (her zaman değil) sahte belge. İsim tek başına türü ele vermez.
- EEE: müteahhit sayısını 6'dan **10-12**'ye çıkar (yeni ClientSO'lar, aynı alanlar).

**Ç4 — Tepkiler ve hafıza (şikayet 1). A ✅ Core kısmı yapıldı; replikler/hediyeler (EEE) ve balon/hediye kartı (B) bekleniyor. Derinlik: metin + birkaç etki.**
- (A) Core artık: `GreetingText`'i doldurur (daha önce karar verilen müteahhit `ReturnLines`, değilse `GreetingLines`), karardan sonra `ReactionText`'i doldurur (onay → `ApproveLines`; ret → zarf geri çevrildiyse `BribeRefusedLines`, değilse `RejectLines`), onayda hediye çıkarır (`ClientSO.Gift` + `GiftChance`; aynı hediye oyunda bir kez), Nuri'nin zarfını her retten sonra büyütür (`BribeRaiseOnReject` = 250, A ayarladı). Müteahhit geçmişi `GameState.ClientHistory`'de, kayıtla saklanır. Replik listesi boşsa metin boş gelir → masa balon göstermesin.
- ⚠️ **Oğuzhan:** selamlamayı artık kendin `GreetingLines`'tan seçme; `project.GreetingText`'i göster (tekrar gelen müteahhitte "geçen sefer…" orada).
- Her karardan sonra müteahhit tepki verir: onay → teşekkür; red → söylenme/kavga; zarf geri çevrilince; tekrar gelince ("Geçen sefer reddettiniz hocam…").
- Etkiler: Rüstem reddedilince cam kırar (zaten var, artık masada görünür); Nuri reddedildikçe sonraki zarfı büyütür.
- **Hediye (Ahmet kararı, güncel):** **sadece dürüst ustalar (Rıfat, Sevim)** hediye verir (Core kuralı: `IsHonest`; diğer müteahhitlere bağlı hediyeler yok sayılır). Projesi onaylanınca **bazen** ofis için bir eşya hediye eder (saat, tablo, masa isimliği...). **Para ya da risk etkisi yok**; kabul edilince ofiste (masa ekranında) kalıcı görünür. **Geri çevrilemez, sadece kabul edilir.**
- A: müteahhit geçmişi (`GameState`), tepki seçimi, hediye kuralı. **EEE:** her müteahhide yeni replik grupları + hediye asset'leri. **B:** karar sonrası konuşma balonu + hediye kartı (sadece "Kabul et") + ofiste hediyelerin görünmesi.

**Ç5 — İsim çeşitliliği (şikayet 5). A ✅ yapıldı (oyun başına tekrar 0); EEE ✅ ad listesi 36.**
- A: bir oyunda aynı bina adı iki kez çıkmaz; ad kalıpları: "… Apartmanı", "… Sitesi B Blok", "… Konutları", "… Evleri".
- [x] EEE: `NamePool.BuildingNames` ~30 ada çıksın (12 var). — (EEE, 17 Ekim) 36 ad: 24 yeni (çiçek, ağaç, kuş, hava; gerçek depremlerde gündeme gelmiş bina/site adları, ilçe/semt adları ve sokak adlarıyla çakışanlar bilerek kullanılmadı).

**Ölçüm (A, `CivilJam > Core > Content Stats`, 300 oyun):** sahte belge 3. günden itibaren günde ~0,7-1; dikkatsiz (rüşvetsiz kötü) proje her gün ~0,7-1; tekrar eden bina adı 0; müteahhit adı türü ele vermiyor (testli). Denge korunuyor: dikkatli oyuncu %100 İyi son, kasa en fazla 1.562 TL'ye iner; tek zarf ~%49 İyi / %39 Enkazın Gölgesi / %12 hapis.
- Gizli `ProjectData.Kind` (Compliant / Sloppy / Bribe / Fake / BribeFake) eklendi; **masada gösterilmez.**

**Core sözleşmesi — KESİN ADLAR (A, push'landı; alanlar var, Ç1/Ç4 mantığı arkadan dolacak)**
- **Ç1 (el kitabı v2):**
  - `RuleSetSO.HandbookIntro` (string), `RuleSetSO.Rules`.
  - `ParamRule`: `MinFloors` / `MaxFloors` (0 = sınırsız), `FloorsLabel` ("1-3 kat", "7+ kat"), `Labels` (seçenekli parametrede kötüden iyiye sıra), `Description`, `IsLabeled`.
  - `RuleSetSO` yardımcıları: `RulesFor(tür)` (el kitabı tablosu için hepsi), `RulesFor(tür, kat)`, `FindRule(tür, parametre, kat)`, **`FormatValue(parametre, değer)`** ("Nervürlü", "C25", "2,5 m"), `DisplayNameOf(parametre)`.
  - Kullanılan `ParamType`'lar: `ColumnThickness`, `FoundationDepth`, `ConcreteGrade`, `RebarType`, `SoilType`. Eski değerler enum'da duruyor ama kullanılmıyor.
  - **KALDIRILDI:** `RuleSetSO.Zones`, `ZoneEntry`, `ZoneOf`, `ParamRule.Zone`, `ProjectData.Neighborhood`, `ProjectData.Zone`.
  - `ParamEntry.Required` kalır ama **masada gösterilmez**.
- **Ç4 (tepkiler ve hediye):**
  - `ProjectData.GreetingText` (masaya gelişte; tekrar gelen müteahhitte "geçen sefer..."), `ProjectData.ReactionText` (karardan hemen sonra), `ProjectData.GiftId` (onaydan sonra hediye varsa `GiftSO.Id`, yoksa boş).
  - `IGameActions.AcceptGift(projectId)` → bool. **`DeclineGift` yok** (geri çevrilemez).
  - `GameState.OfficeGifts` (kabul edilen hediye Id'leri, kayıtla saklanır), `GameContext.Gifts` + `ctx.GetGift(id)` → `GiftSO` (`Id`, `DisplayName`, `Sprite`, `Note`).
  - `ClientSO` yeni alanlar: `ApproveLines`, `RejectLines`, `BribeRefusedLines`, `ReturnLines` ({last} = "onaylamıştınız"/"reddetmiştiniz"), `Gift` (GiftSO), `GiftChance`, `BribeRaiseOnReject`.
- **Zaman:** Ç1 mantığı (bölge/kat kuralları, 8 parametre, üretici, RuleSet verisi) **Cumartesi öğlen**; Ç4 mantığı (geçmiş, tepki seçimi, hediye çıkışı, Nuri) **Cumartesi akşamüstü**. O zamana kadar alanlar boş/varsayılan gelir; masa kodu derlenir.

**Oğuzhan'ın 4 sorusuna cevaplar (Ahmet onayladı)**
1. Adlar: yukarıdaki kesin liste. Taslaktakiler aynen kaldı; eklenenler `FormatValue`, `GreetingText`, `GiftId`, `OfficeGifts`, `GetGift`. Hediye için `DeclineGift` YOK.
2. El kitabı sayfaları: senin önerdiğin sıra uygun → giriş + kontrol listesi → mahalle → bölge tablosu → bölge 1-4 sayfaları → akredite laboratuvarlar. Kontrol listesi genel kalsın ("değerleri el kitabıyla karşılaştır"), cevabı vermesin. Lab listesi önemli: "bilinmeyen laboratuvar" ipucu ancak onunla yakalanır.
3. Hediye: kart değil, **ofis eşyası**. Onaydan sonra (konuşma balonuyla aynı an) "X Usta size bir <eşya> bıraktı" + sadece **Kabul et**. Kabul edilince eşya masa ekranında (ofiste) kalıcı görünür; para/risk yok.
4. Ayrı "Mahalle" satırı yok; mahalle adresten okunur (aramanın parçası). Core adresin "… Mah." ile başlamasını garanti eder.

**Başkasının dosyasına dokundum (not):** `Desk/Sandbox/FakeGameActions.cs` (Oğuzhan) ve `Flow/Sandbox/FlowSandboxBootstrap.cs` (Onur) → `AcceptGift` metodu eklendi; `IGameActions`'a yeni metot gelince yoksa derlenmezdi. Başka satır yok.

**Görev dağılımı (Ahmet kararı)**
- **Oğuzhan (B) — masa tarafı Ç1 + Ç2 + Ç4:**
  - [x] Lab raporundan "min." değerini ve ek test renklerini kaldır; künyede başvuru değerini yan yana yazmayı kaldır (Ç1, Ç2).
    - (B, 17 Ekim) Tablo: Parametre | Beyan | Ek test; değerler `ctx.Rules.FormatValue` ile (`DeskFormat.Value`). Sandbox ve Game.unity'de test edildi.
  - [x] Sayfalı "Denetçi El Kitabı" (sayfa çevirme, masada her an açılabilir): giriş + kontrol listesi, mahalle → bölge tablosu (`ctx.Rules.Zones`), bölge 1-4 sayfaları (`ctx.Rules.RulesFor(tür)`; kural satırı: `DisplayName`, kat aralığı, `FormatValue(Param, Required)`, `Description`), akredite laboratuvarlar (`ctx.Names.LabNames`).
    - (B, 17 Ekim) Yapıldı. Eski tek sayfalık pencere 64 kuralı bölge/kat bilgisi olmadan döküp taşıyordu (sandbox sahte 3 kuralla çalıştığı için B'de görünmüyordu). v2 ile (bölge kalktı) kitap 3 sayfaya indi: giriş → tek tablo + açıklamalar → akredite lab'lar.
  - [ ] Değerleri her yerde `ctx.Rules.FormatValue` ile göster (harfli parametreler).
  - [x] Masaya gelişte `GreetingText`, karardan sonra `ReactionText` konuşma balonu (müteahhit portresiyle).
    - (B, 17 Ekim) `ClientSceneView`: portre koltukta, balon başın üstünde; tepki varsa sıradaki 1,5 sn sonra gelir.
  - [x] Hediye: `GiftId` doluysa "<müteahhit> size <GiftSO.DisplayName> bıraktı" + sadece "Kabul et" → `ctx.Actions.AcceptGift(id)`; kabul edilen hediyeler (`ctx.State.OfficeGifts` → `ctx.GetGift(id).Sprite`) masa ekranında ofiste kalıcı görünsün.
  - [x] Kabadayı reddedilince olay kartı + `sfx_glass_break` (O4).
    - (B) `d9d4aca`'da yapıldı.
- **Tarık (EEE) — içerik:**
  - [x] (EEE, 17 Ekim — Tarık kararı) **Karaktere özel konuşma sesi YOK.** Yerine: konuşma balonundaki yazı **harf harf** çıkar (daktilo efekti), arkasında **tek bir yazı sesi** çalar (Among Us tarzı kısa "bip"). Ses hazır: `Audio/sfx_typing.wav` (75 ms, özgün, script ile üretildi). Karaktere göre perde/ton ayrımı yapılmayacak; `ClientSO`'ya ses alanı gerekmez. Uygulama B'de (bkz. Oğuzhan bölümü).
  - [~] ~~🔊 **Konuşma sesi (Ahmet isteği, 17 Ekim):**~~ (yerine yukarıdaki karar) müteahhitler konuştuğunda (konuşma balonu açıldığında: gelişte `GreetingText` / zarf repliği, karardan sonra `ReactionText`) **ses çıkarsın.** Öneri: gerçek konuşma değil, kısa "mırıltı / bip" sesleri (Animal Crossing tarzı; harf başına ya da balon başına), **her karaktere farklı ton** (Rıfat kalın ve sakin, Nuri tiz ve hızlı, Rüstem boğuk...). CC0 kaynak + `CREDITS_AND_AI.md`. Hassasiyet: çığlık/ağlama yok.
    - Çalınması masa tarafında (Oğuzhan, `ClientSceneView` balonu). Karaktere göre ses seçmek için `ClientSO`'ya bir ses alanı gerekirse (ör. `Voice` klibi, `VoicePitch`) A'ya söyleyin, A ekler; sesleri ilgili müteahhit asset'ine Tarık bağlar.
  - [ ] El kitabı metinleri: `HandbookIntro` ve 5 parametrenin `Description`'ı (A'nın sade taslağı var, istersen düzelt). ~~Mahalle adları~~ gerekmiyor (mahalle kaldırıldı).
  - [x] ~~Her müteahhide `ApproveLines`, `RejectLines`, `BribeRefusedLines`, `ReturnLines` (2-3'er).~~ **→ Ahmet devraldı (Ahmet kararı, Tarık görselle meşgul).** 6 müteahhit için 56 replik yazıldı: okunur kopyası `ICERIK_TASLAK.md` → "Tepki replikleri (Ç4) — A taslağı"; asset'lere `CivilJam > Core > Fill Empty Client Lines` ile yazılır.
    - ⚠️ **Tarık:** bu replikleri **yeniden yazmana gerek yok**; istersen ton/üslup düzeltmesi yap. Düzeltmeyi doğrudan `Data/Clients/*.asset`'te (Inspector) yap ve `ICERIK_TASLAK.md`'deki kopyayı da güncelle. Araç sadece **boş** listeleri doldurur, senin düzeltmelerinin üzerine yazmaz. Yeni müteahhit eklersen onun repliklerini sen yaz (araç sadece bu 6 kişiyi bilir).
    - Kural: `ReturnLines`'taki `{last}` hem "onaylamıştınız" hem "reddetmiştiniz" ile okunur; cümleyi ikisine de uyacak kur. Dürüst ustalar ve Selim rüşvet vermediği için `BribeRefusedLines` boş.
    - [x] Hediye asset'leri (`Data/Gifts/`) ve hangi müteahhidin vereceği **hâlâ sende**. — (EEE, 17 Ekim) 6 `GiftSO` (`Data/Gifts/Gift_<Ad>.asset`), her müteahhide bir tane bağlandı (`ClientSO.Gift`, `GiftChance` 0.3 varsayılan): Rıfat → ahşap bina maketi, Sevim → saksıda kaktüs, Kemal → altın masa saati, Nuri → altın çerçeveli tablo, Selim → dolma kalem seti, Rüstem → balyoz biblosu. Görseller `Art/gift_*.png` (300x300, şeffaf, alta oturan, konturlu).
  - [ ] Hediye asset'leri: `Assets/_Project/Data/Gifts/` → Create → CivilJam → Gift (saat, tablo, masa isimliği...; `Id`, `DisplayName`, `Sprite`, `Note`) ve hangi müteahhidin vereceği (`ClientSO.Gift`). Bitince haber ver → A "Link Data" ile bağlar.
  - [ ] Yeni müteahhitler (10-12'ye) ve ~30 bina adı (Ç3, Ç5).

### 📌 Analiz sonrası işler (A, 17 Ekim gece) — ÖNCE BUNU OKU
Tam analiz ve yol haritası Ahmet'te (claude.ai artifact; bağlantıyı Ahmet paylaşır). Kararlar §2'de. Kodlar: K = kritik, Y = yüksek, O = orta, D = düşük.

**Başkasının dosyasına dokundum (kararlaştırıldığı gibi not):**
- `Flow/UI/EndingScreen.cs` (Onur): `case EndingType.MiddleQuota` → `MiddleCollapsed` (enum değişti, yoksa derlenmezdi) + yedek metin. Başka satır yok.
- `Flow/UI/ResultScreen.cs` (Onur): ayakta kalan binada `ResponsibilityText` gösteriliyor; kapsamda bina yokken ayrı cümle (O3). İki küçük blok, `(A, Core)` yorumlu.

**Onur (C):**
- [x] **Y4 Esc menüsü (C, 17 Ekim):** `Flow/UI/PauseMenu.cs` MainMenu, Game ve sandbox'larda otomatik kurulur; sahne kurulumu gerekmez. Devam / Ana menü / Çıkış; arka UI girdileri, intro ilerlemesi ve debug tuşları engellenir; zaman/ses geri yüklenir. EditMode 15/15 + Flow PlayMode 3/3. Son Windows build kontrolü bekliyor.
- [ ] **K4:** DayIntro / Construction / Save ekranları (önceki madde). Not: build'de artık mavi kutu çıkmıyor, ekranı olmayan faz atlanıyor; yani bunlar görsel kazanç, engel değil.
- [ ] `EndingScreen` ve `ResultScreen` değişikliklerime göz at; istersen kendi üslubunla düzenle.
- [ ] Kötü sonda "hâkim isimleri okuyor" anı: `ctx.State` yerine son afetin `VictimNames`'i gerekiyorsa A'ya söyle, Core'dan veririm.
- [ ] D4: Emeği Geçenler'e takım ve AI satırı.

**Oğuzhan (B):**
- [x] **Y1 öğretici:** masada sabit "Denetçi notu" paneli (değerleri min. değerle karşılaştır · iki belgede parsel/tarih/imza aynı olmalı · lab kaşesi raporun laboratuvarıyla aynı olmalı · şüphede ek test) + 1. gün kısa giriş.
  - (B, 17 Ekim) Sabit panel yerine üst bardaki "El Kitabı" butonuyla açılan pencere; içerik aynı 4 madde + yönetmelik alt sınırları. 1. gün masa açılınca 4 kart. Ayrıca lab raporunda parsel/tarih/imzanın yanında başvurudaki değer soluk yazılıyor (işaretleme yok).
- [x] **Y1 akredite lab listesi:** `ctx.Names.LabNames` = akredite laboratuvarlar. "Hızlı Sonuç Lab. Ltd." bu listede yok → oyuncu listeyle yakalar. Panelde göster.
  - (B, 17 Ekim) El Kitabı penceresinde "Akredite laboratuvarlar" bölümü.
- [x] **Y1 intro (C, 17 Ekim):** Oğuzhan'ın `02d4749` kodu GUID korunarak `Assets/_Project/Scripts/Flow/UI/IntroSequence.cs` içine taşındı. MainMenu Yeni Oyun → hikâye → Game; Devam Et doğrudan Game. Ayrı intro sahnesi ve Game.unity düzenlemesi gerekmiyor. Runtime Flow asmdef'ine `Unity.InputSystem` eklendi. Sel/yangın ve sayısal istatistikler çıkarıldı, kurgu notu eklendi; metnin son incelemesi Tarık'ta. Intro CREDITS satırını Oğuzhan güncelleyecek; C'nin Flow satırına eklenmedi. Test durumu: `ONUR.md`.
- [x] O4: Rüstem reddedilince masada "camın kırıldı" kartı + `sfx_glass_break` (Tarık'tan).
  - (B, 17 Ekim) Yapıldı (`EventCardView`); ses Tarık'ın `7243543`'ü.
- [x] O1: `OGUZHAN.md` "Mevcut durum" (kota satırı, "Sırada" eski).
  - (B, 17 Ekim) Güncellendi.

**Tarık (EEE):**
- [x] (EEE, 01:45: MiddleCollapsed son metni yazıldı — sayı varsaymaz; İyi sondaki "On dört gün" → "Günler boyunca", gün sayısı değişse de çelişmez) **Y2:** `Data/Endings/`: senin 3 asset'in (`3da4deb`) + A'nın `MiddleCollapsed.asset` ("Enkazın Gölgesi", Orta/yıkım sonu). **MiddleCollapsed'ın taslak metnini A yazdı** — son halini sen yaz (ton: suçlamasız, ciddi). Hepsi Game.unity'ye bağlı.
- [ ] (EEE, 02:00: soyadlar, roller, kabadayı maddesi, "dört son", repo linki ve Jüri kısayolları (F9/F10, build'de de çalışıyor) eklendi. KALAN (Pazar): video linki + oyun ekran görüntüsü — README'ye "eklenecek" diye boş yer açılmadı. C, 17 Ekim: Esc menüsü eklendi; son Windows build kontrolü bekliyor) **K3:** `README.md` taslağı A'da hazır (kurulum, kontroller, nasıl oynanır, ekip). Son halini sen ver: soyadlar, oyun görseli, video bağlantısı.
- [ ] (EEE, 02:15: SESLER TAMAM — `sfx_glass_break` (B bağlayacak), `mus_menu` (C), `mus_ending_sad` (C, Kötü + iki Orta son); hepsi CC0, CREDITS + Emeği Geçenler metninde. KALAN: görseller + portreler) O6: masa ve gazete görselleri final, müteahhit portreleri, `sfx_glass_break`, `mus_menu`, `mus_ending_sad`.

Ajanlar: oturum başında bu bölümü oku; ilgili maddeyi tamamlayınca kendi kişisel dosyana not düş ve buradaki maddeyi `[x]` yap. Ayrıntılı Core kullanım rehberi: `AHMET.md` → "B ve C için Core kullanım rehberi".

**Oğuzhan (B / Masa) için:**
- [ ] **(EEE → B, 17 Ekim) Kitaplar yenilendi:** `book_rules.png` (600x577) ve `book_lab.png` (600x546) ChatGPT ile yeniden üretildi, şeffaf, aynı dosya adları (eski 600x400'tü, artık daha kare). İki istek:
  1. **Boyut:** masadaki kitap butonlarının boyutunu yeni orana göre ayarla (preserveAspect açık, basıklık yok ama kutu boyu artık fazla/eksik olabilir). Masanın köşelerinde, masa eşyalarıyla (kupa, kalemlik, kaşe) orantılı dursunlar.
  2. **Fiziksel görünüm:** kitaplar masaya konmuş gibi dursun, ekrana yapıştırılmış UI gibi değil: altına yumuşak bir gölge (yarı saydam koyu elips/blur, kitabın altında biraz sağa-aşağı), masanın perspektifine uygun hafif eğim/döndürme (sol kitap hafif saat yönü tersine, sağ kitap hafif saat yönü), üzerine gelince hafif kalkma (scale/offset) tıklanabilir olduğunu belli etsin.
  - ⚠️ **Tarık — B'den not (Oğuzhan, 17 Ekim öğle): kitapları yeniden çizmen gerekiyor; masa tarafı seni bekliyor.**
    - Oğuzhan istenenleri kodla denedi (henüz push'lanmadı): kitaplar 250 px, altında iki katlı gölge (geniş yumuşak + dar temas gölgesi), sıcak ton, hafif eğim (sol +4° / sağ −4°), üzerine gelince kalkma. Yine de **"ekrana yapıştırılmış" gibi duruyorlar.** Sorun kodda değil, çizimde:
      1. **Bakış açısı uymuyor:** kitaplar yukarıdan, 3/4 açıyla çizilmiş; `desk_bg`'deki masa ise alçak, neredeyse karşıdan görünüyor. Kitaplar masada yatmıyor, ekrana doğru dikilmiş gibi. Masadaki kupa, kaşe ve isimlikle **aynı alçak açıdan**, masaya yatık konmuş halde çizilmeli (kapak yazısı okunacak kadar görünsün).
      2. **Işık uymuyor:** masa soldan gelen sıcak, turuncu gün ışığında; kitaplar soğuk ve kendi ışıklı. Aynı sıcak ışık ve gölge yönüyle (ışık soldan, gölge sağa düşer) çizilmeli.
      3. **Kontur ve doku:** masa eşyalarındaki kalın koyu kahve kontur ve cel gölge tarzında olsun (tarz kararı: el çizimi 2D cartoon).
    - Teknik: aynı dosya adları (`book_lab.png` LAB RAPORU, `book_rules.png` EL KİTABI), şeffaf arka plan, ~600 px genişlik; oran serbest (kod oranı görselden okur). **Gölgeyi görsele çizme**, masa tarafı kodla ekliyor. Sol alt köşede kupa/kaşe, sağ altta kağıt/kalem var; kitaplar bunların önüne konmuş gibi duracak.
    - Bitince haber ver → Oğuzhan masa yerleşimini yeni görsellere göre bitirip push'lar.
    - ✅ **(EEE, 17 Ekim, 2. sürüm) Kitaplar sıfırdan çizildi + masa sadeleştirildi, aynı dosya adları:**
      - Kitaplar masada yatan 3B nesne: kapak (alçak açı, uzak kenar dar), sayfa bloğu/kalınlık, sırt; kalın kahve kontur, soldan sıcak ışık. 640x320 şeffaf, gölge ÇİZİLMEDİ (kod ekliyor).
      - `desk_bg.png` sadeleşti: kaşe+ıstampa, yapışkan notlar, sol kağıtlar, sağ kalem ve proje kağıdı silindi (ahşap dokusuyla dolduruldu). Kalanlar: kupa + kalemlik (sol arka), isimlik (orta). `desk_front.png` bu temiz masadan yeniden üretildi.
      - Önerilen yerleşim (1920x1080 ekranda, sol-üst köşe x,y ve boyut): **LAB RAPORU** x≈238, y≈815, 480x240 (kupa ile isimlik arası); **EL KİTABI** x≈1320, y≈812, 500x250 (sağ boş alan). Gölge: kitabın altında, alt kenara yakın yatay yumuşak elips (~%30 koyu).
      - Masa yerleşimini bitirebilirsin.
  - [x] **(EEE → B, 17 Ekim) Harf harf yazı + yazı sesi:** konuşma balonu (`GreetingText`, zarf repliği, `ReactionText`) metni bir anda değil **harf harf** yazsın (öneri ~35-45 harf/sn; boşluk ve noktalamada ses çalmasın; balona tıklanınca metin anında tamamlansın). Her görünen harfte (ya da 2 harfte bir) `Audio/sfx_typing.wav` çalsın; tekdüze olmasın diye perdeyi her seferinde küçük rastgele oynat (ör. 0.95-1.05). Karaktere özel ses yok.
    - (B, 17 Ekim öğleden sonra) Yapıldı, Sandbox'ta test edildi: `ClientSceneView` 40 harf/sn (`maxVisibleCharacters`; balon baştan tam boyutta), her 2 sesli harfte bir `CharTyped` → `DeskAudio.PlayTyping` (ayrı kaynak, perde 0.95-1.05, ses 0.45, klip baştan yüklenir); boşluk/noktalamada ses yok; `.!?` sonrası 0.18 sn, `,;:` sonrası 0.08 sn duraklama; balona tıklayınca anında tamamlanır. Tepkide sıradaki müteahhit yazı bitince gelir. Ayarlar Inspector'da (`ClientSceneView`, `DeskAudio`).
  - [x] **(EEE → B, 17 Ekim) Hediyeler ofiste:** 6 hediye görseli hazır (`Art/gift_*.png`, 300x300). Kabul edilen hediyeler (`ctx.State.OfficeGifts` → `ctx.GetGift(id).Sprite`) masada/rafta kalıcı görünsün; masadaki eşyalarla orantılı (~140-180 px) ve kitaplar gibi alt gölgeli.
    - (B, 17 Ekim öğleden sonra) Yapıldı, Sandbox'ta test edildi: onaydan sonra `GiftId` doluysa tepki balonuyla aynı anda hediye kartı (`UI/GiftOfferView.cs`, piksel tasarım: görsel + "<müteahhit> size bir <eşya> bıraktı." + `Note` + sadece KABUL ET) → `AcceptGift` → eşya masanın uzak kenarında kalıcı (`UI/OfficeGiftsView.cs`, 160 px, 6 yer, kabul sırasıyla; kitaplar gibi gölge + üzerine gelince kalkma; yeni gelen yaylanarak konur). Gün başında (kayıttan da) `OfficeGifts` masaya konur. Sandbox: Hilmi (dürüst) her onayda henüz alınmamış bir hediye getirir.
- [x] `CivilJam.Desk.asmdef` referanslarına `CivilJam.Core` ekle.
- [x] `DeskScreen` → `CivilJam.Core.IGameScreen` uygulasın: `Phase => GamePhase.Desk`, `Show(GameContext ctx)`, `Hide()`, `event Action Completed` (parametresiz; mesai bitince bir kez tetikle).
- [x] Projeleri `ctx.TodayProjects`'ten oku (Desk modelleri yerine ya da adapter ile). Parametrelerde `Reported` göster; `Tested` ise `Actual` da göster. `Documents[0]` = başvuru, `Documents[1]` = lab raporu (parsel/tarih/lab/kaşe/imza tutarsızlıkları bedava ipucu). Rüşvet: `OffersBribe`, `BribeAmount`. Yönetmelik kitapçığı: `ctx.Rules`. Ek test ücreti: `ctx.Balance.ExtraTestCost`.
- [x] Kararları sadece `ctx.Actions` ile gönder: `Approve(id, acceptBribe)`, `Reject(id)`, `BuyExtraTest(id)` (bool döner). Para/kota hesabını Desk yapmaz; HUD için `ctx.StateChanged`'e abone ol, `ctx.State.Money` oku.
- [x] Bitince haber ver: DeskScreen prefab'ını `Game.unity`'ye A ekler (Game.unity'yi sadece A kaydeder).

**Oğuzhan'a cevaplar (A, 22:40) — `b614831` Desk entegrasyonu için:**
- DeskScreen `Game.unity`'ye eklendi (Canvas 1920×1080 Match 0.5 + EventSystem / Input System UI Input Module). Kurulum aracı: menü CivilJam > Core > Install Screens Into Game Scene. Sahne testi (`GameSceneTests`) geçiyor; Game.unity'de Desk fazı artık senin ekranınla açılır.
- **Kaşe (StampId):** görsele eşlenmeyecek, metin olarak kalıyor; sen bir şey değiştirme. Core'da hata vardı: `StampId` `"STAMP_OK"/"STAMP_FAKE"` geliyordu ve masada sahte belgeyi açık ediyordu. Düzeltildi: başvuru kaşesi `"BELEDİYE · İMAR MÜDÜRLÜĞÜ"`, lab kaşesi `"<LAB ADI> · AKREDİTE"`. Sahte belge ipucu (WrongStamp): kaşedeki lab adı raporun başlığındaki `LabName` ile uyuşmaz. WrongSignature artık lab raporunu farklı bir mühendisin imzalaması (başvurudaki `SignatureName` ile karşılaştırılabilir).
- **Kota:** kota kuralı KALDIRILDI (karar). Masada kota uyarısı gösterme.
- [x] `DeskHud` ve `DeskSandboxBootstrap`'tan `Balance.ApprovalQuota` kullanımını kaldır. İstersen yerine cezasız bilgi olarak "Bugün onaylanan: `ctx.State.ApprovedToday`" göster. Kaldırınca haber ver; A `BalanceSO.ApprovalQuota`'yı siler.
  - (B, 23:10) Kaldırıldı; Desk artık `ApprovalQuota` kullanmıyor. HUD "Bugün onaylanan: N". A `BalanceSO.ApprovalQuota`'yı silebilir.
  - (A, 23:20) `BalanceSO.ApprovalQuota` silindi.
- [x] **TextMesh Pro klasörü (A, 23:30):** `Assets/TextMesh Pro` artık repoda (`701cb5e`); sen commit'leme. Sende yerel kopya varsa pull "untracked working tree files would be overwritten" hatası verir. Çözüm: Unity'yi kapat → yerel `Assets/TextMesh Pro` klasörünü ve `Assets/TextMesh Pro.meta`'yı sil (ya da masaüstüne taşı) → `git pull` → Unity'yi aç. Güvenli: TMP Essential dosyaları ve GUID'leri her projede aynı, prefab font referansları kırılmaz. İstisna: TMP Settings'te bilerek değişiklik yaptıysan (varsayılan font, kendi Türkçe font asset'in) silmeden önce not al, pull'dan sonra tekrar yapıp commit'le.
  - (B, 23:30) Yerel kopya kaldırıldı (GUID'ler aynıydı), pull yapıldı; Unity'de Sandbox_Desk ve Game.unity sorunsuz.

**Tarık (EEE) için (A, 23:40) — `ICERIK_TASLAK.md` müteahhitleri:**
- [x] ClientSO asset'lerini **sen oluştur** (metinler senin; düzeltmeleri de sen yapacaksın). Project'te sağ tık → Create → CivilJam → Client. Konum: `Assets/_Project/Data/Clients/`, dosya adları `Rifat.asset`, `Sevim.asset`, `Kemal.asset`, `Nuri.asset`, `Selim.asset`, `Rustem.asset`.
- [x] `Id` alanı taslaktaki gibi küçük harf: `rifat`, `sevim`, `kemal`, `nuri`, `selim`, `rustem` (Core müşteriyi bu Id ile bulur). Selamlamalar → `GreetingLines`, rüşvet repliklikleri → `BribeLines`, portre → `Portrait`.
- [ ] `Data/` altındaki diğer asset'lere (Balance, RuleSet, Days, Expenses, NamePool) dokunma. Headlines asset'ini doldurmak istersen önce A'ya haber ver.
  - (EEE, 00:15) A ile konuşuldu, Headlines.asset EEE tarafından dolduruldu: 20 manşet, `ICERIK_TASLAK.md` ile aynı. Eski "{count} can kaybı" kullanımı kaldırıldı.
- [x] (A, `5097e93`: Game.unity'ye bağlandı, 6 müteahhit; rol mantığı Core'da) (EEE, 00:00: 6 asset push'landı, A bağlayabilir) Bitince haber ver: asset'leri `Game.unity`'deki GameManager'ın Clients listesine **A** bağlar (sahne A'nın). Bağlanmadan oyunda görünmezler.
- **Selim sorusunun cevabı:** `IsHonest = false`, `BribeRange = 0-0` **doğru**, öyle bırak. Kural: `BribeRange` 0-0 = asla rüşvet teklif etmez. Core müteahhidin rolünü bu alanlardan çıkaracak, sen başka alan doldurmayacaksın:
  - Uygun proje → `IsHonest = true` (Rıfat, Sevim)
  - Rüşvetli proje → dürüst değil + `BribeRange` > 0 (Kemal, Nuri; kabadayı günleri açıksa Rüstem)
  - Sahte belgeli proje → dürüst değil + `BribeRange` 0-0 (Selim)
  - Düz uygunsuz (ilk günler) → dürüst değil + rüşvet vermeyen
  - ✅ (A, `5097e93`) Core rol mantığı uygulandı ve testli: Selim'e zarf düşmez, Kemal/Nuri rüşvetsiz proje getirmez, Rüstem sadece kabadayı günlerinde.
- Not (`{deaths}` ve Kötü sonda kayıp isimleri): şimdilik gerekmiyor; Core'da ayrıca konuşulacak.
- [x] (EEE, 00:40) Headlines.asset 20 manşetle dolduruldu; eski "{count} can kaybı" kaldırıldı. A kontrol etti: tamam. Kural: `{count}` = yıkılan **bina** sayısı (sadece MultiCollapse), `{building}` = bina adı. Credits ekranı → Onur'a sorulacak (metin `CREDITS_AND_AI.md` sonunda).
- [x] (EEE `3da4deb`: `Good`, `Bad`, `MiddleClosed` push'landı; A analiz commit'inde Game.unity'ye bağladı. `MiddleCollapsed` A'nın taslağı.) **Son ekranları.**
  - ⚠️ (A) K2 kararıyla İyi son artık **sadece hiç bina yıkılmadıysa** geliyor. `3da4deb`'de İyi son metninden çıkardığın "imzanı taşıyan binaların hepsi ayaktaydı" cümlesi artık doğru; istersen geri koy. Kötü sondaki "rüşvet varsaymaz" düzeltmen hâlâ geçerli (hapis rüşvetsiz yıkımda da çıkabilir).
- ✅ **Çakışma çözüldü (EEE, 23:50):** Kaşe/imza ipuçları A'nın 22:50 versiyonuyla kalıyor (5 ipucu). 23:15 "kaldırıldı" kararı geri alındı; A kodu değiştirmeyecek.

**Onur (C / Akış) için:**
- ⚠️ **(EEE → A, 17 Ekim) Ahmet: Game.unity'ye yeni ekran.** `GamePhase.Save` için ekran yoktu, IMGUI yedek arayüzü çıkıyordu. `SaveScreen` eklendi ve `CoreSetupBuilder.InstallScreens` listesine yazıldı. **Menüden `CivilJam > Core > Install Screens Into Game Scene` çalıştırıp Game.unity'yi push'la** (sahne senin). Sonra gün sonunda yedek arayüz yerine yeni ekran açılır.
- ℹ️ **(EEE → C, 17 Ekim) Senin klasörüne yeni ekran:** `Scripts/Flow/UI/SaveScreen.cs` + `Prefabs/Flow/SaveScreen.prefab` (FlowScreen tabanı, UI kodla kurulur). Gece ofisi arka planı (`Art/save_bg.png`), ortada kart: "{biten gün}. Gün kaydedildi", kısa metin, kasa/borç, yamuk kırmızı "KAYDEDİLDİ" kaşesi, buton "{yeni gün}. güne geç →". Not: Save fazında Core günü zaten artırmış olur, biten gün = `Day - 1`.
- ⚠️ **(EEE → C, 17 Ekim) Senin iki dosyana dokunuldu (Tarık onayıyla), pull et:**
  - `Prefabs/Flow/DayIntroScreen.prefab`: kök Image artık `Art/dayintro_bg.png` (sabah ofisi, bulanık/loş), renk opak beyaz (eskisi yarı saydam beyaz, varsayılan UI sprite), Image Type = Simple. Başka bir şey değişmedi.
  - `Scripts/Flow/UI/MainMenuScreen.cs`: Emeği Geçenler paneli iki sütunlu oldu (`CreditsLeft`: İMZA, EKİP, YAPAY ZEKÂ; `CreditsRight`: SESLER VE MÜZİKLER, GÖRSELLER, YAZI TİPLERİ), başlık + çizgi, panel ekranın %84'ü, Geri butonu altta. Metin `CREDITS_AND_AI.md` sonuyla aynı. Eski `CreditsText` kaldırıldı.
- [x] (A, `0bebf9b`) Flow ekranları oyunda: prefab'lar Game.unity'ye eklendi (Install Screens). Tamamlananlar (A koddan doğruladı): Flow derlemesi, `.meta`'lar, prefab'lar + MainMenu + Build Settings, Sandbox_Flow, manşet `{count}`, Emeği Geçenler, ekranlar `IGameScreen` (Result / DaySummary / DailyExpenses / Ending), gazete `ctx.LastQuake` alanları, ödeme `SetExpensePaid`, MainMenu `SaveSystem.HasSave` + `GameLaunch.ContinueFromSave`. Game.unity'yi kendin açıp KAYDETME.
- [ ] **Sıradaki iş (Ahmet kararı, `0916894`): sahibi olmayan fazlar senin.** Şu an mavi yedek arayüzle görünüyorlar: `GamePhase.DayIntro` (gün başı kartı: "3. GÜN", para/borç), `GamePhase.Construction` (inşaat geçişi; gazeteden önce kısa bir an), `GamePhase.Save` (gün sonu "kaydedildi" kısa geri bildirimi). Hepsi `IGameScreen` (`FlowScreen` tabanı uygun); ekranı olan faz için mavi kutu kendiliğinden kaybolur. Prefab'ları üretip push'layınca haber ver → A Game.unity'ye ekler.
- Bilgi: son ve borç kuralları kesinleşti (§2). Son ekranı `ctx.State.Ending` (Good / Bad / MiddleClosed / **MiddleCollapsed**) + `ctx.Endings` (4 EndingSO bağlı). Borç varsa ödeme ekranına `debt_payment` kalemi gelir.

### Açık kararlar (öneriler kesinleşmiş değildir)
- JailRisk sayılarının (yıkım +0.15, rüşvetli yıkım +0.25) 10 günlük oyunda dengesi; denge turunda bakılacak.

## 7. Karar günlüğü

- 2026-10-17 (C): Oğuzhan'ın hikâye intro'su Flow'a devredildi; Yeni Oyun için MainMenu içinde gösterilir, bitince mevcut Game sahnesi yüklenir. Devam Et intro'yu atlar. Günlük DayIntro ekranından ayrıdır.
Eski satırlar tarihseldir; 21:09 güncellemesi level/ay sonunu, Ahmet’in 22:00 notu da haftalık ödemeyi kaldırır.

| Tarih-saat | Karar | Gerekçe |
|---|---|---|
| 2026-10-16 | Oyun: "İmza", masa başı mühendis onay oyunu | Tema (afete hazırlık) ile doğrudan bağ, fareyle oynanabilir kapsam |
| 2026-10-16 | Level akışı LevelIntro→Desk→Construction→Quake→Result→MonthEnd | Tek döngü, ekran ekran ayrılabilir iş |
| 2026-10-16 | Strength >= 1 asla yıkılmaz; aksi halde gürültülü Intensity kıyası | Dürüst oyuncu cezalandırılmaz, belirsizlik korunur |
| 2026-10-16 | Deprem şiddeti gizli, bina 2 durumlu | Basit görsel, net sonuç |
| 2026-10-16 | İki katmanlı sahte belge (bedava ipucu + ücretli ek test) | Dikkat ödüllendirilir, ekonomiye bağlanır |
| 2026-10-16 | Kalıcı JailRisk, sayılar BalanceSO'da | Birikimli sonuç, kolay denge ayarı |
| 2026-10-16 | Retry'da yeni seed | Ezber yerine okuma |
| 2026-10-16 | Save: tek slot JSON, sadece ay sonundan sonra, sadece GameState | Minimum kapsam, tutarlı kayıt noktası |
| 2026-10-16 | 3 son: İyi / Kötü / Orta | Oyuncu seçimlerinin net karşılığı |
| 2026-10-16 | Yıkım nedeni metinleri otomatik şablondan | Elle içerik yükü yok |
| 2026-10-16 | Tek Game sahnesi + prefab ekranlar, IGameScreen | Merge çakışmasını azaltır |
| 2026-10-16 | Saf C# mantık; Core/Desk/Flow asmdef, Desk-Flow birbirini görmez | Paralel çalışma, test edilebilirlik |
| 2026-10-16 | Klasör sahipliği; Game.unity sadece A | Sahne/prefab çakışmalarını önler |
| 2026-10-16 | Sadece Türkçe; hassasiyet kuralları | Kapsam + konunun ağırlığı |
| 2026-10-16 21:09 (UTC+3) | Level kaldırıldı; gün sistemi. Afetsiz gün sonrası afet ihtimali artar | Onur tarafından kesinleştirildi; oranlar henüz açık |
| 2026-10-16 21:09 (UTC+3) | Giderler haftalık; kayıt gün sonunda | Ay sonu ödeme/kayıt akışının yerini alır |
| 2026-10-16 21:09 (UTC+3) | C rolü Onur Kabadayılar; kişisel çalışma planı ONUR.md | Plan aşamasında sorumluluk ve açık karar takibi |
| 2026-10-16 | Desk kendi modelleriyle çalışır, Core gelince adapter ile bağlanır | A'yı beklemeden ilerlemek, Core değişikliğinin etkisini tek dosyada tutmak |
| 2026-10-16 | Desk prefab'ı ve Sandbox_Desk editor menüsüyle üretilir | Sahne/prefab YAML'ını elle yazmak kırılgan; üretilen iskelet Editor'de düzenlenebilir |
| 2026-10-16 22:30 (UTC+3) | Desk adapter yerine doğrudan Core tiplerini kullanır; sandbox'ta sahte IGameActions (yukarıdaki adapter kararının yerini alır) | Core erken geldi; tek model, sandbox ve oyun aynı kod; "ekranlar sadece IGameActions ile yazar" kuralına uyum |
| 2026-10-16 21:30 (UTC+3) | A rolü Ahmet İlman; kişisel plan AHMET.md. Ekranlar Core'a sadece IGameActions ile yazar | Para/gün/sonuç tek yerde değişir; çift ödeme/gün ilerlemesi önlenir |
| 2026-10-16 22:00 (UTC+3) | Ödeme haftalıktan günlüğe çekildi (21:09 haftalık gider kararının yerini alır) | Ahmet kararı; ödenmeyen giderin sonucu açık |
| 2026-10-16 22:00 (UTC+3) | Afet son afetten bu yana onaylanan binaları değerlendirir; afet olunca olasılık sıfırlanır | Onur'un sorularına Ahmet'in cevabı (AHMET.md) |
| 2026-10-16 22:00 (UTC+3) | Afetsiz gün gazete yok; afette yıkım durumu, ölen isimleri, ölü/yaralı sayısı, oyuncu sorumluluğu gösterilir | Temanın ağırlığı sonuç ekranında; Core'da BuildingOutcome genişletilecek |
| 2026-10-16 22:30 (UTC+3) | Görsel/ses önce placeholder; final aynı dosya adıyla üzerine yazılır. Sprite Mode Single, PPU 100, ses `.ogg` | .meta/GUID korunur, B/C prefab referansları bozulmaz |
| 2026-10-16 22:30 (UTC+3) | `weekly_bg.png` → `payment_bg.png` (günlük ödeme kararına uyum) | Henüz hiçbir sahne/prefab referans vermiyordu; GUID korundu |
| 2026-10-16 22:45 (UTC+3) | Deprem sahne olarak gösterilmez, gazeteden öğrenilir; Quake ekranı ve deprem/yıkılma sesleri kaldırıldı | Takım kararı (EEE iletti); hassasiyet + kapsam. GameManager akışı ve QuakeResult verisi etkilenmez, sadece ekran yok |
| 2026-10-16 22:15 (UTC+3) | Afet olasılığı taban %50, tavan %100; afet günü sadece gazete, afetsiz gün sadece gün özeti | Ahmet kararı; Core'da uygulandı (`GamePhase.Quake` silindi) |
| 2026-10-17 `61353fd` | "Yeni kural modeli" (`62165c0`, Yapı Denetim Formu) REDDEDİLDİ, commit geri alındı; mevcut model (Reported/Actual, ek test, sahte belge ipuçları) geçerli | Ahmet onay vermemişti; Core ve Desk'in yeniden yazılması gece yarısı riskli |
| 2026-10-16 22:40 (UTC+3) | Kota kuralı yok; baskıyı günlük giderler kurar | Ahmet kararı (seçenek A). Ödenmeyen giderin sonucu sonra kararlaştırılacak; ağır olmalı |
| 2026-10-16 22:40 (UTC+3) | Belge kaşesi (StampId) görsel değil okunabilir metin; sahte kaşe = başlıktaki lab adıyla uyuşmayan kaşe | Görsel iş yükü yok; ipucu okuyarak yakalanır, veri gerçek/sahte bilgisini sızdırmaz |
| 2026-10-16 22:15 (UTC+3) | Ekranı olmayan faz için GameManager IMGUI yedek arayüzü gösterir | Desk/Flow beklenmeden Game.unity baştan sona oynanabilir; ekran eklendikçe kendiliğinden devre dışı |
| 2026-10-16 23:15 (UTC+3) | Kaşe/imza ipucu kaldırıldı; sahte belge ipuçları: parsel, tarih, laboratuvar. Kaşe ve imza oyunda kalır, hep gerçek | `STAMP_FAKE` yazısı ipucunu doğrudan veriyordu, imzadaki nokta fark edilemiyordu; ek görsel yükü yok (EEE kararı) |
| 2026-10-16 23:50 (UTC+3) | 23:15 kararı geri alındı: kaşe/imza ipuçları A'nın versiyonuyla kalır (lab adı uyuşmazlığı, farklı mühendis imzası). 5 ipucu | A'nın düzeltmesi iki sorunu da çözdü (açık STAMP_FAKE yazısı, görünmeyen nokta); görsel yükü yok (EEE kararı) |
| 2026-10-17 `4f33750` | (GERİ ALINDI `cbd8587` — kural modeli reddedildi) EEE içerikleri yeni kural modeline uyarlanmıştı: Selim "kurnaz", Sevim/Selim replikleri, manşet ve sonlarda afet-türünden bağımsız dil | Model Ahmet tarafından onaylanmadı; içerik mevcut modele (deprem, lab raporu, sahte belge) geri döndürüldü |
| 2026-10-17 `4f33750` | (ÖNERİ, KARAR DEĞİL) Görsel stil: piksel art, toprak tonları | Tarık açıkça onaylamadı; Claude referans görsellere bakarak yazmıştı. Karar Tarık'ta. Not: ilk referans olan `Docs/Tasarim` görselleri `61353fd` ile kaldırıldı (geçmişte `62165c0`) |
| 2026-10-17 (gece) | Görsel stil KARARI: piksel art YOK (yukarıdaki öneri iptal). Final görseller hazır, yeniden dağıtılabilir lisanslı (tercihen CC0) asset'lerden; mümkünse tek kaynak / tek stil ailesinden | Tarık piksel art araçlarını bilmiyor, kalan süre ~12 saat; görsel bütünlük için tek stil ailesi (EEE kararı) |
| 17 Ekim (öğlen) | Görsel tarz GÜNCEL: "İyi Pizza, Güzel Pizza" referansı KALDIRILDI. Tarz: el çizimi 2D cartoon (kalın koyu kahve kontur, yumuşak cel gölge, sıcak palet), görseller Gemini/ChatGPT ile ortak tarz prompt'uyla üretilir. Aşağıdaki satırdaki akış (önden oturan müteahhit, konuşma balonu, yürüme yok) geçerli | Tarık kararı. Görsel bütünlük için tüm ekranlar aynı tarz prompt'uyla |
| 2026-10-17 (gece) | (TARZ KISMI ESKİDİ, bkz. üst satır) Görsel tarz referansı: "İyi Pizza, Güzel Pizza" (Good Pizza, Great Pizza). Düz renkli, kalın konturlu, sevimli çizgi film (flat cartoon). Masa önden görünür; müteahhit yandan kayarak masanın önüne gelir, hafif sallanır, replik konuşma balonunda çıkar, sonra çıkar. Yürüme animasyonu/sprite sheet YOK, tek karakter resmi yeterli. Masa, menü ve son görselleri de aynı tarzda. Oyunun kendi görselleri kopyalanmaz, sadece tarz referansıdır | Tarık kararı. Repliklerin mizahıyla uyumlu, düşük animasyon maliyeti. Masadaki kayma/balon B'nin (Oğuzhan) işi, onayı bekleniyor |
| 2026-10-17 `3da4deb` | Son metinleri kod koşullarına uyduruldu: İyi son "binaların hepsi ayaktaydı" demez (İyi son şu an yıkım olsa da hapis yoksa geliyor); Kötü son rüşvet varsaymaz (rüşvetsiz yıkımla da hapis var); Orta son dürüstlük varsaymaz | Metin oyuncuya yanlış bir şey söylememeli. A'ya öneri: İyi son için "hiç yıkım yok" şartı + yıkımlı ama hapissiz durum için ayrı son (EEE yazar) |
| 17 Ekim (analiz sonrası) | Borç kuralı: kasa eksiye düşmez, yetmeyen zorunlu gider borca; borç > 2.000 TL → ofis kapanır (Orta son). Başlangıç 2.500 TL, onay ücreti 400-700 | Ahmet kararı (K1). "Her şeyi reddet" stratejisi artık 7. gün ofisi kapatıyor |
| 17 Ekim (analiz sonrası) | Sonlar ve öncelik: Kötü (hapis) > Orta/ofis kapandı > Orta/yıkım (yeni `MiddleCollapsed`) > İyi (hiç yıkım yok) | Ahmet kararı (K2). İyi son metni "binaların hepsi ayaktaydı" ile tutarlı |
| 17 Ekim (analiz sonrası) | Oyun 10 gün; fabrika/yangın ve sigorta kapsam dışı | Ahmet kararı. Jüri süresi; yarım kalmış yangın kuralı ve etkisiz sigorta oyuncuyu yanıltıyordu |
| 17 Ekim (analiz sonrası) | Son ekranına girince kayıt silinir; build'de ekranı olmayan faz otomatik geçilir (mavi kutu sadece Editor/Development Build); F9/F10 demo tuşları | A (Y3, K4, O7) |
| 17 Ekim `2118715` sonrası | Denge (senaryo E): ücret 300-500, kira 600, deprem %20 +%15/gün | Ahmet kararı, simülasyonla seçildi: dikkatli oyuncuda para gerilimi, tek zarf yazı-tura, deprem her gece değil |
| 17 Ekim oynanış testi | Ç1 el kitabı (bölge + kat + 8 parametre, masada "min." yok), Ç2 uygunsuz proje karışımı, Ç3 gevşek müteahhit rolleri, Ç4 tepkiler + birkaç etki (hediye, Nuri zarf artırır, Rüstem cam), Ç5 isim çeşitliliği | Ahmet oynadı: karşılaştırma çok kolay, isimden tür tahmin ediliyor, sahte belge hissedilmiyor, isimler tekrar ediyor. Sırası: Ç5+Ç2+Ç3 → Ç1 → Ç4 |
| Ç1 Core | Deprem bölgesi 1 = en riskli (en sıkı değerler); kat grupları 1-4 / 5+; projede 5-6 parametre; harfli değerde yetersizlik 1-2 kademe | A. Simülasyon: dengeye etki yok (dikkatli %100 İyi; tek zarf %41/%45/%14) |
| Ç4 Core | Tepki/selamlama Core'da seçilir (ProjectData metinleri); hediye onayda GiftChance ile, oyunda bir kez; Nuri her retten sonra +250 TL zarf | A. Replik listesi boşsa metin boş |
| 17 Ekim öğle | El kitabı v2: mahalle/bölge tamamen kaldırıldı; 5 parametre (kolon kalınlığı, temel derinliği, beton sınıfı, demir tipi, zemin türü), gerekli değer kat grubuna göre (1-3 / 4-6 / 7+) | Ahmet kararı: daha az ama daha anlaşılır ve keyifli parametre; tek tabloda arama. Denge simülasyonla korundu |
| 17 Ekim öğle | Demir tipi kat grubuna göre 3 farklı tür (B420 / B500 / B500S); B220 düz hiçbir binada kabul edilmez; gerçek sınıf kodları | Ahmet kararı: tek tip demir parametreyi anlamsızlaştırıyordu. Denge korundu (tek zarf %45/%45/%10) |
| 17 Ekim öğleden sonra | Belge tasarımı: Yapı Denetim Formu + Denetçi El Kitabı görünümü (Docs/Tasarim), içerik v2; başvuru → Yapı Denetim Formu, lab raporu ayrı kalır | Ahmet kararı. Tasarımın örnek içeriği (yangın/sel/kaçak kat) reddedilmiş modelden; kullanılmaz |
| 17 Ekim öğleden sonra | Hediyeyi sadece dürüst ustalar verir (Core: `IsHonest`) | Ahmet kararı: rüşvetçinin hediyesi zarfla karışıyordu. Tarık'ın 6 GiftSO'su Game.unity'de; rüşvetçilerinkiler kullanılmaz |

## Güncelleme kuralı
- Önemli bir karar, mimari değişiklik, yeni kural veya tamamlanan büyük iş olduğunda ilgili bölümü güncelle ve karar günlüğüne tarih-saatle satır ekle.
- Dosyayı kısa tut; kod kopyalama, sadece karar ve yönlendirme yaz.
