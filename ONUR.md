# Onur Kabadayılar — Kişisel çalışma planı (C / Akış)

Bu dosya Onur’un görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir. Repo ile push edildiğinde ekip tarafından da okunabilir; gizli belge değildir.

## Mevcut durum
- (18 Ekim, **belge turu C1**) `StampTrayView` hazır: T1 görselleriyle kendini kuran sola açılan çekmece, ONAY/RED sürükle-bas, temas anında tek olay, yanlış bırakmada geri dönüş, izin/girdi kilidi ve kapatma/gizleme iptalleri. Taban `2e8643d` (A1 dahil), izole Unity 6000.6.4f1'de **58/58 EditMode + 13/13 PlayMode** (4 mevcut + 9 geçici C1 kabulü), gerçek URP görsel kontrolü geçti. Rehber: `Docs/Onur/StampTrayView_baglanti.md`. C2 henüz başlamadı; Faz 2 kapısı ve Oğuzhan'ın B2 bağlantısı bekleniyor. Prefab, builder, Sandbox_Desk ve Game sahnesi değiştirilmedi; Tarık'ın işleri devralınmadı.
- (18 Ekim, poliş; `a4da352`) Onur'un son düzeltmesiyle Tarık'ın T1/T2 işleri devralınmıyor. Bizim kapsamımız C1 üst bar ve C2 ana menü. Tarık görsel/metin teslimlerini tamamladı (`f8842b5`, `1dd7870`). C1 `1b47ce6` hazır: 80 px DeskHeaderView, üç API, T1 görünümü ve görselsiz yedek. 55/55 EditMode + 10/10 PlayMode ve Unity render kontrolü geçti. Bağlantı rehberi `Docs/Onur/DeskHeaderView_baglanti.md`. C2 ve C1-K `d2dddb9` ile tamam: Paytone One ile ortalanmış kayıtlı/kayıtsız menü, aynı stilde Geri; gerçek Sandbox_Desk'te kaşe/balon karartmanın üstünde. B2 `1c652eb` dahil birleşik sürümde 55/55 EditMode + 10/10 PlayMode geçti. Kabul/görüntüler: `Docs/Onur/Faz2_kabul_notlari.md`. Tarık'a menü dolgu izi rötuş notu bırakıldı. A3 son entegrasyon/build Ahmet'te. Plan: `Docs/Onur/Polis_turu_yol_haritasi.md`.
- (7 günlük içerik hazır, Core `e79203b`) Onur bu pakette Tarık'ın görevlerini devraldı. Sağladığı son 10 ChatGPT JPEG'i 1000×1000 şeffaf PNG'ye hazırlandı; g01/g04 ikilileri tek müşteri. On Generic ClientSO portrelere bağlı; yedi havuzda 42 ortak replik, README 7 gün, Art README/CREDITS güncel. İzole Unity 6000.6.4f1 kopyasında 52 mevcut + 2 geçici gerçek içerik kontrolü = 54/54 EditMode geçti; 300 gerçek kadro programı ve kayıt roundtrip'i doğrulandı. Game.unity kaydedilmedi; Ahmet Link Data + final oynanış/build, Oğuzhan masa yerleşimi yapacak. Dosya eşlemesi ve önizleme: `Docs/Onur/YediGun_icerik_yol_haritasi.md`.
- (17 Ekim, diyalog Core'u `8772f57` sonrası) Yeni `Desk/UI/InspectorBubbleView.cs` hazır: Show/Hide/IsTyping/CompleteTyping, DENETÇİ etiketi, 40 harf/sn, yazı sesi ve güvenli tıklama/iptal. Altı balon + üç mevcut Esc testiyle 9/9 PlayMode ve Unity render kontrolü geçti. Oğuzhan'ın masa/diyalog bağlantısı bekliyor; rehber `Docs/Onur/InspectorBubbleView_baglanti.md`. Önceki kaşe bağlantısı `c3bb9f8` ile tamamlandı ve Ahmet `d159506` notunda Game/build kontrolünü kaydetti.
- (17 Ekim, Core `78a847c` sonrası) İnceleme sonucu kaşesi `Scripts/Desk/UI/InspectResultView.cs` hazır: gerçek Core sonucu, üç renk, animasyon, ses alanları, güvenli Show/Hide. 4/4 ayrı PlayMode + mevcut 39/39 EditMode testi ve Unity görsel kontrolü geçti. Oğuzhan'ın prefab/etkileşim bağlantısı bekliyor; rehber `Docs/Onur/InspectResultView_baglanti.md`.
- (A, 17 Ekim ~17:30 temizledi; eski hâli `f14d2ed`.) Flow ekranlarının hepsi Game.unity'de: gün başı, inşaat, gazete, gün özeti, günlük ödeme, kayıt (Tarık yazdı), son; ana menü (Tarık yeni görsele uyarladı), hikâye intro'su, Esc menüsü. Yeni hedef arayüz referansı (`Docs/Tasarim/hedef_arayuz.png`) Onur'un.
- Açık işler: memory.md §7 "Onur (C)" — `hedef_arayuz.png` için CREDITS satırı (jam kuralı), ödeme ekranında `ExpenseCalculator.Preview`, kayıt ekranında `ctx.LastSave` + `RetrySave`, müzikler (`mus_menu`, `mus_ending_sad`) bağlı değil.

## Benim alanım
- `Assets/_Project/Scripts/Flow/`
- `Assets/_Project/Prefabs/Flow/`
- `Sandbox_Flow` ve `MainMenu` sahneleri.
- Gün özeti, inşaat/afet sunumu, gazete, günlük hesaplaşma, ana menü ve son ekranları.
- `Game` sahnesinin sahibi A (Ahmet İlman). Desk sahne/prefab sahibi B. Başkasının sahne/prefab dosyasını açıp kaydetme.

## Sorumluluk sınırı
| Core / A | Ben / C |
|---|---|
| Afet olasılığı ve bina sonucu hesaplama | Hazır sonucu görselleştirme |
| Para, borç ve ceza hesabı | Kalemleri ve ödeme seçeneklerini gösterme |
| Ödeme talebini doğrulama ve uygulama | Oyuncunun seçimini Core’a iletme |
| Gün ve bitiş koşulları | Core’un seçtiği ekranı sunma |
| Save/load ve kayıt durumu | Devam Et ve kayıt geri bildirimi |

Ekranlar parayı değiştirmez, gün artırmaz, afet sonucu üretmez. Flow sadece Core’a bağlıdır; Desk’e doğrudan bağımlılık eklenmez. IGameScreen sözleşmesi A ile doğrulanır.

## A’ya soracağım açık noktalar
- Hepsi cevaplandı: kurallar memory.md §2, Core kullanımı AHMET.md "B ve C için Core kullanım rehberi". (Eski liste `f14d2ed`'de.)

## Oturum notu
- 2026-10-16 21:09 (UTC+3): Kişisel plan oluşturuldu; ortak hafıza gün sistemine uyarlandı. Kodlama başlamadı. Commit/push Onur tarafından yapılacak.

- 2026-10-16: İlk adım tamamlandı. Unity Editor üzerinden Lit 2D (URP) şablonuyla `Assets/_Project/Scenes/Sandbox_Flow.unity` oluşturuldu ve kayıt doğrulandı. Canvas/UI henüz eklenmedi.

- 2026-10-16: İkinci adım tamamlandı. Canvas + EventSystem oluşturuldu; Scale With Screen Size, 1920×1080 ve Match 0.5 kaydedilip sahne dosyasından doğrulandı. Sırada ResultScreen paneli var.

- 2026-10-16: Ahmet’in `7545c64` Core sözleşmesi alındı ve incelendi. Flow ekranları `CivilJam.Core.IGameScreen` (Phase, Show, Hide, Completed) kullanacak; gazete `GameContext.LastQuake`, haftalık hesap `PendingWeekly` okuyacak. Ödeme seçimi `Actions.SetExpensePaid` ile iletilecek. GameManager/save-load uygulaması henüz yok. `AHMET.md` içindeki denge değerleri ekip onayı bekleyen önerilerdir.

- 2026-10-16: Canvas altında ResultScreen paneli oluşturuldu. Tam ekran stretch, sıfır ofset ve sahne kaydı doğrulandı. Sırada gazete görseli var.

- 2026-10-16: `14350ec` Desk v0 ve `cedfa0d` Ahmet cevapları pullandı. Günlük ödeme, iki afet arası bina kapsamı ve gazete içeriğine göre plan güncellendi. Önceki haftalık plan tarihsel; kod dönüşümü A’da. Yerel ResultScreen korundu.

- 2026-10-16 (gece): Flow kodu yazıldı (derleme Unity'de henüz doğrulanmadı). `Scripts/Flow/`: `CivilJam.Flow.asmdef`, `UI/` (ResultScreen = gazete, DaySummaryScreen, DailyExpensesScreen, EndingScreen, MainMenuScreen, FlowScreen tabanı, UiKit), `Sandbox/FlowSandboxBootstrap`, `Editor/FlowBuilder`. Güncel Core adları: `PendingExpenses` (eski `PendingWeekly` yok), `GamePhase.DailyExpenses`. Kurulum: Unity'de menü CivilJam > Flow > 1 (prefab'lar), 2 (MainMenu sahnesi + Build Settings), 3 (Sandbox_Flow). Game.unity'ye ekleme A'da.
