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
| `sfx_paper.wav` | Desk, belge açma | P |
| `sfx_stamp.ogg` | Desk, onay/red | P |
| `sfx_click.ogg` | Tüm butonlar | P |
| `sfx_glass_break.wav` | Kabadayı olayı (masada cam kırıldı kartı, B) | P |
| `mus_office_ambient.wav` | Desk (loop) | P |
| `mus_menu.wav` | MainMenu (loop) | P |
| `mus_ending_sad.mp3` | Kötü ve Orta sonlar (Hüküm, Enkazın Gölgesi, Kapanan Kapı) | P |
| `sfx_typing.wav` | Konuşma balonu: harf harf yazı sesi (Among Us tarzı kısa bip; özgün, script ile üretildi). Karaktere özel ses yok | F |

Durum: `-` yok, `P` placeholder, `F` final.
