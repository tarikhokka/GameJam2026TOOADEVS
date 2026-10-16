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
- Eski level başına proje sayıları geçersizdir. Toplam gün sayısı, günlük proje sayıları ve içerik açılma günleri henüz belirlenmedi. Konut, sahte belgeler, fabrika/yangın içerik hedefleri; sel ve okul/hastane ek kapsamdır.
- Deprem ayrı bir sahne/animasyon olarak gösterilmez; oyuncu depremi ve sonuçlarını gazeteden öğrenir (Quake ekranı yok, deprem/yıkılma sesi yok).
- Deprem şiddeti oyuncuya gösterilmez. Bina sadece 2 durumlu: ayakta / yıkık.
- Yıkılma kuralı (EarthquakeSimulator):
  - Strength = min(Actual/Required).
  - Strength >= 1 ise ASLA yıkılmaz (dürüst yol garantisi).
  - Aksi halde Strength + gürültü(±0.15) < Intensity ise yıkılır. Intensity yapılandırılmış afet aralığından çekilir, max <= 1.0.
- Rüşvet teklif eden her proje uygunsuzdur. Bazı uygunsuz projeler rüşvetsiz, sahte belgeyle temiz görünür; ek testle yakalanır. Dürüst usta = rüşvetsiz gelen gerçekten uygun proje.
- İki katmanlı sahte belge: (1) bedava ipuçları (parsel/tarih/lab tutarsızlığı), (2) ücretli ek test (Reported vs Actual farkını açar).
  - Kaşe ve imza belgelerde her zaman gerçek görünür; sahte belge bunlardan anlaşılmaz (WrongStamp/WrongSignature ipucu kaldırıldı). Uygulama: A `ClueType`/`ProjectGenerator`dan iki ipucunu çıkarır; B ekranda kaşe kodunu yazı olarak göstermez.
- Hapis: kalıcı JailRisk. Yıkım varsa zar atılır.
  - Önceki denge: yıkılan bina +0.15, rüşvetle onaylanan yıkılan bina +0.25. Eski temiz level -0.05 kuralının gün sistemindeki karşılığı açık; afetsiz günün riski azaltacağı varsayılmamalı.
  - İlk yıkımda bile hapis olasılığı var. Tüm sayılar BalanceSO'da.
- Eski level tekrar kuralı kaldırıldı; başarısız gün sonrası devam/tekrar davranışı henüz kesinleşmedi.
- Giderler her gün ödenecek (haftalık ödeme kaldırıldı). Kalemler: kira, kredi taksiti, ek test, sigorta, kabadayı cam tamiri. Ek testin anında tahsil edilmesi ve sigortanın kapsam başlangıcı henüz netleştirilmeli. Ödenmeyen giderlerin sonucu henüz cevaplanmadı (önceki plan: borç birikir, limit aşılırsa ofis kapanır / orta son).
- Kota YOK (karar, 22:40). "Her şeyi reddet" stratejisine karşı baskıyı günlük giderler kurar (onay ücreti gelmezse gider ödenemez). Bunun çalışması için ödenmeyen giderin sonucu yeterince ağır olmalı; o sonuç henüz kararlaştırılmadı.
- Save/Load: kayıt gün sonunda yapılacak; ay sonu kaydı kaldırıldı. Tek slot JSON / persistentDataPath yaklaşımı korunuyor. GameState gün, para, borç ve afet olasılığı gibi devam için gereken durumu kapsamalı; kesin kayıt şeması A ile netleştirilecek. Gün içi manuel kayıt kararı yok.
- Sonlar:
  - İyi: gün sistemindeki kesin bitiş koşulu henüz belirlenecek (eski koşul son level yıkımsızdı).
  - Kötü: mahkeme/hapis, kürsüde kurgusal isimler. Rüşvetli yıkımın doğrudan son mu yoksa JailRisk üzerinden mi işleyeceği netleştirilmeli.
  - Orta: borçtan ofis kapanışı (koşul, ödenmeyen gider kararına bağlı). Kota sonu yok.
- Yıkım nedeni kartları elle yazılmaz; sonuç ekranı FailedParams / Actual / Required değerlerinden otomatik üretilir (şablon cümle).
- Sadece Türkçe. Jüri demosu için gizli gün atlama kısayolu sonra eklenecek.

