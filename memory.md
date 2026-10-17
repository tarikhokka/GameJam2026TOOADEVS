# memory.md — Projenin ortak hafızası

> **17 Ekim ~17:30 temizlendi (A).** Eskimiş ve bitmiş notlar silindi; bu dosya artık sadece **güncel** kuralları, yapıyı ve açık işleri anlatır. Eski hâli git geçmişinde: `f14d2ed`. Ajanlar: oturum başında §7'yi oku; bir maddeyi bitirince orada `[x]` yap ve tek satır not düş.

## 1. Proje özeti
- CIVIL GAME JAM (ODTÜ KKK, KALTEV), 48 saat: 16 Ekim 2026 18:00 → 18 Ekim 2026 18:00 (UTC+3). Tema: "Afete Hazırlık ve Farkındalık".
- **İmza** (ürün adı "Imza", şirket "TOOADEVS"): masa başı karar oyunu. Oyuncu yapı denetim mühendisi. Müteahhitler sırayla gelir; oyuncu Yapı Denetim Formu, Laboratuvar Raporu ve Denetçi El Kitabı'nı karşılaştırıp binayı onaylar ya da reddeder. Bazıları zarf (rüşvet) bırakır. Gece deprem olabilir; sonucu ertesi sabah gazeteden öğrenir.
- Unity 6000.6.4f1, 2D (URP), uGUI + TextMeshPro, Input System. Windows, 1920x1080. Sadece fare; Esc menü açar. Sadece Türkçe.
- Jüri ölçütleri: Oynanabilirlik 25, Temayla ilişki 25, Yaratıcılık 20, Görsel/işitsel bütünlük 15, Teknik bütünlük 15.
- **Hassasiyet:** isimler kurgusal; gerçek afete, kişiye, kuruma atıf yok (gerçek bakanlık adı, logo, standart kodu kullanılmaz). Depremzedeler asla olumsuz gösterilmez. Mizah karakterlerde; deprem sonrası ve sonlar ciddi.

