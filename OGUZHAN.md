# Oğuzhan — Kişisel çalışma planı (B / Masa)

Bu dosya Oğuzhan'ın görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir. Repo ile push edildiğinde ekip tarafından da okunabilir; gizli belge değildir.

## Mevcut durum
- Desk Core tiplerini doğrudan kullanıyor: `DeskScreen` = `IGameScreen` (Phase = Desk), projeler `ctx.TodayProjects`, kararlar `ctx.Actions`, HUD `ctx.StateChanged`.
- Gün sistemine uygun: başlık "Gün N", kota `ApprovedToday / ApprovalQuota`.
- Sandbox_Desk sahte `IGameActions` (`FakeGameActions`) ve sahte günle (`SandboxDeskContext`) test edildi.
- Sırada: DeskScreen prefab'ının Game.unity'ye eklenmesi (A yapar).
- Desk placeholder görselleri Tarık tarafından eklendi: `desk_bg`, `doc_paper`, `stamp_approve`, `stamp_reject`, `signature` (henüz prefab'a takılmadı).

## Benim alanım
- `Assets/_Project/Scripts/Desk/`
- `Assets/_Project/Prefabs/Desk/`
- `Assets/_Project/Scenes/Sandbox_Desk.unity`
- Masa ekranı: gelen proje dosyaları, belge okuma (proje + lab raporu), bedava ipuçları, ücretli ek test, rüşvet zarfı, onay/red, kabadayı olayının masadaki kısmı.
- `Game` sahnesinin sahibi A (Ahmet İlman). Flow sahne/prefab sahibi C (Onur). Başkasının sahne/prefab dosyasını açıp kaydetme.

## Sorumluluk sınırı
| Core / A | Ben / B |
|---|---|
| Günün projelerini üretme (ProjectGenerator) | Projeleri masada belge olarak gösterme |
| Required / Reported / Actual değerleri | Değerleri tabloda gösterme; Actual'ı sadece ek testle açma |
| Ek test ücretini ve parayı hesaplama | Oyuncunun "ek test iste" talebini iletme |
| Rüşvet parası, JailRisk, sonuçlar | Zarfı gösterme; alındı/alınmadı kararını iletme |
| Kota ve gün sonu kuralları | Onay sayısını / kotayı gösterme |
| Onaylanan binaların afet sonucu | — (C gösterir) |

Desk parayı değiştirmez, gün ilerletmez, afet sonucu üretmez; kararları sadece `ctx.Actions` ile iletir, mesai bitince `Completed` tetikler. Desk sadece Core'a bağlıdır; Flow'a bağımlılık eklenmez.

## Adımlar
- [x] Adım 0: AI kullanımı `CREDITS_AND_AI.md`'ye, Desk kararları `memory.md`'ye yazıldı ve push'landı.
- [x] Adım 1: Unity'de derle → `CivilJam > Desk > Build Sandbox_Desk Scene` → Play. 3 dosya görünüyor; onay, red, zarf, ek test çalışıyor; Console'a sonuç düşüyor.
- [x] Adım 2: Desk v0 güvenli nokta olarak push'landı (`14350ec`).
- [x] Adım 3: Core tiplerine geçiş + gün sistemi (adapter yerine doğrudan Core; "Gün N", günlük kota). Unity'de test edildi.
- [x] Adım 4: Commit + push, A'ya haber → A `DeskScreen` prefab'ını `Game.unity`'ye ekledi; Game.unity'de gerçek GameSession ile test edildi. Kota kaldırıldı, kaşe auto-size, sandbox Core belge kurallarına uyduruldu.
- [ ] Adım 5: (İsteğe bağlı) Sandbox'ta `FakeGameActions` yerine Core'un `GameSession`'ını kullanmak.
- [ ] Adım 6: Bedava ipuçlarını okunur yapma (kaşe/imza görseli, iki belgeyi karşılaştırma kolaylığı).
- [ ] Adım 7: Sahte belgeler + kabadayı olayının masadaki kısmı.
- [ ] Adım 8: Fabrika / yangın parametreleri (yeni satır tipleri).
- [ ] Adım 9: His katmanı: kaşe animasyonu, kağıt kayma, `sfx_paper`, `sfx_stamp`, `sfx_click`, `mus_office_ambient` tetikleri.
- [ ] Adım 10: Placeholder → final görsellerin prefab'a takılması.
- [ ] Adım 11: Denge, metinler, hassasiyet kontrolü (kurgusal isimler, ton), hata temizliği. Pazar 12:00 freeze.

## Sahte veri senaryoları (Sandbox_Desk)
1. Dürüst usta: rüşvet yok, belgeler tutarlı, değerler uygun → onaylanmalı.
2. Rüşvetçi müteahhit: zarf var, beyan edilen değerler gerekenin altında → reddedilmeli.
3. Sahte belgeli: beyan temiz görünüyor; parsel ters (14/41), tarih geçersiz (31.09), lab akredite değil, kaşe farklı ("YYL · ONAY"); ek test gerçek değerlerin düşük olduğunu gösteriyor → reddedilmeli.

## Açık noktalar
**A'ya:**
- DeskScreen prefab'ı hazır (`IGameScreen`, Phase = Desk). Game.unity'ye ekleyince haber ver; sahne senin.
- (Şu an açık soru yok.)

**Onur'a (C):**
- Kabadayı olayı masada mı (tehdit) yoksa gün özetinde mi (cam kırıldı) gösterilecek?
- `Assets/Scripts/Intro/IntroSequence.cs` akışın parçası; senin alanına taşıyalım mı? ("Office" sahnesini yüklüyor, öyle bir sahne yok.)

**EEE'ye:**
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