## 3. Mimari
- Tek `Game` sahnesi + ekranlar prefab. Diğer sahneler: MainMenu, Sandbox_Desk, Sandbox_Flow.
- Oyun mantığı saf C# (MonoBehaviour yok): üretim, deprem, ekonomi (GameSession). Tek MonoBehaviour GameManager: faz sırası + ekran gösterme.
- Veri:
  - ScriptableObject: DayConfigSO, RuleSetSO, ClientSO, ExpenseSO, BalanceSO, HeadlineSO, EndingSO, NamePoolSO.
  - Runtime plain C#: ProjectData, DocumentData, ParamEntry, GameState, GameContext, QuakeResult (+BuildingOutcome), DailyExpenseResult.
- Ekranlar IGameScreen uygular: Phase, Show(GameContext), Hide(), event Completed. GameManager sahnedeki ekranları kendisi bulur; ekranı olmayan faz için IMGUI yedek arayüz gösterir. Ekranlar Core'a sadece `ctx.Actions` (IGameActions) ile yazar. B/C kullanım rehberi: `AHMET.md`.
- Assembly'ler: CivilJam.Core, CivilJam.Desk, CivilJam.Flow. Desk ve Flow sadece Core'a bağlı, birbirini görmez.

### Dosya haritası
- `Assets/_Project/Scenes/Sandbox_Flow.unity` — Onur için Unity Editor üzerinden oluşturulan Lit 2D (URP) deneme sahnesi; Canvas ve EventSystem eklendi. Canvas: Screen Space Overlay, Scale With Screen Size, 1920×1080, Match 0.5. Canvas altında tam ekran stretch ResultScreen paneli hazır; gazete görseli ve içerikler henüz yok.

