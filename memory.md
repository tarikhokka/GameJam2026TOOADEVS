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
- Her gün sonunda afet olasılığı vardır. Taban %50, tavan %100 (karar). Afet olmazsa sonraki gün olasılık artar (öneri +%10); afet olunca tabana (%50) döner.
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
- Giderler her gün ödenir: kira 400 + kredi 300 (zorunlu), kabadayı cam tamiri (olursa). Ek test gider değildir, anında kasadan düşer. **Sigorta kapsam dışı** (karar 17 Ekim, etkisi yoktu).
- **Borç kuralı (karar 17 Ekim):** kasada yetmeyen gider ödenemez, zorunluysa borca yazılır; kasa asla eksiye düşmez. Borç varsa isteğe bağlı "Borç ödemesi" kalemi çıkar (kısmen ödenebilir). **Borç 2.000 TL'yi aşarsa ofis kapanır → Orta son.** Başlangıç parası 2.500 TL, onay ücreti 400-700 TL. Her şeyi reddeden oyuncu 7. gün ofisini kaybeder (testli).
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
  - `UI/DeskAudio.cs` — masa sesleri: tık, kağıt, kaşe + ofis müziği (döngü, %25); klipler prefab'da referansla bağlı.
  - `UI/` diğerleri — DeskHud, ProjectQueueView/Item, DocumentView (karar kaşesi animasyonu dahil), ParamRowView, ActionPanel, UiPool, DeskFormat (parametre adı/birimi RuleSetSO'dan).
  - `Sandbox/FakeGameActions.cs` — sadece sandbox için sahte IGameActions.
  - `Sandbox/SandboxDeskContext.cs` — sahte gün: 3 proje (dürüst, rüşvetçi, sahte belgeli), 3 müşteri, RuleSet.
  - `Sandbox/DeskSandboxBootstrap.cs` — Sandbox_Desk'i başlatır, mesai bitince sonucu loglar.
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
- **Flow (C, 00:17):** Gazete, gün özeti, günlük ödeme, son ekranı prefab'ları + MainMenu (Emeği Geçenler dahil) + Sandbox_Flow; manşet `{count}` düzeltildi. A `0bebf9b`'de Game.unity'ye ekledi.
- **Art/Audio/İçerik (EEE, 00:40):** 13 placeholder sprite + ending görselleri; masa sesleri (`sfx_click/stamp/paper`, `mus_office_ambient` CC BY 4.0 → credits MainMenu'de var); 6 ClientSO; Headlines.asset 20 manşet; son metinleri `ICERIK_TASLAK.md`'de. Stil: piksel art, toprak tonları.
- **Sırada:** bkz. aşağıda "📌 Analiz sonrası işler" (kişi bazlı). Özet: C → Esc menüsü + ara ekranlar; B → öğretici/denetçi notu + lab listesi + intro kararı; EEE → MiddleCollapsed son metni, README son hali, final görsel/ses; A → denge turu, 20:00 ikinci build.
- **Bilinen sorunlar:**
  - Esc menüsü yok (README'de var, Onur yapacak).
  - Oyuncuya kuralları anlatan öğretici yok (Oğuzhan + Tarık).
  - Unity'yi **6000.6.5f1 ile açmayın** (proje sürümünü yükseltiyor; ekip 6000.6.4f1). `ProjectVersion.txt` değişirse commit'lemeyin.
  - Game.unity Editor'den kaydedilirse ekran anchor'ları bozulabiliyor (bir kez oldu); kaydetmeyin.

### Ekibe notlar (A / Ahmet'ten, 2026-10-16 22:20)
> ⚠️ **Oğuzhan — ÖNCE BUNU OKU (A, `61353fd`):** `62165c0`'daki "yeni kural modeli" (Yapı Denetim Formu + Denetçi El Kitabı, tek ihlal = RED, ek test/lab raporu/sahte belge ipuçlarının kaldırılması, sel afeti) **Ahmet tarafından onaylanmadı**; notta yazan "Ahmet onayladı" yanlıştı. Commit geri alındı (`git revert`): memory.md eski kurallara döndü, `Docs/Tasarim/*.png` kaldırıldı (geçmişte duruyor).
> - Masa ekranını yeni forma göre değiştirmeye başladıysan **dur**; mevcut modelle devam et: başvuru + lab raporu, Reported/Actual, ücretli ek test, parsel/tarih/lab/kaşe/imza ipuçları, rüşvet zarfı. Core sözleşmesi değişmedi (`ctx.TodayProjects`, `ctx.Actions`).
> - Oyunun kural modelini değiştiren öneriler için önce Ahmet'le konuş; "Ahmet onayladı" notunu sadece Ahmet (ya da onun açık onayıyla) yazar.
> - Fikirlerden ileride yararlanmak istersek (ör. el kitabı, kaçak kat, yangın kuralları) Ahmet'le ayrıca konuşulur.
>
> ⚠️ **Tarık — (A, `cbd8587`):** `4f33750`'daki yeni modele uyarlamalar da geri alındı: Selim ("sahte belgeci" tekrar), Sevim replikleri, Headlines.asset (deprem / "Belgeler sahte, imza gerçek" tekrar), ICERIK_TASLAK.md'deki son metinleri (lab raporu / ruhsat tekrar). **Kalanlar:** credits çözüldü notu, piksel art stil kararı (Art/README; silinen `Docs/Tasarim` atfı düzeltildi), Selim "A onayladı" notu. Oyunda sel yok; manşet/sonlarda "deprem" dili geçerli.

### 📌 Analiz sonrası işler (A, 17 Ekim gece) — ÖNCE BUNU OKU
Tam analiz ve yol haritası Ahmet'te (claude.ai artifact; bağlantıyı Ahmet paylaşır). Kararlar §2'de. Kodlar: K = kritik, Y = yüksek, O = orta, D = düşük.

**Başkasının dosyasına dokundum (kararlaştırıldığı gibi not):**
- `Flow/UI/EndingScreen.cs` (Onur): `case EndingType.MiddleQuota` → `MiddleCollapsed` (enum değişti, yoksa derlenmezdi) + yedek metin. Başka satır yok.
- `Flow/UI/ResultScreen.cs` (Onur): ayakta kalan binada `ResponsibilityText` gösteriliyor; kapsamda bina yokken ayrı cümle (O3). İki küçük blok, `(A, Core)` yorumlu.

**Onur (C):**
- [ ] **Y4 Esc menüsü:** her ekranda Esc → Devam / Ana menü / Çıkış. README "Esc menü açar" diyor; gelene kadar yalan.
- [ ] **K4:** DayIntro / Construction / Save ekranları (önceki madde). Not: build'de artık mavi kutu çıkmıyor, ekranı olmayan faz atlanıyor; yani bunlar görsel kazanç, engel değil.
- [ ] `EndingScreen` ve `ResultScreen` değişikliklerime göz at; istersen kendi üslubunla düzenle.
- [ ] Kötü sonda "hâkim isimleri okuyor" anı: `ctx.State` yerine son afetin `VictimNames`'i gerekiyorsa A'ya söyle, Core'dan veririm.
- [ ] D4: Emeği Geçenler'e takım ve AI satırı.

**Oğuzhan (B):**
- [ ] **Y1 öğretici:** masada sabit "Denetçi notu" paneli (değerleri min. değerle karşılaştır · iki belgede parsel/tarih/imza aynı olmalı · lab kaşesi raporun laboratuvarıyla aynı olmalı · şüphede ek test) + 1. gün kısa giriş.
- [ ] **Y1 akredite lab listesi:** `ctx.Names.LabNames` = akredite laboratuvarlar. "Hızlı Sonuç Lab. Ltd." bu listede yok → oyuncu listeyle yakalar. Panelde göster.
- [ ] **Y1 intro:** `Assets/Scripts/Intro/IntroSequence.cs` repoda yok ama CREDITS'te kayıtlı ve var olmayan "Office" sahnesini yüklüyor. Ya repoya al (Game sahnesine geçecek şekilde, A'ya haber ver) ya da CREDITS satırını sil.
- [ ] O4: Rüstem reddedilince masada "camın kırıldı" kartı + `sfx_glass_break` (Tarık'tan).
- [ ] O1: `OGUZHAN.md` "Mevcut durum" (kota satırı, "Sırada" eski).

**Tarık (EEE):**
- [ ] **Y2:** `Data/Endings/`: senin 3 asset'in (`3da4deb`) + A'nın `MiddleCollapsed.asset` ("Enkazın Gölgesi", Orta/yıkım sonu). **MiddleCollapsed'ın taslak metnini A yazdı** — son halini sen yaz (ton: suçlamasız, ciddi). Hepsi Game.unity'ye bağlı.
- [ ] **K3:** `README.md` taslağı A'da hazır (kurulum, kontroller, nasıl oynanır, ekip). Son halini sen ver: soyadlar, oyun görseli, video bağlantısı.
- [ ] O6: masa ve gazete görselleri final, müteahhit portreleri, `sfx_glass_break`, `mus_menu`, `mus_ending_sad`.

Ajanlar: oturum başında bu bölümü oku; ilgili maddeyi tamamlayınca kendi kişisel dosyana not düş ve buradaki maddeyi `[x]` yap. Ayrıntılı Core kullanım rehberi: `AHMET.md` → "B ve C için Core kullanım rehberi".

**Oğuzhan (B / Masa) için:**
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
- [x] (A, `0bebf9b`) Flow ekranları oyunda: prefab'lar Game.unity'ye eklendi (Install Screens). Tamamlananlar (A koddan doğruladı): Flow derlemesi, `.meta`'lar, prefab'lar + MainMenu + Build Settings, Sandbox_Flow, manşet `{count}`, Emeği Geçenler, ekranlar `IGameScreen` (Result / DaySummary / DailyExpenses / Ending), gazete `ctx.LastQuake` alanları, ödeme `SetExpensePaid`, MainMenu `SaveSystem.HasSave` + `GameLaunch.ContinueFromSave`. Game.unity'yi kendin açıp KAYDETME.
- [ ] **Sıradaki iş (Ahmet kararı, `0916894`): sahibi olmayan fazlar senin.** Şu an mavi yedek arayüzle görünüyorlar: `GamePhase.DayIntro` (gün başı kartı: "3. GÜN", para/borç), `GamePhase.Construction` (inşaat geçişi; gazeteden önce kısa bir an), `GamePhase.Save` (gün sonu "kaydedildi" kısa geri bildirimi). Hepsi `IGameScreen` (`FlowScreen` tabanı uygun); ekranı olan faz için mavi kutu kendiliğinden kaybolur. Prefab'ları üretip push'layınca haber ver → A Game.unity'ye ekler.
- Bilgi: son ve borç kuralları kesinleşti (§2). Son ekranı `ctx.State.Ending` (Good / Bad / MiddleClosed / **MiddleCollapsed**) + `ctx.Endings` (4 EndingSO bağlı). Borç varsa ödeme ekranına `debt_payment` kalemi gelir.

### Açık kararlar (öneriler kesinleşmiş değildir)
- Afet olasılığının günlük artış miktarı (taban %50, tavan %100 ve sıfırlama kararlaştı).
- JailRisk sayılarının (yıkım +0.15, rüşvetli yıkım +0.25) 10 günlük oyunda dengesi; denge turunda bakılacak.

## 7. Karar günlüğü
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
| 2026-10-17 `3da4deb` | Son metinleri kod koşullarına uyduruldu: İyi son "binaların hepsi ayaktaydı" demez (İyi son şu an yıkım olsa da hapis yoksa geliyor); Kötü son rüşvet varsaymaz (rüşvetsiz yıkımla da hapis var); Orta son dürüstlük varsaymaz | Metin oyuncuya yanlış bir şey söylememeli. A'ya öneri: İyi son için "hiç yıkım yok" şartı + yıkımlı ama hapissiz durum için ayrı son (EEE yazar) |
| 17 Ekim (analiz sonrası) | Borç kuralı: kasa eksiye düşmez, yetmeyen zorunlu gider borca; borç > 2.000 TL → ofis kapanır (Orta son). Başlangıç 2.500 TL, onay ücreti 400-700 | Ahmet kararı (K1). "Her şeyi reddet" stratejisi artık 7. gün ofisi kapatıyor |
| 17 Ekim (analiz sonrası) | Sonlar ve öncelik: Kötü (hapis) > Orta/ofis kapandı > Orta/yıkım (yeni `MiddleCollapsed`) > İyi (hiç yıkım yok) | Ahmet kararı (K2). İyi son metni "binaların hepsi ayaktaydı" ile tutarlı |
| 17 Ekim (analiz sonrası) | Oyun 10 gün; fabrika/yangın ve sigorta kapsam dışı | Ahmet kararı. Jüri süresi; yarım kalmış yangın kuralı ve etkisiz sigorta oyuncuyu yanıltıyordu |
| 17 Ekim (analiz sonrası) | Son ekranına girince kayıt silinir; build'de ekranı olmayan faz otomatik geçilir (mavi kutu sadece Editor/Development Build); F9/F10 demo tuşları | A (Y3, K4, O7) |

## Güncelleme kuralı
- Önemli bir karar, mimari değişiklik, yeni kural veya tamamlanan büyük iş olduğunda ilgili bölümü güncelle ve karar günlüğüne tarih-saatle satır ekle.
- Dosyayı kısa tut; kod kopyalama, sadece karar ve yönlendirme yaz.
