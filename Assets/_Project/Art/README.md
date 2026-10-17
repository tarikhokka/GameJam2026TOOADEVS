# Art — sahibi: 4. kişi (EEE)

Bu klasöre sadece EEE yazar. Başkası görsel isterse EEE'ye söyler.

## Kurallar
- **Tarz: el çizimi 2D cartoon** — kalın koyu kahve kontur, yumuşak cel gölge, sıcak palet; Gemini/ChatGPT ile ortak tarz prompt'uyla üretilir. Müteahhit önden, masanın karşısında oturur; yürüme animasyonu yok. Beyaz arka planlı üretilen görsellerin arka planı script ile silinir.
- **Stil (karar 02:20): hazır asset, piksel art yok.** Yeniden dağıtılabilir lisanslı (tercihen CC0) asset'ler; mümkünse tek kaynak/tek stil ailesinden. Her asset eklendiği anda `CREDITS_AND_AI.md`'ye yazılır; CC BY ise oyun içi Emeği Geçenler metnine de.
- Referans çözünürlük 1920x1080. Sprite import: Texture Type = Sprite (2D and UI), Sprite Mode = Single, Pixels Per Unit = 100.
- Önce placeholder konur; gerçek görsel **aynı dosya adıyla üzerine yazılır** (.meta/GUID korunur, prefab referansları bozulmaz).
- İsimler küçük harf + alt çizgi, Türkçe karakter yok: `alan_ad_durum.png`.
- Hazır asset eklendiği anda `CREDITS_AND_AI.md`'ye yazılır.
- Hassasiyet: gerçek deprem görüntüsü yok, enkaz altında insan yok, depremzedeler olumsuz gösterilmez.

## İhtiyaç listesi (B/C ile netleştir)
| Dosya | Ekran | Kim kullanıyor | Durum |
|---|---|---|---|
| `desk_bg.png` | Desk ARKA katman: oda + koltuk + masa (ChatGPT, 1920x1080). Çizim sırası: desk_bg → portre → desk_front → kitaplar | B | F |
| `desk_front.png` | Desk ÖN katman: sadece masa (şeffaf, 1920x1080, tam ekran). Portrenin ÖNÜNE çizilir, karakterin belden aşağısını kapatır | B | F |
| `doc_paper.png` | Desk | B | P |
| `stamp_approve.png` | Desk | B | P |
| `stamp_reject.png` | Desk | B | P |
| `signature.png` | Desk | B | P |
| `bld_house_standing.png` | Construction/Result | C | P |
| `bld_house_collapsed.png` | Result (gazete) | C | P |
| `city_bg.png` | Construction | C | P |
| `newspaper_bg.png` | Result | C | F |
| `daysummary_bg.png` | DaySummary | C | P |
| `payment_bg.png` | Günlük ödeme | C | P |
| `menu_bg.png`, `logo.png` | MainMenu (akşam şehir + vinçler; "İMZA" logosu, Lilita One + imza kıvrımı, 900x330 şeffaf) | C | F |
| `ending_good.png` | Ending (İyi) | C | P |
| `ending_bad.png` | Ending (Kötü, mahkeme) | C | P |
| `ending_closed.png` | Ending (Orta, ofis kapandı) | C | P |
| `portrait_rifat/sevim/kemal/nuri/selim/rustem.png` | Desk (müteahhit, ClientSO.Portrait) | B | F (ChatGPT ile üretildi, arka planı silindi; 600x600 şeffaf, karakter alta ortalı, belden yukarısı) |
| `chair_back.png` | KULLANILMIYOR (yeni desk_bg'de koltuk var). Silinmedi, referans kırılmasın diye duruyor | B | — |
| `book_rules.png` | Desk: "EL KİTABI — Olması gereken değerler" (Gemini, 600x577 şeffaf; masada gölgeli ve hafif eğik durmalı), masada sağ altta HEP durur (tıklanınca El Kitabı penceresi) | B | F |
| `book_lab.png` | Desk: "LAB RAPORU — Beyan edilen değerler" (Gemini, 600x546 şeffaf; masada gölgeli ve hafif eğik durmalı), müteahhitle GELİR, sol altta durur, kararla birlikte GİDER | B | F |

Durum: `-` yok, `P` placeholder, `F` final.
