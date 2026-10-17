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
- [x] Flow prefab'ları (Onur `0716dc9`) Game.unity'ye eklendi (`InstallScreens`: Desk + Result + DaySummary + DailyExpenses + Ending); sahne testi 5 fazı kontrol ediyor. Testler 10/10. (`0bebf9b`)
- **Faz 1 KAPANDI — 17 Ekim, `a7a733c`** (A kısmı `5097e93`, Flow entegrasyonu `0bebf9b`). Not: Oğuzhan'ın 00:07 "yeni kural modeli" önerisi Ahmet tarafından REDDEDİLDİ (`61353fd`): `62165c0` revert edildi, Tarık'ın `4f33750` model uyarlamaları geri alındı (credits + stil notu korundu). Mevcut Core sözleşmesi geçerli.
- [x] B ve C'ye haber ver: memory.md → "Ekibe notlar (A / Ahmet'ten)"

### Analiz sonrası düzeltmeler (17 Ekim gece) — A kısmı
Kaynak: analiz raporu (artifact). Kararlar: borç → ofis kapanır (2.500 / 2.000), İyi = hiç yıkım yok, 10 gün, fabrika ve sigorta kapsam dışı.
- [x] **K1** Kasa eksiye düşmez; yetmeyen zorunlu gider borca; borç > `DebtLimit` → `MiddleClosed`; "Borç ödemesi" kalemi (kısmen ödenebilir). Test: her şeyi reddeden oyuncu 7. gün ofisi kaybeder.
- [x] **K2** Son öncelikleri: Bad > MiddleClosed > MiddleCollapsed (yeni) > Good. `EndingType.MiddleQuota` kaldırıldı.
- [x] **K3** İlk Windows build (`Builds/Imza/Imza.exe`, 111 MB, açılış testi temiz) + `README.md` taslağı.
- [x] **K4** Build'de yedek arayüz kapalı; ekranı olmayan faz otomatik geçilir (Editor ve Development Build'de mavi kutu durur).
- [x] **Y2** 4 EndingSO (`Data/Endings`): Tarık'ın 3 metni + MiddleCollapsed taslağı. Game.unity'ye bağlı.
- [x] **Y3** Son ekranına girince kayıt silinir.
- [x] **Y5** Fabrika kapalı (Day08). **Y6** Başlangıç 2.500, onay ücreti 400-700. **O5** Sigorta kaldırıldı.
- [x] **O3** Gazetede ayakta kalan binaya sorumluluk satırı (Onur'un dosyası, memory.md'de not). **O7** F9/F10 demo tuşları. **O8** Ürün adı "Imza", şirket "TOOADEVS".
- [x] **D1** SampleScene, Welcome ve eğitim paketi kaldırıldı. **D2** ölü alanlar. **D3** `.gitignore`. **O2** yanlış saatler commit hash'iyle değiştirildi.
- [ ] Ekibe kalanlar (memory.md "Analiz sonrası işler"): Y1 öğretici + lab listesi + intro (B), Y4 Esc menüsü (C), K4 ara ekranları (C), O4 kabadayı kartı (B), O6 görsel/ses (EEE), README son hali (EEE).

### Faz 3 — İçerik ve his (Cts 14 → 20)
- [x] Denge turu: `BalanceReport` simülasyonu (6 oyuncu tipi × 300 oyun) + senaryo karşılaştırması → senaryo E seçildi (ücret 300-500, kira 600, deprem %20 +%15). JailRisk sayıları değişmedi (tek zarf = yazı-tura, yeterli).
- [ ] Vakit kalırsa: yangın ayrı afet. Yeni asset'leri bağla (`CivilJam > Core > Link Data`).
- [ ] 20:00 ikinci build.

