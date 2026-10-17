# Oğuzhan — Kişisel çalışma planı (B / Masa)

Bu dosya Oğuzhan'ın görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir. Repo ile push edildiğinde ekip tarafından da okunabilir; gizli belge değildir.

## Mevcut durum
- Desk Core tiplerini doğrudan kullanıyor: `DeskScreen` = `IGameScreen` (Phase = Desk), projeler `ctx.TodayProjects`, kararlar `ctx.Actions`, HUD `ctx.StateChanged`.
- Gün sistemine uygun: başlık "Gün N", kota yok (HUD: "Bugün onaylanan: N").
- DeskScreen Game.unity'de (A ekledi, `3a0d58d`); gerçek GameSession ile test edildi. Sandbox_Desk sahte `IGameActions` (`FakeGameActions`) ve sahte günle (`SandboxDeskContext`) çalışıyor.
- Sesler + ONAYLANDI/REDDEDİLDİ kaşe animasyonu var.
- Y1 (analiz raporu) masada bitti: "El Kitabı" penceresi (kontrol listesi, yönetmelik, akredite lab listesi), lab raporunda başvuru değerleri yan yana, 1. gün 4 öğretici kart. Kart metinleri Tarık'ın onayını bekliyor.
- O4 bitti: kabadayı reddedilince cam sesi + "Camın kırıldı!" kartı (`EventCardView`).
- Sırada: O6 görseller (Adım 10), intro kararı (Onur).
- Desk placeholder görselleri Tarık tarafından eklendi: `desk_bg`, `doc_paper`, `stamp_approve`, `stamp_reject`, `signature` (henüz prefab'a takılmadı).

## Benim alanım
- `Assets/_Project/Scripts/Desk/`
- `Assets/_Project/Prefabs/Desk/`
- `Assets/_Project/Scenes/Sandbox_Desk.unity`
- Masa ekranı: gelen proje dosyaları, belge okuma (proje + lab raporu), bedava ipuçları, ücretli ek test, rüşvet zarfı, onay/red, El Kitabı, 1. gün öğreticisi, kabadayı olayının masadaki kısmı.
- `Game` sahnesinin sahibi A (Ahmet İlman). Flow sahne/prefab sahibi C (Onur). Başkasının sahne/prefab dosyasını açıp kaydetme.

## Sorumluluk sınırı
| Core / A | Ben / B |
|---|---|
| Günün projelerini üretme (ProjectGenerator) | Projeleri masada belge olarak gösterme |
| Required / Reported / Actual değerleri | Değerleri tabloda gösterme; Actual'ı sadece ek testle açma |
| Ek test ücretini ve parayı hesaplama | Oyuncunun "ek test iste" talebini iletme |
| Rüşvet parası, JailRisk, sonuçlar | Zarfı gösterme; alındı/alınmadı kararını iletme |
| Gün sonu kuralları (kota yok) | Bugün onaylanan sayısını gösterme |
| Onaylanan binaların afet sonucu | — (C gösterir) |

Desk parayı değiştirmez, gün ilerletmez, afet sonucu üretmez; kararları sadece `ctx.Actions` ile iletir, mesai bitince `Completed` tetikler. Desk sadece Core'a bağlıdır; Flow'a bağımlılık eklenmez.

## Adımlar
- [x] Adım 0: AI kullanımı `CREDITS_AND_AI.md`'ye, Desk kararları `memory.md`'ye yazıldı ve push'landı.
- [x] Adım 1: Unity'de derle → `CivilJam > Desk > Build Sandbox_Desk Scene` → Play. 3 dosya görünüyor; onay, red, zarf, ek test çalışıyor; Console'a sonuç düşüyor.
- [x] Adım 2: Desk v0 güvenli nokta olarak push'landı (`14350ec`).
- [x] Adım 3: Core tiplerine geçiş + gün sistemi (adapter yerine doğrudan Core; "Gün N", günlük kota). Unity'de test edildi.
- [x] Adım 4: Commit + push, A'ya haber → A `DeskScreen` prefab'ını `Game.unity`'ye ekledi; Game.unity'de gerçek GameSession ile test edildi. Kota kaldırıldı, kaşe auto-size, sandbox Core belge kurallarına uyduruldu.
- [ ] Adım 5: (İsteğe bağlı) Sandbox'ta `FakeGameActions` yerine Core'un `GameSession`'ını kullanmak.
- [x] Adım 6 (= Y1): İpuçlarını okunur yapma: El Kitabı penceresi, akredite lab listesi, lab raporunda başvuru değerleri yan yana, 1. gün öğretici kartları.
- [x] Adım 7: Kabadayı olayının masadaki kısmı (O4: Rüstem reddedilince "camın kırıldı" kartı + `sfx_glass_break`). Sahte belgeleri Core üretiyor; masa ipuçlarını zaten gösteriyor.
- [-] Adım 8: Fabrika / yangın — **kapsam dışı** (A kararı, 17 Ekim). Masada iş yok.
- [x] Adım 9: His katmanı: karar kaşesi animasyonu + `sfx_paper`, `sfx_stamp`, `sfx_click`, `mus_office_ambient` tetikleri. (Kağıt kayma animasyonu yapılmadı; gerekirse cila adımında.)
- [ ] Adım 10: Placeholder → final görsellerin prefab'a takılması.
- [ ] Adım 11: Denge, metinler, hassasiyet kontrolü (kurgusal isimler, ton), hata temizliği. Pazar 12:00 freeze.

## Sahte veri senaryoları (Sandbox_Desk)
1. Dürüst usta: rüşvet yok, belgeler tutarlı, değerler uygun → onaylanmalı.
2. Rüşvetçi müteahhit: zarf var, beyan edilen değerler gerekenin altında → reddedilmeli.
3. Sahte belgeli (Menekşe): beyan temiz görünüyor; lab raporunda parsel 87/41 (başvuruda 87/14), tarih farklı, başlıktaki "Hızlı Sonuç Lab. Ltd." akredite listede yok ve kaşedeki lab'la uyuşmuyor, imza başka mühendisin; ek test gerçek değerlerin düşük olduğunu gösteriyor → reddedilmeli.

## Açık noktalar
**A'ya:**
- (Şu an açık soru yok.) Prefab yeniden kurulunca Game.unity'deki masa kendiliğinden güncellenir.

**Onur'a (C):**
- Kabadayı olayı masada gösterilecek (analiz raporu O4); bilgin olsun.
- `Assets/Scripts/Intro/IntroSequence.cs` akışın parçası; senin alanına taşıyalım mı? ("Office" sahnesini yüklüyor, öyle bir sahne yok.)

**EEE'ye:**
- 1. gün öğretici kart metinleri (`Desk/UI/TutorialView.cs`) ve El Kitabı kontrol listesi (`Desk/UI/RulebookView.cs`) taslak; onaylar ya da düzeltir misin?
- Müteahhit portreleri. (`sfx_glass_break` geldi, teşekkürler — `7243543`.)
- Kaşe/imza görsellerinin boyutu ve belge kağıdının kenar boşluğu; zarf görseli (`envelope.png`) listeye eklenebilir mi?

## Çalışma kuralı
- Adım adım ilerle; her adım Unity'de test edilmeden commit'lenmez.
- Commit/push sadece Oğuzhan "yap" dedikten sonra.
- Saatte bir `main`'i çek. Commit'te `.meta` dosyalarını unutma.
- Her AI kullanımı / hazır asset anında `CREDITS_AND_AI.md`'ye.

## Oturum notu
- 2026-10-16 (UTC+3): Desk v0 kodu yazıldı (derleniyor, test edilmedi). Adım 0 push'landı. Kişisel plan oluşturuldu. Sırada Adım 1 (Unity'de deneme).
- 2026-10-16 ~21:45: Adım 1 tamam, Sandbox_Desk'te tüm senaryolar çalıştı. A'nın Core'u geldi; yeni kural: ekranlar Core'a sadece IGameActions ile yazar. Karar: adapter yerine Desk doğrudan Core tiplerine geçecek (sandbox'ta sahte IGameActions). Desk v0 güvenli nokta olarak push'landı.
- 2026-10-16 ~22:30: Desk Core'a geçti (eski modeller silindi; FakeGameActions + SandboxDeskContext). A'nın Faz 1'indeki Weekly→Daily ad değişikliğine uyarlandı (ApprovedToday, ApprovalQuota). Unity'de test edildi. memory.md'deki "Ekibe notlar" maddeleri [x] yapıldı. Sırada commit + push ve A'ya haber.
- 2026-10-16 ~23:10: A DeskScreen'i Game.unity'ye ekledi (`3a0d58d`). A'nın cevapları: kaşe metin olarak kalır (lab kaşesi "<LAB ADI> · AKREDİTE"; ipucu = kaşe/başlık lab adı uyuşmazlığı, farklı mühendis imzası); kota kaldırıldı. Desk'ten ApprovalQuota çıkarıldı, kaşe auto-size, sandbox verisi ProjectGenerator kurallarına uyduruldu. Sandbox_Desk ve Game.unity'de test edildi.
- 2026-10-16 ~23:30: A TMP Essentials'ı repoya ekledi (`701cb5e`). Yerel kopya yedeğe alınıp kaldırıldı, pull yapıldı; Unity'de sorunsuz. Kaşe/imza ipucunun kalıp kalmayacağı A'nın kararını bekliyor (kalkarsa sadece sandbox verisi değişir).
- 2026-10-16 ~23:50: Kaşe/imza ipuçları kalıyor (Tarık 23:32). Adım 9: sesler + karar kaşesi animasyonu (karar sonrası 0.6 sn bekleme, çift tıklama engelli). Müzik masa kapanınca hemen durur (obje pasifleştiği için fade-out yok). Sandbox ve Game.unity'de test edildi. Sırada Adım 6 (ipuçlarını okunur yapma), sonra Adım 10 (görseller).
- 2026-10-17: Ahmet'in analiz raporundaki B maddeleri incelendi (Y1, O4, O6, O1). Y1 masada bitti: El Kitabı penceresi, belge karşılaştırma, 1. gün öğretici (sandbox artık her turda gün sayısını artırıyor). Sandbox ve Game.unity'de test edildi. A'nın yeni kararları: 10 gün, fabrika/yangın kapsam dışı (Adım 8 düştü). Sırada O4 kabadayı kartı, sonra O6 görseller.
- 2026-10-17: O4 bitti: `EventCardView` (masada olay kartı); kabadayı reddedilince kaşeden sonra cam sesi + "Camın kırıldı!" kartı. Masa Core'un `WindowRepairsToday` sonucunu okur, kuralı kopyalamaz. Sandbox'a 4. dosya (Rüstem) eklendi. Ses Tarık'tan (`sfx_glass_break`). Sandbox ve Game.unity'de test edildi. Sırada O6 görseller.
- 2026-10-17: Ahmet'in oynanış testi kararları (Ç1/Ç2/Ç4) için B yol haritası: 1) min./renk/yan yana kaldır, 2) yeni masa düzeni (büyük sahne, müşteri oturunca belgeler açılır, konuşma balonu), 3) sayfalı El Kitabı, 4) hediye, 5) metin/sandbox/test. Adım 1 bitti: tablo Parametre | Beyan | Ek test, değerler `FormatValue` ile. Test edildi.
- 2026-10-17: Adım 2 (yeni masa düzeni) bitti: dosya listesi kalktı, müteahhitler sırayla gelip koltuğa oturuyor (koltuk → portre → masa, EEE ölçüleri), konuşma balonu (GreetingText/ReactionText), masada LAB RAPORU kitabı (tıklayınca dosya ortada açılır, kararlar içinde) ve EL KİTABI kitabı. Sırada: yeni 600x600 portrelerin yerleşim kontrolü, Adım 3 sayfalı El Kitabı.