`Assets/_Project/Scripts/Core/` (asmdef `CivilJam.Core`, namespace `CivilJam.Core`):
- `Enums.cs` — GamePhase, ParamType, BuildingType, DocumentKind, ClueType, ProjectVerdict, BuildingStatus, EndingType.
- `Runtime/` — ParamEntry (Required/Reported/Actual), DocumentData, ProjectData (Strength), GameState (kayıt), GameContext (ekranlara verilen), QuakeResult + BuildingOutcome (kayıplar, sorumluluk metni), DailyExpenseResult.
- `Screens/` — IGameScreen (Phase/Show/Hide/Completed), IGameActions (ekranlardan Core'a tek giriş: ek test, onay, ret, gider seçimi).
- `Config/` — BalanceSO, DayConfigSO, RuleSetSO, ClientSO, ExpenseSO, HeadlineSO, EndingSO, NamePoolSO (kurgusal isimler).
- `Simulation/` — EarthquakeSimulator (yıkılma kuralı, son afetten beri onaylananlar, JailRisk, hapis zarı), DisasterSystem (gün sonu zar + olasılık), ProjectGenerator (seed'li günlük projeler), CasualtyGenerator, ExpenseCalculator (günlük gider v0).
- `Game/` — GameSession (saf C# mantık + IGameActions), GameManager (MonoBehaviour, faz sırası), FallbackGui (geçici IMGUI), SaveSystem (tek slot JSON), GameLaunch (MainMenu "Devam Et" bayrağı).
- `Editor/CoreSetupBuilder.cs` — menü: CivilJam > Core > Create Missing Data Assets / Build Game Scene (var olanın üzerine yazmaz).
- `Tests/Editor/CoreRulesTests.cs` — EditMode testleri (Test Runner > EditMode).
- `Assets/_Project/Data/` (A, içerik EEE ile) — Balance, RuleSet, NamePool, Headlines, Days/DayXX (FromDay 1/3/5/8), Expenses/.
- `Assets/_Project/Scenes/Game.unity` (sadece A) — GameManager + veri bağlı; Build Settings'te.
- Flow asmdef'i C oluşturur; Desk ve Flow referansı sadece `CivilJam.Core`. Ayrıntı ve Core önerileri: `AHMET.md`.
- `Assets/_Project/Scripts/Desk/` (B) — Core tiplerini doğrudan kullanır (ProjectData, GameContext, IGameActions).
  - `CivilJam.Desk.asmdef` — referans: CivilJam.Core, TMP, uGUI.
  - `UI/DeskScreen.cs` — IGameScreen (Phase = Desk). Show(GameContext); kararlar context.Actions ile; "Mesaiyi bitir" → Completed.
  - `UI/` diğerleri — DeskHud, ProjectQueueView/Item, DocumentView, ParamRowView, ActionPanel, UiPool, DeskFormat (parametre adı/birimi RuleSetSO'dan).
  - `Sandbox/FakeGameActions.cs` — sadece sandbox için sahte IGameActions.
  - `Sandbox/SandboxDeskContext.cs` — sahte gün: 3 proje (dürüst, rüşvetçi, sahte belgeli), 3 müşteri, RuleSet.
  - `Sandbox/DeskSandboxBootstrap.cs` — Sandbox_Desk'i başlatır, mesai bitince sonucu loglar.
  - `Editor/DeskSandboxBuilder.cs` — menü: CivilJam > Desk > Build Sandbox_Desk Scene (prefab + sahneyi yeniden kurar).
- `Assets/_Project/Prefabs/Desk/DeskScreen.prefab` (B) — masa ekranı; Game.unity'de Canvas altına konur (kendi içinde tam ekran stretch).
- `Assets/_Project/Art/` (EEE) — placeholder sprite'lar (Single, PPU 100). Dosya adları ve hangi ekranın kullandığı: `Art/README.md`. Final görsel aynı adla üzerine yazılır.
- `Assets/_Project/Audio/` (EEE) — sesler `.ogg`. İsim listesi: `Audio/README.md`.
- `CREDITS_AND_AI.md` (EEE) — hazır asset + AI kullanım kaydı.
- `Assets/TextMesh Pro/` — TMP Essential Resources (A import etti, 23:15). Desk/Flow yazıları için gerekli; tekrar import etme, silme.
- `ICERIK_TASLAK.md` (EEE) — SO metin taslakları (şimdilik 6 müteahhit). Onaylananlar `Data/` altındaki asset dosyalarına taşınır (A ile koordineli).

## 4. Takım ve klasör sahipliği
- A (Core) — Ahmet İlman: Scripts/Core, Data.
- B (Masa) — Oğuzhan: Scripts/Desk, Prefabs/Desk, Scenes/Sandbox_Desk.
- C (Akış) — Onur Kabadayılar: Scripts/Flow, Prefabs/Flow, Scenes/Sandbox_Flow, MainMenu.
- 4. kişi (EEE): Art, Audio, SO içerikleri, CREDITS_AND_AI.md.
- Scenes/Game.unity sadece A.
- Onur’un kişisel C rolü planı: `ONUR.md`. Ortak kararların kaynağı bu dosyadır; kişisel plan ortak kararları değiştirmez.
- Kural: kimse başkasının sahne/prefab dosyasını açıp kaydetmez. Saatte bir main'i çek.

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

### Durum
- **Plan durumu:** Gün sistemi tasarımı sürüyor.
- **Core (A, 22:15):** Faz 1 dikey dilim: günlük ödeme / afet kapsamı / kayıp verisi sözleşmeye uygulandı (Weekly→Daily, Quake fazı kaldırıldı). ProjectGenerator, DisasterSystem, GameSession, GameManager, SaveSystem, Data asset'leri ve Game.unity hazır. Game.unity IMGUI yedek arayüzle baştan sona oynanabilir. Derleme uyarısız, EditMode 7/7. GEÇİCİ kurallar (ödenmeyen gider → borç, sonlar, kayıt anı): `AHMET.md`.
- **Desk (B, 23:10):** Core tiplerine geçti (IGameScreen, Phase = Desk; ctx.TodayProjects + ctx.Actions). A Game.unity'ye ekledi; Game.unity'de gerçek GameSession ile test edildi. Kota kaldırıldı (HUD: "Bugün onaylanan: N"); kaşe yazısı auto-size; Sandbox belge kuralları ProjectGenerator ile aynı.
- **Art/Audio (EEE, 23:00):** 13 placeholder sprite push'landı (masa, belge, kaşeler, imza, bina ayakta/yıkık, şehir, gazete, gün özeti, günlük ödeme, menü, logo). Masa sesleri hazır: `sfx_click`, `sfx_stamp`, `sfx_paper`, `mus_office_ambient` (loop, Streaming). Ortam sesi CC BY 4.0 → oyunda bir credits ekranı gerekli; metni `CREDITS_AND_AI.md` sonunda hazır. Sırada: ClientSO metin taslakları, menü/son müzikleri.
- **Sırada:** DeskScreen'in Game.unity'ye eklenmesi (A); Flow ekranları (C: Result, DaySummary, DailyExpenses); A: ekonomi/kota/sonlar (Faz 2).
- **Bilinen sorunlar:** —

### Güncel entegrasyon durumu (cedfa0d sonrası yerel kontrol)
- Desk v0 kodu, DeskScreen prefab ve Sandbox_Desk sahnesi artık repoda (`14350ec`). (Güncelleme 22:30) Desk artık Core tiplerini kullanıyor; bkz. Durum.
- Flow: Sandbox_Flow, Canvas ve ResultScreen paneli hazır; içerik/script entegrasyonu yapılmadı.
- (22:15 güncel) Günlük ödeme, iki afet arası bina kapsamı ve kayıp bilgileri Core'da uygulandı: `DailyExpenseResult` / `ctx.PendingExpenses`, `GamePhase.DailyExpenses`, `BuildingOutcome.Deaths/Injured/VictimNames/ResponsibilityText`. C bunları üretmez, Core'dan gösterir.
- C'nin sıradaki işi gazete görseli/yerleşimi, ardından gazetesiz gün özeti ve günlük hesap ekranıdır. Mevcut panel kullanılabilir.

### Ekibe notlar (A / Ahmet'ten, 2026-10-16 22:20)
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

**Onur (C / Akış) için:**
- [ ] Ekranlar `IGameScreen` uygulasın; `Phase` değerleri: gazete `GamePhase.Result`, gün özeti `GamePhase.DaySummary`, günlük ödeme `GamePhase.DailyExpenses`, son `GamePhase.Ending`. (`Quake` ve `WeeklyExpenses` artık yok.)
- [ ] Gazete: `ctx.LastQuake.Outcomes` → `BuildingName`, `Collapsed`, `Deaths`, `Injured`, `VictimNames`, `ResponsibilityText`, `WasBribed`; toplamlar `ctx.LastQuake.TotalDeaths/TotalInjured`; tutuklama `ctx.LastQuake.Jailed`. Afetsiz gün Result fazı hiç açılmaz.
- [ ] Günlük ödeme: `ctx.PendingExpenses.Lines` (DisplayName, Amount, Mandatory, Paid). Seçim `ctx.Actions.SetExpensePaid(expenseId, paid)`; uygulama Completed'dan sonra Core'da (ekran parayı değiştirmez).
- [ ] MainMenu "Devam Et": `SaveSystem.HasSave` ile göster; tıklanınca `GameLaunch.ContinueFromSave = true;` sonra Game sahnesini yükle. "Yeni Oyun" bayrağı false bırakır.
- [ ] Bilgi: sonlar ve ödenmeyen gider kuralları GEÇİCİ (`AHMET.md`); son ekranı şimdilik `ctx.State.Ending` okusun.

### Açık kararlar (öneriler kesinleşmiş değildir)
- Toplam gün sayısı / günlük proje sayısı; 14 gün önerisi onaylanmadı.
- Afet olasılığının günlük artış miktarı (taban %50, tavan %100 ve sıfırlama kararlaştı).
- Ödenmeyen günlük giderlerin sonucu; ek test tahsilatı ve sigortanın kapsam dönemi.
- Başarısız gün, sonlar ve JailRisk kurallarının gün sistemine uyarlanması.
- Save sonrası yüklemenin sonraki sabaha dönmesi, son ekranında kayıt ve kayıt şemasının kesinleştirilmesi.
- Hapis ve ofis kapanışı aynı anda tetiklenirse son önceliği.

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
| 2026-10-16 22:40 (UTC+3) | Kota kuralı yok; baskıyı günlük giderler kurar | Ahmet kararı (seçenek A). Ödenmeyen giderin sonucu sonra kararlaştırılacak; ağır olmalı |
| 2026-10-16 22:40 (UTC+3) | Belge kaşesi (StampId) görsel değil okunabilir metin; sahte kaşe = başlıktaki lab adıyla uyuşmayan kaşe | Görsel iş yükü yok; ipucu okuyarak yakalanır, veri gerçek/sahte bilgisini sızdırmaz |
| 2026-10-16 22:15 (UTC+3) | Ekranı olmayan faz için GameManager IMGUI yedek arayüzü gösterir | Desk/Flow beklenmeden Game.unity baştan sona oynanabilir; ekran eklendikçe kendiliğinden devre dışı |
| 2026-10-16 23:15 (UTC+3) | Kaşe/imza ipucu kaldırıldı; sahte belge ipuçları: parsel, tarih, laboratuvar. Kaşe ve imza oyunda kalır, hep gerçek | `STAMP_FAKE` yazısı ipucunu doğrudan veriyordu, imzadaki nokta fark edilemiyordu; ek görsel yükü yok (EEE kararı) |

## Güncelleme kuralı
- Önemli bir karar, mimari değişiklik, yeni kural veya tamamlanan büyük iş olduğunda ilgili bölümü güncelle ve karar günlüğüne tarih-saatle satır ekle.
- Dosyayı kısa tut; kod kopyalama, sadece karar ve yönlendirme yaz.
