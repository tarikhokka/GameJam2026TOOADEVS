# CREDITS_AND_AI.md — Hazır asset ve yapay zekâ kullanımı

Jam kuralı: Her hazır asset ve her AI kullanımı **eklendiği anda** buraya yazılır (küçük kullanım dahil).
Yeniden dağıtılamayan (redistribute edilemeyen) asset'ler repoya konmaz.

Satır biçimi: `| Ne | Nerede kullanıldı | Kaynak (link) | Lisans | Ekleyen | Tarih |`

## Görsel
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|

## Ses / Müzik
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Kenney Interface Sounds — `bong_001.ogg` (adı `sfx_click.ogg` yapıldı) | Buton tıklama sesi | https://kenney.nl/assets/interface-sounds | CC0 1.0 | Tarık | 2026-10-16 |
| Kenney Impact Sounds — `impactWood_light_001.ogg` (adı `sfx_stamp.ogg` yapıldı) | Kaşe vurma sesi (onay/red) | https://kenney.nl/assets/impact-sounds | CC0 1.0 | Tarık | 2026-10-16 |
| Freesound #63514 "paper5" — florian_reinke (adı `sfx_paper.wav` yapıldı) | Belge açma / kağıt hışırtısı | https://freesound.org/people/florian_reinke/sounds/63514/ | CC0 1.0 | Tarık | 2026-10-16 |
| Freesound #468415 "Suburban House Ambience 02" — msepitaph (adı `mus_office_ambient.wav` yapıldı) | Masa ekranı ortam sesi (loop) | https://freesound.org/people/msepitaph/sounds/468415/ | **CC BY 4.0 — atıf zorunlu** | Tarık | 2026-10-16 |

## Kod
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Unity .gitignore şablonu | `.gitignore` | https://github.com/github/gitignore/blob/main/Unity.gitignore | CC0-1.0 | Tarık | 2026-10-16 |

## Font
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|

## Diğer
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Unity 2D proje şablonu (Welcome klasörü, URP ayarları) | `Assets/Welcome`, `Assets/Settings` | Unity Hub şablonu | Unity Companion License | Tarık | 2026-10-16 |
| TextMesh Pro Essential Resources (LiberationSans SDF font, shader'lar, TMP Settings) | `Assets/TextMesh Pro` (tüm TMP yazıları) | Unity uGUI paketi (com.unity.ugui) içinden import | Unity Companion License; LiberationSans: SIL OFL 1.1 | Ahmet İlman | 2026-10-16 |

## Yapay zekâ kullanımı
| Araç | Ne için kullanıldı | Hangi dosyalar | Kullanan | Tarih |
|---|---|---|---|---|
| Claude (Anthropic) | Proje hafıza dokümanı, iş planı, bu dosyanın şablonu | `memory.md`, `CLAUDE.md`, `CREDITS_AND_AI.md` | Tarık | 2026-10-16 |
| Claude (Anthropic) | Geçici placeholder görselleri üreten script (düz renk + dosya adı yazısı); finalde değiştirilecek | `Assets/_Project/Art/*.png` | Tarık | 2026-10-16 |
| Claude (Anthropic) | Müteahhit karakter taslakları (isimler, selamlama ve rüşvet replikleri), gazete manşet ve son metni taslakları; EEE inceleyip onayladı. Müteahhit metinlerinden ClientSO asset dosyalarını üreten script; manşetlerin Headlines.asset dosyasına taşınması | `ICERIK_TASLAK.md`, `Assets/_Project/Data/Clients/*.asset`, `Assets/_Project/Data/Headlines.asset` | Tarık | 2026-10-16 |
| Claude Code (Anthropic) | Core veri sözleşmesi kodu, deprem simülatörü, A rolü çalışma planı | `Assets/_Project/Scripts/Core/**`, `AHMET.md`, `memory.md` | Ahmet İlman | 2026-10-16 |
| Claude Code (Anthropic) | Core Faz 1: proje üretici, afet sistemi, oyun oturumu/yöneticisi, kayıt, debug arayüzü, testler, veri asset ve Game sahnesi kurulum aracı; kurgusal isim listeleri | `Assets/_Project/Scripts/Core/**`, `Assets/_Project/Data/**`, `Assets/_Project/Scenes/Game.unity`, `AHMET.md`, `memory.md` | Ahmet İlman | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Gün sistemi, haftalık gider ve gün sonu kayıt kararlarıyla ortak hafızanın güncellenmesi; Onur için C rolü çalışma planı | `memory.md`, `ONUR.md`, `CREDITS_AND_AI.md` | Onur Kabadayılar | 2026-10-16 |
| Claude (Anthropic) | Masa ekranı kodu: UI script'leri, Core'a bağlantı (IGameScreen/IGameActions), sahte gün verisi ve sandbox IGameActions, prefab/sahne kurulum aracı; B iş planı | `Assets/_Project/Scripts/Desk/**`, `OGUZHAN.md` | Oğuzhan | 2026-10-16 |
| Claude (Anthropic) | Intro hikaye ekranı (daktilo efekti, sayfa geçişi) ve intro metinleri | `Assets/Scripts/Intro/IntroSequence.cs` | Oğuzhan | 2026-10-16 |
| Claude (Anthropic) | Yapı Denetim Formu ve Denetçi El Kitabı tasarım görselleri (referans; kural metinleri dahil) ve kural sözleşmesi tablosu | `Docs/Tasarim/*.png`, `memory.md` | Oğuzhan | 2026-10-17 |
| ChatGPT / Codex (OpenAI) | Unity arayüzünde Lit 2D (URP) deneme sahnesi oluşturma ve kayıt doğrulama; durum notları | `Assets/_Project/Scenes/Sandbox_Flow.unity`, `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Unity arayüzünde Canvas kurulumu, 1920×1080 ölçekleme ve kayıt doğrulama | `Assets/_Project/Scenes/Sandbox_Flow.unity`, `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Unity arayüzünde ResultScreen panelini oluşturma, tam ekran yerleşim ve kayıt doğrulama | `Assets/_Project/Scenes/Sandbox_Flow.unity`, `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Ahmet’in günlük ödeme ve afet kapsamı kararlarını inceleme; ortak hafıza ve C planını güncelleme | `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| Gemini / Antigravity (Google) | Gazete sonuç ekranı arka plan şablonu ("ŞEHİR POSTASI") görsel üretimi | `Assets/_Project/Art/newspaper_bg.png` | Onur Kabadayılar | 2026-10-16 |
| Claude Code (Anthropic) | Flow kodu: gazete, gün özeti, günlük ödeme, son ve ana menü ekranları, Flow sandbox ve prefab/sahne kurulum aracı | `Assets/_Project/Scripts/Flow/**` | Onur Kabadayılar | 2026-10-16 |

## Oyun içi "Emeği Geçenler" metni
CC BY (atıf zorunlu) asset'ler oyunun credits ekranında da görünmeli. Yeni asset eklenince bu blok da güncellenir; credits ekranı bu metni aynen kullanır.

```
SESLER
Ortam sesi: "Suburban House Ambience 02" — msepitaph (freesound.org), CC BY 4.0
Kağıt sesi: "paper5" — florian_reinke (freesound.org), CC0
Arayüz ve kaşe sesleri: Kenney (kenney.nl), CC0
```
