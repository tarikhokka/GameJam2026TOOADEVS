using System;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// Flow ekranlarının ortak tabanı. UI ilk Show'da kodla kurulur; Completed bir kez tetiklenir
    /// (çift tıklamayla çift ödeme / çift gün ilerlemesi olmasın diye).
    /// Ekranlar state değiştirmez; sadece ctx'ten okur, tek yazma yolu ctx.Actions.
    /// </summary>
    public abstract class FlowScreen : MonoBehaviour, IGameScreen
    {
        public abstract GamePhase Phase { get; }
        public event Action Completed;

        // (EEE, 17 Ekim) Ekrandaki düğmeler ana menüdeki tahta düğme gibi görünsün ve fare üstündeyken vurgulansın.
        // Boşsa sadece vurgu eklenir (UiKit düğmelerinde vurgu zaten var).
        [SerializeField] Sprite buttonSprite;       // btn_menu
        [SerializeField] TMP_FontAsset buttonFont;  // FiraSans-Bold SDF
        // Başlık / bilgi yazıları için (UiKit.StyleTitle / StyleInfo). Boşsa prefab fontu kalır.
        [SerializeField] protected TMP_FontAsset titleFont; // PaytoneOne-Regular SDF
        [SerializeField] protected TMP_FontAsset infoFont;  // FiraSans-Bold SDF
        [SerializeField] protected TMP_FontAsset bodyFont;  // FiraSans-Regular SDF
        [SerializeField] protected Sprite paperSprite;      // paper (9-slice kağıt kart)

        protected GameContext Context { get; private set; }
        bool _built;
        bool _completed;

        public void Show(GameContext context)
        {
            Context = context;
            _completed = false;
            gameObject.SetActive(true);
            if (!_built)
            {
                Build();
                StylePrefabButtons();
                _built = true;
            }
            Refresh();
        }

        void StylePrefabButtons()
        {
            // buttonSprite bağlıysa ekrandaki tüm düğmeler (prefab'taki de UiKit'le kurulan da) tahta düğme olur.
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (buttonSprite != null && button.targetGraphic is Image image)
                {
                    image.sprite = buttonSprite;
                    image.type = Image.Type.Simple;
                    image.color = Color.white;
                    foreach (var label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        label.color = UiKit.Paper;
                        if (buttonFont != null) label.font = buttonFont;
                    }
                }
                if (button.GetComponent<ButtonHover>() == null) button.gameObject.AddComponent<ButtonHover>();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>UI nesnelerini bir kez kur.</summary>
        protected abstract void Build();

        /// <summary>Her Show'da (ve gerekirse sonra) context'ten yeniden doldur.</summary>
        protected abstract void Refresh();

        protected void Finish()
        {
            if (_completed || PauseMenu.InputBlocked) return;
            _completed = true;
            Completed?.Invoke();
        }
    }
}
