# İmza

**CIVIL GAME JAM 2026** · Tema: *Afete Hazırlık ve Farkındalık* · Takım: **TOOADEVS**

Bir şehrin yapı denetim mühendisisin. Her gün masana yeni bina projeleri geliyor: yapı denetim formu, laboratuvar raporu ve bazen de masaya "unutulmuş" bir zarf. İmzanı attığın her bina şehirde yükseliyor. Ve her gece bir deprem olabilir.

Ertesi sabah gazete, imzanın neye mal olduğunu yazıyor.

## Kurulum

1. `Imza.zip` dosyasını bir klasöre çıkar.
2. `Imza.exe`'yi çalıştır.

Windows 10/11, 64 bit. Kurulum gerekmez. Oyun 1920×1080 tam ekran açılır.

### Bilinen sorunlar

- **Windows SmartScreen uyarısı:** `Imza.exe` imzasız olduğu için Windows "bilgisayarınız korundu" diyebilir. **Ek bilgi → Yine de çalıştır** ile açılır.
- **ZIP içinden çalıştırmayın:** Önce klasöre çıkarın; ZIP içinden açılan oyun dosyalarını bulamayabilir.
- **Pencere modu / çözünürlük:** Ekran farklı oranda ise kenarlarda boşluk kalabilir. Esc → Ayarlar → Tam ekran ile pencere ve tam ekran arasında geçilir.
- **Kayıt:** Oyun her gün sonunda kaydedilir (gün içinde çıkılırsa o günün ilerlemesi kaydedilmez). Kayıt Windows'ta `%USERPROFILE%\AppData\LocalLow\TOOADEVS\Imza\` klasöründedir; silinirse ana menüde "Devam Et" görünmez.

## Kontroller

| Eylem | Kontrol |
|---|---|
| Tüm etkileşimler | Fare (sol tık) |
| Belge açma | Masanın solundaki belge ikonuna tıkla |
| Karar | Kaşe düğmesi → kaşeyi forma sürükle (yeşil ONAY / kırmızı RED) |
| Belgeleri geri verme | Kaşeden sonra belgeyi müteahhidin üstüne sürükle |
| İnceleme modu | **İNCELE** düğmesi, sonra iki alana tıkla; sağ tık seçimi iptal eder |
| Menü (ses ayarı, tam ekran) | Esc → Ayarlar |

## Nasıl oynanır

