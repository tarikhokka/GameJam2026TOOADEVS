# Onur Kabadayılar — Kişisel çalışma planı (C / Akış)

Bu dosya Onur’un görev ve oturum takibi içindir. Ortak proje kararları `memory.md` içindedir. Repo ile push edildiğinde ekip tarafından da okunabilir; gizli belge değildir.

## Mevcut durum
- Plan aşamasındayız. Bu çalışma kapsamında oyun kodu veya Unity sahnesi oluşturulmadı.
- Kesinleşenler: level yerine gün; her gün afet olasılığı; afetsiz gün sonrası ihtimal artışı; haftalık gider; gün sonunda kayıt.
- 14 gün, afet sonrası ihtimal sıfırlama, eski binaların tekrar değerlendirilmesi ve ek testin anında ödenmesi henüz öneridir.

## Benim alanım
- `Assets/_Project/Scripts/Flow/`
- `Assets/_Project/Prefabs/Flow/`
- `Sandbox_Flow` ve `MainMenu` sahneleri.
- Gün özeti, inşaat/afet sunumu, gazete, haftalık hesaplaşma, ana menü ve son ekranları.
- `Game` sahnesinin sahibi A (Ahmet İlman). Desk sahne/prefab sahibi B. Başkasının sahne/prefab dosyasını açıp kaydetme.

## Sorumluluk sınırı
| Core / A | Ben / C |
|---|---|
| Afet olasılığı ve bina sonucu hesaplama | Hazır sonucu görselleştirme |
| Para, borç, ceza ve sigorta hesabı | Kalemleri ve ödeme seçeneklerini gösterme |
| Ödeme talebini doğrulama ve uygulama | Oyuncunun seçimini Core’a iletme |
| Gün, hafta ve bitiş koşulları | Core’un seçtiği ekranı sunma |
| Save/load ve kayıt durumu | Devam Et ve kayıt geri bildirimi |

Ekranlar parayı değiştirmez, gün artırmaz, afet sonucu üretmez. Flow sadece Core’a bağlıdır; Desk’e doğrudan bağımlılık eklenmez. IGameScreen sözleşmesi A ile doğrulanır.

## Nereden başlayacağım?
- [ ] A ile gün/afet sonucu, haftalık gider, kayıt durumu ve ekran tamamlanma sözleşmesini netleştir.
- [ ] Gazete ve haftalık hesap ekranlarının basit taslağını çiz.
- [ ] Kodlama onayından sonra Sandbox_Flow içinde gazete ekranını hazır örnek verilerle kur.
- [ ] Afetsiz gün özeti ve haftalık ödeme ekranını ekle.
- [ ] Basit inşaat ve afet sunumunu ekle; fiziksel bina yıkımı gerektirme.
- [ ] MainMenu: Yeni Oyun / Devam Et; ardından üç son ekranını bağla.
- [ ] A ile entegre et; ekran olaylarının iki kez ödeme veya gün ilerlemesi yaratmadığını kontrol et.

## İlk prototip senaryoları
1. Afetsiz normal gün → gün özeti → kayıt.
2. Afetli normal gün → afet → gazete → kayıt.
3. Afetsiz haftalık ödeme günü → gün özeti → hesaplaşma → kayıt.
4. Afetli haftalık ödeme günü → afet → gazete → hesaplaşma → kayıt.

Haftalık gün numarası ve son koşullarının akıştaki önceliği Core ile kesinleşecek. Kayıt tamamlandı sinyali gelmeden başarılı kayıt gösterme.

## A’ya soracağım açık noktalar
- Afet sonucu hangi binaları içeriyor; önceki günlerin binaları var mı?
- Hangi veriler gazete/gün özetinde gösterilecek?
- Haftalık ödeme günü ve ödenmeyen giderlerin sonucu ne?
- Afet gerçekleşince ihtimal sıfırlanıyor mu?
- Gün sonu yüklemesi sonraki sabahı mı açacak? Son ekranında kayıt nasıl davranacak?
- Kota, hapis ve ofis kapanışı aynı anda tetiklenirse öncelik ne?

## Oturum notu
- 2026-10-16 21:09 (UTC+3): Kişisel plan oluşturuldu; ortak hafıza gün sistemine uyarlandı. Kodlama başlamadı. Commit/push Onur tarafından yapılacak.
