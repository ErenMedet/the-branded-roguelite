using Branded.Boons;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // One clickable boon card in the morning choice.
    public class BoonCard : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("button")] UnityEngine.UI.Button _buttonComponent;
        [SerializeField, FormerlySerializedAs("frame")] UnityEngine.UI.Image _frameComponent;
        [SerializeField, FormerlySerializedAs("icon")] UnityEngine.UI.Image _iconComponent;
        [SerializeField, FormerlySerializedAs("title")] TMP_Text _titleComponent;
        [SerializeField, FormerlySerializedAs("element")] TMP_Text _elementComponent;
        [SerializeField, FormerlySerializedAs("description")] TMP_Text _descriptionComponent;

        BoonData _boon;
        UnityAction<BoonData> _picked;

        void Awake() => _buttonComponent.onClick.AddListener(OnButtonClicked);

        void OnButtonClicked() => _picked?.Invoke(_boon);

        public void Show(BoonData boon, UnityAction<BoonData> picked)
        {
            _boon = boon;
            _picked = picked;

            Color color = ElementColor(boon.ElementType);
            _frameComponent.color = color;
            _iconComponent.sprite = boon.Icon;
            _iconComponent.color = boon.Icon ? Color.white : color; // no icon art yet: a swatch in the element color
            _titleComponent.text = boon.BoonName;
            _elementComponent.text = ElementName(boon.ElementType);
            _elementComponent.color = color;
            _descriptionComponent.text = boon.Description;
        }

        static Color ElementColor(EElementType type)
        {
            switch (type)
            {
                case EElementType.Fire: return new Color(1f, 0.5f, 0.15f);
                case EElementType.Holy: return new Color(0.95f, 0.82f, 0.4f);
                case EElementType.Bleed: return new Color(0.8f, 0.12f, 0.12f);
                default: return new Color(0.7f, 0.7f, 0.72f);
            }
        }

        static string ElementName(EElementType type)
        {
            switch (type)
            {
                case EElementType.Fire: return "Ateş";
                case EElementType.Holy: return "Kutsal";
                case EElementType.Bleed: return "Kanama";
                default: return "Fiziksel";
            }
        }
    }
}
