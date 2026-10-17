# memory.md — Projenin ortak hafızası

> **18 Ekim ~01:30 temizlendi (B, ekibin izniyle).** Bitmiş özelliklerin planlama notları, iş bölümü tabloları ve `[x]` maddeler silindi; kurallar güncel hâliyle §2-§3'e taşındı. Eski hâli git geçmişinde: `45d3503` (önceki temizlik `f14d2ed`). Ajanlar: oturum başında §7'yi oku; bir maddeyi bitirince orada `[x]` yap ve tek satır not düş.

## 1. Proje özeti
- CIVIL GAME JAM (ODTÜ KKK, KALTEV), 48 saat: 16 Ekim 2026 18:00 → 18 Ekim 2026 18:00 (UTC+3). Tema: "Afete Hazırlık ve Farkındalık".
- **İmza** (ürün adı "Imza", şirket "TOOADEVS"): masa başı karar oyunu. Oyuncu yapı denetim mühendisi. Müteahhitler sırayla gelir; oyuncu Yapı Denetim Formu, Laboratuvar Raporu ve Denetçi El Kitabı'nı karşılaştırıp binayı onaylar ya da reddeder. Bazıları zarf (rüşvet) bırakır. Gece deprem olabilir; sonucu ertesi sabah gazeteden öğrenir.
- Unity 6000.6.4f1, 2D (URP), uGUI + TextMeshPro, Input System. Windows, 1920x1080. Sadece fare; Esc menü açar. Sadece Türkçe.
- Jüri ölçütleri: Oynanabilirlik 25, Temayla ilişki 25, Yaratıcılık 20, Görsel/işitsel bütünlük 15, Teknik bütünlük 15.
- **Hassasiyet:** isimler kurgusal; gerçek afete, kişiye, kuruma atıf yok (gerçek bakanlık adı, logo, standart kodu kullanılmaz). Depremzedeler asla olumsuz gösterilmez. Mizah karakterlerde; deprem sonrası ve sonlar ciddi.