### Oynanış testi işleri (Ahmet oynadı, 17 Ekim) — ayrıntı memory.md "🎮 Oynanış testi"
- [x] Ç5 isim çeşitliliği: `GameState.UsedBuildingNames`, ad kalıpları (Apartmanı / Sitesi X Blok / Konutları / Evleri). Oyun başına tekrar 0.
- [x] Ç2 `ProjectKind` (Compliant / Sloppy / Bribe / Fake / BribeFake); dikkatsiz proje her gün (%85-95), sahte belge 3. günden (Day03), rüşvetlilerin %25'i sahte belgeli.
- [x] Ç3 gevşek roller: uygun %80 dürüst usta; dikkatsiz %50/%50; sahte belge %70 Selim tipi. Dürüst usta asla rüşvet/sahte belge getirmez. Testler 15/15; `CivilJam > Core > Content Stats`.
- [x] Ç1 bölge + kat kuralları, 8 parametre, harfli değerler (`ParamEntry.Labeled`), el kitabı verisi (`HandbookDataBuilder`, 16 mahalle, 60 kural), üretici (mahalle → bölge, 5-6 parametre), testler 19/19, simülasyon (denge korunuyor). Gazete yıkım nedeni zaten `Rules`'tan ad okuyor.
- [x] Ç4 `ClientReactions` (selamlama/tepki seçimi, hediye çıkışı, zarf artışı), `GameState.ClientHistory` (`ClientRecord`), Nuri `BribeRaiseOnReject` 250. Testler 23/23.
- [x] El kitabı v2 (Ahmet kararı): mahalle/bölge kaldırıldı, 5 parametre × 3 kat grubu (15 kural), her projede 5 parametre, binalar 2-9 kat, seçenekli kademe cezası %25 (simülasyonla). Testler 23/23.
- [x] Onur'un DayIntro / Construction ekranları (`8c05324`) Game.unity'de; sahne testi 7 fazı kontrol ediyor.

### Faz 4-6 — MVP, cila, teslim (Cts 20 → Paz 18)
- [ ] Baştan sona oynanış, 4 son erişilebilir (F9/F10 ile de).
- [ ] Paz 12:00 freeze → final build, ZIP, video, README, yükleme.

## Onur'un sorularına cevaplar (ONUR.md "A'ya soracağım açık noktalar")
Ahmet tarafından 2026-10-16 22:00 civarı cevaplandı.

| # | Soru | Cevap | Durum |
|---|---|---|---|
| 1 | Afet sonucu hangi binaları içeriyor? | İki afet arasında oyuncunun onayladığı binalar: son afetten bu yana onaylananlar. Daha önceki afetleri atlatmış binalar tekrar değerlendirilmez. | ✅ Karar |
| 2 | Gazete / gün özetinde ne gösterilecek? | Afetsiz günün sonunda gazete **gösterilmez**. Afet olunca şimdilik: her binanın yıkılıp yıkılmadığı, ölenlerin isimleri, ölü ve yaralı sayısı, binanın yıkılmasından ya da ayakta kalmasından oyuncunun sorumlu olduğu bilgisi. | ✅ Karar |
| 3 | Ödeme günü ve ödenmeyen giderlerin sonucu? | Ödeme **günlük**. Yetmeyen zorunlu gider borca yazılır; borç 2.000 TL'yi aşarsa ofis kapanır (Orta son). | ✅ Karar (17 Ekim) |
| 4 | Afet olunca olasılık sıfırlanıyor mu? | Evet. | ✅ Karar |
| 5 | Gün sonu yüklemesi / son ekranında kayıt? | Yükleme yeni günün başını açar; son ekranına girince kayıt silinir. | ✅ Karar (17 Ekim) |
| 6 | Kota, hapis ve ofis kapanışı aynı anda tetiklenirse öncelik? | Kota yok. Öncelik: Hapis > Ofis kapandı > Yıkım > İyi. | ✅ Karar (17 Ekim) |

Ek kararlar (Ahmet, 22:10): deprem animasyonu/sahnesi yok; sadece gazete (afet günü) veya gün özeti (afetsiz gün) gösterilir. Afet olasılığı tabanı **%50**, tavanı **%100**.

Kesinleşenler (17 Ekim): 10 gün, kota yok, ek test parası anında düşer. Hâlâ öneri: afetsiz gün +%10 artış. Değerler `BalanceSO`'da.

### Kurallar kodda nerede (geçici kural kalmadı)
- Giderler ve borç: `ExpenseCalculator` (Build / Apply).
- Sonlar ve öncelik: `GameSession.EvaluateEnding`.
- Kayıt: `GameManager.Enter` (Save fazında gün +1 ve kayıt, Ending fazında silme).
- Sayılar: `Data/Balance.asset`.