## 2. Oyun kuralları (güncel, kodda uygulanmış)
**Akış:** MainMenu → (Yeni Oyun: hikâye intro'su) → Game: DayIntro → Desk → Construction → (afet ? gazete (Result) : gün özeti (DaySummary)) → günlük ödeme → (son ? Ending : Save) → ertesi gün. Deprem sahnesi/animasyonu yok; afetsiz günde gazete yok.

**Gün ve içerik (`Data/Days`):** 10 gün. 1. gün 3 proje, rüşvet/sahte belge yok. 3. günden 4 proje, rüşvet ve sahte belge. 5. günden 5 proje + kabadayı. 8. günden 6 proje. Fabrika, yangın, sel, sigorta, kota **yok**.

**El Kitabı v2 (`Data/RuleSet.asset`, 15 kural):** her projede 5 parametre; gereken değer sadece kat grubuna bağlı (binalar 2-9 kat). Değerler "en az"; seçeneklilerde sağdaki daha iyi.

| Sekme | Parametre | 1-3 kat | 4-6 kat | 7+ kat |
|---|---|---|---|---|
| BETON | Beton sınıfı (C16 / C20 / C25 / C30 / C35) | C20 | C25 | C30 |
| DONATI | Demir tipi (B220 düz / B420 / B500 / B500S) | B420 | B500 | B500S |
| YAPI | Kolon kalınlığı | 30 cm | 35 cm | 40 cm |
| ZEMİN | Temel derinliği | 1,5 m | 2,0 m | 2,5 m |
| ZEMİN | Zemin türü (Dolgu / Yumuşak kil / Sert kil / Kaya) | Yumuşak kil | Sert kil | Kaya |

**Belgeler ve ipuçları:** Form (`Documents[0]`) ve lab raporu (`Documents[1]`) beyan değerini (`Reported`) gösterir; gerçek değeri (`Actual`) sadece ücretli **ek test** (300 TL, anında düşer) açar. Sahte belgede değerler temiz görünür; bedava ipuçları iki belgenin farkındadır: parsel, tarih, bilinmeyen laboratuvar (akredite listede yok), kaşedeki lab adı ≠ raporun lab'ı, imzalayan mühendis farklı. Masa hiçbir şeyi "yanlış" diye işaretlemez.

**Proje türleri (gizli `ProjectKind`):** Compliant, Sloppy (rüşvetsiz ama bir değeri eksik; her gün), Bribe, Fake (3. günden), BribeFake. Rüşvet teklif edilen her proje uygunsuzdur.

**Müteahhitler (6, `Data/Clients`):** Rıfat ve Sevim dürüst (çoğunlukla uygun, bazen dikkatsiz; asla zarf/sahte belge yok); Kemal ve Nuri rüşvetçi (bazen temiz gelir); Selim çoğunlukla sahte belgeci (zarf vermez); Rüstem kabadayı (5. günden; reddedilince cam kırar, tamir 200 TL). İsim türü tek başına ele vermez. Core selamlama (`GreetingText`; tekrar gelende "geçen sefer…") ve tepki (`ReactionText`) seçer. Nuri her retten sonra zarfı +250 TL büyütür. **Hediye:** sadece dürüst ustalar, onayda bazen, oyunda bir kez; sadece kabul edilir, ofiste kalıcı görünür, para/risk etkisi yok.

**Yıkılma (`EarthquakeSimulator`):** Strength = en kötü oran (sayısalda Actual/Required; seçenekli parametrede her eksik kademe −%25). Strength ≥ 1 asla yıkılmaz. Değilse Strength + gürültü (±0,15) < şiddet (0,5-0,9) ise yıkılır. Afet sadece son afetten bu yana onaylanan binaları değerlendirir. Afet olasılığı %20, afetsiz her gün +%15, tavan %100, afette %20'ye döner.

**Ekonomi (`Data/Balance.asset`, `Data/Expenses`):** başlangıç 2.500 TL; onay ücreti 300-500 TL; her gün kira 600 + kredi 300 (+ cam tamiri). Kasa asla eksiye düşmez; yetmeyen zorunlu gider borca yazılır; borç varsa isteğe bağlı (kısmen ödenebilir) "Borç ödemesi" kalemi çıkar.

**Hapis ve sonlar** (günlük ödemeden sonra, öncelik sırasıyla): yıkılan bina JailRisk +0,15, rüşvetle onaylanmış yıkılan bina +0,25; her yıkımdan sonra hapis zarı.
1. **Kötü** "Hüküm": hapis zarı tuttu.
2. **Orta** "Kapanan Kapı": borç > 2.000 TL (her gün bakılır).
3. **Orta** "Enkazın Gölgesi": 10. gün bitti, en az bir bina yıkıldı.
4. **İyi** "Ayakta Kalanlar": 10. gün bitti, hiç yıkım yok.

Denge (simülasyon, 300 oyun/tip): dikkatli oyuncu %100 İyi; tek zarf ~%45 İyi / %45 Enkazın Gölgesi / %10 hapis; her zarfı alan ~%85+ hapis; her şeyi reddeden ~5. gün ofisi kaybeder.

**Kayıt:** tek slot JSON (`persistentDataPath/save.json`), sadece `GameState`. Gün sonunda gün +1 yapılıp kaydedilir; "Devam Et" yeni günün başını açar. Son ekranına girince kayıt silinir.

**Jüri demo tuşları (Game sahnesi, build'de de):** F9 = son güne atla, F10 = bu gece afet kesin.

## 3. Arayüz (yeni hedef arayüz, 17 Ekim akşam — kuruldu)
Referans: `Docs/Tasarim/hedef_arayuz.png` (Onur). El kitabı ilk sekmesi: `Docs/Tasarim/el_kitabi_hizli_bakis.html`.
- **Masa (Desk, B):** üst yarı ofis (`office_bg`) + müteahhit portresi + konuşma balonu (harf harf yazı + yazı sesi); masa kenarı (`desk_edge`) ve yüzeyi (`desk_surface`). Masada üç belge düz ve yan yana:
  - **Yapı Denetim Formu** (`doc_form`): proje bilgileri (proje, ada/parsel, adres, yapı sınıfı, kat adedi, yükseklik, kat başı kişi, müteahhit, proje müellifi, onay ücreti) + kat grubuna göre bina resmi, teknik bilgiler (beyan değerleri), belediye kaşesi, ONAY / RED, zarf varsa "ZARFI AL VE ONAYLA".
  - **Laboratuvar Raporu** (`doc_lab`): laboratuvar, rapor tarihi, proje, ada/parsel, Parametre | Beyan | Ek test, karar vermeyen değerlendirme cümlesi, yetkili imzası, lab kaşesi + kaşedeki lab adı, EK TEST.
  - **Denetçi El Kitabı** (`doc_handbook`), sekmeler: HIZLI BAKIŞ · BETON · DONATI · YAPI · ZEMİN · LAB. Hızlı Bakış'ta oyuncu kat grubunu **kendisi seçer** (otomatik seçilmez, yeni müteahhitte sıfırlanır); 5 parametre kırmızı/yeşil çip ya da cetvel olarak boyanır. Eşikler Core'dan (`FindRule`), kodda sabit değer yok.
  - Zarf müteahhidin sağında masa kenarında; kabul edilen hediyeler masa kenarında müteahhidin iki yanında. 1. gün 4 öğretici kart; kabadayı reddedilince "Camın kırıldı!" kartı.
- **Akış ekranları (Flow, C):** gün başı (DayIntro), inşaat (Construction), gazete (Result), gün özeti (DaySummary), günlük ödeme (DailyExpenses), kayıt (Save), son (Ending), ana menü (MainMenu: Yeni Oyun / Devam Et / Emeği Geçenler / Çıkış), hikâye intro'su, Esc menüsü (her sahnede kendiliğinden kurulur).
- **Görsel dil:** el çizimi 2D cartoon (kalın kahve kontur, sıcak palet), görseller ChatGPT ile üretilip Claude script'leriyle hazırlandı. Yazı tipleri Paytone One (başlık) + Fira Sans (metin), ikisi de OFL, TMP asset'leri `Assets/_Project/Fonts/` (Static).

## 4. Mimari ve dosya haritası
- Tek `Game` sahnesi + prefab ekranlar. Diğer sahneler: `MainMenu` (Build Settings 1. sıra), `Sandbox_Desk`, `Sandbox_Flow`.
- Oyun mantığı saf C# (`GameSession`); tek MonoBehaviour `GameManager` faz sırasını yürütür ve sahnedeki `IGameScreen`'leri bulur. Ekranı olmayan faz: Editor'de mavi IMGUI yedeği, build'de atlanır (şu an 8 fazın hepsinin ekranı var).
- **Ekranlar Core'a sadece `ctx.Actions` (IGameActions) ile yazar** (Approve, Reject, BuyExtraTest, SetExpensePaid, AcceptGift). Okuma `GameContext`'ten. Kullanım rehberi: `AHMET.md`.
- Assembly'ler: `CivilJam.Core` (+ `.Editor`, `.Tests`), `CivilJam.Desk`, `CivilJam.Flow` (+ PlayMode testleri). Desk ve Flow sadece Core'a bağlı, birbirini görmez.
- `Assets/_Project/`
  - `Scripts/Core/` (A): `Runtime/` (ProjectData, ParamEntry, DocumentData, GameState, GameContext, QuakeResult, DailyExpenseResult), `Config/` (BalanceSO, RuleSetSO, ClientSO, GiftSO, DayConfigSO, ExpenseSO, HeadlineSO, EndingSO, NamePoolSO), `Simulation/` (ProjectGenerator, EarthquakeSimulator, DisasterSystem, CasualtyGenerator, ExpenseCalculator, ClientReactions), `Game/` (GameSession, GameManager, FallbackGui, SaveSystem, GameLaunch), `Editor/` (menü CivilJam > Core: Install Screens, Link Data, Rebuild Handbook Rules, Balance Report, Content Stats), `Tests/Editor/` (30 EditMode testi).
  - `Scripts/Desk/` (B): `UI/DeskScreen` (IGameScreen), `ClientSceneView`, `FormView`, `LabReportView`, `RulebookView` + `HandbookUi`, `PaperWindow`, `DraggableSheet`, `DecisionStampView`, `GiftOfferView`, `OfficeGiftsView`, `EventCardView`, `TutorialView`, `DeskAudio`, `DeskMotion`, `PopupMotion`, `DeskHud`; `Sandbox/`; `Editor/DeskSandboxBuilder(.Papers)` (menü CivilJam > Desk > Build Sandbox_Desk Scene: prefab + sahne yeniden kurulur).
  - `Scripts/Flow/` (C): `UI/` (FlowScreen tabanı, UiKit, ekranlar, MainMenuScreen, IntroSequence, PauseMenu), `Sandbox/`, `Editor/FlowBuilder`, `Tests/PlayMode/`.
  - `Prefabs/Desk/DeskScreen.prefab` (B), `Prefabs/Flow/*` (C). `Scenes/Game.unity` (**sadece A**, CoreSetupBuilder ile değişir).
  - `Data/` (A, içerik EEE): Balance, RuleSet, NamePool (36 bina adı, akredite lab'lar), Headlines (20 manşet), Days, Expenses, Clients (6), Gifts (6), Endings (4).
  - `Art/` ve `Audio/` (EEE): listeler `Art/README.md`, `Audio/README.md`. `Fonts/` (EEE).
- Belgeler: `README.md` (jüri), `CREDITS_AND_AI.md` (asset + AI kaydı, oyun içi Emeği Geçenler metni), `ICERIK_TASLAK.md` (metin taslakları; oyun asset'lerden okur), kişisel planlar `AHMET.md`, `OGUZHAN.md`, `ONUR.md`.

## 5. Takım, sahiplik, çalışma kuralları
- **A (Core) — Ahmet İlman:** Scripts/Core, Data, Game.unity, build. **B (Masa) — Oğuzhan Yurt:** Scripts/Desk, Prefabs/Desk, Sandbox_Desk. **C (Akış) — Onur Kabadayılar:** Scripts/Flow, Prefabs/Flow, Sandbox_Flow, MainMenu. **EEE — Tarık Hokkaömeroğlu:** Art, Audio, Fonts, içerik metinleri, CREDITS_AND_AI.md.
- Başkasının sahne/prefab dosyasını açıp kaydetme; başkasının script'ine dokunduysan burada not düş.
- Oyun kuralını değiştiren her öneri önce Ahmet'e; "Ahmet onayladı" notunu sadece Ahmet (ya da açık onayıyla) yazar.
- **Push'tan önce: pull → Unity'de derle → testler.** Commit'te `.meta`'ları unutma.
- Unity'yi **6000.6.5f1 ile açma** (proje sürümünü yükseltir); `ProjectVersion.txt` değişirse commit'leme.
- `Game.unity`'yi Editor'den kaydetme (prefab override'ları bozuluyor); değişiklikler CoreSetupBuilder ile.
- Unity'nin kendiliğinden değiştirdiği şu dosyaları commit'leme, pull engellerse `git checkout -- <yol>`: `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`, `Assets/_Project/Fonts/*SDF.asset`, `Assets/Settings/*`, `ProjectSettings/UnityConnectSettings.asset`.
- Görsel/ses: önce placeholder, final **aynı dosya adıyla** üzerine yazılır (GUID ve prefab bağlantıları korunur).

## 6. Jam kuralları (ihlal = diskalifiye)
- Repo **public** kalmalı; build ile repo kodu uyumlu olmalı.
- Her hazır asset ve **her yapay zekâ kullanımı** eklendiği anda `CREDITS_AND_AI.md`'ye (küçük kullanım dahil). Yeniden dağıtılamayan asset repoya girmez. Jam öncesi hazırlanmış kod/asset yok.
- Teslim: Windows build ZIP + en fazla 1 dk video (MP4/AVI) + public repo + kurulum/kontrol talimatı (`README.md`).
- Saatler: **feature freeze ve Git desteği 18 Ekim 12:00**; yükleme 15:00-16:30; son teslim 18 Ekim 18:00.

## 7. Durum ve açık işler (güncelleme: A, 17 Ekim ~17:30)

### 🔎 YENİ ÖZELLİK — İnceleme modu (Papers, Please tarzı) — PLANLAMA AŞAMASI, KOD YAZILMAZ
> ⛔ **Henüz kimse kod yazmıyor.** Plan Ahmet'in onayını bekliyor. Onay gelince Ahmet (A) Onur ve Oğuzhan için ayrıntılı uygulama notunu buraya yazar; işe ondan sonra başlanır. Ajanlar: bu bölümü okuyup kendi başına uygulamaya geçme.

**Fikir (Ahmet + Onur):**
- Oyuncu masadaki "İncele" düğmesiyle inceleme moduna geçer.
- İki **ayrı** belgeden iki alana tıklar; sıra önemli değil. Belgeler: Yapı Denetim Formu, Laboratuvar Raporu, Denetçi El Kitabı.
- Oyun üç mesajdan birini verir:
  - **SORUN YOK:** iki alan aynı bilgiye ait ve birbirini tutuyor ya da kurala uyuyor.
  - **UYUŞMAZLIK SAPTANDI:** aynı bilgiye ait ama tutmuyor ya da kurala uymuyor.
  - **ALAKASIZ:** farklı bilgiler seçildi.
- Kontrol ücretsiz ve sınırsız; para, risk ya da karar etkisi yok. Oyuncu yine ONAY / RED'e kendisi basar.

**Kesin kararlar (Ahmet, 17 Ekim akşam):**
1. El kitabı tarafında eşik, oyuncunun Hızlı Bakış'ta **seçtiği kat grubundan** alınır. Seçilen grup binanın kat grubu değilse (ya da grup seçilmemişse) sonuç **ALAKASIZ** olur.
2. Künye alanları (parsel, tarih, imza, laboratuvar, kaşe) kapsama **girer**.
3. Kaşe, el kitabının LAB sekmesindeki **akredite listesiyle** karşılaştırılır (aynı belge içinde karşılaştırma yok).
4. Masada **"İncele" modu** var. Mod kapalıyken belgeler bugünkü gibi çalışır (sürükleme, ONAY / RED / EK TEST).
5. Bedeli yok.
6. İş bölümü:
   - **A:** kontrol mantığı (Core, saf C#, testli).
   - **Oğuzhan:** İncele düğmesi, tıklanabilir alanlar, seçim, çizgi (masa / `DeskScreen.prefab`).
   - **Onur:** sonuç mesajının görünümü ve sesi (ayrı bileşen; masa prefab'ına Oğuzhan bağlar).

**Kural tablosu (tek kaynak; tabloda olmayan her çift = ALAKASIZ, aynı belgeden iki alan = ALAKASIZ):**

| # | 1. alan | 2. alan | Kontrol | Ne zaman uyuşmazlık |
|---|---|---|---|---|
| 1 | Form: parametre X | El Kitabı: parametre X (seçili grup) | Formdaki beyan eşiğe yetiyor mu | Uygunsuz proje (rüşvetli, dikkatsiz) |
| 2 | Lab: beyan X | El Kitabı: parametre X (seçili grup) | Aynısı | Aynısı |
| 3 | Lab: **ek test** X (test yapıldıysa) | El Kitabı: parametre X (seçili grup) | Gerçek değer eşiğe yetiyor mu | Sahte belge dahil her uygunsuz proje |
| 4 | Form: parametre X | Lab: **ek test** X (test yapıldıysa) | Beyan = gerçek değer mi | Sahte belge (beyan temiz, gerçek kötü) |
| 5 | Form: parametre X | Lab: beyan X | Aynı mı | Hiçbir zaman (Core'da iki belge aynı beyanı taşır) |
| 6 | Form: Ada/Parsel | Lab: Ada/Parsel | Aynı mı | Sahte belge ipucu "parsel farkı" |
| 7 | Form: Tarih | Lab: Rapor tarihi | Aynı mı | Sahte belge ipucu "tarih farkı" |
| 8 | Form: Proje müellifi | Lab: Laboratuvar yetkilisi | Aynı kişi mi | Sahte belge ipucu "farklı mühendis imzası" |
| 9 | Lab: Laboratuvar (başlık) | El Kitabı: LAB listesi | Akredite mi | Sahte belge ipucu "bilinmeyen laboratuvar" |
| 10 | Lab: kaşe | El Kitabı: LAB listesi | Kaşedeki lab akredite mi | Sahte belge ipucu "başka laboratuvarın kaşesi" (aşağıdaki Core düzeltmesiyle) |

- **Olasılıklar değişmiyor.** Künye uyuşmazlıkları zaten var: sahte belgeler 3. günden itibaren günde ~1 tane geliyor, her birinde 5 ipucundan 1-2'si bulunuyor (parsel / tarih / bilinmeyen lab / başka lab kaşesi / farklı imza).
- Form ile lab raporunun beyan değerleri her zaman aynı (5. satır). Değer farkı sadece ek testte görünür (3. ve 4. satırlar).
- **Onay bekleyen küçük Core düzeltmeleri (A):**
  - (a) "Başka laboratuvarın kaşesi" ipucunda kaşeye akredite **olmayan** bir lab adı yazılsın; yoksa 10. satır bu ipucunu hiç yakalayamaz.
  - (b) Sahte parsel ve tarih gerçeğiyle asla aynı çıkmasın (şu an rastgele, nadiren aynı çıkabiliyor).

**Yol haritası (onaydan sonra):**
| Adım | Kim | İş | Süre |
|---|---|---|---|
| 0 | Ahmet | Bu planı onayla. A, Onur ve Oğuzhan için ayrıntılı uygulama notunu buraya yazar | 15 dk |
| 1 | A | Core: alan tanımı, kontrol fonksiyonu (tablo), (a)+(b) düzeltmeleri, her satır için test, `AHMET.md` sözleşme satırı | ~45 dk |
| 2 | Oğuzhan | İncele modu, tıklanabilir alanlar (vurgu, seçim, iki alan arası çizgi), sağ tık / Esc iptal, yeni müteahhitte sıfırlama. Arayüz kural bilmez, sadece Core'u çağırır | ~2 sa |
| 3 | Onur | Sonuç mesajı bileşeni: yeşil SORUN YOK / kırmızı UYUŞMAZLIK SAPTANDI / gri ALAKASIZ (kaşe ya da şerit) + kısa ses. Adım 2 ile paralel | ~1 sa |
| 4 | A | Testler, Game.unity, ekran görüntülü otomatik oynanış (dürüst, uygunsuz, sahte belge, ek test sonrası), Windows build | ~45 dk |
| 5 | Tarık | 1. gün öğretici kartı + README satırı | ~20 dk |

**Yalın tutmak için kapsam dışı:** sorgulama / müteahhide soru yok; sonuç kayda geçmez; tablo dışı çiftler için özel mesaj yok.

**Durum:** Yeni hedef arayüz kuruldu (Onur → Tarık → Oğuzhan zinciri tamam, `f14d2ed`). A Game.unity'de oyunu oynatarak kontrol etti: müteahhit gelişi, balon, üç belge, El Kitabı, ONAY kaşesi, kasa ve sıradaki müteahhit çalışıyor; EditMode testleri 30/30. Eski "yeni iş başlatma" durdurması **kalktı**. Aşağıdaki hata/eksik listesi sürüyor; yeni özellikler Ahmet'in kararıyla eklenecek (eklenince buraya yazılır). Freeze 18 Ekim 12:00.

**A (Ahmet):**
- [x] (A, yapıldı, testli) Core: projelerdeki parametre sırası El Kitabı sekme sırasıyla aynı olsun (beton, demir, kolon, temel, zemin). Şu an form/lab "beton, kolon, temel, demir, zemin" sırasında, El Kitabı farklı → oyuncu satır eşleştirirken şaşırıyor.
- [x] Windows build aracı: `CivilJam > Core > Build Windows (Release)` → `Builds/Imza/Imza.exe` (148 MB; ZIP'e `Imza_BackUpThisFolder_ButDontShipItWithYourGame` klasörü KONMAZ). 17 Ekim akşam build'i hatasız açıldı.
- [x] Baştan sona otomatik oynanış (PlayMode, ekran görüntüleriyle): menü → gün başı → masa → inşaat → gazete → ödeme → kayıt → son (İyi) akışı hatasız; borç kuralı doğru çalışıyor. Bulunanlar aşağıda sahiplerinde.
- [ ] Elle oynanış (Ahmet): 4 sonun hepsi (F9/F10), sahte belge ve ek test, kabadayı, hediye.
- [ ] Teslim: final build, ZIP, README kontrolü, video planı.

**Oğuzhan (B):**
- [x] 1. gün öğretici kartı: başlık iki satıra taşınca altındaki metnin üstüne biniyor ("…hoş geldin" satırı metinle çakışıyor). Atla / İleri düğmeleri de çok büyük, yeni görsel dile uymuyor. — (EEE yaptı) `TutorialView.StyleCard`: başlık tek satır + otomatik küçülme (Paytone), metin Fira Sans, alt satır 64 px, düğmeler 150/190 px (Fira Bold). Fontlar prefab'a ve builder'a (`Wire titleFont/bodyFont/boldFont`) bağlı; prefab yeniden üretilmedi.
- [ ] El Kitabı Hızlı Bakış: 5 satır üstte toplanıyor, alt yarı boş; "B220 (düz)" ve "Yumuşak kil" çipleri okunmayacak kadar küçük. Satırlar sayfayı doldursun, çipler/cetvel daha geniş olsun.
- [ ] Üst bar (Gün / müteahhit · proje / Kasa / Onaylanan · Bekleyen) eski koyu bant ve eski font; yeni görsel dile (Paytone/Fira, kağıt/ahşap) uydur.
- [x] ~~Kurum adı tutarlılığı~~ **Değişiklik yok (Tarık kararı, `54c580d`):** bunlar bilerek ayrı kurumlar. "Ulusal Yapı Denetim Kurumu" = oyuncunun çalıştığı ve son onayı veren yer; "Afet Kontrol Bürosu" = El Kitabı'nı yayımlayan kurum; "Betonya Belediyesi" = imar kaşesi. Metinler eşitlenmez.
- [ ] Formdaki "Proje müellifi" ile lab raporundaki "Laboratuvar Yetkilisi" aynı kişi gösteriliyor (imza ipucu böyle çalışıyor: sahtede farklı mühendis). Oyuncu için mantıklı olsun: formdaki satırın adı "Denetim mühendisi" / "Raporu onaylayan mühendis" gibi, iki belgede aynı kişinin imzası beklendiği belli olsun (README de "aynı mühendisin imzası" diyor).

**Onur (C):**
- [ ] 🔴 **Jam kuralı:** `Docs/Tasarim/hedef_arayuz.png`'yi hangi araçla ürettiğini `CREDITS_AND_AI.md` → "Yapay zekâ kullanımı" tablosuna yaz.
- [ ] Ödeme ekranı yanlış rakam gösterebiliyor (`DailyExpensesScreen.UpdateTotals` kasadan basit çıkarma yapıyor). Core hazır: seçim her değiştiğinde `var pv = ExpenseCalculator.Preview(Context.State, Context.PendingExpenses);` → `pv.MoneyAfter`, `pv.DebtAfter`, `pv.PaidTotal`, `pv.AddedToDebt` (borca yazılacak zorunlu gider), `pv.DebtPaid`, kalem bazında `pv.Lines[i].Paid`.
- [ ] Kayıt ekranı (`SaveScreen`, Tarık yazdı, Flow'da) her zaman "Kaydedildi" diyor. Core hazır: `Context.LastSave` (`Ok`, `Day`, `Error`); `Ok` false ise "Kayıt yapılamadı" + "Tekrar dene" (`Context.RetrySave()`, günü artırmaz) + "Kaydetmeden devam".
- [ ] Müzikler oyunda çalmıyor: `mus_menu` (ana menü) ve `mus_ending_sad` (Kötü + iki Orta son) hiçbir sahne/prefab/koda bağlı değil.
- [ ] (İsteğe bağlı) Gün başında `Context.LoadFailed` ise "Kayıt okunamadı, yeni oyun başladı." yaz.
- [ ] Akış ekranları yeni görsel dile uymuyor: gün başı, inşaat, ödeme, kayıt ve son ekranlarında eski varsayılan font ve düz beyaz/siyah düğmeler; gün başı arka planı hâlâ eski ofis (`dayintro_bg`, eski desk_bg'den). Paytone One / Fira Sans ve masadaki kağıt/ahşap diline yaklaştır (Tarık gerekirse arka planı yeni ofisten üretir).
  - (EEE, 17 Ekim ~18:00) Kısmen yapıldı: `dayintro_bg` yeni ofis+masadan üretildi. Gün başı ve inşaat ekranındaki prefab düğmeleri `FlowScreen.StylePrefabButtons` ile tahta düğme (`btn_menu`) + Fira Sans Bold + fare vurgusu aldı (prefab'larda `buttonSprite`/`buttonFont`). UiKit düğmeleri (ödeme, kayıt, gün özeti, gazete, son, Esc) fare vurgusunu `ButtonHover` ile alıyor ama hâlâ koyu düz; başlık/metin fontları hâlâ eski. Kalanı Onur'da.
- [ ] Son ekranı: uzun metin parlak arka plan görselinin üstünde beyaz yazı, zor okunuyor (özellikle İyi son). Metnin arkasına yarı saydam koyu panel.
- [ ] Gazete: "Hesaplaşmaya geç" düğmesi gazetenin sağ alt köşesinden taşıyor.

**Tarık (EEE):**
- [x] El Kitabı BETON / DONATI / YAPI / ZEMİN sekmeleri için birer kısa afet farkındalık cümlesi (doğru, korkutmayan). `ICERIK_TASLAK.md`'ye yaz; Oğuzhan `RulebookView.sectionNotes`'a koyar (şu an boş). — (EEE, `54c580d`) Yazıldı: `ICERIK_TASLAK.md` → "El Kitabı sekme notları". **Oğuzhan:** Inspector'dan aynen koy.
- [x] Kurum adı kararı: değişiklik yok, üç ayrı kurum (Oğuzhan maddesine bak).
- [ ] `README.md`: A "Nasıl oynanır"ı yeni arayüze göre güncelledi; son hali, oyun ekran görüntüsü ve video bağlantısı (Pazar).
- [ ] Pazar: 1 dakikalık video (A ile senaryo).

**Öneri (B, 17 Ekim akşam) — ZORUNLU DEĞİL, karar Ahmet'in / takımın:**
Jüriden bir hocanın notu ("Aynı Tema, Başka Bir Dünya", Murat Oktay): "Oyuncu neden sizi seçsin, oynadıktan sonra neyi hatırlasın? Oyuncunun konumunu değiştirin: afet, kimin açısından afet?" Bizim oyun ilk bakışta "Papers Please'in deprem versiyonu" gibi duruyor. Ama asıl farkımız oyunda zaten var, sadece oyuncuya gösterilmiyor: imza iki saniyelik sıradan bir hareket, sonucunu ise o binada oturan insanlar yaşıyor. Elimizde "kat başı kişi" verisi de var. Freeze'e sığabilecek ucuz fikirler:
1. **Afetten sonra sakinin gözünden kısa kart (gazete/sonuç ekranında):** onaylanan binada oturan birinin tek paragraflık notu. Yıkıldıysa "3. kattaydık…", sağlamsa "Sallandı ama ayakta kaldı, çocuklar uyuyordu." Metinler EEE'nin, ekran C'nin.
2. **"Yeniden bakmak":** afetten sonraki gün masada sakinlerden bir iz kalsın: bir fotoğraf, bir mektup ya da yıkıldıysa bir haber kupürü. Oyuncu sıradaki formu imzalarken o kâğıt masada durur. Hediye slotlarıyla (`OfficeGiftsView`) büyük ölçüde aynı altyapıyla yapılabilir (B).
3. **İmza anını ağırlaştırmak:** ONAY'a basınca formda kısa bir vurgu çıksın: "Bu imza 48 kişiyi bağlıyor" (kat adedi × kat başı kişi). FormView'de, birkaç saatlik iş (B).
Bence en güçlüsü 1 ile 2'nin birlikte yapılması. Hiçbiri kural ya da Core değişikliği değil. Yapılmazsa da sorun yok; önce açık hatalar biter.

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
| 17 Ekim akşam | İnceleme modu (Papers, Please tarzı) planlandı: iki ayrı belgeden iki alan → SORUN YOK / UYUŞMAZLIK SAPTANDI / ALAKASIZ; eşik seçili kat grubundan (yanlış grup = ALAKASIZ), künye dahil, kaşe akredite listeyle, İncele modu, bedelsiz; A Core, Oğuzhan masa, Onur mesaj. **Uygulama Ahmet onayından sonra** | Ahmet + Onur fikri; Ahmet kararları. Olasılıklar değişmez |
| 17 Ekim `1368c4f` | Aynı müteahhit arka arkaya gelmez (gün içinde ve dünün sonu → bugünün başı); yerine sadece o türe uyan biri gelir, rol kuralları ve oranlar aynı | Tarık isteği; `ProjectGenerator.PickClient(previousClientId)`, test `Clients_DoNotComeTwiceInARow` (EditMode 32/32). Core A dosyası: A inceledi, testler 32/32 |
| 17 Ekim akşam | "Mesaiyi bitir" düğmesi kalktı: son müteahhitten sonra mesai zili (`sfx_shift_end.wav`) + "MESAİ BİTTİ" geçiş ekranı (günün özeti), ardından inşaat ekranı koyu örtüyle açılır | Tarık önerisi, A ve B onayladı. `DeskScreen.EndShift` + `ShiftEndOverlay`; süreler Inspector'da (1,2 / 0,7 / 1,8 sn) |

## Güncelleme kuralı
- Karar, mimari değişiklik, yeni kural ya da biten büyük iş olunca ilgili bölümü güncelle ve §8'e satır ekle.
- Kısa tut: kod kopyalama, uzun oturum notu yazma (onlar kişisel dosyalara). Biten maddeyi §7'de `[x]` yap; bir sonraki temizlikte silinir.
