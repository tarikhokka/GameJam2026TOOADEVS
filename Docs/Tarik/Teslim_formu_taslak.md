# Teslim formu taslağı (kural kitapçığı v09, madde 3.4, 4.2, 5.2)

Takım temsilcisi teslim ekranında (https://game.ktimo.com.tr/jam/teslim) bu metinleri kopyalar. Kaynak: `CREDITS_AND_AI.md`. **Teslimden hemen önce CREDITS ile son bir kez karşılaştırın**; eksik ya da yanlış beyan diskalifiye sebebidir (4.4, 10.1).

> 18 Ekim güncellemesi: Onur, `Docs/Tasarim/hedef_arayuz.png` tasarım referansını ChatGPT ile oluşturduğunu teyit etti; AI / Görsel açıklamasına ve CREDITS kaydına eklendi.

## Oyun bilgileri
- **Oyun adı:** İmza
- **Kısa açıklama:** Bir şehrin yapı denetim mühendisisin. Müteahhitlerin getirdiği formu ve laboratuvar raporunu El Kitabı ile karşılaştır, uyuşmazlıkları incele, kaşeni bas. Rüşvet zarfı ve kabadayılar kararını zorlar; her gece bir deprem olabilir ve ertesi sabah gazete imzanın neye mal olduğunu yazar. 7 gün, 4 son.
- **Platform:** Windows
- **Repo:** https://github.com/tarikhokka/GameJam2026TOOADEVS
- **Kurulum / çalıştırma / kontroller:** README.md'deki "Kurulum", "Kontroller", "Jüri için kısayollar" ve "Bilinen sorunlar" bölümleri.

## Hazır kaynaklar (madde 3.4)

**Görsel — Kullanılmadı**
Oyundaki görsellerin hepsi ekip tarafından üretildi (yapay zekâ ile ya da script ile çizildi; aşağıda AI beyanında). Kenney "Modular Characters" bir süre denendi, sonra kaldırıldı; oyunda yok. Ahşap mühür görselleri Claude ile SVG olarak sıfırdan çizildi; genel biçim için bir ürün fotoğrafına yalnızca bakıldı, fotoğraf kullanılmadı ve repoda yok.

**Ses — Kullanıldı**
- Kenney Interface Sounds, `bong_001` → düğme tıklama sesi · https://kenney.nl/assets/interface-sounds · CC0 1.0
- Kenney Impact Sounds, `impactWood_light_001` → kaşe sesi · https://kenney.nl/assets/impact-sounds · CC0 1.0
- Freesound #63514 "paper5", florian_reinke → kağıt sesi · https://freesound.org/people/florian_reinke/sounds/63514/ · CC0 1.0
- Freesound #468415 "Suburban House Ambience 02", msepitaph → masa ortam sesi · https://freesound.org/people/msepitaph/sounds/468415/ · **CC BY 4.0** (atıf oyunda Emeği Geçenler'de)
- Freesound #341070 "Breaking Glass", hifijohn → kabadayı cam kırma · https://freesound.org/people/hifijohn/sounds/341070/ · CC0 1.0
- Freesound #811581 "Ambient Paulstretched Piano (short)", waveplaysfx → ana menü müziği · https://freesound.org/people/waveplaysfx/sounds/811581/ · CC0 1.0
- Freesound #330599 "The Sad Piano", dasgoat → Hüküm sonu müziği · https://freesound.org/people/dasgoat/sounds/330599/ · CC0 1.0
- Freesound #468927 "Medium Text Blip", malakme → konuşma balonu yazı sesi · https://freesound.org/people/malakme/sounds/468927/ · CC0 1.0
- Dosyaların adı değiştirildi (`sfx_*`, `mus_*`); içerik değiştirilmedi. Hepsi yeniden dağıtılabilir, repoda.

**Kod — Kullanıldı**
- Unity .gitignore şablonu · https://github.com/github/gitignore/blob/main/Unity.gitignore · CC0-1.0
- Unity paketleri (uGUI + TextMeshPro, Input System, URP) · Unity Companion License
- ui-ux-pro-max skill (nextlevelbuilder/ui-ux-pro-max-skill) · MIT · sadece animasyon tasarım rehberi olarak okundu, repoya dosya eklenmedi

**Metin — Kullanılmadı**
Oyundaki bütün metinler ekip tarafından yazıldı (yapay zekâ kullanımı AI beyanında).

**Diğer — Kullanıldı**
- Yazı tipleri: Paytone One (The Paytone Project Authors), Fira Sans (Mozilla Foundation / Telefonica), Liberation Sans (TMP ile gelen) · SIL OFL 1.1 · lisans dosyaları `Assets/_Project/Fonts/`. Lilita One (Juan Montoreano, OFL 1.1) sadece logo görselindeki yazı için kullanıldı, font dosyası repoda yok.
- Unity 2D (URP) proje şablonunun ayarları · Unity Companion License
- TextMesh Pro Essential Resources · Unity Companion License

## Yapay zekâ (madde 4.2)

**Görsel — Kullanıldı**
- ChatGPT (OpenAI): müteahhit portreleri (6 ana + 10 genel), ofis ve masa, belge kabukları (form, lab raporu, El Kitabı), son ekranı görselleri, ana menü arka planı ve logo, rüşvet zarfı. Prompt'ların bir kısmı Claude ile yazıldı.
- Gemini (Google): gazete şablonu ("ŞEHİR POSTASI").
- Claude / Claude Code (Anthropic): script ile çizilen görseller — düğmeler, üst bar, ikonlar, kaşeler, kaşe çekmecesi, belge ikonları, hediyeler, gazete masası, menü ve son ekranı canlandırma katmanları; AI görsellerinin arka plan temizliği, kırpma, renk/ışık türevleri (gün sonu, ödeme, kayıt, Enkazın Gölgesi ekranları).
- ChatGPT (OpenAI): masa arayüzünü planlamak için görsel tasarım referansı (`Docs/Tasarim/hedef_arayuz.png`, Onur Kabadayılar; araç 18 Ekim'de teyit edildi). Görsel tasarım referansı olarak kullanıldı; oyunda doğrudan asset olarak yer almıyor.

**Ses — Kullanıldı**
- Claude (Anthropic) ile yazılan sentez script'leri: üç son müziği (İyi son, Kapanan Kapı, Enkazın Gölgesi) ve mesai zili. Sesler kodla üretildi, dış kaynak yok.

**Kod — Kullanıldı**
- Claude Code (Anthropic): oyun mantığı (Core), masa ekranı, akış ekranları, editör araçları, testler; ekip üyeleri kodu yönlendirdi, inceledi ve Unity'de test etti.
- ChatGPT / Codex (OpenAI): akış ekranları ve bileşenler (gazete, gün özeti, Esc menüsü, üst bar, kaşe çekmecesi, konuşma balonu, inceleme sonucu), PlayMode testleri.

**Metin — Kullanıldı**
- Claude (Anthropic): müteahhit replikleri, gazete manşetleri, son metinleri, öğretici kart ve rehber metinleri, README taslakları; ekip inceleyip düzenledi.
- ChatGPT / Codex (OpenAI): genel müteahhitler için ortak replik havuzu, README'nin 7 güne uyarlanması.

**Diğer — Kullanıldı**
- Claude / Claude Code ve ChatGPT / Codex: proje planı ve ortak hafıza dokümanı (`memory.md`), iş bölümü, ekonomi denge simülasyonu, görsel prompt'larının yazımı.
- Ayrıntılı dosya bazlı liste: `CREDITS_AND_AI.md`.
