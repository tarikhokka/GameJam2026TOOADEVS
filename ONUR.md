# Onur Kabadayılar — Kişisel çalışma planı (C / Akış)

Bu dosya Onur’un görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir. Repo ile push edildiğinde ekip tarafından da okunabilir; gizli belge değildir.

## Mevcut durum
- 2026-10-17 intro devri: Oğuzhan'ın `02d4749` kodu `.meta` GUID'i korunarak `Assets/_Project/Scripts/Flow/UI/IntroSequence.cs` konumuna alındı. MainMenu: Yeni Oyun → 6 sayfalık hikâye → Game; Devam Et doğrudan kayıtlı Game'e gider. Ayrı intro sahnesi veya Game.unity değişikliği gerekmiyor. Flow runtime asmdef'ine `Unity.InputSystem` eklendi.
- Tarık incelemesi: intro'daki sel/yangın ve sayısal bina/kayıp istatistikleri çıkarıldı; deprem ve güvenli yapı denetimi metni kullanıldı, kurgu notu eklendi. Bu metin değişikliği Tarık'ın son incelemesine açık. CREDITS intro satırının güncellemesi Oğuzhan'da; kendi Flow satırıma ekleme yapılmadı.
- Ahmet'in `efc003a` commit'i alındı: Ç2/Ç3/Ç5 uygulandı (gizli ProjectKind, gevşek roller, UsedBuildingNames ile tekrarsız bina adları); DayIntro/Construction Game'e eklendi. Ç1 el kitabı/bölge kuralları ve Ç4 tepki/hediye alanları hâlâ plan; kesin sözleşme gelmeden bunlara bağlanılmayacak. Tarık'ın `17bf7ba` görsel kararı: flat cartoon referansı, hazır asset, piksel art yok.
- Intro doğrulaması (17 Ekim): İlk Unity EditMode 13/13; Ahmet'in `efc003a` kodu alındıktan sonra son koşuda 15/15 geçti. MainMenu Play testinde 6 sayfa, daktilo efekti, fare/Space/Enter ile ilerleme ve son sayfadan Game'in 1. gününe geçiş doğrulandı; Play kapatıldı. Yerelde kayıt olmadığından Devam Et butonunun canlı testi yapılmadı; mevcut doğrudan Game yükleme yolu koddan kontrol edildi. İlk Play kontrolünden sonra `efc003a` geldi; bu commit ekranları Game'e ekliyor. Tarık: intro'nun 2. ve 3. sayfasındaki yeni metni incele. Oğuzhan: CREDITS satırındaki yolu yeni Flow konumuna güncelle; CREDITS'e tarafımdan dokunulmadı.
- Flow scriptleri ve mevcut dört ekran hazır. 2026-10-17: DayIntroScreen ve ConstructionScreen script/prefab'ları da Sandbox_Flow'a bağlandı. Gün başı testi Onur tarafından; inşaatta 0/2 onay, yeniden açılış ve devam/çift tıklama testi Codex tarafından Unity Play modunda doğrulandı. İnşaat testinde her ekran açılışı için tek Completed kaydı, hata satırı yok.
- DayIntro ve Construction prefab'ları Ahmet tarafından `efc003a` ile Game.unity'ye kuruldu; Windows build testi henüz yapılmadı. Sandbox'ta inşaat butonu hazır afet sonucuna gider; gerçek oyunda afet hesabını Core yapar.
- Sırada Save ekranı var. Core, kayıt sonucunu Flow'a iletmeli; mevcut GameManager kayıt metodunun bool sonucunu kullanmıyor. Başarı bilgisi olmadan "Kaydedildi" gösterilmeyecek.
- Kesinleşenler: level yerine gün; her gün afet olasılığı; afetsiz gün sonrası ihtimal artışı; günlük gider; gün sonunda kayıt.
- Afet olunca olasılık sıfırlanır; değerlendirilecek binalar sadece son afetten bu yana onaylananlardır. Önceki afeti atlatanlar tekrar değerlendirilmez.
- Afetsiz günlerde gazete yok. Gazete; bina durumları, tamamen kurgusal ölen isimleri, ölü/yaralı sayıları ve oyuncunun imzasının sorumluluğunu gösterecek.
- Güncel kesin kararlar: 10 gün; deprem olasılığı %20 + afetsiz gün başına %15; ek test ücreti anında düşer. Gün sonunda sonraki gün kaydedilir; Devam Et o günün başını açar; son ekranında kayıt silinir.

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
| Para, borç, ceza ve sigorta hesabı | Kalemleri ve ödeme seçeneklerini gösterme |
| Ödeme talebini doğrulama ve uygulama | Oyuncunun seçimini Core’a iletme |
| Gün ve bitiş koşulları | Core’un seçtiği ekranı sunma |
| Save/load ve kayıt durumu | Devam Et ve kayıt geri bildirimi |

Ekranlar parayı değiştirmez, gün artırmaz, afet sonucu üretmez. Flow sadece Core’a bağlıdır; Desk’e doğrudan bağımlılık eklenmez. IGameScreen sözleşmesi A ile doğrulanır.

