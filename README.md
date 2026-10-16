# İmza

**CIVIL GAME JAM 2026** · Tema: *Afete Hazırlık ve Farkındalık* · Takım: **TOOADEVS**

Bir şehrin yapı denetim mühendisisin. Her sabah masana yeni bina projeleri geliyor: başvuru dosyası, laboratuvar raporu ve bazen de masaya "unutulmuş" bir zarf. İmzanı attığın her bina şehirde yükseliyor. Ve her gece bir deprem olabilir.

Ertesi sabah gazete, imzanın neye mal olduğunu yazıyor.

## Kurulum

1. `Imza.zip` dosyasını bir klasöre çıkar.
2. `Imza.exe`'yi çalıştır.

Windows 10/11, 64 bit. Kurulum gerekmez. Oyun 1920×1080 tam ekran açılır.

## Kontroller

| Eylem | Kontrol |
|---|---|
| Tüm etkileşimler | Fare (sol tık) |
| Menü | Esc |

## Nasıl oynanır

- **Masa:** Her projede belgedeki değerleri yönetmelikteki en düşük değerle karşılaştır. Değeri yetmeyen bina güvenli değildir.
- **Belgeleri karşılaştır:** Başvuru dosyası ile laboratuvar raporu aynı parseli, aynı tarihi, aynı mühendisin imzasını ve laboratuvarın kendi kaşesini taşımalı. Tutmayan bir ayrıntı sahte belge işaretidir.
- **Ek test:** Şüphelendiğin projede ücretli ek test iste; gerçek değerler ortaya çıkar.
- **Zarf:** Bazı müteahhitler rüşvet teklif eder. Rüşvet teklif edilen her proje kurallara aykırıdır.
- **Kabadayı:** Bazı müteahhitler "hayır" cevabını sevmez. Reddedersen ofisinin camı kırılabilir; tamiri sana kalır.
- **Gün sonu:** Afet olursa gazete, onayladığın binaların akıbetini yazar. Olmazsa günün özetini görürsün.
- **Ödeme:** Her gün kira ve kredi ödenir. Kasada yetmeyen gider borca yazılır; borç çok büyürse ofis kapanır.

Oyun 10 gün sürer. Dört farklı son var; hangisini göreceğin, attığın imzalara bağlı.

## Jüri için kısayollar

10 günü baştan sona oynamadan bütün sonları görebilmeniz için oyun sırasında:

| Tuş | Etkisi |
|---|---|
| F9 | Son güne atlar (o günün sonunda son belirlenir) |
| F10 | O gecenin sonunda afet kesin olur |

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