- **Masa:** Müteahhitler sırayla karşına oturur. Getirdiği **Yapı Denetim Formu** ve **Laboratuvar Raporu** masanın solunda küçük ikonlar olarak durur; tıklayınca açılır. **Denetçi El Kitabı** hep açıktır.
- **Kaşe:** Kararını El Kitabı'nın sağındaki **kaşe düğmesiyle** verirsin: bölme açılır, yeşil **ONAY** ya da kırmızı **RED** kaşesini tutup formdaki kaşe alanına sürükle. Basılan kaşe değiştirilemez.
- **Belgeleri geri ver:** Kaşeden sonra iki belgeyi de müteahhide sürükle. İkisi de verilince karar kesinleşir ve sıradaki müteahhit gelir.
- **El Kitabı:** **Hızlı Bakış** sayfası formdaki kat adedine göre doğru kat grubunu kendisi gösterir. Beş değerin (beton sınıfı, demir tipi, kolon kalınlığı, temel derinliği, zemin türü) hepsi yeşil bölgede olmalı; tek bir kırmızı, binanın güvenli olmadığı demektir. El Kitabı'nda Hızlı Bakış ve LAB (akredite laboratuvarlar) sayfaları var. İlk gün kısa bir anlatım seni karşılar; masadaki **NASIL OYNANIR** ile istediğin an yeniden açarsın.
- **Belgeleri karşılaştır:** Form ile laboratuvar raporu aynı parseli, aynı tarihi ve aynı mühendisin imzasını taşımalı. Rapordaki kaşe, raporu yazan laboratuvara ait olmalı ve o laboratuvar akredite listede bulunmalı. Tutmayan bir ayrıntı sahte belge işaretidir.
- **İncele:** **İNCELE** düğmesine bas, iki ayrı belgeden aynı bilgiye tıkla (ör. formdaki ve rapordaki parsel ya da formdaki bir değer ile El Kitabı'ndaki satırı). Oyun **SORUN YOK**, **UYUŞMAZLIK SAPTANDI** ya da **ALAKASIZ** der. Uyuşmazlık bulursan müteahhide söylersin, o da cevap verir. İnceleme ücretsiz ve sınırsızdır; kararı yine sen verirsin.
- **Ek test:** Şüphelendiğin projede ücretli ek test iste; gerçek değerler ortaya çıkar. Sahte belgede kâğıttaki beyan temiz görünür, gerçek değer ancak ek testte görünür. Form ile lab raporunda bir değer farklı yazıyorsa ya masum bir yazım hatasıdır ya da biri yalan söylüyordur; hangisi olduğunu ek test gösterir. Ama her test kasadan çıkar.
- **Zarf:** Rüşvetçi müteahhit, sen bir uyuşmazlık bulunca masaya zarf bırakır. Zarfa tıklarsan açılır ve içindeki parayı görürsün: **rüşveti kabul et** (zarfı alırsın, forma yeşil ONAY kaşesi kendiliğinden basılır) ya da **reddet** (ya da boş bir yere tıkla; zarf masaya döner, belgelere bakmaya devam edersin). Zarf masadayken yeşil **ONAY** kaşesi zarfla onay demektir; kırmızı **RED** kaşesi zarfı da reddeder. Rüşvet teklif edilen her proje kurallara aykırıdır.
- **Kabadayı:** Bazı müteahhitler "hayır" cevabını sevmez. Reddedersen camın kırılabilir, masana tekme atılabilir, kapının kilidi ya da arabanın lastikleri gidebilir; tamiri sana kalır.
- **Gün sonu:** Afet olursa gazete, onayladığın binaların akıbetini yazar. Olmazsa günün özetini görürsün.
- **Ödeme:** Her gün kira ve kredi ödenir. Kasada yetmeyen gider borca yazılır; borç çok büyürse ofis kapanır.

Oyun 7 gün sürer. Dört farklı son var; hangisini göreceğin, attığın imzalara bağlı.

## Jüri için kısayollar

7 günü baştan sona oynamadan bütün sonları görebilmeniz için oyun sırasında:

| Tuş | Etkisi |
|---|---|
| F9 | Son güne atlar (o günün sonunda son belirlenir) |
| F10 | O gecenin sonunda afet kesin olur |
| **J** | Jüri menüsü: istenen sahneye atlar (dört son, rüşvet, hediye, kabadayı). Sol alttaki "Jüri menüsü: J" etiketine tıklayarak da açılır |

## Ekip

| Kişi | Rol |
|---|---|
| Ahmet İlman | Oyun mantığı (Core), veri, entegrasyon, build |
| Oğuzhan Yurt | Masa ekranı (belgeler, kararlar, ses ve kaşe) |
| Onur Kabadayılar | Akış ekranları (gazete, gün özeti, ödeme, sonlar, ana menü) |
| Tarık Hokkaömeroğlu | Görsel ve ses, içerik metinleri (müteahhitler, gazete manşetleri, sonlar), asset ve yapay zekâ kaydı |

## Hazır asset ve yapay zekâ kullanımı

Kullanılan bütün hazır asset'ler (lisanslarıyla) ve yapay zekâ kullanımı [`CREDITS_AND_AI.md`](CREDITS_AND_AI.md) dosyasında listelenir. Atıf gerektiren asset'ler oyun içinde ana menü → **Emeği Geçenler** ekranında da gösterilir.

## Hassasiyet notu

Oyundaki bütün kişi, bina, mahalle ve şirket adları kurgusaldır. Gerçek bir afete ya da kişiye atıf yoktur. Oyun, depremden etkilenen insanları değil, yapıları denetleyenlerin sorumluluğunu konu alır.

## Teknik

- Kaynak kod: https://github.com/tarikhokka/GameJam2026TOOADEVS
- Unity **6000.6.4f1**, 2D (URP), uGUI + TextMeshPro, Input System.
- Kod klasörleri: `Assets/_Project/Scripts/` (`Core`, `Desk`, `Flow`).
- Geliştirici notları: `memory.md` (ortak kararlar), `AHMET.md`, `OGUZHAN.md`, `ONUR.md`.
- Testler: Unity → Window → General → Test Runner → EditMode.
