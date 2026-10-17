# Oğuzhan — Kişisel çalışma notu (B / Masa)

Ortak kararlar ve açık işler `memory.md`'de (§7 "Oğuzhan (B)"). Bu dosya masa ekranının kısa özetidir. Eski adım listesi ve oturum notları git geçmişinde (`fba19e6` ve öncesi).

## Benim alanım
- `Assets/_Project/Scripts/Desk/`, `Assets/_Project/Prefabs/Desk/`, `Assets/_Project/Scenes/Sandbox_Desk.unity`.
- `Game.unity` A'nın (Ahmet), Flow sahne/prefab'ları C'nin (Onur). Başkasının sahne/prefab dosyasını açıp kaydetme.
- Desk klasöründe başkalarının bileşenleri de var: `InspectResultView`, `InspectorBubbleView` (Onur), `BribeEnvelopeView` (Ahmet), `ShiftEndOverlay`, `ChipFontEqualizer`, `DocTextBoost` (Tarık). Prefab'a bağlamak benim işim.

## Sorumluluk sınırı
| Core / A | Ben / B |
|---|---|
| Günün projeleri, Required / Reported / Actual | Belgeleri masada göstermek; Actual'ı sadece ek testle açmak |
| Ek test ücreti, para, rüşvet, JailRisk, sonuçlar | Oyuncunun isteğini `ctx.Actions` ile iletmek |
| İnceleme kararı ve diyalog satırları (`Inspect`) | Kaşeyi, çizgiyi ve satırları sırayla oynatmak |
| Afet sonucu | — (Flow gösterir) |

Desk parayı değiştirmez, gün ilerletmez, kural kodu yazmaz; kararları sadece `ctx.Actions` ile iletir, mesai bitince `Completed` tetikler. Desk sadece Core'a bağlıdır, Flow'a bağımlılık eklenmez.

## Masa ekranı bugün
- Üstte ofis ve müteahhit (konuşma balonu, harf harf yazı); masada üç belge yan yana: Yapı Denetim Formu, Laboratuvar Raporu, Denetçi El Kitabı (Hızlı Bakış + sekmeler).
- Kararlar formda (ONAY / RED); lab raporunda ücretli ek test; kabul edilen hediyeler masada.
- İNCELE modu (`InspectController`, `InspectSpot`): iki belgeden iki alan → Core `Inspect` → kaşe. Uyuşmazlıkta diyalog (`DeskDialogue`): denetçi balonu + müteahhit balonu, sürerken masa kilitli.
- Zarf sadece Core teklif edince (`BribeRevealed`) diyalogdan sonra masaya düşer; tıklanınca açılır (Ahmet'in `BribeEnvelopeView`).
- Son müteahhitten sonra mesai zili + "MESAİ BİTTİ" (Tarık).
- Prefab builder'dan kurulur: `CivilJam > Desk > Build Sandbox_Desk Scene` (derleme bittikten sonra). Prefab'ı elle değiştirme; değişiklik builder'a yazılır.

## Sandbox_Desk senaryoları
1. **Hilmi Usta, Papatya Apartmanı (5 kat):** dürüst, hepsi geçer → onay; onaylanınca hediye.
2. **Cevdet Bey, Lale Sitesi B Blok (8 kat):** rüşvetçi; beton ve demir 7+ kat için yetersiz → uyuşmazlık bulununca teklif, sonra zarf.
3. **Nurettin Kalfa, Menekşe Konutları (6 kat):** sahte belge; parsel, tarih, imza farklı, başlıktaki lab akredite değil; ek test gerçek değerleri düşük gösterir.
4. **Balyoz Rüstem, Yıldız Pasajı (4 kat):** kabadayı; beton ve kolon yetersiz, zarf var; reddedilirse intikam olayı.

## Çalışma kuralı
- Adım adım ilerle; Unity'de test edilmeden commit yok. Commit/push sadece Oğuzhan "yap" dedikten sonra.
- Commit öncesi Unity'nin kendiliğinden değiştirdiği font dosyaları geri alınır, `.meta` dosyaları unutulmaz.
- Her AI kullanımı / hazır asset `CREDITS_AND_AI.md`'ye.
