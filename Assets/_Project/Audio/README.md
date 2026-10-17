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
| `mus_ending_sad.mp3` | Kötü son (Hüküm), EndingScreen'e bağlı | F |
| `mus_ending_good.wav` / `mus_ending_closed.wav` / `mus_ending_collapsed.wav` | İyi son / Kapanan Kapı / Enkazın Gölgesi (loop), EndingScreen'e bağlı | Ekibin kendi eseri (kodla sentezlendi, EEE 18 Ekim) |
| `sfx_typing.ogg` | Konuşma balonu: harf harf yazarken döngüde çalan blip (Freesound #468927, CC0, 8,8 sn). Yeni harf gelmezse 0,25 sn sonra durur. Karaktere özel ses yok | F |
| `sfx_shift_end.wav` | Masa: mesai bitince (son müteahhitten sonra) "MESAİ BİTTİ" geçiş ekranıyla çalan iki notalı zil. Kodla sentezlendi (ekibin kendi eseri), 2,4 sn. DeskAudio.shiftEnd | F |

Durum: `-` yok, `P` placeholder, `F` final.
