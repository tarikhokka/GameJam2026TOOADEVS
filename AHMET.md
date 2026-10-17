# Ahmet İlman — Kişisel çalışma planı (A / Core)

Ahmet'in görev takibi. Ortak kararlar `memory.md`'dedir; bu dosya onları değiştirmez. Repo ile push edilir, ekip ve ajanları okuyabilir. (17 Ekim ~17:30 temizlendi; eski hâli `f14d2ed`.)

## Ajanlar için hızlı özet
- Core = saf C# oyun mantığı + veri sözleşmesi (`Assets/_Project/Scripts/Core/`, namespace `CivilJam.Core`). Desk (B) ve Flow (C) sadece Core'a bağlı.
- Ekranlar parayı, günü, afet sonucunu değiştirmez; oyuncu seçimini `ctx.Actions` ile iletir, işi bitince `Completed` tetikler.
- Core'un public imzası değişirse: önce aşağıdaki "Sözleşme değişiklikleri"ne yaz, sonra memory.md §7'de ekibe haber ver.

## Benim alanım
- `Scripts/Core/`, `Data/` (SO asset'leri; içerik Tarık'la ortak), `Scenes/Game.unity` (sadece ben; CoreSetupBuilder ile), build ve teslim.

## Onur'dan teslim notu — 18 Ekim: yeni müşteriler ve kalan A işleri

**Teslim edildi (`b1a40e8`):** Onur bu pakette Tarık'ın işlerini de üstlendi; 10 şeffaf 1000×1000 portre, `Generic01`–`Generic10` ClientSO, yedi havuzda 42 ortak replik, README'nin 7 güne uyarlanması ve AI kayıtları hazır. **g01 (Mahir ve Orhan) ve g04 (Vedat ve İdil), Onur'un açık tercihiyle birlikte gelen tek müşteridir:** tek ClientSO, tek ziyaret, tek proje; kişileri ayırma. Eşleme/önizleme: `Docs/Onur/YediGun_icerik_yol_haritasi.md`.

**Tamamlananlar:**
- [x] A, `17179b4` ile yeni müşterileri Game.unity'ye bağladı: 6 ana + 10 yeni = 16 müşteri. Link Data adımı tekrar yapılacak bir açık iş değil.
- [x] A, gerçek kadroyla denge raporunu aldı: dikkatli %82 İyi / %18 ofis kapanır; tek zarf %44 İyi / %45 Enkazın Gölgesi / %11 hapis; her zarfı alan %76 hapis; reddeci 3. gün kapanır (`memory.md`). Ekonomi değişikliği kararı A'da.
- [x] Onur'un teslim tabanı `8981203` üzerinde izole doğrulaması: 54/54 EditMode (52 mevcut + yalnızca geçici kopyada iki içerik kontrolü), 4/4 Flow PlayMode; gerçek kadroyla 300 program ve JSON kayıt roundtrip'i. Bunlar son zarf değişikliği sonrası Game.unity'de elle kabul testi yerine geçmez.

**A'da kalanlar:**
- [x] (A, 18 Ekim ~00:30, `e6a1a0b` üzerinde otomatik PlayMode kabul oynanışı, ekran görüntüleriyle) Güncel `main` ile **yeni oyun** üzerinden uçtan uca 7 günlük akışı kontrol et. → 3/3/4/4/4/5/5, ana 3'er, yeni 1'er, 14 uygunsuz; 7 gün kesintisiz, İyi son; gazete 5-6. gün; dikkatli oyuncu tam 14 onay. Eski kayıt, yeni kadronun kabul testi sayılmasın: program oyun başında üretilip kayıtla saklanıyor. 28 proje (3/3/4/4/4/5/5), ana müşteriler üçer, yeni müşteriler birer, toplam 14 uygun / 14 uygunsuz; zarf 2., sahte belge 3., kabadayı 4. günden.
- [ ] Oğuzhan'ın yeni portrelerin masa kadrajı (özellikle iki ikili), isim/balon okunurluğu ve masadaki 10 → 7 metin kontrolü tamamlandığında final oyunda doğrula. README zaten Onur tarafından 7 güne çevrildi; Tarık'tan ayrıca içerik beklenmiyor.
- [x] (A: g02 ve Kemal ile 5 satır diyalog; masada sadece "ZARF"; reddet → proje Pending, zarf 0; sonraki müşteride zarf ekranı kapalı) Yeni müşterilerle selamlama → uyuşmazlık → teklif → tepki/kapanış akışını ve `4ff9139` zarf ekranını birlikte kontrol et: tutar açılmadan görünmesin; zarfı reddetmek projeyi sonuçlandırmasın; kabul etmek zarfla onaylasın; sonraki müşteride eski balon/zarf kalmasın.
- [x] (A: kayıt Ok, Devam Et 2. günü açtı, program birebir aynı, LoadFailed yok; kabadayı 4. ve 6. gün "Masan kırıldı" 600 TL; hediye kartı ilk koşuda çıktı; 4 son ekranı görüntülendi. Sahte belge / ek test Core testleriyle doğrulu, arayüzde ayrıca denenmedi) Gün sonu kayıt → Devam Et akışında aynı ziyaret programının korunduğunu, yedi günlük yapıda dört sonu (F9/F10 dahil), sahte belge/ek test, kabadayı ve hediyeyi son entegrasyon üzerinde kontrol et.
- [ ] Güncel main'de EditMode/PlayMode kontrollerini çalıştır; ardından `CivilJam > Core > Build Windows (Release)` ile final build/ZIP üret. Build ve repo aynı sürümde olsun; teslim belgeleri ve video bağlantıları son build'e göre kontrol edilsin.

