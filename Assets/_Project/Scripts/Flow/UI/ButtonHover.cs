using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// (EEE, 17 Ekim) Fare düğmenin üstündeyken düğmeyi belirginleştirir: hafif büyür, aydınlanır, yazısı altın rengine döner.
    /// UiKit.Button her düğmeye ekler. Duraklatmada da çalışsın diye unscaled zaman kullanır.
    /// </summary>
    [RequireComponent(typeof(Button))]
    internal class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        const float HoverScale = 1.07f, Speed = 14f;
        static readonly Color HoverText = new Color(1f, 0.85f, 0.45f, 1f);

        static readonly Color Glow = new Color(1f, 0.8f, 0.35f, 1f);

        public bool AllowWhilePaused; // duraklatma menüsünün kendi düğmeleri

        Button _button;
        Image _image;
        Outline _outline; // sprite'lı (tahta) düğmede altın çerçeve
        TextMeshProUGUI _label;
        Color _imageColor, _labelColor;
        bool _hover;
        float _t; // 0 normal, 1 tam vurgulu

        void Awake()
        {
            _button = GetComponent<Button>();
            _image = _button.targetGraphic as Image;
            _label = GetComponentInChildren<TextMeshProUGUI>();
            // Düğme tonunu bu bileşen veriyor; Button'ın kendi tonu sadece basılınca kararsın.
            var colors = _button.colors;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            _button.colors = colors;
        }

        void Start()
        {
            // Ekranlar Style() gibi ayarları düğme kurulduktan sonra yapıyor; asıl renkleri burada al.
            if (_image != null) _imageColor = _image.color;
            if (_label != null) _labelColor = _label.color;
            if (_image != null && _image.sprite != null)
            {
                _outline = gameObject.AddComponent<Outline>();
                _outline.effectDistance = new Vector2(3f, -3f);
                _outline.effectColor = new Color(Glow.r, Glow.g, Glow.b, 0f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => _hover = true;
        public void OnPointerExit(PointerEventData eventData) => _hover = false;

        void OnDisable()
        {
            _hover = false;
            _t = 0f;
            Apply();
        }

        void Update()
        {
            bool on = _hover && _button.interactable && (AllowWhilePaused || !PauseMenu.InputBlocked);
            float target = on ? 1f : 0f;
            if (Mathf.Approximately(_t, target)) return;
            _t = Mathf.MoveTowards(_t, target, Speed * Time.unscaledDeltaTime);
            Apply();
        }

        void Apply()
        {
            float k = 1f - (1f - _t) * (1f - _t); // yumuşak çıkış
            transform.localScale = Vector3.one * Mathf.Lerp(1f, HoverScale, k);
            if (_image != null && _imageColor.a > 0f)
                _image.color = Color.Lerp(_imageColor, Brighten(_imageColor), k);
            if (_label != null && _labelColor.a > 0f)
                _label.color = Color.Lerp(_labelColor, HoverText, k);
            if (_outline != null)
                _outline.effectColor = new Color(Glow.r, Glow.g, Glow.b, 0.9f * k);
        }

        // Koyu düz düğmeler belirgin açılsın; beyaz tonlu (sprite'lı) düğmeler zaten beyaz, onlarda büyüme + yazı yeter.
        static Color Brighten(Color c) => new Color(
            Mathf.Min(1f, c.r + 0.18f), Mathf.Min(1f, c.g + 0.14f), Mathf.Min(1f, c.b + 0.10f), c.a);
    }
}
