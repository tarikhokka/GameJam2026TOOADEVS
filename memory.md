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

**İnceleme modu (`Inspection.Check`, masadan `ctx.Actions.Inspect`):** oyuncu İNCELE ile iki **ayrı** belgeden iki alan seçer → SORUN YOK / UYUŞMAZLIK SAPTANDI / ALAKASIZ. Ücretsiz, sınırsız; kararı yine oyuncu verir. El Kitabı eşiği oyuncunun Hızlı Bakış'ta **seçtiği kat grubundan** alınır (seçilmemiş ya da binaya ait olmayan grup → ALAKASIZ). ⚠️ **Poliş turunda değişiyor (K1, §7):** eşik her zaman binanın kendi kat grubundan; yanlış grup durumu kalkar. Tabloda olmayan her çift ve aynı belgeden iki alan → ALAKASIZ.

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
  - **Denetçi El Kitabı** (`doc_handbook`), sekmeler: HIZLI BAKIŞ · BETON · DONATI · YAPI · ZEMİN · LAB. Hızlı Bakış'ta oyuncu kat grubunu **kendisi seçer** (otomatik seçilmez, yeni müteahhitte sıfırlanır) ⚠️ **Poliş turunda değişiyor (§7):** kat grubu otomatik, bölüm sekmeleri ve RED kutusu kalkıyor; 5 parametre kırmızı/yeşil çip ya da cetvel olarak boyanır. Eşikler Core'dan (`FindRule`), kodda sabit değer yok.
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

## 7. Durum ve açık işler (güncelleme: A, 18 Ekim ~01:50)
**Durum:** İnceleme modu, uyuşmazlık diyaloğu, tıklanabilir zarf ve 7 günlük yapı bitti ve oyunda doğrulandı. EditMode 52/52, PlayMode 4/4; otomatik 7 günlük kabul oynanışı geçti (`AHMET.md`). **Final aday build** `Builds/Imza/Imza.exe` + teslim ZIP'i `Builds/Imza.zip` alındı. A'nın güncel teslim/kabul listesi: `AHMET.md` → "Onur'dan teslim notu — 18 Ekim". Freeze 18 Ekim 12:00.

### 🛠️ POLİŞ TURU — 7 madde (Ahmet kararı, 18 Ekim ~01:50) — HERKES OKUSUN · lider: A (Ahmet'in ajanı)
> Ahmet 7 maddelik özellik ve cila listesi verdi. A iş bölümünü ve fazları kurdu, Ahmet 4 kararı onayladı (aşağıda **Kararlar**).
>
> Herkes şunları yapsın:
> 1. **Önce pull.**
> 2. Kendi bölümünü ve **Dosya kilitleri** tablosunu oku.
> 3. Bitirdiğin işi burada `[x]` yap, yanına commit hash'ini yaz (ör. "B1 ✅ `abc1234`"). Senkron noktaları bu satırlara bakar.
> 4. Gecikeceksen tahmini saatini yaz.
>
> Feature freeze **12:00**. A'nın build alabilmesi için son kod push'u **en geç 11:00**.