## Yol haritası
- [x] Faz 0 veri sözleşmesi, Faz 1 dikey dilim (16 Ekim gece).
- [x] Analiz sonrası düzeltmeler (borç, 4 son, 10 gün, kayıt silme, F9/F10, build'de yedek arayüz kapalı), denge senaryosu E.
- [x] Oynanış testi işleri: proje türleri, gevşek roller, tekrarsız adlar, El Kitabı v2, demir tipleri, tepki/hafıza/hediye (sadece dürüst ustalar).
- [x] Yeni hedef arayüz için Core: el kitabı sekmeleri (`SectionsFor`), ödeme önizlemesi, kayıt sonucu + tekrar deneme. Testler 30/30.
- [x] Oğuzhan'ın yeni masası (`f14d2ed`) Game.unity'de oynatılarak kontrol edildi (PlayMode ekran görüntüleri).
- [x] Parametre sırası = El Kitabı sekme sırası (ProjectGenerator, test `GeneratedParams_FollowHandbookTabOrder`). Testler 31/31.
- [x] Build aracı `ReleaseBuilder` (`CivilJam > Core > Build Windows (Release)`); 17 Ekim akşam build'i temiz. Otomatik uçtan uca oynanış temiz; görsel eksikler memory.md §7'de.
- [ ] Elle oynanış: 4 son (F9/F10), sahte belge, ek test, kabadayı, hediye.
- [x] **7 günlük yapı — Core** (52/52; Onur'un içerik paketi `b1a40e8`, Game.unity'ye bağlantı + gerçek kadro denge raporu `17179b4`) (memory.md "📅"): `IsGeneric`, ortak havuz, `GenericBribeRange`, program kurucu, 14 / 14, gün kuralları, 7 gün verisi ve testler. Son kabul/build işleri yukarıdaki teslim notunda.
- [x] İnceleme modu Core'u (`78a847c`) + Game.unity'de doğrulama + build.
- [x] **Uyuşmazlık diyaloğu + rüşvet uyuşmazlıktan sonra — Core** (46/46, denge raporu) (memory.md "💬"): `IGameActions.Inspect` → `InspectOutcome` (`Result`, `Lines`, `BribeOffered`), `DialogueLine` / `DialogueSpeaker`, `ProjectData.BribeRevealed`, Approve/ret kuralları, `DialogueSO` + `Data/Dialogue.asset` + `GameContext.Dialogue`, `ClientSO.MismatchLines` / `BribeCloseLines`, LinkData, sandbox `Inspect` stub'ları, testler, denge simülasyonu.
- [ ] Paz 12:00 freeze → final build, ZIP, README kontrolü, video (Tarık ile), yükleme 15:00-16:30.

## B ve C için Core kullanım rehberi
- Ekran = sahnede MonoBehaviour + `IGameScreen` (`Phase`, `Show(ctx)`, `Hide()`, `Completed`). GameManager ekranları kendisi bulur; yeni ekranı Game.unity'ye A ekler (`CivilJam > Core > Install Screens Into Game Scene`).
- **Okuma (`GameContext`):**
  - Masa: `ctx.TodayProjects` → `Params` (`Reported`; `Tested` ise `Actual`), `Documents[0]` form, `Documents[1]` lab raporu (`ParcelNo`, `Date`, `LabName`, `StampId`, `SignatureName`), `OffersBribe` / `BribeAmount`, `GreetingText` / `ReactionText`, `GiftId`, `FileNo`, `Floors`, `HeightMeters`, `PeoplePerFloor`, `Address`, `Fee`, `ClientId` → `ctx.GetClient(id)`.
  - Kurallar: `ctx.Rules.SectionsFor(tür)` (sekmeler), `FindRule(tür, parametre, kat)`, `FormatValue(parametre, değer)`, `DisplayNameOf(parametre)`; `ctx.Names.LabNames` (akredite lab'lar); `ctx.Balance.ExtraTestCost`.
  - Gazete: `ctx.LastQuake.Outcomes` (`BuildingName`, `Collapsed`, `Deaths`, `Injured`, `VictimNames`, `ResponsibilityText`, `WasBribed`, `FailedParams`), `TotalDeaths`, `TotalInjured`, `Jailed`.
  - Ödeme: `ctx.PendingExpenses.Lines`; ödeme sonrası rakamlar için **`ExpenseCalculator.Preview(ctx.State, ctx.PendingExpenses)`** (`MoneyAfter`, `DebtAfter`, `PaidTotal`, `AddedToDebt`, `DebtPaid`, `Lines[i].Paid`). Ekran kendi hesabını yapmaz.
  - Kayıt: `ctx.LastSave` (`Ok`, `Day`, `Error`); tekrar deneme `ctx.RetrySave()` (günü artırmaz). Yükleme hatası: `ctx.LoadFailed`, `ctx.LoadError`.
  - Son: `ctx.State.Ending` + `ctx.Endings`. Hediyeler: `ctx.State.OfficeGifts` → `ctx.GetGift(id)`.
- **Yazma sadece `ctx.Actions`:** `Approve(id, acceptBribe)`, `Reject(id)`, `BuyExtraTest(id)` → bool, `SetExpensePaid(expenseId, paid)`, `AcceptGift(id)` → bool. Aynı projeye ikinci karar etkisiz.
- HUD: `ctx.StateChanged`. Ana menü "Devam Et": `SaveSystem.HasSave`, `GameLaunch.ContinueFromSave = true` + Game sahnesini yükle.

## Sözleşme değişiklikleri
| Ne zaman | Değişiklik | Etkilenen |
|---|---|---|
| 16 Ekim | İlk sözleşme; günlük ödeme (`DailyExpenseResult`, `PendingExpenses`), afet kapsamı, kayıp verisi, `GameSession` / `GameManager` / `SaveSystem` / `GameLaunch` | B, C |
| `5097e93` | `ClientSO.OffersBribes` | EEE, B |
| 17 Ekim analiz | `EndingType.MiddleCollapsed`; `ExpenseCalculator.DebtPaymentId` / `WindowRepairId`; F9/F10 | C, B |
| Oynanış testi | `ProjectKind` + `ProjectData.Kind` (gizli); `GameState.UsedBuildingNames`; tepki/hediye alanları (`GreetingText`, `ReactionText`, `GiftId`, `GiftSO`, `ClientSO` replik listeleri, `OfficeGifts`, `AcceptGift`, `ClientHistory`) | B, C, EEE |
| El Kitabı v2 | Bölge/mahalle kaldırıldı; `RulesFor(tür, kat)`, `FindRule(tür, parametre, kat)`, `ParamRule.FloorsLabel/Labels/Description/IsLabeled`, `ParamEntry.Labeled` + `LabelStepPenalty` | B, EEE |
| Belge tasarımı | `ParamRule.Section`, `ProjectData.FileNo`, `PeoplePerFloor`, `HeightMeters` | B |
| Uyuşmazlık diyaloğu | YENİ: `IGameActions.Inspect` → `InspectOutcome` (`Result`, `Lines`, `BribeOffered`), `DialogueLine`, `DialogueSpeaker`, `InspectionDialogue.Run`, `ProjectData.BribeRevealed` / `InspectedMismatches`, `ClientSO.MismatchLines` / `BribeCloseLines`, `DialogueSO` + `GameContext.Dialogue`. DEĞİŞTİ: zarf sadece teklif edildiyse alınır / geri çevrilmiş sayılır | B, C, EEE |
| İnceleme modu | YENİ: `InspectDoc`, `InspectField`, `InspectTarget` (`Of`, `OfParam`, `OfTest`), `InspectVerdict`, `InspectResult`, `Inspection.Check(ctx, project, a, b, kat grubu)` / `IsAvailable` / `NoFloorGroup`, `DocumentData.StampLabName`. DEĞİŞTİ: sahte kaşe akredite olmayan lab, sahte parsel hep farklı | B, C |
| Yeni arayüz | `RuleSetSO.SectionsFor`; `Section` = BETON/DONATI/YAPI/ZEMİN; `ExpenseCalculator.Preview` + `DailyExpenseResult.PaidTotal/AddedToDebt/DebtPaid/Clone`; `GameContext.LastSave/RetrySave/LoadFailed/LoadError`, `SaveResult`, `GameSession.SaveNow`, `SaveSystem.Save(state, out error)` / `LastLoadError` | B, C, EEE |

## Günlük (kısa)
- 16 Ekim: Faz 0 + Faz 1; Desk ve Flow Game.unity'ye bağlandı.
- 17 Ekim gece: analiz sonrası kritik maddeler, ilk Windows build, denge simülasyonu.
- 17 Ekim gündüz: oynanış testi işleri (Ç1-Ç5), El Kitabı v2, hediyeler, ekranlar ve veriler Game.unity'de.
- 17 Ekim akşam: yeni hedef arayüz kararı; Core desteği; yeni masa oynanarak kontrol edildi; md dosyaları temizlendi.