## 2. Oyun kuralları (güncel, kodda uygulanmış)
**Akış:** MainMenu → (Yeni Oyun: hikâye intro'su) → Game: DayIntro → Desk → Construction → (afet ? gazete (Result) : gün özeti (DaySummary)) → günlük ödeme → (son ? Ending : Save) → ertesi gün. Deprem sahnesi/animasyonu yok; afetsiz günde gazete yok.

**Gün ve içerik (`Data/Days`, `BalanceSO.TotalDays` = 7):** günlük proje 3 · 3 · 4 · 4 · 4 · 5 · 5 = 28. 1. gün zarf ve sahte belge yok; 2. gün zarf, 3. gün sahte belge, 4. gün kabadayı başlar. Oyun başında sabit rastgele ziyaret programı kurulur (`VisitPlanner`, `GameState.Schedule`, kayıtla saklanır): 6 ana müteahhit 3'er kez, 10 kişiliksiz karakter 1'er kez; aynı müteahhit aynı gün iki kez ve arka arkaya gelmez; projeler tam 14 uygun / 14 uygunsuz, hiçbir yüz kararı ele vermez. Fabrika, yangın, sel, sigorta, kota **yok**.

**El Kitabı v2 (`Data/RuleSet.asset`, 15 kural):** her projede 5 parametre; gereken değer sadece kat grubuna bağlı (binalar 2-9 kat). Değerler "en az"; seçeneklilerde sağdaki daha iyi.

| Sekme | Parametre | 1-3 kat | 4-6 kat | 7+ kat |
|---|---|---|---|---|
| BETON | Beton sınıfı (C16 / C20 / C25 / C30 / C35) | C20 | C25 | C30 |
| DONATI | Demir tipi (B220 düz / B420 / B500 / B500S) | B420 | B500 | B500S |
| YAPI | Kolon kalınlığı | 30 cm | 35 cm | 40 cm |
| ZEMİN | Temel derinliği | 1,5 m | 2,0 m | 2,5 m |
| ZEMİN | Zemin türü (Dolgu / Yumuşak kil / Sert kil / Kaya) | Yumuşak kil | Sert kil | Kaya |

**Belgeler ve ipuçları:** Form (`Documents[0]`) ve lab raporu (`Documents[1]`) beyan değerini (`Reported`) gösterir; gerçek değeri (`Actual`) sadece ücretli **ek test** (400 TL, anında düşer) açar. Sahte belgede değerler temiz görünür; bedava ipuçları iki belgenin farkındadır: parsel, tarih, bilinmeyen laboratuvar (akredite listede yok), kaşedeki lab adı ≠ raporun lab'ı (kaşedeki lab da akredite değil), imzalayan mühendis farklı. Sahte parsel/tarih gerçeğiyle asla aynı çıkmaz. Masa hiçbir şeyi "yanlış" diye işaretlemez.
**Değer farkı:** lab raporunun beyanı formdan farklı yazabilir (`ParamEntry.LabDiffers` / `LabReported` / `LabValue`); ikisi de yönetmeliği geçiyor görünür. Sahtelerin %65'inde (gerçek değer kötü; bunların yarısında künye ipucu yok → sadece ek test kanıtlar), uygun projelerin %15'inde masum yazım hatası (gerçek değer iyi). Hangisi olduğunu ek test söyler.

**Proje türleri (gizli `ProjectKind`):** Compliant, Sloppy (rüşvetsiz ama bir değeri eksik), Bribe, Fake, BribeFake. Rüşvet teklif edilen her proje uygunsuzdur.

**Müteahhitler (`Data/Clients`, 16):** 6 ana karakter: Rıfat ve Sevim dürüst (çoğunlukla uygun, bazen dikkatsiz; asla zarf/sahte belge yok); Kemal ve Nuri rüşvetçi (bazen temiz gelir); Selim çoğunlukla sahte belgeci (zarf vermez; 1-2. gün gelirse dikkatsiz proje); Rüstem kabadayı (4. günden; reddedilince 4 olaydan biri: cam, masa, kapı kilidi ya da araba lastikleri — art arda aynısı olmaz; gider "Hasar tamiri"). 10 kişiliksiz karakter (`Generic01-10`, `IsGeneric`; uygun, dikkatsiz, zarflı ya da sahte belgeli gelebilir; zarf `BalanceSO.GenericBribeRange`); kendi replik listesi boşsa `Data/Dialogue.asset` ortak havuzu kullanılır. Core selamlama (`GreetingText`; tekrar gelende "geçen sefer…") ve tepki (`ReactionText`) seçer. Nuri her retten sonra zarfı +250 TL büyütür. **Hediye:** sadece dürüst ustalar, onayda bazen, oyunda bir kez; sadece kabul edilir, ofiste kalıcı görünür, para/risk etkisi yok.

**İnceleme modu (`Inspection.Check`, masadan `ctx.Actions.Inspect`):** oyuncu İNCELE ile iki **ayrı** belgeden iki alan seçer → SORUN YOK / UYUŞMAZLIK SAPTANDI / ALAKASIZ. Ücretsiz, sınırsız; kararı yine oyuncu verir. El Kitabı eşiği oyuncunun Hızlı Bakış'ta **seçtiği kat grubundan** alınır (seçilmemiş ya da binaya ait olmayan grup → ALAKASIZ). Tabloda olmayan her çift ve aynı belgeden iki alan → ALAKASIZ.

| # | 1. alan | 2. alan | Uyuşmazlık ne zaman |
|---|---|---|---|
| 1 | Form: parametre X | El Kitabı: parametre X | Beyan eşiğe yetmiyor (rüşvetli, dikkatsiz) |
| 2 | Lab: beyan X | El Kitabı: parametre X | Aynısı |
| 3 | Lab: ek test X (test yapıldıysa) | El Kitabı: parametre X | Gerçek değer eşiğe yetmiyor (sahte dahil) |
| 4 | Form: parametre X | Lab: ek test X | Beyan ≠ gerçek değer (sahte) |
| 5 | Form: parametre X | Lab: beyan X | Değer farkı varsa |
| 6 | Form: Ada/Parsel | Lab: Ada/Parsel | Parsel ipucu |
| 7 | Form: Tarih | Lab: Rapor tarihi | Tarih ipucu |
| 8 | Form: Proje müellifi | Lab: Laboratuvar yetkilisi | Farklı mühendis imzası |
| 9 | Lab: Laboratuvar (başlık) | El Kitabı: LAB listesi | Bilinmeyen laboratuvar |
| 10 | Lab: kaşe | El Kitabı: LAB listesi | Başka (akredite olmayan) laboratuvarın kaşesi |

**Uyuşmazlık diyaloğu ve rüşvet (`InspectionDialogue`):** zarf başta yoktur, rüşvetçi sadece selamlar. UYUŞMAZLIK bulununca (bu projede bu alan çifti için ilk kez) kaşeden sonra kısa diyalog: denetçi cümlesi (`Dialogue.asset` şablonu, gerçek veriyle) + müteahhit cevabı (`ClientSO.MismatchLines`). Projede zarf varsa **ilk uyuşmazlıkta** devam eder: teklif (`BribeLines`) + denetçi tepkisi (`BribeReactions`) + kapanış (`BribeCloseLines`); `ProjectData.BribeRevealed` = true ve zarf masaya düşer. Uyuşmazlık bulunmazsa zarf hiç teklif edilmez; teklif edilmemiş zarf alınamaz, ret sonrası "zarf geri çevrildi" repliği çıkmaz. SORUN YOK ve ALAKASIZ'da diyalog yok.
**Zarf:** masada sadece "ZARF" yazar; tıklanınca açılır (para + tutar): RÜŞVETİ KABUL ET → zarfla onay; RÜŞVETİ REDDET → zarf masaya döner, proje karar bekler.

**Yıkılma (`EarthquakeSimulator`):** Strength = en kötü oran (sayısalda Actual/Required; seçenekli parametrede her eksik kademe −%25). Strength ≥ 1 asla yıkılmaz. Değilse Strength + gürültü (±0,15) < şiddet (0,5-0,9) ise yıkılır. Afet sadece son afetten bu yana onaylanan binaları değerlendirir. Afet olasılığı %20, afetsiz her gün +%15, tavan %100, afette %20'ye döner.

**Ekonomi (`Data/Balance.asset`, `Data/Expenses`):** başlangıç 1.500 TL; onay ücreti 500-700 TL; her gün kira 750 + kredi 400 (+ kabadayı hasarı 600); ek test 400; borç limiti 2.000. **Bilerek zor (Tarık kararı).** Kasa asla eksiye düşmez; yetmeyen zorunlu gider borca yazılır; borç varsa isteğe bağlı (kısmen ödenebilir) "Borç ödemesi" kalemi çıkar.

**Hapis ve sonlar** (günlük ödemeden sonra, öncelik sırasıyla): yıkılan bina JailRisk +0,15, rüşvetle onaylanmış yıkılan bina +0,25; her yıkımdan sonra hapis zarı.
1. **Kötü** "Hüküm": hapis zarı tuttu.
2. **Orta** "Kapanan Kapı": borç > 2.000 TL (her gün bakılır).
3. **Orta** "Enkazın Gölgesi": 7. gün bitti, en az bir bina yıkıldı.
4. **İyi** "Ayakta Kalanlar": 7. gün bitti, hiç yıkım yok.

Denge (`CivilJam > Core > Balance Report`, gerçek 16 kişilik kadroyla, A `17179b4`): dikkatli %82 İyi / %18 ofis kapanır; tek zarf %44 İyi / %45 Enkazın Gölgesi / %11 hapis; her zarfı alan %76 hapis; her şeyi reddeden 3. gün ofisi kaybeder.

**Kayıt:** tek slot JSON (`persistentDataPath/save.json`), sadece `GameState`. Gün sonunda gün +1 yapılıp kaydedilir; "Devam Et" yeni günün başını açar. Son ekranına girince kayıt silinir.

**Jüri demo tuşları (Game sahnesi, build'de de):** F9 = son güne (7) atla, F10 = bu gece afet kesin.

## 3. Arayüz
Referans: `Docs/Tasarim/hedef_arayuz.png` (Onur). El kitabı ilk sekmesi: `Docs/Tasarim/el_kitabi_hizli_bakis.html`.
- **Masa (Desk, B):** üst yarı ofis (`office_bg`) + müteahhit portresi + konuşma balonu (harf harf yazı + yazı sesi); masa kenarı (`desk_edge`) ve yüzeyi (`desk_surface`). Masada üç belge düz ve yan yana (sürüklenebilir):
  - **Yapı Denetim Formu** (`doc_form`): proje bilgileri (proje, ada/parsel, adres, yapı sınıfı, kat adedi, yükseklik, kat başı kişi, müteahhit, proje müellifi, onay ücreti) + kat grubuna göre bina resmi, teknik bilgiler (beyan değerleri), belediye kaşesi, ONAY / RED.
  - **Laboratuvar Raporu** (`doc_lab`): laboratuvar, rapor tarihi, proje, ada/parsel, Parametre | Beyan | Ek test, karar vermeyen değerlendirme cümlesi, yetkili imzası, lab kaşesi + kaşedeki lab adı, EK TEST.
  - **Denetçi El Kitabı** (`doc_handbook`), sekmeler: HIZLI BAKIŞ · BETON · DONATI · YAPI · ZEMİN · LAB. Hızlı Bakış'ta oyuncu kat grubunu **kendisi seçer** (otomatik seçilmez, yeni müteahhitte sıfırlanır); 5 parametre kırmızı/yeşil çip ya da cetvel olarak boyanır. Eşikler Core'dan (`FindRule`), kodda sabit değer yok.
  - **İNCELE** düğmesi (lab raporunun üstünde, Tarık'ın yeri): mod açıkken belgeler sürüklenmez, karar düğmeleri pasif; alanlar sarı çerçeve alır; iki alan arasına çizgi + Onur'un sonuç kaşesi (`InspectResultView`). Sağ tık ya da boş masa seçimi temizler.
  - **Diyalog** (`DeskDialogue`): denetçi satırı solda Onur'un balonunda (`InspectorBubbleView`), müteahhit satırı kendi balonunda; sürerken masa kilitli, tıklama satırı tamamlar / geçer.
  - Zarf diyalogdan sonra masaya düşer, tıklanınca açılır (`BribeEnvelopeView`); kabul edilen hediyeler masa kenarında. 1. gün 5 öğretici kart (Geri düğmesi; masadaki "NASIL OYNANIR" yeniden açar); kabadayı olayı kartı; son müteahhitten sonra mesai zili + "MESAİ BİTTİ".
- **Akış ekranları (Flow, C):** gün başı (DayIntro), inşaat (Construction), gazete (Result), gün özeti (DaySummary), günlük ödeme (DailyExpenses), kayıt (Save), son (Ending), hareketli ana menü (MainMenu: Yeni Oyun / Devam Et / Emeği Geçenler / Çıkış), hikâye intro'su, Esc menüsü (her sahnede kendiliğinden kurulur; Ayarlar: ana ses + tam ekran).
- **Görsel dil:** el çizimi 2D cartoon (kalın kahve kontur, sıcak palet), görseller ChatGPT ile üretilip Claude script'leriyle hazırlandı. Yazı tipleri Paytone One (başlık) + Fira Sans (metin), ikisi de OFL, TMP asset'leri `Assets/_Project/Fonts/` (Static).

## 4. Mimari ve dosya haritası
- Tek `Game` sahnesi + prefab ekranlar. Diğer sahneler: `MainMenu` (Build Settings 1. sıra), `Sandbox_Desk`, `Sandbox_Flow`.
- Oyun mantığı saf C# (`GameSession`); tek MonoBehaviour `GameManager` faz sırasını yürütür ve sahnedeki `IGameScreen`'leri bulur. Ekranı olmayan faz: Editor'de mavi IMGUI yedeği, build'de atlanır (şu an 8 fazın hepsinin ekranı var).
- **Ekranlar Core'a sadece `ctx.Actions` (IGameActions) ile yazar** (Approve, Reject, BuyExtraTest, Inspect, SetExpensePaid, AcceptGift). Okuma `GameContext`'ten. Kullanım rehberi: `AHMET.md`.
- Assembly'ler: `CivilJam.Core` (+ `.Editor`, `.Tests`), `CivilJam.Desk`, `CivilJam.Flow` (+ PlayMode testleri). Desk ve Flow sadece Core'a bağlı, birbirini görmez (bu yüzden Onur'un masa bileşenleri Desk klasöründe).
- `Assets/_Project/`
  - `Scripts/Core/` (A): `Runtime/` (ProjectData, ParamEntry, DocumentData, GameState, GameContext, InspectOutcome, …), `Config/` (BalanceSO, RuleSetSO, ClientSO, DialogueSO, GiftSO, DayConfigSO, ExpenseSO, HeadlineSO, EndingSO, NamePoolSO), `Simulation/` (ProjectGenerator, VisitPlanner, Inspection, InspectionDialogue, EarthquakeSimulator, DisasterSystem, CasualtyGenerator, ExpenseCalculator, ClientReactions), `Game/` (GameSession, GameManager, FallbackGui, SaveSystem, GameLaunch), `Editor/` (menü CivilJam > Core: Install Screens, Link Data, Rebuild Handbook Rules, Balance Report, Content Stats, Build Windows (Release)), `Tests/Editor/` (EditMode 52/52).
  - `Scripts/Desk/` (B): `UI/DeskScreen` (IGameScreen) ve masa görünümleri (ayrıntı `OGUZHAN.md`); `Sandbox/`; `Editor/DeskSandboxBuilder(.Papers/.Inspect/.Dialogue)` (menü CivilJam > Desk > Build Sandbox_Desk Scene: prefab + sahne yeniden kurulur; prefab elle değiştirilmez).
  - `Scripts/Flow/` (C): `UI/` (FlowScreen tabanı, UiKit, ekranlar, MainMenuScreen, MenuAmbience, IntroSequence, PauseMenu), `Sandbox/`, `Editor/FlowBuilder`, `Tests/PlayMode/`.
  - `Prefabs/Desk/DeskScreen.prefab` (B), `Prefabs/Flow/*` (C). `Scenes/Game.unity` (**sadece A**, CoreSetupBuilder ile değişir).
  - `Data/` (A, içerik EEE): Balance, RuleSet, NamePool (bina adları, akredite lab'lar), Dialogue (denetçi şablonları + ortak replik havuzu), Headlines, Days (7 gün), Expenses, Clients (16), Gifts (6), Endings (4).
  - `Art/` ve `Audio/` (EEE): listeler `Art/README.md`, `Audio/README.md`. `Fonts/` (EEE).
- Build: `CivilJam > Core > Build Windows (Release)` → `Builds/Imza/Imza.exe`; teslim ZIP'ine `…BackUpThisFolder_ButDontShipItWithYourGame` klasörü konmaz. `Builds/` git dışında.
- Belgeler: `README.md` (jüri), `CREDITS_AND_AI.md` (asset + AI kaydı, oyun içi Emeği Geçenler metni), `ICERIK_TASLAK.md` (metin taslakları; oyun asset'lerden okur), kişisel planlar `AHMET.md`, `OGUZHAN.md`, `ONUR.md`, Onur'un bağlantı notları `Docs/Onur/`.

## 5. Takım, sahiplik, çalışma kuralları
- **A (Core) — Ahmet İlman:** Scripts/Core, Data, Game.unity, build. **B (Masa) — Oğuzhan Yurt:** Scripts/Desk, Prefabs/Desk, Sandbox_Desk. **C (Akış) — Onur Kabadayılar:** Scripts/Flow, Prefabs/Flow, Sandbox_Flow, MainMenu. **EEE — Tarık Hokkaömeroğlu:** Art, Audio, Fonts, içerik metinleri, CREDITS_AND_AI.md.
- Başkasının sahne/prefab dosyasını açıp kaydetme; başkasının script'ine dokunduysan burada not düş.
- Oyun kuralını değiştiren her öneri önce Ahmet'e; "Ahmet onayladı" notunu sadece Ahmet (ya da açık onayıyla) yazar.
- **Push'tan önce: pull → Unity'de derle → testler.** Commit'te `.meta`'ları unutma.
- Unity'yi **6000.6.5f1 ile açma** (proje sürümünü yükseltir); `ProjectVersion.txt` değişirse commit'leme.
- `Game.unity`'yi Editor'den kaydetme (prefab override'ları bozuluyor); değişiklikler CoreSetupBuilder ile.
- Unity'nin kendiliğinden değiştirdiği şu dosyaları commit'leme, pull engellerse `git checkout -- <yol>`: `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`, `Assets/_Project/Fonts/*SDF.asset`, `Assets/Settings/*`, `ProjectSettings/UnityConnectSettings.asset`.
- Görsel/ses: önce placeholder, final **aynı dosya adıyla** üzerine yazılır (GUID ve prefab bağlantıları korunur).
- Final aday build alındı: bundan sonra kod değişirse A yeniden build + ZIP alır.

## 6. Jam kuralları (ihlal = diskalifiye)
- Repo **public** kalmalı; build ile repo kodu uyumlu olmalı.
- Her hazır asset ve **her yapay zekâ kullanımı** eklendiği anda `CREDITS_AND_AI.md`'ye (küçük kullanım dahil). Yeniden dağıtılamayan asset repoya girmez. Jam öncesi hazırlanmış kod/asset yok.
- Teslim: Windows build ZIP + en fazla 1 dk video (MP4/AVI) + public repo + kurulum/kontrol talimatı (`README.md`).
- Saatler: **feature freeze ve Git desteği 18 Ekim 12:00**; yükleme 15:00-16:30; son teslim 18 Ekim 18:00.

## 7. Durum ve açık işler (güncelleme: B, 18 Ekim ~01:30)
**Durum:** İnceleme modu, uyuşmazlık diyaloğu, tıklanabilir zarf ve 7 günlük yapı bitti ve oyunda doğrulandı. EditMode 52/52, PlayMode 4/4; otomatik 7 günlük kabul oynanışı geçti (`AHMET.md`). **Final aday build** `Builds/Imza/Imza.exe` + teslim ZIP'i `Builds/Imza.zip` alındı. A'nın güncel teslim/kabul listesi: `AHMET.md` → "Onur'dan teslim notu — 18 Ekim". Freeze 18 Ekim 12:00.

**A (Ahmet):**
- [ ] Elle oynanış: 4 sonun hepsi (F9/F10), sahte belge ve ek test, kabadayı, hediye.
- [ ] Teslim: final build (kod değişirse yeniden), ZIP, README kontrolü, video planı.

**Oğuzhan (B):**
- [ ] El Kitabı sekme notları: `ICERIK_TASLAK.md` → "El Kitabı sekme notları" metnini `RulebookView.sectionNotes`'a aynen koy (prefab'da hâlâ boş).
- [ ] Formdaki "Proje müellifi" ile lab raporundaki "Laboratuvar Yetkilisi" aynı kişi (imza ipucu; inceleme 8. satır). Formdaki satırın adı "Denetim mühendisi" / "Raporu onaylayan mühendis" gibi, iki belgede aynı kişinin imzası beklendiği belli olsun (README de "aynı mühendisin imzası" diyor).
- [ ] El Kitabı Hızlı Bakış: 5 satır üstte toplanıyor, alt yarı boş. (Çipler ve yazılar `921fc8e`'de EEE tarafından büyütüldü; oyunda bakıp kalan boşluk varsa düzelt ya da maddeyi kapat.)
- [ ] Üst bar (Gün / müteahhit · proje / Kasa / Onaylanan · Bekleyen) eski koyu bant ve eski font; yeni görsel dile (Paytone/Fira, kağıt/ahşap) uydur.
- Not: ikili portre `g04` (boydan çift) masada kafalar küçük kalıyor; sırıtırsa Onur görseli yakınlaştırır (kod değişikliği değil).

**Onur (C):**
- [ ] 🔴 **Jam kuralı:** `Docs/Tasarim/hedef_arayuz.png`'yi hangi araçla ürettiğini `CREDITS_AND_AI.md` → "Yapay zekâ kullanımı" tablosuna yaz.
- [ ] Ödeme ekranı yanlış rakam gösterebiliyor (`DailyExpensesScreen.UpdateTotals` kasadan basit çıkarma yapıyor). Core hazır: seçim her değiştiğinde `var pv = ExpenseCalculator.Preview(Context.State, Context.PendingExpenses);` → `pv.MoneyAfter`, `pv.DebtAfter`, `pv.PaidTotal`, `pv.AddedToDebt`, `pv.DebtPaid`, kalem bazında `pv.Lines[i].Paid`.
- [ ] Kayıt ekranı (`SaveScreen`) her zaman "Kaydedildi" diyor. Core hazır: `Context.LastSave` (`Ok`, `Day`, `Error`); `Ok` false ise "Kayıt yapılamadı" + "Tekrar dene" (`Context.RetrySave()`, günü artırmaz) + "Kaydetmeden devam".
- [ ] Müzikler oyunda çalmıyor: `mus_menu` (ana menü) ve `mus_ending_sad` (Kötü + iki Orta son) hiçbir sahne/prefab/koda bağlı değil.
- [ ] (İsteğe bağlı) Gün başında `Context.LoadFailed` ise "Kayıt okunamadı, yeni oyun başladı." yaz.
- [ ] Gazete: "Hesaplaşmaya geç" düğmesi gazetenin sağ alt köşesinden taşıyor.
- [ ] Akış ekranlarının görsel dili: gün başı, inşaat, gün özeti, ödeme, kayıt, son EEE tarafından yapıldı (`c30100b`); **gazete ve Esc menüsü** hâlâ eski görünümde.
- ℹ️ `Flow/UI/PauseMenu.cs`'e A dokundu (Ahmet isteği): Ayarlar (ana ses `PlayerPrefs` "imza.masterVolume" + tam ekran), tam ekran ekranın gerçek çözünürlüğüne geçer. Test `Tests/PlayMode/SettingsMenuTests.cs`.

**Tarık (EEE):**
- [ ] `README.md`: son hâli, oyun ekran görüntüsü ve video bağlantısı (Pazar).
- [ ] Pazar: 1 dakikalık video (A ile senaryo).

## 8. Karar günlüğü (güncel kararlar; eski satırlar `f14d2ed`'de)
| Ne zaman | Karar | Gerekçe |
|---|---|---|
| 16 Ekim | Oyun "İmza": masa başı mühendis onay oyunu; tek Game sahnesi + prefab ekranlar; saf C# Core, Core/Desk/Flow asmdef; Game.unity sadece A | Tema ile doğrudan bağ, paralel çalışma |
| 16 Ekim | Strength ≥ 1 asla yıkılmaz; şiddet gizli, bina ayakta/yıkık | Dürüst oyuncu cezalandırılmaz |
| 16 Ekim | İki katmanlı sahte belge: bedava ipuçları (parsel/tarih/lab/kaşe/imza) + ücretli ek test; kaşe ve imza metin olarak | Dikkat ödüllendirilir; ipucu veriden sızmaz |
| 16 Ekim | Gün sistemi; günlük ödeme; afet son afetten beri onaylananları değerlendirir; deprem sahnesi yok, gazete var | Ahmet / ekip |
| 16 Ekim | Kota yok; baskıyı günlük gider ve borç kurar | Ahmet |
| 17 Ekim | Borç: kasa eksiye düşmez, yetmeyen zorunlu gider borca; borç > 2.000 → ofis kapanır. 4 son ve öncelik (hapis > ofis > yıkım > iyi). 10 gün; fabrika/yangın/sigorta yok. Son ekranında kayıt silinir. F9/F10 | Ahmet (analiz sonrası) |
| 17 Ekim | Denge (senaryo E): ücret 300-500, kira 600, deprem %20 +%15 | Simülasyonla seçildi |
| 17 Ekim | Oynanış testi: gizli proje türleri, gevşek müteahhit rolleri, tekrarsız bina adları, tepki/hafıza/hediye | Ahmet oynadı: oyun çok kolay ve tekdüzeydi |
| 17 Ekim öğle | El Kitabı v2: mahalle/bölge kaldırıldı; 5 parametre, kat grubuna göre 3 değer; demir B420/B500/B500S, B220 hiç kabul edilmez | Ahmet: daha az ama anlaşılır parametre |
| 17 Ekim | Hediyeyi sadece dürüst ustalar verir | Rüşvetle karışmasın |
| 17 Ekim | 6 müteahhit kalır; karaktere özel ses yok, harf harf yazı sesi; fontlar Paytone One + Fira Sans | Tarık |
| 17 Ekim ~15:00 | **Yeni hedef arayüz:** üst yarı ofis + müteahhit, alt yarı 3 sabit belge. Kurallar, lab raporunun rolü ve Core sözleşmesi değişmez. Eğimli masa, masadaki kitaplar, piksel tasarım iptal | Ahmet |
| 17 Ekim | El Kitabı sekmeleri BETON / DONATI / YAPI / ZEMİN; ilk sekme "Hızlı Bakış" (oyuncu kat grubunu seçer, otomatik değil) | Ahmet: eski sayfa kullanıcı dostu değildi; zorluk korunur |
| 17 Ekim | Ödeme önizlemesi ve kayıt sonucu Core'dan (`ExpenseCalculator.Preview`, `ctx.LastSave`, `ctx.RetrySave`) | Onur'un ihtiyacı; ekran ikinci ekonomi hesabı yazmasın |
| 17 Ekim 16:10 → ~17:30 | Yeni arayüz bitene kadar yeni iş yok (bitti, kalktı). Yeni özellikler Ahmet'in kararıyla eklenebilir | Ahmet |
| 17 Ekim `1368c4f` | Aynı müteahhit arka arkaya gelmez (gün içinde ve dünün sonu → bugünün başı); yerine sadece o türe uyan biri gelir, rol kuralları ve oranlar aynı | Tarık isteği; `ProjectGenerator.PickClient(previousClientId)`, test `Clients_DoNotComeTwiceInARow` (EditMode 32/32). Core A dosyası: A inceledi, testler 32/32 |
| 17 Ekim akşam | "Mesaiyi bitir" düğmesi kalktı: son müteahhitten sonra mesai zili (`sfx_shift_end.wav`) + "MESAİ BİTTİ" geçiş ekranı (günün özeti), ardından inşaat ekranı koyu örtüyle açılır | Tarık önerisi, A ve B onayladı. `DeskScreen.EndShift` + `ShiftEndOverlay`; süreler Inspector'da (1,2 / 0,7 / 1,8 sn) |
| 17 Ekim akşam | İnceleme modu (Papers, Please tarzı) planlandı: iki ayrı belgeden iki alan → SORUN YOK / UYUŞMAZLIK SAPTANDI / ALAKASIZ; eşik seçili kat grubundan (yanlış grup = ALAKASIZ), künye dahil, kaşe akredite listeyle, İncele modu, bedelsiz; A Core, Oğuzhan masa, Onur mesaj. **Uygulama Ahmet onayından sonra** | Ahmet + Onur fikri; Ahmet kararları. Olasılıklar değişmez |
| 17 Ekim akşam | İnceleme modu ONAYLANDI + Core düzeltmeleri: (a) başka lab kaşesi ipucunda kaşeye akredite olmayan lab adı, (b) sahte parsel gerçeğiyle asla aynı değil. Onur'un mesaj bileşeni Desk klasöründe (asmdef kuralı) | Ahmet |
| 17 Ekim ~19:20 | Rüşvet uyuşmazlıktan sonra: zarf başta yok; ilk UYUŞMAZLIK'tan sonra denetçi ve müteahhit birer cümle konuşur, rüşvetçi o anda teklif eder (teklif + denetçi tepkisi + kapanış); uyuşmazlık bulunmazsa zarf hiç teklif edilmez; diyalog farklı alan başına bir kez; denetçi cümlesi Core şablonundan; replikleri Tarık yazar | Ahmet kararı. Rüşvet "yakalanan müteahhidin hamlesi" olur; tema ve Papers, Please hissi |
| 17 Ekim gece | Yeni ekonomi: başlangıç 1.500, kira 700, kredi 350, ücret 350-550, ek test 250, borç limiti 2.500 | Tarık önerisi, Ahmet onayı. Eski değerlerde dikkatli oyuncu hiç borca girmiyordu; şimdi kıl payı kazanıyor (Balance Report §2) |
| 17 Ekim gece | Değer farkı: lab beyanı formdan farklı yazabilir (sahtede gerçek kötü, uygun projede masum yazım hatası); bazı sahtelerde künye ipucu yok, sadece ek test kanıtlar | Tarık önerisi, Ahmet onayı: ek testin net bir işi yoktu (her sahtede bedava ipucu vardı). Core + testler EEE yazdı (`ProjectGenerator.AddLabDifference`, `Inspection`, `DialogueSO.FormVsLab`); EditMode yeşil |
| 17 Ekim gece | Bir müteahhit günde bir kez gelir (günün tamamı birlikte yerleştirilir, geri izlemeli); 5. günden kötü proje oranı 0,6; ekonomi bilerek zor: ücret 500-700, kira 750, kredi 400, kabadayı hasarı 600 (4 farklı olay), ek test 400, borç limiti 2.000 | Tarık kararı. Balance Report: dürüst oyuncu %47 İyi / %53 iflas (600-800 ile %86 İyi idi). EditMode 47/47 (InspectionDialogue seed 5→7, Reaction'da bir test seed 12: yeni sırada eski seed'ler rüşvetli proje üretmiyordu) |
| 17 Ekim ~23:30 | **7 gün** (3-3-4-4-4-5-5 = 28 proje); 6 ana müteahhit 3'er kez (oyun başı sabit rastgele program, aynı gün iki kez değil, arka arkaya değil); 10 kişiliksiz karakter kalan 10 ziyaret (zarf/sahte belge getirebilir); projeler tam 14 uygun / 14 uygunsuz; gün planı: 2. gün zarf, 3. gün sahte belge, 4. gün kabadayı | Ahmet kararı: 10 gün sıkıcıydı; yüz kararı ele vermesin |
| 17 Ekim gece | Esc menüsüne Ayarlar: ana ses (kaydedilir) + tam ekran | Ahmet isteği; müzik / efekt ayrımı şimdilik yok (ses kaynakları dağınık, ekibin dosyalarına dokunmamak için) |
| 18 Ekim ~00:30 | Rüşvet zarfı tıklanabilir: masada tutar görünmez; açılınca para + tutar, RÜŞVETİ KABUL ET (zarfla onay) / RÜŞVETİ REDDET (zarf masaya döner, proje bekler); formdaki "ZARFI AL VE ONAYLA" kalktı | Ahmet isteği; görsel Tarık (ChatGPT) |
| 18 Ekim ~01:00 | Denetçi balonu solda ofisin önünde; İNCELE düğmesi lab raporunun üstünde (Tarık'ın yeri) kalır | Oğuzhan kararı (A'nın yerleşim notu üzerine) |

## Güncelleme kuralı
- Karar, mimari değişiklik, yeni kural ya da biten büyük iş olunca ilgili bölümü güncelle ve §8'e satır ekle.
- Kısa tut: kod kopyalama, uzun oturum notu yazma (onlar kişisel dosyalara). Biten maddeyi §7'de `[x]` yap; bir sonraki temizlikte silinir.
