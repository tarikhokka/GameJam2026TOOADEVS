# Art — sahibi: 4. kişi (EEE)

Bu klasöre sadece EEE yazar. Başkası görsel isterse EEE'ye söyler.

## Kurallar
- Referans çözünürlük 1920x1080. Sprite import: Texture Type = Sprite (2D and UI), Sprite Mode = Single, Pixels Per Unit = 100.
- Önce placeholder konur; gerçek görsel **aynı dosya adıyla üzerine yazılır** (.meta/GUID korunur, prefab referansları bozulmaz).
- İsimler küçük harf + alt çizgi, Türkçe karakter yok: `alan_ad_durum.png`.
- Hazır asset eklendiği anda `CREDITS_AND_AI.md`'ye yazılır.
- Hassasiyet: gerçek deprem görüntüsü yok, enkaz altında insan yok, depremzedeler olumsuz gösterilmez.

## İhtiyaç listesi (B/C ile netleştir)
| Dosya | Ekran | Kim kullanıyor | Durum |
|---|---|---|---|
| `desk_bg.png` | Desk | B | P |
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
| `menu_bg.png`, `logo.png` | MainMenu | C | P |

Durum: `-` yok, `P` placeholder, `F` final.
