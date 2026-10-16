# Oğuzhan — Kişisel çalışma planı (B / Masa)

Bu dosya Oğuzhan'ın görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir. Repo ile push edildiğinde ekip tarafından da okunabilir; gizli belge değildir.

## Mevcut durum
- Desk v0 kodu yazıldı ve derleniyor (`Assets/_Project/Scripts/Desk/`). Unity'de henüz test edilmedi, kod commit'lenmedi.
- Kod hâlâ eski level/kota modeliyle; gün sistemine uyarlanacak (bkz. açık noktalar).
- Core (A) repoda yok. Desk kendi modelleriyle (`DeskProject`, `DeskDocument`, `DeskParam`) sahte veriyle çalışıyor; Core gelince tek adapter ile bağlanacak.
- Desk placeholder görselleri Tarık tarafından eklendi: `desk_bg`, `doc_paper`, `stamp_approve`, `stamp_reject`, `signature`.

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

Desk parayı kalıcı olarak değiştirmez, gün ilerletmez, afet sonucu üretmez; kararları `DeskResult` ile bildirir. Desk sadece Core'a bağlıdır; Flow'a bağımlılık eklenmez.

## Adımlar
- [x] Adım 0: AI kullanımı `CREDITS_AND_AI.md`'ye, Desk kararları `memory.md`'ye yazıldı ve push'landı.
- [ ] Adım 1: Unity'de derle → `CivilJam > Desk > Build Sandbox_Desk Scene` → Play. 3 dosya görünüyor; onay, red, zarf, ek test çalışıyor; Console'a sonuç düşüyor.
- [ ] Adım 2: Hata/layout düzeltmeleri → Desk kodunun ilk commit + push'u.
- [ ] Adım 3: Gün sistemine uyarlama (başlık "Gün N", kota ve ek test tahsilatı kararlara göre).
- [ ] Adım 4: Core gelince adapter: `ProjectData` → `DeskProject`, `DeskScreen` → `IGameScreen`.
- [ ] Adım 5: `DeskScreen` prefab'ını A'ya teslim; A `Game.unity`'ye bağlar.
- [ ] Adım 6: Bedava ipuçlarını okunur yapma (kaşe/imza görseli, iki belgeyi karşılaştırma kolaylığı).
- [ ] Adım 7: Sahte belgeler + kabadayı olayının masadaki kısmı.
- [ ] Adım 8: Fabrika / yangın parametreleri (yeni satır tipleri).
- [ ] Adım 9: His katmanı: kaşe animasyonu, kağıt kayma, `sfx_paper`, `sfx_stamp`, `sfx_click`, `mus_office_ambient` tetikleri.
- [ ] Adım 10: Placeholder → final görsellerin prefab'a takılması.
- [ ] Adım 11: Denge, metinler, hassasiyet kontrolü (kurgusal isimler, ton), hata temizliği. Pazar 12:00 freeze.

## Sahte veri senaryoları (Sandbox_Desk)
1. Dürüst usta: rüşvet yok, belgeler tutarlı, değerler uygun → onaylanmalı.
2. Rüşvetçi müteahhit: zarf var, beyan edilen değerler gerekenin altında → reddedilmeli.
3. Sahte belgeli: beyan temiz görünüyor; parsel ters, tarih geçersiz (31.09), kaşe yok; ek test gerçek değerlerin düşük olduğunu gösteriyor → reddedilmeli.

## Açık noktalar
**A'ya:**
- `ProjectData` / `DocumentData` / `ParamEntry` alanları ve `IGameScreen`, `GameContext` imzası ne zaman push'lanacak?
- Ek test ücreti anında mı düşüyor, haftalık gidere mi yazılıyor?
- Kota var mı; günlük mü haftalık mı?
- Bir projenin kaç belgesi olabilir; bedava ipuçları veride işaretli mi (örn. `IsForged` alanı) yoksa sadece metin farkı mı?

**Onur'a (C):**
- Desk bitince akış nereye gidiyor; `DeskResult`'tan hangi verileri gün özetinde göstereceksin?
- Kabadayı olayı masada mı (tehdit) yoksa gün özetinde mi (cam kırıldı) gösterilecek?
- `Assets/Scripts/Intro/IntroSequence.cs` akışın parçası; senin alanına taşıyalım mı? ("Office" sahnesini yüklüyor, öyle bir sahne yok.)

**EEE'ye:**
- Kaşe/imza görsellerinin boyutu ve belge kağıdının kenar boşluğu; zarf görseli (`envelope.png`) listeye eklenebilir mi?

## Çalışma kuralı
- Adım adım ilerle; her adım Unity'de test edilmeden commit'lenmez.
- Saatte bir `main`'i çek. Commit'te `.meta` dosyalarını unutma.
- Her AI kullanımı / hazır asset anında `CREDITS_AND_AI.md`'ye.

## Oturum notu
- 2026-10-16 (UTC+3): Desk v0 kodu yazıldı (derleniyor, test edilmedi). Adım 0 push'landı. Kişisel plan oluşturuldu. Sırada Adım 1 (Unity'de deneme).
