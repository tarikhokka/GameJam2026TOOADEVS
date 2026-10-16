# Ahmet İlman — Kişisel çalışma planı (A / Core)

Bu dosya Ahmet'in görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir; bu dosya ortak kararları değiştirmez. Repo ile push edildiğinde ekip ve ajanları da okuyabilir; gizli belge değildir.

## Ajanlar için hızlı özet
- Core = saf C# oyun mantığı + veri sözleşmesi. Desk (B) ve Flow (C) sadece `CivilJam.Core`'a bağlanır.
- Core tipleri `Assets/_Project/Scripts/Core/` altında, namespace `CivilJam.Core`.
- Ekranlar parayı, günü, afet sonucunu değiştirmez; oyuncu seçimini `GameContext.Actions` (`IGameActions`) üzerinden Core'a iletir, işini bitirince `Completed` olayını tetikler.
- Core imzası (public tip/alan/metot) değişirse: önce bu dosyadaki "Sözleşme değişiklikleri" bölümüne yaz, sonra ekibe haber ver.

## Benim alanım
- `Assets/_Project/Scripts/Core/`, `Assets/_Project/Data/` (SO asset'leri; içerik dolumu 4. kişiyle ortak).
- `Scenes/Game.unity` sadece ben düzenlerim.
- Afet olasılığı ve bina sonucu, para/borç/ceza/sigorta, gün/hafta/bitiş koşulları, save/load.

## Roadmap (gün sistemine göre)
### Faz 0 — Veri sözleşmesi (Cum → 22:00) ⚠️ B ve C bunu bekliyor
- [x] `CivilJam.Core` asmdef
- [x] Enum'lar, runtime tipler (ProjectData, DocumentData, ParamEntry, GameState, GameContext, QuakeResult, WeeklyExpenseResult → Faz 1'de DailyExpenseResult)
- [x] `IGameScreen`, `IGameActions`
- [x] SO sınıfları: DayConfigSO, RuleSetSO, ClientSO, ExpenseSO, BalanceSO, HeadlineSO, EndingSO
- [x] EarthquakeSimulator (yıkılma kuralı)
- [x] Unity'de derle (hatasız), .meta'ları üret, commit + push
- [x] Ekibe haber ver (memory.md → "Ekibe notlar")
- [x] `memory.md` dosya haritasını doldur