**Maddeler (Ahmet'in istediği):**
1. **Kat grubu otomatik.** El Kitabı'nın Hızlı Bakış sayfasında binanın kat grubu (1-3 / 4-6 / 7+), formdaki kat adedine göre kendiliğinden seçili gelir. Oyuncu seçmez.
2. **İnceleme modu.** Tıklanabilir (sorgulanabilir) alanlar ve ONAY / RED dışındaki her yer **çok az** kararır. ONAY ve RED inceleme modunda da basılabilir.
3. **Düğmeler.** Masadaki İNCELE ve NASIL OYNANIR düğmeleri ile ana menüdeki Devam Et / Yeni Oyun / Emeği Geçenler / Çıkış düğmeleri oyunun temasına uyar. ONAY / RED / EK TEST ile aynı aileden görünürler.
4. **Üst bar.** Gün, müteahhit · proje, Kasa, Onaylanan · Bekleyen gösteren bar temaya uygun yeniden tasarlanır.
5. **RED kutusu.** El Kitabı Hızlı Bakış'ın en altındaki kutu ("RED / Tek bir kırmızı yeter…") kalkar. Sayfadaki 5 parametre büyür, okunması kolaylaşır.
6. **Bölüm sekmeleri.** El Kitabı'nda BETON (kırmızı), DONATI (mavi), YAPI (sarı) ve ZEMİN (yeşil) sekmeleri, açtıkları sayfalarla birlikte kalkar. **HIZLI BAKIŞ ve LAB sekmelerine ve LAB sayfasına dokunulmaz.** Hızlı Bakış'ın içi sadece madde 5 kadar değişir.
7. **Ziyaretler.** 10 kişiliksiz karakter bir oyunda 1'er kez, 6 ana müteahhit eşit olarak 3'er kez gelir. Hiçbiri aynı gün iki kez gelmez.

**Kararlar (Ahmet onayladı, 18 Ekim ~01:50):**
- **K1 (madde 1):** 1-3 / 4-6 / 7+ düğmeleri **sadece gösterge** olur:
  - binanın grubu koyu (seçili) görünür, diğer ikisi soluk durur ve tıklanmaz;
  - inceleme her zaman binanın gerçek grubuyla yapılır, grubu Core kendisi hesaplar;
  - "yanlış kat grubu = ALAKASIZ" durumu oyundan kalkar;
  - Hızlı Bakış'taki 2. adım yazısı değişir.
- **K2 (madde 2):** İnceleme modunda ONAY ya da RED'e basılınca **inceleme modu kendiliğinden kapanır ve karar verilir**. Kaşe, tepki ve sıradaki müteahhit bugünkü gibi gelir.
  - **EK TEST ve zarf inceleme modunda pasif kalır ve kararır.**
  - **İNCELE düğmesi aydınlık kalır**, çünkü mod onunla kapanır.
- **K3 (madde 6):** Bölüm sekmeleri **tamamen kalkar**. "AFETE HAZIRLIK" notları ve parametre açıklamaları başka yere taşınmaz; tema mesajı gazete, sonlar, diyalog ve öğretici kartlarda zaten var. **Oğuzhan'ın "El Kitabı sekme notları" işi iptal.**
- **K4 (madde 3-4):**
  - **Tarık** görselleri üretir (düğmeler, üst bar, temiz menü arka planı).
  - **Onur** ana menü düğmelerini ve kendini kuran üst bar bileşenini (`DeskHeaderView`) yazar. Bileşen `BribeEnvelopeView` gibi Desk klasöründe durur.
  - **Oğuzhan** İNCELE / NASIL OYNANIR düğmelerini ve üst barı masaya bağlar.
- **Madde 7 kodda zaten var:**
  - `VisitPlanner` (`GameState.Schedule`, kayıtla saklanır);
  - test `Plan_FollowsAllRules_ForManySeeds` (300 seed) var;
  - otomatik 7 günlük oynanışta görüldü.

  A sadece gerçek 16 kişilik kadroyla (`Data/Clients`) bir test ekler. Başka kimsenin işi yok.

**İş bölümü:**
| İş | Kim | Madde | Başlamak için beklediği push |
|---|---|---|---|
| **A1** Core: kat grubunu Core hesaplar + madde 7 için gerçek kadro testi | A | 1, 7 | yok |
| **A2** El Kitabı: otomatik kat grubu, RED kutusu kalkar, parametreler büyür, bölüm sekmeleri kalkar (Oğuzhan'ın dosyası, A dokunur) | A | 1, 5, 6 | A1 (aynı kişi, sıralı) |
| **B1** İnceleme modunda karartma + ONAY/RED aktif | Oğuzhan | 2 | yok |
| **B2** İNCELE / NASIL OYNANIR yeni görselle; üst bar → `DeskHeaderView` | Oğuzhan | 3, 4 | B1 (aynı prefab), **T1** (görsel), **C1** (bileşen) |
| **C1** `DeskHeaderView` (görselsiz de çalışır) + kendi bileşenlerinin karartmanın üstünde kaldığının kontrolü | Onur | 4, 2 | yok (kontrol kısmı B1'den sonra) |
| **C2** Ana menü düğmeleri yeni görselle (+ Emeği Geçenler'deki Geri; vakit varsa Esc menüsü) | Onur | 3 | **T1** |
| **T1** Görseller: düğme ailesi, üst bar, temiz `menu_bg` | Tarık | 3, 4 | yok |
| **T2** Metinler: öğretici kartlar, README, ICERIK_TASLAK, Art/README | Tarık | 1, 5, 6 | yok |
| **A3** Entegrasyon: testler, otomatik 7 günlük oynanış, ekran görüntüleri, §2-§3, build + ZIP | A | hepsi | hepsi |

**Fazlar ve bekleme kuralı (Ahmet kararı, 18 Ekim ~02:15) — HERKES OKUSUN**

> ⛔ **KURAL: Faz 1'in 6 işinin hepsi ✅ olmadan kimse Faz 2'ye geçmez.** Kendi Faz 1 işini erken bitiren de Faz 2 işine başlamaz, kod yazmaz, prefab kurmaz. Faz 2 de bitmeden Faz 3 başlamaz.
> Kapıyı aşağıdaki **Faz durumu** tablosu açar. Her oturum başında pull et ve bu tabloya bak.

**Faz durumu** (işini bitiren kendi satırını günceller):
| Faz | İş | Kim | Durum |
|---|---|---|---|
| 1 | A1 Core: kat grubu Core'da + madde 7 gerçek kadro testi | A | ✅ `fb60575` |
| 1 | A2 El Kitabı: otomatik kat grubu, RED kutusu, parametreler, sekmeler | A | ✅ `93686f0` |
| 1 | B1 İnceleme modunda karartma + ONAY/RED aktif | Oğuzhan | ✅ `822495c` |
| 1 | C1 `DeskHeaderView` (üst bar bileşeni, prefab'a dokunmadan) | Onur | ✅ `1b47ce6` |
| 1 | T1 Görseller | Tarık | ✅ `f8842b5` |
| 1 | T2 Metinler | Tarık | ✅ `1dd7870` |
| 2 | B2 İNCELE / NASIL OYNANIR görselleri + üst barı masaya bağlama | Oğuzhan | 🔒 Faz 1 bekleniyor |
| 2 | C2 Ana menü düğmeleri (+ Geri, vakit varsa Esc menüsü) | Onur | 🔒 Faz 1 bekleniyor |
| 2 | C1-K Karartma kontrolü (`InspectResultView`, `InspectorBubbleView` karartmanın üstünde mi) | Onur | 🔒 Faz 1 bekleniyor |
| 3 | A3 Entegrasyon, test, ekran görüntüleri, build + ZIP | A | 🔒 Faz 2 bekleniyor |

**Faz 0 (bitti, `d0c4e59`):** Plan push'landı.

**Faz 1 (hedef 05:00) — herkes paralel, kimse kimseyi beklemez:**
- A: A1, sonra A2 (A2, A1'in Core fonksiyonunu kullanır; ikisi de A'nın, sıralı).
- Oğuzhan: B1. Kimseyi beklemez.
- Onur: C1. Kimseyi beklemez; T1 görselleri hazır, doğrudan onları kullanabilir.
- Tarık: T1 ✅ ve T2 ✅. Faz 1'i bitti.
- Faz 1'de aynı dosyaya iki kişi dokunuyor: `DeskScreen.cs`. A2 tek bir satırı (`rulebook.ResetFloorGroup()`) değiştirir, B1 başka satırları. Bu bir bekleme değil: önce kim push'larsa, diğeri pull edince git kendisi birleştirir. Çakışma çıkarsa ikisini de tutup birleştir.
- Faz 1'ini bitiren:
  - Faz durumu tablosunda satırını ✅ ve hash yapar.
  - Kapı açılana kadar sadece **bu turla ilgisi olmayan eski §7 maddelerine** (Önceki açık işler) ya da teslim hazırlığına (video, README görseli) bakabilir.
  - Faz 2 dosyalarına dokunmaz: `MainMenuScreen.cs`, `DeskSandboxBuilder*.cs`, `DeskHud.cs`, `DeskPropsView.cs`, `DeskScreen.prefab`.
- Gecikecek olan tahmini saatini tablodaki satırına yazar. **Faz 1 06:00'ya kadar bitmezse ne yapılacağına Ahmet karar verir.**

**Kapı 1 → 2:** A1, A2, B1, C1, T1 ve T2'nin hepsi ✅ olduğunda A bu tabloda Faz 2 satırlarını "🟢 başla" yapar ve push'lar. Herkes bunu pull ile görür.

**Faz 2 (hedef 08:30) — kapı açılınca:**
- **Oğuzhan, B2.** İhtiyaç duyduğu her şey Faz 1'de geldi: kendi B1'i (aynı prefab), Tarık'ın görselleri (T1) ve Onur'un `DeskHeaderView`'u (C1). Başlamadan önce pull eder. B2, `DeskScreen.prefab`'ı builder'la kuran **tek** iştir.
- **Onur, C2.** T1 görselleri hazır, kimseyi beklemez. B2 ile aynı anda yapılır; dosyaları ayrı (Flow ve Desk).
- **Onur, C1-K.** B1 artık pull'da. Sandbox_Desk'te kendi sonuç kaşesinin ve denetçi balonunun karartmanın üstünde kaldığını kontrol eder.
  - Sorun yoksa satırı ✅ yapar.
  - Sorun varsa §7'de Oğuzhan'a yazar; düzeltme B2'nin içinde yapılır. **Bu, Faz 2'deki tek bekleme:** sorun çıkarsa Oğuzhan, B2'yi Onur'un notundan sonra bitirir.
- Faz 2'de A ve Tarık:
  - A, Faz 1'de gelen push'ları test eder ve sorunları ilgili kişiye yazar.
  - Tarık, video ve README ekran görüntüsü planını hazırlar.
  - Faz 2 dosyalarına dokunmazlar.

**Kapı 2 → 3:** B2, C2 ve C1-K ✅ olduğunda A Faz 3'e başlar.

**Faz 3 (08:30 → 11:00) — A:**
- A3: hepsini pull, EditMode + PlayMode testleri, otomatik 7 günlük oynanış, ekran görüntüleri (masa, inceleme modu, El Kitabı, üst bar, ana menü, Esc menüsü), §2-§3 ve README son kontrol, build + ZIP.
- A hata bulursa sahibine §7'de yazar. Sahibi sadece o hatayı düzeltir, push'lar ve haber verir; A tekrar test eder. **Faz 3'te bekleme bu döngüdür.**
- **Son kod push'u en geç 11:00.** 11:00-12:00 arası sadece A'nın istediği hata düzeltmeleri yapılır. **12:00 freeze:** sonrasında kod push'u yok.

**Kim kimi bekliyor (özet):**
| Bekleyen | Beklenen | Neden |
|---|---|---|
| Herkesin Faz 2 işi | Faz 1'in 6 işinin hepsi | Ahmet kuralı: Faz 1 bitmeden Faz 2 yok |
| A2 | A1 | A2, A1'in Core fonksiyonunu kullanır (aynı kişi) |
| B2 | B1, T1, C1 (hepsi Faz 1'de) | Aynı prefab; görseller; üst bar bileşeni |
| C2 | T1 (Faz 1'de) | Yeni menü görselleri |
| C1-K | B1 (Faz 1'de) | Karartma katmanı olmadan kontrol edilemez |
| B2'nin bitişi | C1-K (sadece sorun çıkarsa) | Karartma sırası düzeltmesi B2'de yapılır |
| A3 (Faz 3) | B2, C2, C1-K | Birleştirme ve build için herkesin işi |
| Build + ZIP | A3'teki hata düzeltmeleri | Son build tek seferde, temiz koddan |

**Dosya kilitleri** (bu turda dosyaya sadece yazılan kişi dokunur; başkasına bir şey lazımsa burada yazar):
| Dosya | Kim | Not |
|---|---|---|
| `Scripts/Core/**`, `Data/**`, `Scenes/Game.unity` | A | |
| `Desk/UI/RulebookView.cs`, `Desk/UI/HandbookUi.cs` | **A (bu tur için)** | Oğuzhan ve Tarık bu turda dokunmaz |
| `Desk/Sandbox/FakeGameActions.cs` | A (1 satır) | |
| `Desk/UI/DeskScreen.cs` | Oğuzhan; A sadece 1 satır | A2'de `rulebook.ResetFloorGroup()` satırı değişir. Farklı satırlar olduğu için git kendisi birleştirir |
| `Desk/UI/InspectController.cs`, `InspectSpot.cs`, `InspectCatcher.cs`, `FormView.cs`, `LabReportView.cs`, `DeskHud.cs`, `DeskPropsView.cs`, `Desk/Editor/**`, `Prefabs/Desk/DeskScreen.prefab` | Oğuzhan | **Prefab'ı sadece Oğuzhan kurar** (builder). Başkası prefab commit'lemez |
| `Desk/UI/DeskHeaderView.cs` (yeni), `InspectResultView.cs`, `InspectorBubbleView.cs` | Onur | |
| `Flow/**`, `Prefabs/Flow/**`, `Scenes/MainMenu.unity` | Onur | |
| `Desk/UI/TutorialView.cs` (sadece metin dizeleri), `Art/**`, `README.md`, `ICERIK_TASLAK.md`, `CREDITS_AND_AI.md` | Tarık | A, A3'te README'ye son kontrol için dokunabilir |

#### A (Ahmet) — A1, A2, A3
- [x] **A1 ✅ `fb60575`** + **A2 ✅ `93686f0`** (EditMode 55/55, PlayMode 4/4; El Kitabı ekran görüntüsüyle doğrulandı: 4 katlı binada 4-6 KAT, 7 katlıda 7+ KAT kendiliğinden seçili; sekmeler HIZLI BAKIŞ + LAB). **Oğuzhan, pull et:**
  - `DeskScreen.Select`'te tek satır değişti: `rulebook.ResetFloorGroup()` → `rulebook.ShowProject(context.GetTodayProject(projectId))`. `ResetFloorGroup` artık yok.
  - `RulebookView.SelectedFloorGroup` duruyor (DeskScreen.Inspect hâlâ gönderiyor), ama Core onu yok sayar (`Inspection.FloorGroupOf`).
  - Kat düğmeleri artık `Button` değil (sadece gösterge). B1'de karartmayla öne alınacak El Kitabı alanları sadece 5 parametre satırı ve LAB'daki akredite liste (`InspectSpot`).
  - `HandbookUi.Chips.Paint`: boyanan çipin etiketi de iki satır kalır (önceden "Yumuşak kil" tek satıra dönüp `ChipFontEqualizer` bütün satırı küçültüyordu; EEE'nin kodu, A düzeltti).
  - `RulebookView.sectionNotes` silindi (builder'da kullanılmıyordu); `tabSprites` ve `floorIcons` builder için yerinde.
- [ ] **A1 Core (madde 1, 7):**
  - Yeni `Inspection.FloorGroupOf(GameContext, ProjectData)`: binanın kat adedine göre RuleSet aralıklarından 0 / 1 / 2 döner; bulunamazsa `NoFloorGroup`.
  - `GameSession.Inspect`, gelen `handbookFloorGroup`'u **yok sayar** ve binanın grubunu kullanır.
  - `IGameActions.Inspect` imzası **değişmez**; kimsenin kodu kırılmaz. `Inspection.Check(…, group)` testler için aynı kalır.
  - `FakeGameActions.Inspect` (Desk sandbox) da aynı şeyi yapar.
  - Testler:
    - oturumdan yapılan inceleme her zaman doğru grupla yapılır;
    - madde 7: `Data/Clients`'taki gerçek kadroyla 500 seed. Her ana müteahhit 3 kez ve 3 ayrı günde gelir, her kişiliksiz karakter 1 kez gelir, bir günde aynı kişi iki kez yoktur.
- [ ] **A2 El Kitabı (madde 1, 5, 6; Oğuzhan'ın dosyası, Ahmet isteğiyle A dokunur):**
  - Yeni `RulebookView.ShowProject(ProjectData)` binanın grubunu seçili gösterir. `DeskScreen`'deki `rulebook.ResetFloorGroup()` satırı bununla değişir.
  - Kat düğmeleri tıklanmaz: seçili olan koyu, diğerleri soluk.
  - Adım yazıları: "1 · Formda KAT ADEDİ'ne bak", "2 · Kat grubu kendiliğinden seçilir", "3 · Her değer yeşil mi?".
  - RED kutusu ve alttaki boşluk kalkar; 5 satır sayfayı doldurur. Satır yüksekliği, parametre adı, çipler ve cetvel yazıları büyür. 1920x1080'de hiçbir yazının kesilmediği ekran görüntüsüyle doğrulanır.
  - BETON / DONATI / YAPI / ZEMİN sayfaları ve sekmeleri kalkar; geriye HIZLI BAKIŞ ve LAB kalır. LAB sayfası aynen durur.
  - `tabSprites` ve `sectionNotes` alanları kullanılmasa da yerinde kalır, böylece builder ve prefab değişmez (sayfalar çalışırken kuruluyor).
- [ ] **A3 Entegrasyon (Faz 3):**
  - hepsini pull, EditMode + PlayMode testleri, otomatik 7 günlük oynanış;
  - ekran görüntüleri: masa, inceleme modu, El Kitabı, üst bar, ana menü, Esc menüsü;
  - §2-§3'ü yeni hâle göre yaz, ⚠️ işaretlerini kaldır; README son kontrol;
  - build + ZIP.

#### Oğuzhan (B) — B1, B2
- [x] **B1 ✅ `822495c`** (Sandbox_Desk'te denendi: karartma, öne alınan alanlar + kağıt zemin, ONAY/RED modda karar verir, diyalog sırasında kilit, zarf modda açılmaz. Öne alma `InspectLift` ile: zemin +1, alanlar / ONAY / RED / müteahhit balonu +2, İNCELE ve diyalog katmanları (`InspectResultView`, `InspectorBubbleView`) +3. **Onur (C1 kontrolü):** bileşenlerin bu katmanlarda, ek iş beklemiyorum.) **B1 İnceleme modunda karartma + ONAY/RED aktif (madde 2, K2):**
  - **Karartma katmanı:**
    - tam ekran siyah `Image`, alfa **yaklaşık 0,2** ("çok azıcık"); Inspector'dan ayarlanır, ör. `inspectDimAlpha`;
    - `raycastTarget = false`;
    - mod açılınca yaklaşık 0,15 sn'de yumuşakça gelir, kapanınca gider (`Time.unscaledDeltaTime`);
    - ofis, portre, masa, belgeler ve üst bar kararır.
  - **Aydınlık kalanlar** (karartmanın üstünde):
    - mod açıkken **tıklanabilir her `InspectSpot`**. Bu projede tıklanamayan alan karanlık kalır; ör. ek test yapılmadıysa ek test hücresi (`Inspection.IsAvailable`);
    - **ONAY** ve **RED**;
    - **İNCELE** düğmesi;
    - çizgi ve sonuç kaşesi (Onur'un `InspectResultView`'u);
    - **diyalog balonları**: denetçi balonu `InspectorBubbleView` ve müteahhit balonu. Uyuşmazlık diyaloğu mod açıkken başlıyor, okunabilmeli.
    - Öğretici kartlar, olay kartı, zarf ekranı, mesai bitti ekranı ve Esc menüsü zaten en üstte kalmalı.
  - **Yöntem önerisi** (uGUI'de karartmaya delik açılamaz):
    - karartmayı belgelerin üstüne koy;
    - aydınlık kalacak her öğeye mod açıkken `Canvas` (`overrideSorting = true`, `sortingOrder` = karartma + 1) ve `GraphicRaycaster` ekle;
    - mod kapanınca bunları kaldır ya da `overrideSorting = false` yap;
    - arka planı şeffaf satırlar kararmış kağıdın üstünde kalır; öne alınan alanın arkasına kağıt renginde ince bir zemin koyarsan vurgu gibi görünür.
    - Daha temiz bir yol bulursan onu kullan.
  - **ONAY / RED:**
    - `FormView.SetLocked(on)` artık ONAY ve RED'i kapatmaz. EK TEST lab raporunda kapalı kalır; `takeBribeButton` zaten gizli.
    - `DeskScreen.Decide`: mod açıksa önce `inspect.SetMode(false)`, sonra bugünkü karar sırası. `stamping` ve diyalog kilidi aynen kalır.
    - Zarf (`OpenEnvelope`) inceleme modunda bugünkü gibi açılmaz.
  - **Dikkat:**
    - `InspectCatcher` ve spot'lar ONAY / RED tıklamasını yutmamalı;
    - ONAY / RED tıklaması "boş masaya tıklandı, seçimi temizle" sayılmamalı.
  - Sandbox_Desk'te dene: mod aç/kapat, alan seç, karşılaştır, uyuşmazlık diyaloğu, modda ONAY ve RED. Prefab'ı builder'la kur, push, "B1 ✅ hash" yaz.
- [ ] **B2 Düğmeler ve üst bar (madde 3, 4) — FAZ 2: Faz 1'in hepsi ✅ olmadan başlama:**
  - **Düğmeler:**
    - İNCELE → `btn_inspect` (`DeskSandboxBuilder.Inspect.cs`);
    - NASIL OYNANIR → `btn_help` (builder'daki `helpSprite`; TutorialView kurar);
    - yazılar kodla, Paytone One; renk görsele göre;
    - fare üstüne gelince ONAY / RED gibi davranır (hafif büyüme / parlama).
  - **Üst bar:**
    - `BuildHud`'daki eski koyu bant (`HudColor`, 80 px) yerine `DeskHeaderView` kurulur;
    - **yükseklik aynı kalır (80 px)**: ofis ve belgeler yerinden oynamaz;
    - `DeskHud.Refresh` ve `DeskPropsView`'daki müteahhit · proje yazısı (`clientTagText`) `DeskHeaderView` API'sine bağlanır (C1'deki API);
    - eski metin alanları silinebilir.
  - Prefab'ı builder'la kur, push, "B2 ✅ hash" yaz.
- Eski §7 maddelerin:
  - "sekme notları" **iptal** (K3);
  - "Hızlı Bakış alt yarı boş" **A2'ye geçti**;
  - "Üst bar" **B2'ye geçti**;
  - "Proje müellifi / Laboratuvar Yetkilisi" maddesi duruyor (vakit kalırsa).

#### Onur (C) — C1, C2
> **Onur'un kapsam düzeltmesi:** Tarık'ın T1/T2 işleri devralınmıyor; Onur yalnız C1/C2'yi yapacak. T1/T2 teslimleri alındı. C1 hazır; C2 ve C1-K için Faz 2 kapısı beklenir. Plan: `Docs/Onur/Polis_turu_yol_haritasi.md`.

- [x] **C1 ✅ `1b47ce6` — `DeskHeaderView` hazır:** T1 görselleri/fontlar, 80 px yükseklik, üç API ve görselsiz yedek görünüm. İzole Unity 6000.6.4f1: 55/55 EditMode + 10/10 PlayMode (6 geçici C1 kontrolü + 4 mevcut Flow testi) ve gerçek render kontrolü geçti. Bağlantı: `Docs/Onur/DeskHeaderView_baglanti.md`; önizleme: `Docs/Onur/DeskHeaderView_onizleme.png`. **B2:** builder/prefab bağlantısı Oğuzhan'da; **C1-K** Faz 2'de ayrıca yapılacak.
  - İlk sözleşme / uygulama ayrıntıları:
  - Yeni dosya `Assets/_Project/Scripts/Desk/UI/DeskHeaderView.cs`: Desk asmdef'inde, `namespace CivilJam.Desk`.
  - `BribeEnvelopeView` gibi Awake'te kendini kurar. `Reset()` fontları (başlıkta Paytone One, metinde Fira Sans) ve T1 sprite'larını bağlar.
  - **Prefab'a dokunma**; Oğuzhan B2'de builder'a ekler.
  - **API** (Oğuzhan buna bağlar; değiştirirsen burada yaz):
    - `public void Refresh(GameContext ctx)`:
      - "Gün N";
      - Kasa (`DeskFormat.Money`), borç varsa kırmızı "Borç: X";
      - "Onaylanan: X · Bekleyen: Y": onaylanan `ctx.State.ApprovedToday`, bekleyen bugünkü `Pending` dosya sayısı (`DeskHud`'daki hesapla aynı).
    - `public void SetClient(string clientName, string projectName)`: ortadaki "Rıfat Usta · Çınar Apartmanı" etiketi.
    - `public void SetDayDone(bool allDecided)`: ortaya "Bugünkü başvurular bitti." ya da boş yazar (`DeskPropsView.ShowEmpty` ile aynı).
  - **Yerleşim:**
    - bugünküyle aynı: solda Gün, ortada müteahhit · proje, sağda Kasa ile Onaylanan · Bekleyen;
    - kök RectTransform ekranın üstüne yaslı, **80 px yüksek**;
    - NASIL OYNANIR düğmesi barın altında, ofiste kalır.
  - **Görsel:**
    - T1 geldiyse `hud_bar` (ahşap pano şeridi), `hud_tag` (9-slice kağıt etiket) ve ikonlar (`hud_icon_day`, `hud_icon_money`, `hud_icon_count`) kullanılır;
    - sprite yoksa aynı renk dünyasında kodla çizer (kahve şerit, krem etiket, kalın kahve kontur), böylece **T1'i beklemeden bitirilebilir**.
  - İsteğe bağlı: kasa değişince rakam kısa bir süre yeşil (+) ya da kırmızı (−) parlar. Vakit yoksa yapma.
  - Push, "C1 ✅ hash" yaz.
  - **C1-K, FAZ 2'de (madde 2):** Sandbox_Desk'te `InspectResultView` ve `InspectorBubbleView`'un karartmanın üstünde kaldığını kontrol et. Sorun varsa burada Oğuzhan'a yaz.
- [ ] **C2 Ana menü düğmeleri (madde 3) — FAZ 2: Faz 1'in hepsi ✅ olmadan başlama:**
  - `MainMenuScreen`: Devam Et / Yeni Oyun / Emeği Geçenler / Çıkış yeni `btn_menu` ile. Tarık aynı adla üzerine yazar, bağlantı korunur.
  - Yazılar kodla, Paytone One; `ButtonHover` aynı kalır.
  - **`menu_bg`'deki çizili 3 tahta yuva kalkıyor (T1).** Düğmeleri yuvalara değil düzgün bir sütuna diz:
    - kayıt varken 4 düğme;
    - kayıt yokken 3 düğme, ortalanmış.
  - Yuva sabitlerini (`SlotTop`, `SlotLeft`, `SlotRight`, `SlotHeight`, `SlotGap`) yeni görsele göre ayarla.
  - Emeği Geçenler'deki Geri düğmesi de aynı görselle olsun. Vakit varsa Esc menüsü düğmeleri (`UiKit.Button`, `PauseMenu`) de.
  - `SettingsMenuTests` ve `PauseMenuTests` geçmeli. Push, "C2 ✅ hash" yaz.

#### Tarık (EEE) — T1, T2
- [x] **T1 ✅ `f8842b5`** — Hepsi `Art/`'ta, Sprite (Single), yazısız (kod basar). Notlar:
  - `btn_inspect` 420x120 (koyu kömür, solda büyüteç → yazı x ≈ 120-410), `btn_help` 420x100 (hardal, solda "?" → yazı x ≈ 100-410), `btn_menu` 520x120 (koyu ahşap, ikonsuz; **oran 4,33**, eskisi 4,27 — İNCELE/NASIL OYNANIR/akış düğmeleri B2/C2'ye kadar bunu kullanıyor, sorun değil).
  - `menu_bg`: çizili 3 yuva silindi (x 745-1160, y 540-865 bölgesi şehirle dolduruldu; yakından aynalama izi var, düğme sütunu o bölgeye gelince örtülür). Logo, güneş, vinçler ve `MenuAmbience` katmanları aynı yerde. **Onur:** C2'de düğmeleri bu bölgeye (yaklaşık x 0,39-0,60, y 0,20-0,50) dizersen iz görünmez.
  - Üst bar: `hud_bar` 1920x**88** (76 px ahşap pano + alt 12 px yumuşak gölge; 80 px bar için alta yasla ya da 88 kullan), `hud_tag` 256x72 krem kağıt **9-slice, Sprite Border 24** (ayarlı), ikonlar 64x64 `hud_icon_day` (takvim), `hud_icon_money` (banknot + madeni para), `hud_icon_count` (klasör).
- ~~T1 Görseller (madde 3, 4; Onur ve Oğuzhan bunu bekliyor, önce bunu yap):~~ (yapıldı, yukarıda)
  - **Hepsi için ortak kurallar:**
    - el çizimi 2D cartoon, kalın kahve kontur, sıcak palet;
    - ONAY (`btn_approve`), RED (`btn_reject`) ve EK TEST (`btn_test`) ile **aynı aile**: aynı kontur kalınlığı, köşe yuvarlaklığı, gölge ve kağıt dokusu;
    - **Görsellerde yazı yok**, yazıyı kod basar (Türkçe karakter ve boyut sorunu çıkmasın);
    - şeffaf PNG, Sprite olarak import.
  - **Dosyalar:**
    - `btn_inspect.png` (yaklaşık 420x120): İNCELE, solda büyüteç ikonu.
    - `btn_help.png` (yaklaşık 420x100): NASIL OYNANIR, solda "?" ikonu.
    - `btn_menu.png` (yaklaşık 520x120): **aynı adla üzerine yaz**. Ana menü düğmeleri için; ikonsuz ya da çok küçük ikonlu. Şu an İNCELE ve NASIL OYNANIR da bunu kullanıyor, B2'de kendi görsellerine geçecekler.
    - `menu_bg.png`: **aynı adla üzerine yaz**. Çizili 3 tahta düğme yuvası silinir; boyut ve kadraj aynı kalır, logo ve ambiyans katmanları kaymasın. Onur'a haber ver.
    - Üst bar:
      - `hud_bar.png`: 1920x80-96, ahşap pano/raf şeridi, altında hafif gölge;
      - `hud_tag.png`: kağıt etiket, 9-slice için kenar boşluklu (import'ta Sprite Border'ı sen ayarla);
      - ikonlar (yaklaşık 64x64): `hud_icon_day.png` (takvim yaprağı), `hud_icon_money.png` (kasa/para), `hud_icon_count.png` (klasör).
  - Her görsel için `CREDITS_AND_AI.md` "Yapay zekâ kullanımı" satırı ve `Art/README.md` satırı (sahip B/C, durum F).
  - Hepsini **tek push'ta** at, "T1 ✅ hash" yaz.
- [x] **T2 ✅ `1dd7870`** — `TutorialView.cs` 2. kart ("Formdaki KAT ADEDİ'ne bak: El Kitabı'nın HIZLI BAKIŞ sayfası doğru kat grubunu kendisi gösterir.") ve 5. kart ("kat grubunu seçmeyi unutma" çıktı); README 27 ve 29, ICERIK_TASLAK, Art/README (`tab_*` artık kullanılmıyor, silinmedi). RED kutusundaki bilgi öğretici kartlarda zaten var (2. kart "tek bir kırmızı", 3. kart parsel/tarih/imza/kaşe + ek test) → ekleme gerekmedi. ⚠️ **Oğuzhan:** kart metinleri prefab'a da kayıtlı; B1/B2'de builder'la kurunca koddaki yeni metinler gelir (ayrı iş yok).
- ~~T2 Metinler (madde 1, 5, 6):~~ (yapıldı, yukarıda)
  - `TutorialView.cs` (sadece metin dizeleri, satır ~31 ve ~43) ve `ICERIK_TASLAK.md`'deki aynı metinler:
    - "kat grubunu seç" → kat grubu kendiliğinden seçilir (ör. "El Kitabı'nın HIZLI BAKIŞ sayfası formdaki kat adedine göre doğru kat grubunu kendisi gösterir.");
    - "doğru kat grubunu seçmeyi unutma" cümlesi çıkar.
  - `README.md`:
    - satır ~27: kat grubu otomatik; "Sekmelerde parametrelerin açıklamaları…" yerine "El Kitabı'nda Hızlı Bakış ve LAB (akredite laboratuvarlar) sayfaları var";
    - satır ~29: "önce doğru kat grubunu seç" çıkar.
  - `Art/README.md`: `tab_beton/donati/yapi/zemin.png` "artık kullanılmıyor" diye işaretlenir. **Silme**, prefab'da bağlılar.
  - Kalkan RED kutusundaki bilgi ("tek kırmızı yeter; değer tutuyor ama belge şüpheliyse ek test iste, parsel/tarih/kaşe/imzayı karşılaştır") öğretici kartlarda ve LAB sayfasında var mı, kontrol et. Yoksa öğretici karta yarım cümle ekle; LAB sayfasında değişiklik gerekirse A'ya yaz.
  - Push, "T2 ✅ hash" yaz.

### Önceki açık işler

**A (Ahmet):**
- [ ] Elle oynanış: 4 sonun hepsi (F9/F10), sahte belge ve ek test, kabadayı, hediye.
- [ ] Teslim: final build (kod değişirse yeniden), ZIP, README kontrolü, video planı.

**Oğuzhan (B):**
- ~~El Kitabı sekme notları~~ → **iptal** (poliş K3: bölüm sekmeleri kalkıyor).
- [ ] Formdaki "Proje müellifi" ile lab raporundaki "Laboratuvar Yetkilisi" aynı kişi (imza ipucu; inceleme 8. satır). Formdaki satırın adı "Denetim mühendisi" / "Raporu onaylayan mühendis" gibi, iki belgede aynı kişinin imzası beklendiği belli olsun (README de "aynı mühendisin imzası" diyor).
- ~~El Kitabı Hızlı Bakış alt yarı boş~~ → **A2'ye geçti** (poliş madde 5).
- ~~Üst bar~~ → **B2 + C1 + T1'e geçti** (poliş madde 4).
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
| 18 Ekim ~01:50 | **Poliş turu (7 madde):** kat grubu otomatik ve düğmeler sadece gösterge (inceleme hep binanın grubuyla, Core hesaplar); inceleme modunda tıklanabilir alanlar + ONAY/RED + İNCELE dışı hafif kararır, ONAY/RED basılabilir (basınca mod kapanır, karar verilir; EK TEST ve zarf pasif); İNCELE, NASIL OYNANIR ve ana menü düğmeleri ONAY/RED/EK TEST ailesinden; üst bar yeniden tasarlanır; Hızlı Bakış'taki RED kutusu kalkar, parametreler büyür; BETON/DONATI/YAPI/ZEMİN sekmeleri kalkar (HIZLI BAKIŞ ve LAB kalır, sekme notları taşınmaz); ziyaret kuralı (6×3, 10×1, aynı gün yok) zaten kodda. İş bölümü ve fazlar §7 | Ahmet kararı; plan A |
| 18 Ekim ~02:15 | Poliş turu kapı kuralı: Faz 1'in 6 işi (A1, A2, B1, C1, T1, T2) hepsi ✅ olmadan kimse Faz 2'ye geçmez; Faz 2 bitmeden Faz 3 yok. Kapıyı A açar (§7 Faz durumu tablosu). Yedek plan (görsel gelmezse eskiyle bağla) kalktı, T1 geldi | Ahmet kararı |

## Güncelleme kuralı
- Karar, mimari değişiklik, yeni kural ya da biten büyük iş olunca ilgili bölümü güncelle ve §8'e satır ekle.
- Kısa tut: kod kopyalama, uzun oturum notu yazma (onlar kişisel dosyalara). Biten maddeyi §7'de `[x]` yap; bir sonraki temizlikte silinir.
