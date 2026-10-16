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
- [ ] EconomyCalculator: haftalık giderler, borç, ofis kapanışı
- [ ] Rüşvet + sahte belge (bedava ipucu + ücretli ek test), JailRisk
- [ ] Kota kontrolü

### Faz 3 — İçerik mantığı (Cts 14 → 20)
- [ ] Fabrika/yangın; yıkım nedeni şablon cümleleri; manşet seçimi
- [ ] EndingResolver (İyi / Kötü / Orta); Save/Load JSON
- [ ] Gizli gün atlama kısayolu

### Faz 4-5 — MVP, denge, build (Cts 20 → Paz 12 freeze)
- [ ] Baştan sona oynanış, 3 son erişilebilir; denge BalanceSO'dan
- [ ] Windows build (Game.unity sahibi olarak)

## Onur'un sorularına Core önerileri (ekip onayı bekliyor, kesin değil)
Varsayılan değerler `BalanceSO`'da duruyor; karar değişirse sadece asset değişir.
- Afet sonucu: bugüne kadar onaylanmış ve ayakta olan TÜM binalar dahil (`GameState.Buildings`). Yıkılan bina listede kalır, tekrar değerlendirilmez.
- Gazete / gün özeti verisi: `QuakeResult.Outcomes` (bina adı, yıkıldı mı, FailedParams), gün no, para, JailRisk değişimi.
- Afet olasılığı: başlangıç 0.10, afetsiz gün +0.10, tavan 0.80, afet olunca başlangıca döner.
- Hafta 7 gün; ödeme günü 7, 14…; toplam 14 gün öneri. Ödenmeyen zorunlu gider borca eklenir; borç limiti aşılırsa ofis kapanır (Orta son).
- Ek test parası anında düşülür (oyuncu sonucu hemen görür).
- Kayıt gün sonunda; yükleme sonraki günün sabahını (DayIntro) açar. Son ekranında kayıt silinir.
- Son önceliği: Kötü (hapis) > Orta (ofis kapandı) > Orta (kota) > İyi.

## Sözleşme değişiklikleri
| Tarih-saat | Değişiklik | Etkilenen |
|---|---|---|
| 2026-10-16 21:20 | İlk sözleşme (gün sistemi adlarıyla) | B, C |

## Oturum notu
- 2026-10-16 21:20 (UTC+3): AHMET.md oluşturuldu. Faz 0: Core veri sözleşmesi yazıldı, Unity'de hatasız derlendi, push'landı. Desk (B) kendi modelleriyle ilerliyor; Core'a adapter ile bağlanacak.