### Faz 1 — Dikey dilim v0 (Cum 22 → Cts 02)
- [x] Sözleşme kararlara uyarlandı: günlük ödeme, afet kapsamı, kayıp verisi, Quake fazı kaldırıldı
- [x] ProjectGenerator v0 (seed'li; düz uygunsuz / rüşvetli / sahte belgeli; fabrika DayConfig ile)
- [x] DisasterSystem: taban %50, afetsiz gün +%10, tavan %100, afette tabana döner
- [x] GameSession (saf C#, IGameActions) + GameManager (MonoBehaviour, faz sırası)
- [x] Game.unity + IMGUI yedek arayüz (ekranı olmayan fazlar için) + Data asset'leri (Editor menüsüyle)
- [x] EditMode testleri (7/7 geçti): Strength >= 1 asla yıkılmaz, afet kapsamı, olasılık, üretici, çift ödeme
- [x] Unity Editor'de Game.unity'yi elle oynayıp kontrol et (Ahmet, 00:30: gün döngüsü, gazete verisi, dürüst bina, save.json ✅)
- [x] Müteahhit rolleri: uygun → dürüst, rüşvetli → `ClientSO.OffersBribes`, diğer uygunsuz → rüşvet vermeyen (Selim); kabadayı sadece AllowThugs günleri. Tarık'ın 6 ClientSO'su Game.unity'ye bağlandı (`CoreSetupBuilder.LinkData`). Testler 10/10.
- [ ] (C'ye bağlı) Flow prefab'larını Game.unity'ye ekle — Onur prefab'ları + Flow .meta'larını push'layınca `InstallScreens`.
- **Faz 1 (A kısmı) KAPANDI — 2026-10-17 00:45.** Not: Oğuzhan'ın 00:07 "yeni kural modeli" önerisi Core'un büyük kısmını değiştirir; Ahmet'in onayı bekleniyor.
- [x] B ve C'ye haber ver: memory.md → "Ekibe notlar (A / Ahmet'ten)"

### Faz 2 — Ekonomi ve hile (Cts 08 → 14)
- [ ] EconomyCalculator: günlük giderler, borç, ofis kapanışı
- [ ] Rüşvet + sahte belge (bedava ipucu + ücretli ek test), JailRisk
- [ ] Kota kontrolü

### Faz 3 — İçerik mantığı (Cts 14 → 20)
- [ ] Fabrika/yangın; yıkım nedeni şablon cümleleri (manşet seçimi Flow'da yapıldı — ResultScreen, Tarık'ın Headlines.asset'i)
- [ ] EndingResolver (İyi / Kötü / Orta); Save/Load JSON
- [ ] Gizli gün atlama kısayolu

### Faz 4-5 — MVP, denge, build (Cts 20 → Paz 12 freeze)
- [ ] Baştan sona oynanış, 3 son erişilebilir; denge BalanceSO'dan
- [ ] Windows build (Game.unity sahibi olarak)

## Onur'un sorularına cevaplar (ONUR.md "A'ya soracağım açık noktalar")
Ahmet tarafından 2026-10-16 22:00 civarı cevaplandı.

| # | Soru | Cevap | Durum |
|---|---|---|---|
| 1 | Afet sonucu hangi binaları içeriyor? | İki afet arasında oyuncunun onayladığı binalar: son afetten bu yana onaylananlar. Daha önceki afetleri atlatmış binalar tekrar değerlendirilmez. | ✅ Karar |
| 2 | Gazete / gün özetinde ne gösterilecek? | Afetsiz günün sonunda gazete **gösterilmez**. Afet olunca şimdilik: her binanın yıkılıp yıkılmadığı, ölenlerin isimleri, ölü ve yaralı sayısı, binanın yıkılmasından ya da ayakta kalmasından oyuncunun sorumlu olduğu bilgisi. | ✅ Karar |
| 3 | Ödeme günü ve ödenmeyen giderlerin sonucu? | **ÖNEMLİ:** ödeme haftalık değil **günlük** olacak. Ödenmeyen giderlerin sonucu henüz cevaplanmadı. | ✅ Günlük ödeme / ❓ ödenmeyen gider |
| 4 | Afet olunca olasılık sıfırlanıyor mu? | Evet. | ✅ Karar |
| 5 | Gün sonu yüklemesi / son ekranında kayıt? | Şimdilik cevaplanmadı. | ❓ Açık |
| 6 | Kota, hapis ve ofis kapanışı aynı anda tetiklenirse öncelik? | Şimdilik cevaplanmadı. | ❓ Açık |

Ek kararlar (Ahmet, 22:10): deprem animasyonu/sahnesi yok; sadece gazete (afet günü) veya gün özeti (afetsiz gün) gösterilir. Afet olasılığı tabanı **%50**, tavanı **%100**.

Hâlâ öneri (onay bekliyor): afetsiz gün +%10 artış; toplam 14 gün; ek test parası anında düşülür. Kota yok (karar, 22:40). Değerler `BalanceSO`'da.

### GEÇİCİ kurallar (kararlar netleşene kadar kodda böyle; tek yerde değişir)
- Ödenmeyen zorunlu gider borca eklenir (`ExpenseCalculator.Apply`). Ofis kapanışı yok.
- Sonlar (`GameSession.EvaluateEnding`): hapis → Kötü; son güne ulaşınca → İyi. Orta sonlar (ofis/kota) yok.
- Kayıt: günlük ödemeden sonra gün +1 yapılıp kaydedilir; "Devam Et" yeni günün DayIntro'sunu açar. Son ekranında kayıt silinmiyor.
- Kota yok (karar). `BalanceSO.ApprovalQuota` silindi (23:20).

## B ve C için Core kullanım rehberi
- Ekran = sahnede bir MonoBehaviour + `IGameScreen`. `Phase` hangi fazda gösterileceğini söyler; GameManager sahnedeki ekranları kendisi bulur. Bir faz için ekran yoksa IMGUI yedek arayüz çıkar.
- `Show(GameContext ctx)` içinde okunacaklar:
  - Desk: `ctx.TodayProjects` (Params: `Reported` göster, `Tested` ise `Actual` da; `Documents[0]` başvuru, `Documents[1]` lab raporu; `OffersBribe`/`BribeAmount`), `ctx.Rules` (yönetmelik kitapçığı), `ctx.Balance.ExtraTestCost`.
  - Result (gazete): `ctx.LastQuake.Outcomes` → `BuildingName`, `Collapsed`, `Deaths`, `Injured`, `VictimNames`, `ResponsibilityText`, `WasBribed`, `FailedParams`; toplamlar `TotalDeaths` / `TotalInjured`; `Jailed`.
  - DaySummary: `ctx.State` (Day, Money, Debt, DisasterChance), `ctx.TodayProjects` kararları.
  - DailyExpenses: `ctx.PendingExpenses.Lines` (DisplayName, Amount, Mandatory, Paid).
- Yazma sadece `ctx.Actions`: `Approve(id, acceptBribe)`, `Reject(id)`, `BuyExtraTest(id)` → bool, `SetExpensePaid(expenseId, paid)`. Aynı projeye ikinci karar etkisizdir.
- HUD: `ctx.StateChanged` olayına abone ol.
- MainMenu "Devam Et": `GameLaunch.ContinueFromSave = true;` sonra Game sahnesini yükle. Kayıt var mı: `SaveSystem.HasSave`.

## Sözleşme değişiklikleri
| Tarih-saat | Değişiklik | Etkilenen |
|---|---|---|
| 2026-10-16 21:20 | İlk sözleşme (gün sistemi adlarıyla) | B, C |
| 2026-10-16 22:00 | PLANLI: günlük ödeme yeniden adlandırması, afet kapsamı, BuildingOutcome kayıp verisi | B, C |
| 2026-10-16 22:15 | UYGULANDI: `WeeklyExpenseResult` → `DailyExpenseResult`; `GameContext.PendingWeekly` → `PendingExpenses`; `IsPaymentDay`/`Week`/`BalanceSO.DaysPerWeek` silindi; `GamePhase.Quake` silindi, `WeeklyExpenses` → `DailyExpenses`; `GameState`: `LastDisasterDay`, `ApprovedToday`, `WindowRepairsToday`, `TotalDeaths`, `TotalInjured`, `Jailed`; `BuildingOutcome`: `Address`, `DayApproved`, `Residents`, `Deaths`, `Injured`, `VictimNames`, `ResponsibilityText`; `QuakeResult.TotalDeaths/TotalInjured/ChanceBefore`; yeni `NamePoolSO`, `GameContext.Names`, `GameLaunch`, `SaveSystem`, `GameSession`, `GameManager` | B, C |

## Yapılan değişiklikler günlüğü
- `7545c64` — Core Faz 0 push'u: `Assets/_Project/Scripts/Core/**` (+ .meta), `AHMET.md`, `memory.md` (dosya haritası, durum, karar satırı), `CREDITS_AND_AI.md` (Claude Code satırı).
  - Rebase sırasında `memory.md` ve `CREDITS_AND_AI.md`'de çakışma çıktı; iki tarafın içeriği de korunarak çözüldü (B'nin Desk dosya haritası ve kararları + Tarık'ın placeholder satırı kaldı).
  - Commit'lenmeyenler: `.vsconfig`, Unity'nin kendiliğinden değiştirdiği `ProjectSettings/Packages/com.unity.learn.iet-framework/Settings.json`.
- 22:00 — Onur'un sorularına cevaplar ve günlük ödeme kararı `AHMET.md` + `memory.md`'ye işlendi. Kod değişmedi.
- 22:15 — Faz 1: yukarıdaki sözleşme değişiklikleri + `Simulation/` (ProjectGenerator, DisasterSystem, CasualtyGenerator, ExpenseCalculator), `Game/` (GameSession, GameManager, FallbackGui, SaveSystem, GameLaunch), `Util/RandomExtensions`, `Editor/CoreSetupBuilder` (menü: CivilJam > Core), `Tests/Editor/CoreRulesTests`.
  - Üretilenler: `Assets/_Project/Data/` (Balance, RuleSet, NamePool, Headlines, Days/Day01-03-05-08, Expenses/Rent-Loan-Insurance), `Assets/_Project/Scenes/Game.unity`, `EditorBuildSettings`'e Game.unity eklendi.
  - Commit'lenmeyenler: `.vsconfig`, `ProjectSettings/SceneTemplateSettings.json` (Unity kendiliğinden üretti).
- 22:20 — `memory.md`'ye "Ekibe notlar (A / Ahmet'ten)" bölümü: Oğuzhan ve Onur için yapılacaklar listesi (ajanları her oturumda okur).
- 22:50 — Kota kaldırıldı (karar A). Kaşe düzeltmesi: `StampId` artık okunabilir metin (`STAMP_OK/FAKE` sızıntısı giderildi); WrongStamp = kaşedeki lab adı başlıkla uyuşmaz; WrongSignature = lab raporunu farklı mühendis imzalar. DeskScreen (`b614831`) Game.unity'ye eklendi (`CoreSetupBuilder.InstallScreens`, Canvas + EventSystem). Testler 9/9. `BalanceSO.ApprovalQuota` Desk kullandığı için şimdilik duruyor (Oğuzhan kaldırınca silinecek).
  - Commit'lenmeyen: `ProjectSettings/ProjectVersion.txt` (Unity 6000.6.5f1 ile açılınca yükseltilmişti, geri alındı; ekip sürümü 6000.6.4f1).
- 23:20 — TMP Essential Resources import edilip commit'lendi (`Assets/TextMesh Pro`, masa yazıları için gerekliydi; CREDITS'e eklendi). Oğuzhan HUD'dan kotayı kaldırdı → `BalanceSO.ApprovalQuota` silindi.
- 00:30 — Ahmet'in Unity testi: gün döngüsü, gazete verisi, dürüst bina kuralı ve save.json doğrulandı. Sorun: Flow (Onur, `ebe816d`) prefab'ları üretilmemiş → Game.unity'de gazete yok, mavi yedek arayüz. Kararlar: Flow prefab'larını Onur üretip push'layacak (A sonra Game.unity'ye ekler); DayIntro/Construction/Save ekranları Onur'a verildi. Not: memory.md → Onur maddeleri.
  - Pull'u engelleyen Unity oto-değişiklikleri stash'lendi (`Game.unity`'de DeskScreen anchor'larını 0'layan kayıt dahil). Game.unity'yi Editor'den kaydetme.

## Oturum notu
- 2026-10-16 21:20 (UTC+3): AHMET.md oluşturuldu. Faz 0: Core veri sözleşmesi yazıldı, Unity'de hatasız derlendi, push'landı. Desk (B) kendi modelleriyle ilerliyor; Core'a adapter ile bağlanacak.
- 2026-10-16 22:00 (UTC+3): Onur'un 6 sorusundan 4'ü cevaplandı (5 ve 6 açık; ödenmeyen gider sonucu açık). Ödeme günlüğe çekildi. Faz 1'e henüz başlanmadı.
- 2026-10-16 22:15 (UTC+3): Faz 1 kodu tamam, derleme uyarısız, 7/7 test geçti. Game.unity yedek arayüzle baştan sona oynanabilir durumda (Editor'de elle henüz denenmedi). Sırada: elle oynanış kontrolü, Desk'in Core'a bağlanması (B), Flow ekranları (C).
