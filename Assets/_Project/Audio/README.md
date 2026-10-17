# Audio — sahibi: 4. kişi (EEE)

Bu klasöre sadece EEE yazar.

## Kurallar
- Format: `.ogg` (Kenney), `.wav` ya da `.mp3` (Freesound). Büyük dosyalar Git LFS ile gider; uzun müziklerde Load Type = Streaming. İsim: `sfx_ad.ogg`, `mus_ad.ogg`. Kodda uzantıya güvenmeyin, AudioClip referansı kullanın.
- Placeholder → final değişiminde aynı dosya adı korunur.
- Hazır ses eklendiği anda `CREDITS_AND_AI.md`'ye yazılır. Tercih: CC0 (Kenney, freesound CC0 filtresi).
- Hassasiyet: çığlık, ağlama, gerçek haber/siren kaydı yok.

## İhtiyaç listesi
| Dosya | Nerede | Durum |
|---|---|---|
| `sfx_paper.wav` | Desk, belge açma | F |
| `sfx_stamp.ogg` | Desk, onay/red | F |
| `sfx_click.ogg` | Tüm butonlar | F |
| `sfx_glass_break.wav` | Kabadayı olayı (masada cam kırıldı kartı, B) | F |
| `mus_office_ambient.wav` | Desk (loop) | F |
| `mus_menu.wav` | MainMenu (loop) | F (dosya hazır; oyunda henüz bağlı değil → Onur, memory.md §7) |
| `mus_ending_sad.mp3` | Kötü ve Orta sonlar (Hüküm, Enkazın Gölgesi, Kapanan Kapı) | F (dosya hazır; oyunda henüz bağlı değil → Onur, memory.md §7) |
| `sfx_typing.ogg` | Konuşma balonu: harf harf yazarken döngüde çalan blip (Freesound #468927, CC0, 8,8 sn). Yeni harf gelmezse 0,25 sn sonra durur. Karaktere özel ses yok | F |

Durum: `-` yok, `P` placeholder, `F` final.
