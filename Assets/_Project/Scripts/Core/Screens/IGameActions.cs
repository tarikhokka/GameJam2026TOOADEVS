namespace CivilJam.Core
{
    /// <summary>
    /// Ekranların oyuncu seçimini Core'a iletmesinin TEK yolu. Para/gün/sonuç sadece burada değişir.
    /// Uygulaması Core'dadır; ekranlar GameContext.Actions üzerinden çağırır.
    /// </summary>
    public interface IGameActions
    {
        /// <summary>Ücretli ek test: parayı anında düşer, projenin tüm ParamEntry.Tested = true olur. Para yetmezse false.</summary>
        bool BuyExtraTest(string projectId);

        /// <summary>Projeyi onayla. acceptBribe sadece OffersBribe ise anlamlıdır.</summary>
        void Approve(string projectId, bool acceptBribe);

        void Reject(string projectId);

        /// <summary>Haftalık hesaplaşmada bir kalemi ödenecek/ödenmeyecek işaretle. Kesinleştirme ekran Completed olunca yapılır.</summary>
        void SetExpensePaid(string expenseId, bool paid);
    }
}
