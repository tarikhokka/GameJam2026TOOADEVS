# Art — sahibi: 4. kişi (EEE)

Bu klasöre sadece EEE yazar. Başkası görsel isterse EEE'ye söyler.

## Kurallar
- Referans çözünürlük 1920x1080. Sprite import: Texture Type = Sprite (2D and UI), Pixels Per Unit = 100.
- Önce placeholder konur; gerçek görsel **aynı dosya adıyla üzerine yazılır** (.meta/GUID korunur, prefab referansları bozulmaz).
- İsimler küçük harf + alt çizgi, Türkçe karakter yok: `alan_ad_durum.png`.
- Hazır asset eklendiği anda `CREDITS_AND_AI.md`'ye yazılır.
- Hassasiyet: gerçek deprem görüntüsü yok, enkaz altında insan yok, depremzedeler olumsuz gösterilmez.

## İhtiyaç listesi (B/C ile netleştir)
| Dosya | Ekran | Kim kullanıyor | Durum |
|---|---|---|---|
| `desk_bg.png` | Desk | B | - |
| `doc_paper.png` | Desk | B | - |
| `stamp_approve.png` | Desk | B | - |
| `stamp_reject.png` | Desk | B | - |
| `signature.png` | Desk | B | - |
| `bld_house_standing.png` | Construction/Quake | C | - |
| `bld_house_collapsed.png` | Quake/Result | C | - |
| `city_bg.png` | Construction/Quake | C | - |
| `newspaper_bg.png` | Result | C | - |
| `monthend_bg.png` | MonthEnd | C | - |
| `menu_bg.png`, `logo.png` | MainMenu | C | - |

Durum: `-` yok, `P` placeholder, `F` final.
