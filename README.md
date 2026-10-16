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
- **Gün sonu:** Afet olursa gazete, onayladığın binaların akıbetini yazar. Olmazsa günün özetini görürsün.
- **Ödeme:** Her gün kira ve kredi ödenir. Kasada yetmeyen gider borca yazılır; borç çok büyürse ofis kapanır.

Oyun 10 gün sürer. Sonun, verdiğin kararlara bağlıdır.

## Ekip

| Kişi | Rol |
|---|---|
| Ahmet İlman | Oyun mantığı (Core), veri, entegrasyon, build |
| Oğuzhan | Masa ekranı (belgeler, kararlar, ses ve kaşe) |
| Onur Kabadayılar | Akış ekranları (gazete, gün özeti, ödeme, sonlar, ana menü) |
| Tarık | Görsel, ses, içerik metinleri |

## Hazır asset ve yapay zekâ kullanımı

Kullanılan bütün hazır asset'ler (lisanslarıyla) ve yapay zekâ kullanımı [`CREDITS_AND_AI.md`](CREDITS_AND_AI.md) dosyasında listelenir. Atıf gerektiren asset'ler oyun içinde ana menü → **Emeği Geçenler** ekranında da gösterilir.

## Hassasiyet notu

Oyundaki bütün kişi, bina, mahalle ve şirket adları kurgusaldır. Gerçek bir afete ya da kişiye atıf yoktur. Oyun, depremden etkilenen insanları değil, yapıları denetleyenlerin sorumluluğunu konu alır.

## Teknik

- Unity **6000.6.4f1**, 2D (URP), uGUI + TextMeshPro, Input System.
- Kaynak kod: `Assets/_Project/Scripts/` (`Core`, `Desk`, `Flow`).
- Geliştirici notları: `memory.md` (ortak kararlar), `AHMET.md`, `OGUZHAN.md`, `ONUR.md`.
- Testler: Unity → Window → General → Test Runner → EditMode.
