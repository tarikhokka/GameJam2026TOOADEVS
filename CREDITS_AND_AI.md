# CREDITS_AND_AI.md — Hazır asset ve yapay zekâ kullanımı

Jam kuralı: Her hazır asset ve her AI kullanımı **eklendiği anda** buraya yazılır (küçük kullanım dahil).
Yeniden dağıtılamayan (redistribute edilemeyen) asset'ler repoya konmaz.

Satır biçimi: `| Ne | Nerede kullanıldı | Kaynak (link) | Lisans | Ekleyen | Tarih |`

## Görsel
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Kenney "Modular Characters" parçaları — (KULLANILMIYOR: 17 Ekim gece ChatGPT portreleriyle değiştirildi; git geçmişinde duruyor) | — | https://kenney.nl/assets/modular-characters | CC0 1.0 | Tarık | 2026-10-17 |

## Ses / Müzik
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Kenney Interface Sounds — `bong_001.ogg` (adı `sfx_click.ogg` yapıldı) | Buton tıklama sesi | https://kenney.nl/assets/interface-sounds | CC0 1.0 | Tarık | 2026-10-16 |
| Kenney Impact Sounds — `impactWood_light_001.ogg` (adı `sfx_stamp.ogg` yapıldı) | Kaşe vurma sesi (onay/red) | https://kenney.nl/assets/impact-sounds | CC0 1.0 | Tarık | 2026-10-16 |
| Freesound #63514 "paper5" — florian_reinke (adı `sfx_paper.wav` yapıldı) | Belge açma / kağıt hışırtısı | https://freesound.org/people/florian_reinke/sounds/63514/ | CC0 1.0 | Tarık | 2026-10-16 |
| Freesound #468415 "Suburban House Ambience 02" — msepitaph (adı `mus_office_ambient.wav` yapıldı) | Masa ekranı ortam sesi (loop) | https://freesound.org/people/msepitaph/sounds/468415/ | **CC BY 4.0 — atıf zorunlu** | Tarık | 2026-10-16 |
| Freesound #341070 "Breaking Glass" — hifijohn (adı `sfx_glass_break.wav` yapıldı) | Kabadayı olayı: ofis camı kırılır | https://freesound.org/people/hifijohn/sounds/341070/ | CC0 1.0 | Tarık | 2026-10-17 |
| Freesound #811581 "Ambient Paulstretched Piano (short)" — waveplaysfx (adı `mus_menu.wav` yapıldı) | Ana menü müziği (loop) | https://freesound.org/people/waveplaysfx/sounds/811581/ | CC0 1.0 | Tarık | 2026-10-17 |
| Freesound #330599 "The Sad Piano" — dasgoat (adı `mus_ending_sad.mp3` yapıldı) | Kötü ve Orta son ekranları müziği | https://freesound.org/people/dasgoat/sounds/330599/ | CC0 1.0 | Tarık | 2026-10-17 |

## Kod
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Unity .gitignore şablonu | `.gitignore` | https://github.com/github/gitignore/blob/main/Unity.gitignore | CC0-1.0 | Tarık | 2026-10-16 |

## Font
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Lilita One (Juan Montoreano) — sadece logo görselinde yazı olarak kullanıldı, font dosyası repoda yok | `Assets/_Project/Art/logo.png` | https://fonts.google.com/specimen/Lilita+One | SIL OFL 1.1 | Tarık | 2026-10-17 |

