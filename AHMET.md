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
- [x] Enum'lar, runtime tipler (ProjectData, DocumentData, ParamEntry, GameState, GameContext, QuakeResult, WeeklyExpenseResult)
- [x] `IGameScreen`, `IGameActions`
- [x] SO sınıfları: DayConfigSO, RuleSetSO, ClientSO, ExpenseSO, BalanceSO, HeadlineSO, EndingSO
- [x] EarthquakeSimulator (yıkılma kuralı)
- [x] Unity'de derle (hatasız), .meta'ları üret, commit + push
- [ ] Ekibe haber ver
- [x] `memory.md` dosya haritasını doldur

### Faz 1 — Dikey dilim v0 (Cum 22 → Cts 02)
- [ ] ProjectGenerator v0 (seed'li, konut, uygun/uygunsuz karışık)
- [ ] DisasterChance güncellemesi (afetsiz gün → artar, tavan)
- [ ] GameManager durum makinesi: DayIntro → Desk → Construction → afet kontrolü → Quake/Result | DaySummary → (WeeklyExpenses) → Save → NextDay | Ending
- [ ] Game.unity + placeholder ekranlar; BalanceSO/RuleSetSO/DayConfigSO asset'leri
- [ ] EditMode testi: Strength >= 1 asla yıkılmaz

### Faz 2 — Ekonomi ve hile (Cts 08 → 14)
- [ ] EconomyCalculator: günlük giderler, borç, ofis kapanışı
- [ ] Rüşvet + sahte belge (bedava ipucu + ücretli ek test), JailRisk
- [ ] Kota kontrolü

### Faz 3 — İçerik mantığı (Cts 14 → 20)
- [ ] Fabrika/yangın; yıkım nedeni şablon cümleleri; manşet seçimi
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

Önceki önerilerden hâlâ geçerli (onay bekliyor): afet olasılığı 0.10 başlangıç, afetsiz gün +0.10, tavan 0.80; toplam 14 gün; ek test parası anında düşülür. Değerler `BalanceSO`'da.

### Bu kararların Core koduna etkisi (yapılmadı, Faz 1'de)
- `EarthquakeSimulator.Run` şu an ayaktaki TÜM binaları değerlendiriyor → sadece son afetten sonra onaylananları değerlendirecek (ör. `ProjectData.DayDecided > GameState.LastDisasterDay`).
- `BuildingOutcome`'a eklenecek: ölenlerin isimleri, ölü sayısı, yaralı sayısı, sorumluluk metni (ayakta kaldı / yıkıldı → oyuncunun imzası).
- Günlük ödeme: `WeeklyExpenseResult` → `DailyExpenseResult`, `GameContext.PendingWeekly` / `IsPaymentDay` / `Week`, `BalanceSO.DaysPerWeek`, `GameState.ApprovedThisWeek` / `WindowRepairsThisWeek`, `GamePhase.WeeklyExpenses` yeniden adlandırılacak/sadeleşecek. B ve C bu adları kullanıyorsa haber verilecek.
- Afet sonrası olasılık sıfırlama zaten `BalanceSO.ResetChanceAfterDisaster = true` ile planlı.

## Sözleşme değişiklikleri
| Tarih-saat | Değişiklik | Etkilenen |
|---|---|---|
| 2026-10-16 21:20 | İlk sözleşme (gün sistemi adlarıyla) | B, C |
| 2026-10-16 22:00 | PLANLI (kodda değil): günlük ödeme yeniden adlandırması, afet kapsamı, BuildingOutcome kayıp verisi — yukarıdaki "Core koduna etkisi" | B, C |

## Yapılan değişiklikler günlüğü
- `7545c64` — Core Faz 0 push'u: `Assets/_Project/Scripts/Core/**` (+ .meta), `AHMET.md`, `memory.md` (dosya haritası, durum, karar satırı), `CREDITS_AND_AI.md` (Claude Code satırı).
  - Rebase sırasında `memory.md` ve `CREDITS_AND_AI.md`'de çakışma çıktı; iki tarafın içeriği de korunarak çözüldü (B'nin Desk dosya haritası ve kararları + Tarık'ın placeholder satırı kaldı).
  - Commit'lenmeyenler: `.vsconfig`, Unity'nin kendiliğinden değiştirdiği `ProjectSettings/Packages/com.unity.learn.iet-framework/Settings.json`.
- 22:00 — Onur'un sorularına cevaplar ve günlük ödeme kararı `AHMET.md` + `memory.md`'ye işlendi. Kod değişmedi.

## Oturum notu
- 2026-10-16 21:20 (UTC+3): AHMET.md oluşturuldu. Faz 0: Core veri sözleşmesi yazıldı, Unity'de hatasız derlendi, push'landı. Desk (B) kendi modelleriyle ilerliyor; Core'a adapter ile bağlanacak.
- 2026-10-16 22:00 (UTC+3): Onur'un 6 sorusundan 4'ü cevaplandı (5 ve 6 açık; ödenmeyen gider sonucu açık). Ödeme günlüğe çekildi. Faz 1'e henüz başlanmadı.