## B ve C için Core kullanım rehberi
- Ekran = sahnede bir MonoBehaviour + `IGameScreen`. `Phase` hangi fazda gösterileceğini söyler; GameManager sahnedeki ekranları kendisi bulur. Bir faz için ekran yoksa Editor'de IMGUI yedek arayüz çıkar, gerçek build'de faz atlanır.
- `Show(GameContext ctx)` içinde okunacaklar:
  - Desk: `ctx.TodayProjects` (Params: `Reported` göster, `Tested` ise `Actual` da; `Documents[0]` başvuru, `Documents[1]` lab raporu; `OffersBribe`/`BribeAmount`), `ctx.Rules` (yönetmelik kitapçığı), `ctx.Balance.ExtraTestCost`.
  - Result (gazete): `ctx.LastQuake.Outcomes` → `BuildingName`, `Collapsed`, `Deaths`, `Injured`, `VictimNames`, `ResponsibilityText`, `WasBribed`, `FailedParams`; toplamlar `TotalDeaths` / `TotalInjured`; `Jailed`.
  - DaySummary: `ctx.State` (Day, Money, Debt, DisasterChance), `ctx.TodayProjects` kararları.
  - DailyExpenses: `ctx.PendingExpenses.Lines` (DisplayName, Amount, Mandatory, Paid). Borç varsa `ExpenseId == "debt_payment"` kalemi gelir. Core kasa yetmeyen kalemi ödemez; ekrandaki "ödeme sonrası kasa" sadece önizleme.
  - Ending: `ctx.State.Ending` (Good / Bad / MiddleClosed / MiddleCollapsed) + `ctx.Endings` (EndingSO).
- Yazma sadece `ctx.Actions`: `Approve(id, acceptBribe)`, `Reject(id)`, `BuyExtraTest(id)` → bool, `SetExpensePaid(expenseId, paid)`. Aynı projeye ikinci karar etkisizdir.
- HUD: `ctx.StateChanged` olayına abone ol.
- MainMenu "Devam Et": `GameLaunch.ContinueFromSave = true;` sonra Game sahnesini yükle. Kayıt var mı: `SaveSystem.HasSave`.

