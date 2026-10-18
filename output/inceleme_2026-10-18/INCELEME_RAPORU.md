**İmza - CIVIL GAME JAM v09 kural uyumu ve oyun incelemesi**

18 Ekim 2026. İnceleme: Codex. Bu rapor öneri ve bulgu sunar; oyun kodu, sahne veya prefab değiştirilmedi. Rapor sonrasında Onur'un ChatGPT teyidiyle tasarım referansının beyanı `CREDITS_AND_AI.md` dosyasına eklendi; push hazırlığında güncel teslim taslağı da bu bilgiyle tamamlandı. Aşağıdaki K03 maddesi kapatıldı.

**Genel değerlendirme**

Oyunun güçlü tarafı, yapı denetimini gerçekten oyuncunun yaptığı işe dönüştürmesi. Form, laboratuvar raporu ve El Kitabı arasında karşılaştırma yapmak; ekonomik baskı altında imza atmak ve sonucunu gazetede görmek tema açısından iyi bir bütün oluşturuyor. Core / Desk / Flow ayrımı çalışıyor, yedi günlük akış tamamlanabiliyor ve yeni oklu rehber temel etkileşimi öğretiyor.

İncelenen kaynak ve belgelerde kesinleşmiş bir yarışma ihlali tespit etmedim. Buna karşılık final Windows paketi, ayrı video dosyası, gerçek teslim formu ve teslim makbuzu görülmeden “bütün teslim koşulları sağlandı” denemez. Public repo doğrulandı. Altı davranış/arayüz sorunu testle yeniden üretildi; ödeme ve kayıt geri bildirimi en yüksek öncelikte.

PDF'teki zorunluluklar ile kalite önerileri bu raporda ayrıdır. P0: teslimi veya temel doğruluğu etkileyen iş. P1: oyuncu deneyimini belirgin etkileyen hata/risk. P2: cila veya tasarım önerisi. “Doğrulanmadı”, ihlal bulunduğu anlamına gelmez.

**İncelenen sürüm ve yöntem**