## Diğer
| Ne | Nerede kullanıldı | Kaynak | Lisans | Ekleyen | Tarih |
|---|---|---|---|---|---|
| Unity 2D proje şablonu (URP ayarları; Welcome klasörü ve SampleScene 17 Ekim'de kaldırıldı) | `Assets/Settings` | Unity Hub şablonu | Unity Companion License | Tarık | 2026-10-16 |
| TextMesh Pro Essential Resources (LiberationSans SDF font, shader'lar, TMP Settings) | `Assets/TextMesh Pro` (tüm TMP yazıları) | Unity uGUI paketi (com.unity.ugui) içinden import | Unity Companion License; LiberationSans: SIL OFL 1.1 | Ahmet İlman | 2026-10-16 |

## Yapay zekâ kullanımı
| Araç | Ne için kullanıldı | Hangi dosyalar | Kullanan | Tarih |
|---|---|---|---|---|
| ChatGPT / Codex (OpenAI) | Esc duraklatma menüsü, arka ekran girdi kilidi, intro duraklatma uyumu ve PlayMode testleri | `Assets/_Project/Scripts/Flow/UI/PauseMenu.cs`, `UiKit.cs`, `FlowScreen.cs`, `DayIntroScreen.cs`, `ConstructionScreen.cs`, `IntroSequence.cs`, `Assets/_Project/Scripts/Flow/Tests/PlayMode/` | Onur Kabadayılar | 2026-10-17 |
| ChatGPT / Codex (OpenAI) | Gün başı ve inşaat ekranlarının Unity UI kurulumu, Core verilerini gösteren script'ler, prefab ve sandbox bağlantıları; inşaat ekranı için 0/2 onay ve devam butonu Play testi | `Assets/_Project/Scripts/Flow/UI/DayIntroScreen.cs`, `Assets/_Project/Scripts/Flow/UI/ConstructionScreen.cs`, `Assets/_Project/Scripts/Flow/Editor/ConstructionSetup.cs`, `Assets/_Project/Scripts/Flow/Editor/FlowBuilder.cs`, `Assets/_Project/Scripts/Flow/Sandbox/FlowSandboxBootstrap.cs`, `Assets/_Project/Prefabs/Flow/DayIntroScreen.prefab`, `Assets/_Project/Prefabs/Flow/ConstructionScreen.prefab`, `Assets/_Project/Scenes/Sandbox_Flow.unity`, `ONUR.md` | Onur Kabadayılar | 2026-10-17 |
| Claude (Anthropic) | Proje hafıza dokümanı, iş planı, bu dosyanın şablonu | `memory.md`, `CLAUDE.md`, `CREDITS_AND_AI.md` | Tarık | 2026-10-16 |
| Claude (Anthropic) | Geçici placeholder görselleri üreten script (düz renk + dosya adı yazısı); finalde değiştirilecek | `Assets/_Project/Art/*.png` | Tarık | 2026-10-16 |
| Claude (Anthropic) | (DEĞİŞTİRİLDİ, kullanılmıyor) Kenney Modular Characters parçalarını birleştirip 6 müteahhit portresi üreten script (parça seçimi ve yüz ifadeleri; Rıfat'ın parçalarını Tarık seçti) | `Assets/_Project/Art/portrait_*.png` | Tarık | 2026-10-17 |
| ChatGPT (OpenAI) | 6 müteahhit karakter görselinin üretimi (Rıfat, Sevim, Kemal, Nuri, Selim, Rüstem) | `Assets/_Project/Art/portrait_*.png` | Tarık | 2026-10-17 |
| Claude (Anthropic) | ChatGPT portrelerinin beyaz arka planını silip şeffaf 600x600 PNG'ye çeviren script | `Assets/_Project/Art/portrait_*.png` | Tarık | 2026-10-17 |
| Claude (Anthropic) | Müteahhit koltuğu arkalığı görselini çizen script (flat, kontursuz) | `Assets/_Project/Art/chair_back.png` | Tarık | 2026-10-17 |
| Claude (Anthropic) | Masa arka planı (ofis, inşaat resimli çerçeveler, aksesuarlar) ve iki kitap görselini çizen script (flat) | `Assets/_Project/Art/desk_bg.png`, `book_lab.png`, `book_rules.png` | Tarık | 2026-10-17 |
| Claude (Anthropic) | Ana menü arka planı (akşam şehir, vinçler) ve "İMZA" logosunu çizen script (flat) | `Assets/_Project/Art/menu_bg.png`, `logo.png` | Tarık | 2026-10-17 |
| Claude (Anthropic) | Müteahhit karakter taslakları (isimler, selamlama ve rüşvet replikleri), gazete manşet ve son metni taslakları; EEE inceleyip onayladı. Müteahhit metinlerinden ClientSO asset dosyalarını üreten script; manşetlerin Headlines.asset dosyasına taşınması; son metinlerinin kod koşullarıyla tutarlılık düzeltmesi, EndingSO asset'leri ve "Enkazın Gölgesi" son metninin son hali; README son düzenlemesi (ekip, kabadayı, sonlar, jüri kısayolları); oyun içi Emeği Geçenler metni | `ICERIK_TASLAK.md`, `Assets/_Project/Data/Clients/*.asset`, `Assets/_Project/Data/Headlines.asset`, `Assets/_Project/Data/Endings/*.asset` | Tarık | 2026-10-16 |
| Claude Code (Anthropic) | Core veri sözleşmesi kodu, deprem simülatörü, A rolü çalışma planı | `Assets/_Project/Scripts/Core/**`, `AHMET.md`, `memory.md` | Ahmet İlman | 2026-10-16 |
| Claude Code (Anthropic) | Core Faz 1: proje üretici, afet sistemi, oyun oturumu/yöneticisi, kayıt, debug arayüzü, testler, veri asset ve Game sahnesi kurulum aracı; kurgusal isim listeleri | `Assets/_Project/Scripts/Core/**`, `Assets/_Project/Data/**`, `Assets/_Project/Scenes/Game.unity`, `AHMET.md`, `memory.md` | Ahmet İlman | 2026-10-16 |
| Claude Code (Anthropic) | Proje analizi ve yol haritası; borç/son kuralları, kayıt silme, demo tuşları, son ekranı asset'leri (metinler EEE'nin; MiddleCollapsed taslak metni), README taslağı, build ayarları ve şablon temizliği | `Assets/_Project/Scripts/Core/**`, `Assets/_Project/Data/**`, `README.md`, `memory.md`, `AHMET.md`, `Flow/UI/EndingScreen.cs` ve `ResultScreen.cs` (küçük düzeltme) | Ahmet İlman | 2026-10-17 |
| ChatGPT / Codex (OpenAI) | Gün sistemi, haftalık gider ve gün sonu kayıt kararlarıyla ortak hafızanın güncellenmesi; Onur için C rolü çalışma planı | `memory.md`, `ONUR.md`, `CREDITS_AND_AI.md` | Onur Kabadayılar | 2026-10-16 |
| Claude (Anthropic) | Masa ekranı kodu: UI script'leri, Core'a bağlantı (IGameScreen/IGameActions), sahte gün verisi ve sandbox IGameActions, prefab/sahne kurulum aracı; B iş planı | `Assets/_Project/Scripts/Desk/**`, `OGUZHAN.md` | Oğuzhan | 2026-10-16 |
| Claude (Anthropic) | Masa: El Kitabı penceresi (kontrol listesi metni), belge karşılaştırma, 1. gün öğretici kart metinleri (taslak; EEE onaylayacak); kabadayı olay kartı ve metni | `Assets/_Project/Scripts/Desk/UI/RulebookView.cs`, `TutorialView.cs`, `DocumentView.cs`, `EventCardView.cs`, `DeskScreen.cs` | Oğuzhan | 2026-10-17 |
| Claude (Anthropic) | Intro hikaye ekranı (daktilo efekti, sayfa geçişi) ve intro metinleri (17 Ekim'de Onur'a devredildi; Flow akışına o bağlar, metin Tarık'la gözden geçirilecek) | `Assets/Scripts/Intro/IntroSequence.cs` | Oğuzhan | 2026-10-16 |
| Claude (Anthropic) | Yapı Denetim Formu ve Denetçi El Kitabı tasarım görselleri ve kural tablosu (öneri; Ahmet onaylamadı, 2026-10-17 `61353fd` ile repodan kaldırıldı — `62165c0` geçmişte duruyor) | `Docs/Tasarim/*.png` (kaldırıldı), `memory.md` | Oğuzhan | 2026-10-17 |
| ChatGPT / Codex (OpenAI) | Unity arayüzünde Lit 2D (URP) deneme sahnesi oluşturma ve kayıt doğrulama; durum notları | `Assets/_Project/Scenes/Sandbox_Flow.unity`, `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Unity arayüzünde Canvas kurulumu, 1920×1080 ölçekleme ve kayıt doğrulama | `Assets/_Project/Scenes/Sandbox_Flow.unity`, `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Unity arayüzünde ResultScreen panelini oluşturma, tam ekran yerleşim ve kayıt doğrulama | `Assets/_Project/Scenes/Sandbox_Flow.unity`, `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| ChatGPT / Codex (OpenAI) | Ahmet’in günlük ödeme ve afet kapsamı kararlarını inceleme; ortak hafıza ve C planını güncelleme | `memory.md`, `ONUR.md` | Onur Kabadayılar | 2026-10-16 |
| Gemini / Antigravity (Google) | Gazete sonuç ekranı arka plan şablonu ("ŞEHİR POSTASI") görsel üretimi | `Assets/_Project/Art/newspaper_bg.png` | Onur Kabadayılar | 2026-10-16 |
| Claude Code (Anthropic) | Flow kodu: gazete, gün özeti, günlük ödeme, son ve ana menü ekranları, Flow sandbox ve prefab/sahne kurulum aracı | `Assets/_Project/Scripts/Flow/**` | Onur Kabadayılar | 2026-10-16 |

## Oyun içi "Emeği Geçenler" metni
CC BY (atıf zorunlu) asset'ler oyunun credits ekranında da görünmeli. Yeni asset eklenince bu blok da güncellenir; credits ekranı bu metni aynen kullanır.

```
İMZA
TOOADEVS · Civil Game Jam 2026

EKİP
Ahmet İlman — Oyun mantığı ve sistemler
Oğuzhan Yurt — Masa ekranı
Onur Kabadayılar — Akış ekranları ve menü
Tarık Hokkaömeroğlu — Görsel, ses ve içerik

SESLER
Ortam sesi: "Suburban House Ambience 02" — msepitaph (freesound.org), CC BY 4.0
Kağıt sesi: "paper5" — florian_reinke (freesound.org), CC0
Cam kırılma sesi: "Breaking Glass" — hifijohn (freesound.org), CC0
Menü müziği: "Ambient Paulstretched Piano" — waveplaysfx (freesound.org), CC0
Son müziği: "The Sad Piano" — dasgoat (freesound.org), CC0
Arayüz ve kaşe sesleri: Kenney (kenney.nl), CC0

GÖRSELLER
Müteahhit karakterleri: yapay zekâ (ChatGPT) ile üretildi

YAZI TİPİ
Liberation Sans — SIL Open Font License 1.1
Lilita One (logo) — Juan Montoreano, SIL Open Font License 1.1

YAPAY ZEKÂ
Bu oyunun kodunda, metinlerinde ve bazı görsellerinde yapay zekâ araçları
(Claude, ChatGPT/Codex, Gemini) kullanıldı. Ayrıntılı liste: CREDITS_AND_AI.md
```
Not: Bu blok değişince `MainMenuScreen.cs` içindeki `CreditsText` de güncellenmeli (C).