## Sözleşme değişiklikleri
| Tarih-saat | Değişiklik | Etkilenen |
|---|---|---|
| 2026-10-16 21:20 | İlk sözleşme (gün sistemi adlarıyla) | B, C |
| 2026-10-16 22:00 | PLANLI: günlük ödeme yeniden adlandırması, afet kapsamı, BuildingOutcome kayıp verisi | B, C |
| 2026-10-16 22:15 | UYGULANDI: `WeeklyExpenseResult` → `DailyExpenseResult`; `GameContext.PendingWeekly` → `PendingExpenses`; `IsPaymentDay`/`Week`/`BalanceSO.DaysPerWeek` silindi; `GamePhase.Quake` silindi, `WeeklyExpenses` → `DailyExpenses`; `GameState`: `LastDisasterDay`, `ApprovedToday`, `WindowRepairsToday`, `TotalDeaths`, `TotalInjured`, `Jailed`; `BuildingOutcome`: `Address`, `DayApproved`, `Residents`, `Deaths`, `Injured`, `VictimNames`, `ResponsibilityText`; `QuakeResult.TotalDeaths/TotalInjured/ChanceBefore`; yeni `NamePoolSO`, `GameContext.Names`, `GameLaunch`, `SaveSystem`, `GameSession`, `GameManager` | B, C |
| `5097e93` | `ClientSO.OffersBribes` (dürüst değil + BribeRange > 0) | EEE, B |
| 17 Ekim analiz | `EndingType.MiddleQuota` → `MiddleCollapsed`; `ExpenseCalculator.DebtPaymentId` / `WindowRepairId`; `GameManager.demoKeys` (F9/F10); `BalanceSO` varsayılanları (10 gün, 2.500, 2.000, 400-700) | C (EndingScreen güncellendi), B |
| Oynanış testi Ç2/Ç3/Ç5 | Yeni `ProjectKind` enum + `ProjectData.Kind` (gizli, gösterilmez); `GameState.UsedBuildingNames`; `ProjectGenerator.Generate(..., usedNames)` | B (sandbox verisi isterse Kind kullanabilir) |
| Ç1/Ç4 sözleşme iskeleti | `RuleSetSO` (HandbookIntro, Zones/ZoneEntry, ZoneOf, RulesFor(tür,bölge,kat), FindRule, FormatValue, DisplayNameOf); `ParamRule` (Zone, MinFloors, MaxFloors, Labels, Description, IsLabeled); `ParamType` +5; `ProjectData` (Neighborhood, Zone, GreetingText, ReactionText, GiftId); `GiftSO`; `ClientSO` (Approve/Reject/BribeRefused/ReturnLines, Gift, GiftChance, BribeRaiseOnReject); `GameState.OfficeGifts`; `GameContext.Gifts`/`GetGift`; `IGameActions.AcceptGift` | B, C (sandbox `AcceptGift` eklendi), EEE |
| Ç1 mantığı | `ParamEntry.Labeled` (harfli parametre; değer = etiket sırası); `ProjectGenerator` artık `RuleSetSO.Zones` ve `RulesFor(tür, bölge, kat)` kullanıyor | B (`FormatValue` ile göster) |
| Ç4 mantığı | `ClientRecord` + `GameState.ClientHistory`; `ClientReactions.RecordOf(state, id)` (okuma) | B ("tekrar geldi" göstergesi isterse) |
| El kitabı v2 | KALDIRILDI: `RuleSetSO.Zones`/`ZoneEntry`/`ZoneOf`, `ParamRule.Zone`, `ProjectData.Neighborhood`/`Zone`. DEĞİŞTİ: `RulesFor(tür, kat)`, `FindRule(tür, parametre, kat)`. YENİ: `ParamRule.FloorsLabel`, `ParamType` ColumnThickness/FoundationDepth/RebarType/SoilType, `ParamEntry.LabelStepPenalty` | B (el kitabı tablosu), EEE (metinler) |

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
- (`0916894` öncesi) Ahmet'in Unity testi: gün döngüsü, gazete verisi, dürüst bina kuralı ve save.json doğrulandı. Sorun: Flow (Onur, `ebe816d`) prefab'ları üretilmemiş → Game.unity'de gazete yok, mavi yedek arayüz. Kararlar: Flow prefab'larını Onur üretip push'layacak (A sonra Game.unity'ye ekler); DayIntro/Construction/Save ekranları Onur'a verildi. Not: memory.md → Onur maddeleri.
- `0bebf9b` / `a7a733c` — Flow ekranları Game.unity'de; `LinkData` artık `Data/Endings/` EndingSO'larını da bağlar (Tarık'a not). memory.md temizlendi: Durum yeniden yazıldı (Bilinen sorunlar: EndingSO yok, Unity 6.5 uyarısı, Game.unity'yi Editor'den kaydetmeme), eski entegrasyon bölümü silindi, Onur listesi tek öncelikli listeye indi, dosya haritası tamamlandı.
  - Pull'u engelleyen Unity oto-değişiklikleri stash'lendi (`Game.unity`'de DeskScreen anchor'larını 0'layan kayıt dahil). Game.unity'yi Editor'den kaydetme.

## Oturum notu
- 2026-10-16 21:20 (UTC+3): AHMET.md oluşturuldu. Faz 0: Core veri sözleşmesi yazıldı, Unity'de hatasız derlendi, push'landı. Desk (B) kendi modelleriyle ilerliyor; Core'a adapter ile bağlanacak.
- 2026-10-16 22:00 (UTC+3): Onur'un 6 sorusundan 4'ü cevaplandı (5 ve 6 açık; ödenmeyen gider sonucu açık). Ödeme günlüğe çekildi. Faz 1'e henüz başlanmadı.
- 2026-10-16 22:15 (UTC+3): Faz 1 kodu tamam, derleme uyarısız, 7/7 test geçti. Game.unity yedek arayüzle baştan sona oynanabilir durumda (Editor'de elle henüz denenmedi). Sırada: elle oynanış kontrolü, Desk'in Core'a bağlanması (B), Flow ekranları (C).
- 17 Ekim gece (analiz sonrası): Kritik grup (K1-K4) ve Core'a düşen yüksek/orta/düşük maddeler kapatıldı; ilk Windows build alındı; testler 13/13. Ekibe kalanlar memory.md "Analiz sonrası işler"de.