## Nereden başlayacağım?
- [ ] A ile gün/afet sonucu, günlük gider, kayıt durumu ve ekran tamamlanma sözleşmesini netleştir.
- [ ] Gazete ve günlük hesap ekranlarının basit taslağını çiz.
- [ ] Kodlama onayından sonra Sandbox_Flow içinde gazete ekranını hazır örnek verilerle kur.
- [ ] Afetsiz gün özeti ve günlük ödeme ekranını ekle.
- [ ] Basit inşaat ve afet sunumunu ekle; fiziksel bina yıkımı gerektirme.
- [ ] MainMenu: Yeni Oyun / Devam Et; ardından üç son ekranını bağla.
- [ ] A ile entegre et; ekran olaylarının iki kez ödeme veya gün ilerlemesi yaratmadığını kontrol et.

## İlk prototip senaryoları
1. Afetsiz gün → gazetesiz gün özeti → günlük hesaplaşma → kayıt.
2. Afetli gün → afet → gazete → günlük hesaplaşma → kayıt.
3. İkinci afet → yalnızca önceki afetten sonra onaylanan binaların sonuçları gösterilir.
4. Günlük ödeme seçimi → Core'a iletilir; ekran para/borç hesabını kendisi yapmaz.

Son koşullarının akıştaki önceliği Core ile kesinleşecek. Kayıt tamamlandı sinyali gelmeden başarılı kayıt gösterme.

## Kod ile karar arasındaki geçiş
- `cedfa0d` yalnızca doküman kararıdır; günlük ödeme ve yeni afet kapsamı henüz Core'a uygulanmadı.
- Mevcut `WeeklyExpenseResult`, `PendingWeekly`, `GamePhase.WeeklyExpenses` günlük karşılıklarına A tarafından dönüştürülecek. Yeni alan isimlerini varsayıp kod yazma.
- `BuildingOutcome` içinde ölen isimleri ve ölü/yaralı sayıları henüz yok; Flow bu değerleri hesaplamaz. A'nın sözleşme güncellemesini beklerken görsel yerleşim hazırlanabilir.
- `IGameScreen` ve `IGameActions` üzerinden entegrasyon sürer. Desk artık repoda fakat kendi modelleriyle çalışıyor; Flow doğrudan Desk'e bağlanmaz.
- Sonraki iş: mevcut ResultScreen içine gazete görseli; ardından bina durumları, kayıp/yaralı bilgileri ve sorumluluk metni için yerleşim. Sonra afetsiz gün özeti ve günlük hesap ekranı.

## A’ya soracağım açık noktalar
- Ödenmeyen günlük giderlerin sonucu ve sigortanın kapsam dönemi ne?
- Gün sonu yüklemesi sonraki sabahı mı açacak? Son ekranında kayıt nasıl davranacak?
- Kota, hapis ve ofis kapanışı aynı anda tetiklenirse öncelik ne?
- Günlük ödeme ve kayıp verilerinin kesin Core alanları ne olacak?
- Afet sonrası sıfırlamanın sayısal hedefi başlangıç olasılığı mı? Denge sayıları kesinleşmedi.

## Oturum notu
- 2026-10-16 21:09 (UTC+3): Kişisel plan oluşturuldu; ortak hafıza gün sistemine uyarlandı. Kodlama başlamadı. Commit/push Onur tarafından yapılacak.

- 2026-10-16: İlk adım tamamlandı. Unity Editor üzerinden Lit 2D (URP) şablonuyla `Assets/_Project/Scenes/Sandbox_Flow.unity` oluşturuldu ve kayıt doğrulandı. Canvas/UI henüz eklenmedi.

- 2026-10-16: İkinci adım tamamlandı. Canvas + EventSystem oluşturuldu; Scale With Screen Size, 1920×1080 ve Match 0.5 kaydedilip sahne dosyasından doğrulandı. Sırada ResultScreen paneli var.

- 2026-10-16: Ahmet’in `7545c64` Core sözleşmesi alındı ve incelendi. Flow ekranları `CivilJam.Core.IGameScreen` (Phase, Show, Hide, Completed) kullanacak; gazete `GameContext.LastQuake`, haftalık hesap `PendingWeekly` okuyacak. Ödeme seçimi `Actions.SetExpensePaid` ile iletilecek. GameManager/save-load uygulaması henüz yok. `AHMET.md` içindeki denge değerleri ekip onayı bekleyen önerilerdir.

- 2026-10-16: Canvas altında ResultScreen paneli oluşturuldu. Tam ekran stretch, sıfır ofset ve sahne kaydı doğrulandı. Sırada gazete görseli var.

- 2026-10-16: `14350ec` Desk v0 ve `cedfa0d` Ahmet cevapları pullandı. Günlük ödeme, iki afet arası bina kapsamı ve gazete içeriğine göre plan güncellendi. Önceki haftalık plan tarihsel; kod dönüşümü A’da. Yerel ResultScreen korundu.

- 2026-10-16 (gece): Flow kodu yazıldı (derleme Unity'de henüz doğrulanmadı). `Scripts/Flow/`: `CivilJam.Flow.asmdef`, `UI/` (ResultScreen = gazete, DaySummaryScreen, DailyExpensesScreen, EndingScreen, MainMenuScreen, FlowScreen tabanı, UiKit), `Sandbox/FlowSandboxBootstrap`, `Editor/FlowBuilder`. Güncel Core adları: `PendingExpenses` (eski `PendingWeekly` yok), `GamePhase.DailyExpenses`. Kurulum: Unity'de menü CivilJam > Flow > 1 (prefab'lar), 2 (MainMenu sahnesi + Build Settings), 3 (Sandbox_Flow). Game.unity'ye ekleme A'da.
