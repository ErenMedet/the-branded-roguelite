using System;
using Branded.Boons;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // One clickable boon card in the morning choice.
    public class BoonCard : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Button button;
        [SerializeField] UnityEngine.UI.Image frame;
        [SerializeField] UnityEngine.UI.Image icon;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text element;
        [SerializeField] TMP_Text description;

        BoonData _boon;
        Action<BoonData> _picked;

        void Awake() => button.onClick.AddListener(() => _picked?.Invoke(_boon));

        public void Show(BoonData boon, Action<BoonData> picked)
        {
            _boon = boon;
            _picked = picked;

            Color color = ElementColor(boon.elementType);
            frame.color = color;
            icon.sprite = boon.icon;
            icon.color = boon.icon ? Color.white : color; // no icon art yet: a swatch in the element color
            title.text = boon.boonName;
            element.text = ElementName(boon.elementType);
            element.color = color;
            description.text = boon.description;
        }

        static Color ElementColor(ElementType type)
        {
            switch (type)
            {
                case ElementType.Fire: return new Color(1f, 0.5f, 0.15f);
                case ElementType.Holy: return new Color(0.95f, 0.82f, 0.4f);
                case ElementType.Bleed: return new Color(0.8f, 0.12f, 0.12f);
                default: return new Color(0.7f, 0.7f, 0.72f);
            }
        }

        static string ElementName(ElementType type)
        {
            switch (type)
            {
                case ElementType.Fire: return "Ateş";
                case ElementType.Holy: return "Kutsal";
                case ElementType.Bleed: return "Kanama";
                default: return "Fiziksel";
            }
        }
    }
}
