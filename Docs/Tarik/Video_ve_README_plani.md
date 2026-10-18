# Video ve README ekran görüntüsü planı (Tarık, Faz 2)

Teslim kuralı: en fazla **1 dakika**, MP4. Çekim Faz 3'te (A3) son build'le yapılır; bu plan şimdiden hazır olsun diye.

## Çekim ayarları

- **Kayıt:** OBS (ya da Xbox Game Bar, Win+Alt+R).
- **Görüntü:** 1920×1080, 60 fps, MP4 (H.264).
- **Ses:** Sadece oyun sesi. Mikrofon kapalı, anlatım yok; metin oyunun içinde zaten var.
- **Build:** Son build, tam ekran. Editörde çekilmez (Game penceresi kenarları görünür).
- **Kayıt payı:** Her sahne ayrı ayrı ve uzun çekilir, kurgu sonradan kesilir. Kurgu için Clipchamp (Windows'ta hazır) ya da DaVinci Resolve.
- **Fare:** Ağır hareket ettir, tıklamadan önce yarım saniye bekle. İzleyici neye tıklandığını görmeli.

## Senaryo (60 sn)

| Süre | Sahne | Ne görünür | Nasıl çekilir |
|---|---|---|---|
| 0:00–0:05 | Ana menü | Canlı menü (vinçler, toz, ışık), "İmza" başlığı | Menüde 8 sn bekle, 5 sn'sini kullan |
| 0:05–0:10 | Gün başı | "1. Gün" ekranı, masaya geçiş | Yeni Oyun → ilk gün |
| 0:10–0:22 | Masa: inceleme | Müteahhit oturur, İNCELE'ye basılır, formdaki parsel ile rapordaki parsel seçilir → **UYUŞMAZLIK SAPTANDI** kaşesi | Sahte belgeli bir proje gelene kadar çek (seed belli değil; birkaç müteahhit dene) |
| 0:22–0:30 | Zarf | Müteahhidin cevabı, zarf masaya düşer, açılır, para görünür; **RÜŞVETİ REDDET** | Rüşvetçi müteahhitte; ilk uyuşmazlıktan sonra gelir |
| 0:30–0:35 | Karar | RED kaşesi basılır | Aynı proje |
| 0:35–0:40 | Kabadayı (varsa) | Cam kırılır / masaya tekme | Kabadayı tipi müteahhidi reddet; çıkmazsa bu sahne atlanır, süre gazeteye kalır |
| 0:40–0:50 | Deprem + gazete | İnşaat ekranı, gece, sarsıntı; ertesi sabah manşet | Bir sahte binayı **onayla**, sonra **F10** (o gece afet kesin) |
| 0:50–0:58 | Sonlar | Dört sonun canlı ekranlarından 2 sn'lik kesitler (müzikle) | **F9** (son gün) + uygun kararlar; her son ayrı çekilir |
| 0:58–1:00 | Kapanış | Siyah ekranda "İmza · TOOADEVS · Civil Game Jam 2026" | Kurguda yazı eklenir |

Video müziği: oyunun kendi sesi akar. Kurguda sahne geçişlerinde ses 0,3 sn yumuşak geçişle (crossfade) birleştirilir.

### Dört sonu çekmek (F9)

| Son | Nasıl gelinir |
|---|---|
| İyi (Dürüst İmza) | F9 → dürüst kararlar, borca girmeden günü bitir |
| Hüküm (hapis) | Birkaç rüşvet al + F9 |
| Kapanan Kapı (iflas) | Bol ek test iste, borcu limite dayat |
| Enkazın Gölgesi | Sahte bir binayı onayla + F10 (yıkılsın), rüşvet alma, F9 |

Tam koşullar Core'da (`EndingType`). Bir son gelmezse A'ya sor.

## README ekran görüntüleri

`Docs/Ekran/` klasörüne PNG olarak, 1920×1080. README'ye küçük boyutta (`width="720"`) eklenir.

| Dosya | Ne | README'de yeri |
|---|---|---|
| `01_menu.png` | Ana menü | Başlığın hemen altı (kapak) |
| `02_masa.png` | Masa: müteahhit + üç belge | "Nasıl oynanır" başı |
| `03_incele.png` | İnceleme modu: çizgi + UYUŞMAZLIK kaşesi | "İncele" maddesinin altı |
| `04_zarf.png` | Açık zarf, para ve iki düğme | "Zarf" maddesinin altı |
| `05_gazete.png` | Deprem sonrası manşet | "Gün sonu" maddesinin altı |
| `06_son.png` | Bir son ekranı (İyi ya da Hüküm) | "Dört farklı son" cümlesinin altı |

- **Alma:** Oyunda Win+Shift+S ya da OBS'den kare.
- **Boyut:** README sayfası ağırlaşmasın diye her biri 1 MB altında olsun. Gerekirse JPG kalite 85.

## README'ye eklenecekler (Pazar, Tarık)

- **Kapak:** Başlığın altına `01_menu.png`.
- **Video bölümü:** "Kurulum"un üstüne "Video" başlığı ve bağlantı (YouTube liste dışı ya da repo içinde `Docs/video.mp4`).
- Diğer görseller yukarıdaki tablodaki yerlerine.

## İş bölümü

| İş | Kim | Ne zaman |
|---|---|---|
| Bu plan | Tarık | ✅ Faz 2 |
| Son build | A | Faz 3 (A3) |
| Çekim | Tarık (+ A son koşulları için) | A3 build'inden sonra |
| Kurgu | Tarık | Çekimden sonra |
| README görselleri + video bağlantısı | Tarık | Pazar |