- Başlangıçtaki yerel HEAD: `d7d6961b89e381f691849eb3baa3e1ab5f3b281a`. Çalışma ağacında inceleme öncesinden kalan beş değiştirilmiş izlenen dosya vardı; temiz checkout varsayılmadı.
- İnceleme sırasında public main ilerledi. Son test edilen uzak sürüm: [`2c050338d8a6ae1b4b37ea5f760e3eba5229cd08`](https://github.com/tarikhokka/GameJam2026TOOADEVS/commit/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08). Yerel HEAD'e göre sekiz commit ve 15 dosya farkı geçici kopyaya alındı. Asıl projede pull/checkout yapılmadı.
- Son sürümde ürün kaynakları: 117 C# dosyası, 17.898 satır; dört sahne, sekiz prefab ve 38 veri asset'i. Satır sayısı testleri ve Editor araçlarını da içerir.
- Unity `6000.6.4f1` ile `/tmp` altında ayrı proje kopyası kullanıldı. Ürün adı yalnız kopyada değiştirildi; oyuncunun mevcut kaydı kullanılmadı.
- Kopyada oturum başındaki yerel paket/ayar farkları da vardı (`com.unity.ai.inference 2.6.1` ve ilgili scripting define'lar). Son public commit'in ürün kodu sınandı; bu çalışma temiz bir public clone'dan alınmış final build onayı değildir.
- PDF'nin 13 sayfası çıkarıldı; geliştirme, beyan, teslim ve jüri sayfaları görsel olarak da kontrol edildi. Özellikle s.3-8 ve s.13 esas alındı.
- Temel oyun mantığı, üretim, inceleme, zarf, kaşe/iade, afet, ekonomi, kayıt ve faz geçişleri okundu; sahne/prefab GUID bağlantıları tarandı. Bu, her olası sistem/cihaz durumunun kapsandığı bir garanti değildir.
- Görsel kanıtlar gerçek sahne/prefab referanslarıyla alınan 1920x1080 Unity renderlarıdır; Windows build ekran görüntüsü değildir. Render için overlay Canvas'ları geçici kameraya taşındı. Rehberin ok/çerçeve yerleşimi bu render dönüşümünden etkilenebildiği için ayrıca değiştirilmemiş Game View koordinatlarında kontrol edildi.

| Kontrol | Sonuç | Ne kanıtlıyor / sınırı |
|---|---|---|
| Mevcut EditMode testleri | 58/58 geçti | Kurallar, program, inceleme, ekonomi ve sahne kurulumu için mevcut kapsam |
| Mevcut PlayMode testleri | 4/4 geçti | Esc, intro, menü dönüşü ve ses ayarı için mevcut kapsam |
| Ek tanı senaryoları | Altı sorun yeniden üretildi | Aşağıdaki B01-B06; bunlar mevcut ürün testlerinin kapsamadığı durumlar |
| Yedi günlük faz ve belge akışı | Geçti: 28 karar, 14 onay, 7. gün son | Ekonomi etkisini ayırmak için yüksek başlangıç parası ve doğru kararı seçmek için gizli uygunluk verisi kullanıldı; normal oyuncu başarısı ölçümü değildir |
| Yeni oklu rehber | Geçti | Karşılaştırma olayı, kaşenin hedefe pointer bırakılması ve iki belgenin pointer ile iadesi dahil dokuz adım |
| Rehber hedef koordinatı | Geçti | Değiştirilmemiş 640x480 batch Game View'da hedef/çerçeve merkezleri farkı yaklaşık 0,00003 piksel; render dönüşümünde görülen kayma ürün hatası sayılmadı |
| Denge | 6 strateji x 300 = 1.800 oyun | Gerçek güncel Data asset'leri; insan oynanış testi değildir |
| Kalabalık gazete | Ek 300 rüşvetçi koşusunda gerçek durum seçildi | 22 sonuç / 12 yıkım olan veri arayüzde yeniden gösterildi |
| Asset bağlantıları | Yerelde çözülemeyen GUID, yinelenen GUID ve eksik dosya .meta: 0 | Alanların anlamsal olarak doğru seçildiğini tek başına kanıtlamaz |
| Public repo | Anonim GitHub API ile doğrulandı | Kontrol anında `private=false`, `visibility=public` |
| Windows build / final ZIP / video / makbuz | Doğrulanmadı | Bu checkout'ta final paket ve video bulunmadı; başka ekip üyesinde veya teslim sisteminde bulunabilir |

Son toplu PlayMode çalışması, dört mevcut test ile dokuz ek kontrolü birlikte içeriyor: **13 testin 7'si geçti, 6'sı bulgu üretti**. Sonradan yapılan hedef koordinatı kontrolü ayrıca kaydedildi. Geçici testler ürünün Assets klasörüne eklenmedi.

**Kitapçığa göre uygunluk tablosu**

Kaynak: kullanıcının verdiği `CIVIL_GAME_JAM_Kural_Kitapcigi_v09 kopyası.pdf` (v09). Sayfalar PDF'nin basılı sayfa numaralarıdır.

| Madde | İstenen | Oyundaki / teslimdeki durum | Değerlendirme |
|---|---|---|---|
| 2.1, 2.3, 2.4 - s.2 | 2-4 kayıtlı üye, kayıtlı ekibin çalışması, temsilci | README'de dört kişi var | Sayısal olarak uyumlu; resmi kayıt ve temsilcilik repodan doğrulanamaz |
| 1.3, 3.1 - s.2-3 | Oyuna özgü geliştirme 48 saat içinde | İlk commit 16 Ekim 15:09'da Unity şablonu; `_Project/Scripts` geçmişi 21:26'dan başlıyor | Görülen geçmiş süre koşuluyla tutarlı. Tema öncesi motor/araç hazırlığı 3.1'de zaten serbest |
| 3.2, 6.6 - s.3,5 | Windows veya masaüstü tarayıcıda çalışan, test edilmiş oyun | ReleaseBuilder Windows64 hedefli; README Windows 10/11 diyor | Platform seçimi uygun; final Windows çalışırlığı henüz bu incelemede doğrulanmadı |
| 3.2 - s.3 | Sürüm, donanım, kurulum, kontroller | OS, 64 bit, çözünürlük ve kontroller yazılmış | Donanım bilgisi eklenmeli; ölçülmemiş minimum sistem uydurmak yerine test edilen cihaz belirtilmeli |
| 3.3-3.5 - s.3 | Hazır kaynakların kullanım hakkı ve kaynak/lisans/kullanım beyanı | CREDITS ayrıntılı; CC0, CC BY ve OFL kaynaklar listeli | Beyan temeli güçlü. Her dış kaynağın güncel lisans sayfası bu oturumda tek tek bağımsız doğrulanamadı |
| 3.4, 4.2, 5.2 - s.3-4 | Hazır kaynaklar ve AI için ayrı ayrı beş kategori | Uzak main'de `Teslim_formu_taslak.md` mevcut | Taslak hazır; web formundaki gerçek seçimler ve açıklamalar ayrıca yapılmalı |
| 3.6, 6.5 - s.3,5 | Public repo ve takım kodunun erişilebilirliği | Public repo anonim erişimle doğrulandı | Kontrol anında uygun; değerlendirme sonuna kadar açık kalmalı |
| 3.7, 5.6 - s.3-4 | Oynanabilir sürüm ile kaynak sürümünün uyumu | Yerel ve uzak kaynak arasında sekiz commit vardı; final ZIP görülmedi | Teslim commit'i sabitlenip o sürümden yeniden build alınmadan eşleşme onaylanamaz |
| 3.7 - s.3 | Repoda parola/erişim anahtarı paylaşılmaması | İzlenen metinlerde yaygın anahtar/özel anahtar kalıplarından eşleşme çıkmadı | Sınırlı tarama; bütün geçmişin veya her türlü kişisel verinin denetimi değildir |
| 4.1, 4.3 - s.3 | AI serbest; oran sınırı ve otomatik puan kesintisi yok | Kod, görsel, metin ve ses üretiminde AI yardımı açıklanıyor | AI yoğunluğu tek başına ihlal değil; yaratıcılık/sanatsal nitelikte jüri kanaati etkilenebilir |
| 4.2, 4.4, 10.1 - s.3,8 | Doğru ve eksiksiz beyan | Onur, `hedef_arayuz.png` görselini ChatGPT ile oluşturduğunu teyit etti; CREDITS'e araç, amaç ve kullanım yeri eklendi | Bu kaynak belirsizliği kapandı; bilgi web formundaki AI / Görsel açıklamasına da aktarılmalı |
| 5.2-5.3 - s.4 | Test edilmiş ZIP ve ayrı, <=60 sn MP4/AVI | Video planı var; final dosyalar bu checkout'ta yok | Hazır olduğu / teslim edildiği doğrulanmadı; bağlantı video dosyasının yerine geçmez |
| 5.3 - s.4 | ZIP <=10 GB, video <=5 GB; anonim HTTPS erişimi | Public repo HTTPS ile erişilebilir | Paket/video boyutu dosyalar görülünce ölçülmeli |
| 5.5 - s.4 | ZIP ve video doğrulanınca teslim makbuzu | Teslim oturumu açılmadı | Makbuz numarası, sürüm, zaman ve dosya adları temsilcide kontrol edilmeli |
| 6.1 - s.5 | 18 Ekim 18:00 UTC+3 öncesi sunucuda sürüm kaydı | Teslim kaydı görülmedi | Yalnız düğmeye basmak yetmez; zamanında başlatılan aynı aktarım sonra tamamlanabilir |
| 6.3 - s.5 | Git desteği başvurusu için 12:00 sınırı | Proje notlarında ayrıca 12:00 freeze var | PDF'deki 12:00 geliştirme yasağı değildir; freeze ekibin kendi planıdır |
| 7.3 - s.6 | Video dahil toplam üç dakika; video <=bir dakika | Video planı mevcut | Sunum 3+1 dakika olarak hazırlanmamalı; soru-cevap süresi yok |
| 9.8 - s.7 | İlgili hak sahiplerinin bilgisi/onayı ve temsilci yetkisi | Kaynak koddan görülemez | Temsilcinin tamamlayacağı organizasyon koşulu |
| 10.2-10.3 - s.8 | İddia ile kesinleşmiş ihlalin ayrılması | Bu raporda belirsizler ayrı işaretlendi | Nihai uygunluk/diskalifiye kararı organizasyonun incelemesine aittir |

PDF bütün afet türlerini, gerçek mühendislik simülasyonunu, belirli bir oyun motorunu veya AI için bir yüzde eşiğini zorunlu kılmıyor. Fabrika/sel/yangın gibi yeni özellikler eklememek tek başına kural eksiği değil. Projenin “eklendiği anda CREDITS'e yaz” kuralı ise PDF'nin genel beyan zorunluluğuna ek bir ekip çalışma kuralı.

**Yeniden üretilen altı sorun**

**B01 - P0 - Ödeme ekranının önizlemesi uygulanacak işlemle uyuşmuyor. Sorumlu: Onur.**

500 TL kasa ve 0 borçla, 400 TL kredi + 750 TL kira seçildiğinde ekran “ödeme sonrası: -650 TL” yazıyor. Core krediye 400 TL ödüyor, kirayı borca yazıyor: **100 TL kasa, 750 TL borç**. Sorun yalnız negatif sayı değil; seçim ile gerçekten ödenecek tutarın farklı olması. Borç ödeme kaleminde kısmi ödeme de aynı basit çıkarma hesabına sığmıyor.

Neden: [DailyExpensesScreen.cs:101](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Flow/UI/DailyExpensesScreen.cs#L101) `TotalSelected` değerini doğrudan kasadan çıkarıyor. Hazır çözüm: `ExpenseCalculator.Preview`. UI `PaidTotal`, `MoneyAfter`, `DebtAfter`, `AddedToDebt`, `DebtPaid` ve fiili satır durumlarını göstermeli. Kabul: yeterli kasa, yetersiz kasa, hasar ve kısmi borç ödemesinde ekrandaki sonuç onay sonrası state ile aynı olmalı. [Ekran kanıtı](kanitlar/odeme.png).

**B02 - P0 - Kayıt hatasında da “kaydedildi” gösteriliyor. Sorumlu: Onur.**

`LastSave.Ok=false` ve bir kayıt hatası verildiğinde ekranda yine “1. Gün kaydedildi” ve “KAYDEDİLDİ” kaşesi çıkıyor. Bu kontrol, hata sonucunu UI'a enjekte ederek yapıldı; diske fiziksel arıza verilmedi.

Neden: [SaveScreen.cs:52](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Flow/UI/SaveScreen.cs#L52) `LastSave` okumuyor. Çözüm: başarı/hata görünümü; hatada başarı kaşesinin kaldırılması, `Context.RetrySave()` ve kaydetmeden devam seçeneği. Retry günü artırmamalı. Kabul: başarısız yazmada başarı iddiası yok; başarılı retry ile Devam Et doğru günü açıyor. [Ekran kanıtı](kanitlar/kayit_hatasi.png).

**B03 - P1 - Kayıt yükleme hatası oyuncuya görünmüyor. Sorumlu: Onur.**

`LoadFailed=true` ve `LoadError` olduğunda gün başı ekranı yalnız gün/kasa/borç gösteriyor. Core'un kaydı okuyamadığını bildirdiği durumda oyuncu neden yeni oyuna döndüğünü anlayamıyor.

Neden: [DayIntroScreen.cs:25](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Flow/UI/DayIntroScreen.cs#L25) bu alanları kullanmıyor. Kısa ve görünür bir bildirim yeterli: “Kayıt okunamadı; yeni oyun başladı.” Kabul: normal başlangıçta görünmez, yükleme hatasında görünür. Teknik hata ayrıntısı oyuncuya yığılmamalı.

**B04 - P1 - Esc açıkken müteahhit konuşması ilerliyor. Sorumlu: Oğuzhan + Onur.**

Esc açık, `Time.timeScale=0` iken 0,5 saniyede görünen karakter sayısı **4'ten 24'e** çıktı. Intro'nun duraklatma testi geçiyor; masa konuşması aynı korumayı kullanmıyor.

Neden: [ClientSceneView.cs:136](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Desk/UI/ClientSceneView.cs#L136) gerçek zaman deltası ve gerçek zaman beklemeleri kullanıyor. `DeskMotion`, popup ve inceleme süreleri de benzer açıdan gözden geçirilmeli; hepsinin bozuk olduğu bu tek testten çıkarılmamalı. Çözüm, Desk -> Flow bağımlılığı eklemeden ortak bir duraklatma/zaman yaklaşımı. Kabul: konuşma ve kritik işlem sıraları durur, devamda aynı yerden sürer; tıklama sızmaz.

**B05 - P2 - Geri alınamayan kaşeden sonra ücretli ek test alınabiliyor. Sorumlu: Oğuzhan; Core sınırı için Ahmet.**

ONAY kaşesi basıldıktan, belgeler henüz iade edilmeden EK TEST düğmesi aktif kalıyor. Güncel sürümde tıklamak kasayı **1.500 -> 1.250 TL** düşürdü. Oyuncu sonucunu artık değiştiremeyeceği bir karar için para harcıyor. Bu, doğrulanmış davranış ve kullanıcı deneyimi sorunu; kayıt sistemindeki gibi veri kaybı hatası değil.

Neden: [DeskScreen.cs:590](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Desk/UI/DeskScreen.cs#L590) yalnız Core `Pending` durumuna bakıyor; kaşe ile iade arasındaki yerel `IsStamped` durumunu dışarıda bırakıyor. Düğme ve satın alma işleyicisi kaşeden sonra kapanmalı veya bu davranış bilerek isteniyorsa neden anlamlı olduğu açık olmalı. Kabul: kaşe sonrası tıklama ikinci bir ücret doğurmuyor.

**B06 - P1 - Gerçekte ulaşılabilen kalabalık gazetede metin taşıyor. Sorumlu: Onur.**

300 gerçek veri koşusundan alınan **7. gün, 22 bina sonucu / 12 yıkım** durumunda sol hikâye 14 fonta kadar küçülmesine rağmen kutu ve gazete dışına taşıyor; `isTextOverflowing=true`. Ayrıca alt bina kartı çok küçük yazı kullanıyor ve “YIKILDI” kaşeleri bazı adres/metin satırlarının üstüne geliyor. Bu yalnız uydurma uzun metinle oluşturulmuş bir örnek değil.

Neden: [ResultScreen.cs:338](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Flow/UI/ResultScreen.cs#L338) bütün yıkımların sorumluluk cümlesini sabit sol kutuya ekliyor. Bina tarafında “Ve N bina daha” özeti var, sol hikâyede sınır yok. [Satır 89-91](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Flow/UI/ResultScreen.cs#L89) ve kaşe boyutları da birlikte düzenlenmeli.

Çözüm: sol kutuyu toplamlar + birkaç kısa olay + “N dosya daha” ile sınırlamak; gerekiyorsa ayrıntıya ayrı görünüm. Kaşe için metinden gerçek bir alan ayırmak. Yalnız metni kesmek veya fontu daha da küçültmek yeterli değil. Kabul: 0/1/4/22 sonuçta bilgiler erişilebilir, kaşeler metni örtmüyor ve taşma yok. [Gerçek durumun ekran kanıtı](kanitlar/gazete_gercek_durum.png).

**Koddan görülen ek riskler ve tasarım önerileri**

**B07 - P1 risk - Kayıt değiştirme işlemi eski sağlam kaydı kaybedebilir. Ahmet.** [SaveSystem.cs:27](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Core/Game/SaveSystem.cs#L27) geçici dosyayı yazıyor, eski kaydı siliyor, sonra taşıyor. Silme başarılı ama taşıma başarısız olursa önceki kayıt yok olur; catch geçici dosyayı da temizleyebilir. Bu pencere koddan görüldü, dosya sistemi arızasıyla yeniden üretilmedi. Son sağlam kaydı koruyan değiştirme/yedekleme akışı hedef Windows ortamında doğrulanmalı. `Delete()` hatasının son ekranına geçişi kesmemesi ve yüklenen JSON'un yalnız `Day>=1` ile doğrulanmaması da aynı küçük sağlamlaştırma turuna dahil edilebilir.

**B08 - P1 risk - Flow builder'ları son görsel/işitsel bağlantıları korumuyor. Onur.** [FlowBuilder.cs:132](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Flow/Editor/FlowBuilder.cs#L132) prefab'ı sıfırdan üretip yalnız az sayıdaki sprite alanını bağlıyor. Güncel prefab'larda font, kağıt, düğme, gazete masası/sesleri ve son müzikleri var. `BuildMainMenu` de sadece background/logo bağlıyor. Yeniden üretim bu bağlantıları kaybettirebilir. Bu incelemede mevcut prefab'lar bozulmadı; risk kod karşılaştırmasından. Teslim öncesi builder'ı gereksiz çalıştırmamak, sonrasında builder ile asset'leri eşitlemek uygun.

**B09 - P2 - Menü müziği dosyası var, oynatıcı bağlantısı yok. Onur / Tarık.** `mus_menu.wav` GUID'ine sahne/prefab/asset referansı bulunmadı; MainMenuScreen ve MenuAmbience'de müzik oynatma akışı da yok. Son müzikleri ise güncel EndingScreen prefab'ında bağlı; “bütün müzikler eksik” denmemeli. Menü için bir loop kaynağı ve sahne geçişinde temiz durdurma yeterli. Credits'te menü müziğinin kullanıldığı iddiası final build'deki durumla eşleşmeli.

**B10 - P2 tasarım - “Rüşveti reddet” etiketi oyuncunun beklediği sonucu vermeyebilir. Oğuzhan + Ahmet.** Mevcut karar gereği düğme zarf ekranını kapatıyor; zarf masada kalıyor ve sonraki yeşil kaşe yine rüşvetli onay sayılıyor. README ve öğretici bunu açıklıyor, yani sözleşmeye aykırı bir kod hatası bulmuş değilim. Ancak etiketi “Zarfı kapat / karara dön” yapmak veya rüşveti gerçekten reddetmeyi ayrı durum olarak sunmak oyuncu niyetini daha net temsil eder. Son saatlerde Core kuralını değiştirmek yerine doğru etiket daha küçük iş.

**B11 - P1/P2 tasarım - İyi son, dürüst kararın kesin karşılığı değil. Ahmet + metin sahibi.** `EvaluateEnding`, hapis/borç/yıkıma bakıyor; rüşvet sayısı ve ayakta kalan uygunsuz yapılar iyi sonu dışlamıyor. Güncel simülasyonda her şeyi onaylayıp zarf alan strateji yaklaşık **%2 iyi son** alabildi. “Son gün rüşvet varsa afet kesin” düzeltmesi main'e geldi; bu riski azalttı, bitirmedi. Kumarın sonuçsuz kalması bilerek isteniyorsa metin bunu “güvenli/dürüst denetim kanıtı” gibi sunmamalı. Aksi isteniyorsa son koşulu ayrıca tasarlanmalı. Bu bir PDF ihlali değil.

**B12 - P2 tasarım - Bir depremi atlatan uygunsuz bina sonraki depremlerde tekrar değerlendirilmiyor. Ahmet.** [EarthquakeSimulator.cs:21](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Core/Simulation/EarthquakeSimulator.cs#L21) yalnız son afetten sonra imzalananları kapsıyor. Bu mevcut belgelerde bilinçli karar. Yine de oyuncuya “ayakta kaldıysa artık güvenli” mesajı verebilir; tematik sunumda oyunun basitleştirilmiş modeli olduğu açıklanmalı. Tüm ayakta binaları tekrar değerlendirmek dengeyi değiştirir; son dakika tek satırlık düzeltme gibi yapılmamalı.

**B13 - P2 tasarım - Formdaki nüfus bilgisi kayıp hesabına bağlanmıyor. Ahmet / içerik.** `PeoplePerFloor` formda 8-24 aralığında; [CasualtyGenerator.cs:9](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Assets/_Project/Scripts/Core/Simulation/CasualtyGenerator.cs#L9) kendi 6-10 kişi/kat aralığından nüfus üretiyor. Parametre kodda zaten “görsel” olarak tanımlı. Oyuncu bu veriyi sonuçla ilişkilendiriyorsa güveni azalabilir. Ya modelde kullanın ya da gereksiz kesinlik veren bu alanı sadeleştirin.

**B14 - P2 içerik - Gerçek yapı standardı izlenimi sınırlandırılmalı. Tarık + Ahmet.** Kurgusal kurumlar var fakat C20/B420/kolon/temel tablosunun bir oyun basitleştirmesi olduğu oyuncuya açıkça söylenmiyor. Kısa bir cümle yeterli: “Değerler ve hesaplar bu oyun için basitleştirilmiştir; gerçek mühendislik değerlendirmesi değildir.” Gerçek bir afet, kişi veya kuruma atıf yapma zorunluluğu yok; kurgusal ve ciddi sonuç anlatımı korunabilir.

**B15 - P2 doküman - Tarihsel planlar son sürüm gibi okunabiliyor. Ekip.** Eski analizde 10 günlük yapı ve eski fiyatlar var; Audio README'si son müzikleriyle ilgili farklı tarihlerden notlar taşıyor. Yeni oklu rehber, 250 TL test ücreti ve son gün afet kuralı final README / beyan / videoda aynı sürümü anlatmalı. Eski raporların başına tarih/sürüm notu koymak yeterli; bütün geliştirme geçmişini silmek gerekmiyor.

**Beyan ve teslimde açık kalan somut işler**

**K01 - P0 doğrulama - Final ZIP, video ve kaynak eşleşmesi. Ahmet + Tarık / temsilci.** Bu makinede `Builds/Imza.zip` yok. Proje notlarındaki “aday alındı” beyanı başka makinedeki çıktıyı gösterebilir. Üstelik sonradan rehber, ekonomi ve afet kodu değişti. Final commit kimliği kaydedilmeli, paket o sürümden alınmalı, temiz bir Windows klasörüne çıkarılıp açılmalı. Bu Mac kurulumunda `BuildPipeline.IsBuildTargetSupported(StandaloneWindows64)` false; burada Windows final testi yapılmış gibi davranılamaz.

**K02 - P0 doğrulama - Beyanın web formuna gerçekten aktarılması. Temsilci.** Güncel [`Teslim_formu_taslak.md`](https://github.com/tarikhokka/GameJam2026TOOADEVS/blob/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08/Docs/Tarik/Teslim_formu_taslak.md) beş hazır kaynak + beş AI kategorisini hazırlamış; bu iş sıfırdan yapılacak bir eksik değil. Ancak CREDITS dosyasının repoda olması web formundaki seçimlerin yerini tutmuyor. Ses üretim kodlarında AI yardımı bulunduğu için AI ses açıklamasının da verilmesi doğru.

**K03 - Kapandı (18 Ekim, kullanıcı teyidi) - `hedef_arayuz.png` kaynağı/üretim aracı. Onur.** Onur, görseli **ChatGPT (OpenAI)** ile oluşturduğunu bildirdi. Masa arayüzü için tasarım referansı olduğu ve oyunda doğrudan asset olarak kullanılmadığı açıklanarak `CREDITS_AND_AI.md` tablosuna eklendi. Görselin üretim tarihi tahmin edilmedi; kayıt tarihi araç teyidinin tarihi olarak belirtildi. İncelenen eski teslim taslağındaki araç yer tutucusu, push hazırlığında [güncel teslim taslağında](../../Docs/Tarik/Teslim_formu_taslak.md) bu bilgiyle değiştirildi. Web formuna aktarılırken güncel taslak kullanılmalıdır. [AI / Görsel alanına eklenecek hazır açıklama](TESLIM_BEYANI_EK.md). Web formuna bu oturumda giriş veya gönderim yapılmadı.

**K04 - P1 risk - CC BY atıf bilgisinin oyun paketinde tamamlanması. Tarık / Ahmet.** CREDITS'te ortam sesinin üreticisi, eser adı, kaynak URL'si ve CC BY 4.0 bilgisi var. Oyun içi credits ise yalnız `freesound.org` ve lisans adını gösteriyor; belirli eser ve lisans bağlantıları yok. CREDITS'in final ZIP'e dahil edildiği görülmedi; ReleaseBuilder yalnız Unity build'i üretiyor. README, CREDITS ve ilgili lisans dosyalarının paket içinde bulunduğunu kontrol edin; CC BY kaynağına ve [lisans metnine](https://creativecommons.org/licenses/by/4.0/) açık bağlantı verin, varsa değişikliği belirtin. Bu, final paketin görülmemesinden doğan doğrulama açığı; kesinleşmiş lisans ihlali kararı değil.

**K05 - P1 - Çalıştırma bilgilerinin son pakete göre tamamlanması. Ahmet.** README kurulum ve kontrolleri anlatıyor; gün sonu kayıt açıklaması da son uzak commit'lerde eklendi. Donanım bilgisi eksik. Test edilen Windows sürümü, CPU/RAM/GPU ve ek kurulum gerekip gerekmediği ölçülerek yazılmalı. Kayıt yolu `TOOADEVS/Imza` ürün ayarlarıyla uyumlu kalmalı. Liberation Sans lisansı `_Project/Fonts` yerine `Assets/TextMesh Pro/Fonts` altında; teslim taslağındaki lisans yolu genellemesi de düzeltilmeli.

Ahşap mühür için daha önceki kaynak belirsizliği son uzak commit'lerde açıklanmış: ekip, SVG'yi sıfırdan çizdiğini, ürün fotoğrafına genel biçim için baktığını ve fotoğrafın piksel/doku/markasını kullanmadığını beyan ediyor. Bu maddeyi hâlâ “fotoğraf kaynağı eksik” diye açık iş saymıyorum; üretim beyanının doğruluğu ekibin sorumluluğunda.

**Güncel denge sonucu ve yorumu**

Bu tablo son test edilen `2c050338` sürümüne aittir. Ek test **250 TL**, başlangıç **1.500 TL**, günlük kira+kredi **1.150 TL**, hasar **600 TL**. Yüzdeler aracın yuvarladığı değerlerdir; bazı satırlar 99/101 edebilir.

| Strateji - her biri 300 oyun | İyi | Yıkımlı orta | Ofis kapanması | Hapis |
|---|---:|---:|---:|---:|
| Dikkatli: ipucu + değer, farkta ek test, zarf yok | %88 | %0 | %12 | %0 |
| Yalnız beyan değerlerine bakan, zarf almayan | %43 | %40 | %0 | %16 |
| Her şeyi onaylayan, zarfları alan | %2 | %6 | %0 | %92 |
| Her şeyi reddeden | %0 | %0 | %100 | %0 |
| Dikkatli ama zarfları alan | %3 | %14 | %0 | %84 |
| Dikkatli, yalnız ilk zarfı alan | %46 | %38 | %1 | %15 |

Dikkatli strateji eski yerel sürümde %72 iyi / %28 kapanma iken güncel sürümde %88 / %12. Aynı zaman aralığında fiyat, ilk gün yazım hatası, afet ve ad üretimi değişti; farkı tek bir değişikliğin nedensel etkisi diye sunmuyorum. Her şeyi reddeden stratejinin ortalama 3. günde kapanması ekonomik baskının çalıştığını gösteriyor.

Bu sonuçlar oyunun dengesinin bütünüyle bozuk olduğunu göstermiyor. Ancak dikkatli stratejinin hâlâ ofis kaybedebilmesi, “dürüst oynadım ama neden kaybettim?” duygusu yaratabilir. Bu bilerek isteniyorsa anlatı desteklemeli; değilse başlangıç tamponu, gider veya test ekonomisi küçük adımlarla gözden geçirilebilir.

Simülasyon insan oyuncu değildir: `OffersBribe`, `Clues` gibi yapılandırılmış alanları doğrudan okur; bazı testleri o günün kararlarından önce topluca satın alır. Bu nedenle gerçek sırayla oynayan kişinin deneyimi ve başarı oranı birebir aynı olmayabilir. İki yeni oyuncunun ilk gün + ilk sahte belgeyi yardım almadan oynaması en yararlı kalan gözlem olur.

**Jüri ölçütlerine göre değerlendirme**

| Ölçüt / üst puan | Güçlü taraf | Şu an puan kaybettirebilecek taraf | En verimli iyileştirme |
|---|---|---|---|
| Oynanabilirlik / 25 | Belge karşılaştırma, sürükle kaşe/iade, yeni eylem rehberi | Ödeme yanlış yönlendiriyor; kayıt hataları gizli; rüşvet reddi etiketi şaşırtıcı | B01-B03; kısa dış oyuncu denemesi |
| Temayla ilişki / 25 | Afetten önce denetim ve kararların insani sonucu oynanışta | Güvensiz ama şanslı karar iyi son alabiliyor; güvenlik modeli fazla kesin algılanabilir | Son metnini sonuç koşullarıyla eşleştirmek; kısa basitleştirme notu |
| Yaratıcılık / 20 | Yapı denetimi, ekonomik baskı ve gazete sorumluluğu birleşimi | Papers, Please benzerliği ilk bakışta baskın | Sunumda kendi denetim/sonuç bağınızı göstermek; yeni sistem eklemek gerekmiyor |
| Görsel/işitsel bütünlük / 15 | Masa, karakterler, kağıtlar ve sonlarda tutarlı dil | Kalabalık gazetede taşma/kaşe örtmesi; menü müziği bağlı değil | B06; sonra B09 |
| Teknik bütünlük / 15 | Çalışan faz akışı, 62 mevcut test, veri/UI ayrımı | Kayıt güvenliği ve hata UI'ı; final Windows paketi doğrulanmadı | B02, B07 ve K01 |

Bu bir jüri puanı tahmini değildir. Çalışan final Windows paketi görülmeden sayısal “kaç puan alır” sonucu vermek yanıltıcı olur. Türden esinlenmek tek başına başka ekibin projesini kopyalamış olma kanıtı değildir; özgünlük sunumda sizin uygulamanızla gösterilmeli.

**Önerilen çalışma sırası**

| Sıra | İş | Sorumlu | Bitti sayılması için |
|---|---|---|---|
| 1 | Gerçek form beyanını son CREDITS ile eşleştir; teyit edilen ChatGPT referansını AI / Görsel açıklamasına aktar | Onur + temsilci | Kategori seçimlerinin ve açıklamalarının güncel kayıtla eşleşmesi |
| 2 | Ödeme UI'ını Preview'a bağla | Onur | Önizleme ve gerçek kasa/borç aynı |
| 3 | Save hata/retry ve LoadFailed bildirimi | Onur | Başarısız işlem başarı gibi görünmüyor; retry gün artırmıyor |
| 4 | Gazetenin özet/kaşe yerleşimini sınırla | Onur | 22 sonuç / 12 yıkım verisinde taşma ve örtüşme yok |
| 5 | Masa konuşmasının Esc davranışı; kaşe sonrası ek test kilidi | Oğuzhan + Onur | Duraklatmada yazı sabit; kaşeden sonra ücret kesilmiyor |
| 6 | Eski kaydı güvenli koru | Ahmet | Başarısız değiştirmede son sağlam kayıt açılabiliyor |
| 7 | Final commit ve paket | Ahmet | Aynı kaynak sürümünden Windows build; temiz çıkarma/açılış ve en az bir gün sonu kontrolü |
| 8 | Video, README/CREDITS/lisanslar ve web teslimi | Tarık + Ahmet / temsilci | <=60 sn ayrı MP4/AVI, boyut sınırları, iki dosya yüklemesi ve makbuz |
| 9 | Menü müziği, builder eşliği ve küçük metin cilası | İlgili sahipler | P0/P1 işleri ve teslim tamponu tamamlandıktan sonra |

Kalan süre daralırsa yeni afet türü, karakter, son veya başka büyük mekanik yerine ödeme/kayıt/gazete ve çalışan paket öncelik almalı. Menü müziği ve genel refactor teslimi geciktirmemeli. Son koşulunu değiştirmek de yalnız metin düzeltmesi kadar küçük bir iş sayılmamalı; ekonomi yeniden ölçülmeli.

**Final pakette kısa kabul turu**

1. ZIP'i yeni bir Windows klasörüne çıkar; exe ve bütün Data/oyun dosyalarıyla aç. Kurulum talimatını projeyi bilmeyen kişi uygulayabilsin.
2. Yeni oyun -> ilk rehber -> belge açma -> karşılaştırma -> kaşe -> iki belgeyi verme -> gün sonu -> ödeme -> kayıt -> yeniden açıp Devam Et.
3. Ayrı kısa turlarda: masum değer farkı ve ek test, sahte belge, zarfı kapatma/reddetme/kabul, kabadayı ve hediye. J menüsü gösterim hızını artırabilir; doğal akış kontrolünü tamamen ikame etmez.
4. Dört sonu, uzun gazeteyi, Esc'yi konuşma/kaşe/iade sırasında ve pencere/tam ekran geçişini kontrol et. 1366x768 gibi daha küçük ekranda okunurluğu da gör.
5. Build commit kimliği, README, CREDITS, lisans dosyaları ve video aynı son sürümü anlatsın. Eski adayın videosunu yeni kaynakla eşleşiyormuş gibi kullanma.
6. En fazla bir dakikalık videoyu ayrı MP4/AVI olarak yükle. Repo/video bağlantısı tek başına dosya teslimi değildir. Sunumu video dahil toplam üç dakikaya prova et.
7. 18 Ekim 18:00 UTC+3'ten önce sunucuda teslim/sürüm kaydı oluşsun. ZIP ve video doğrulandıktan sonra makbuz numarası, sürüm, zaman ve dosya adlarını kontrol et. Sonrasında değerlendirilecek oyun sürümünü değiştirme.

Bu incelemede teslim ekranına giriş yapılmadı, koşullar kabul edilmedi, dosya yüklenmedi veya ekip adına ileti gönderilmedi.

**Kanıtlar ve tekrar üretim**

- [Test özeti](kanitlar/test_ozeti.txt), [EditMode XML](kanitlar/editmode.xml), [PlayMode XML](kanitlar/playmode.xml).
- [Güncel denge raporu](kanitlar/denge_raporu.txt), [ek simülasyon ve model notları](kanitlar/model_kontrolleri.txt), [yedi günlük akış kaydı](kanitlar/yedi_gun.txt).
- [Ödeme](kanitlar/odeme.png), [kayıt hatası](kanitlar/kayit_hatasi.png), [gerçek kalabalık gazete](kanitlar/gazete_gercek_durum.png), [masa](kanitlar/masa.png), [iyi sonun görsel düzeni](kanitlar/iyi_son.png), [güncel credits](kanitlar/credits.png).
- [Geçici PlayMode tanı kaynağı](kanitlar/AuditProbeTests.cs), [denge/model kontrol kaynağı](kanitlar/AuditRunner.cs). Bunlar rapor ekidir; ürün Assets'i veya test assembly'lerine otomatik dahil edilmez.
- [Public repo](https://github.com/tarikhokka/GameJam2026TOOADEVS), [test edilen commit](https://github.com/tarikhokka/GameJam2026TOOADEVS/commit/2c050338d8a6ae1b4b37ea5f760e3eba5229cd08), [CC BY 4.0 kaynak şartları](https://creativecommons.org/licenses/by/4.0/).

Bu rapor ve geçici test/analiz araçları ChatGPT / Codex (OpenAI) ile hazırlandı. Kural karşılaştırması, kaynak incelemesi, izole Unity doğrulaması, denge koşuları ve rapor kapsamı `CREDITS_AND_AI.md` dosyasına eklendi; ürün kodu değiştirilmedi. İncelenen teslim taslağının kanıt kopyası, o anki durumu korumak için değiştirilmedi; sonraki kullanıcı teyidi bu raporda ve ek beyan metninde kayıtlıdır.
