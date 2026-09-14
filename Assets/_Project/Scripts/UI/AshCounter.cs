using Branded.Core;
using Branded.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // HUD demon ash total.
    public class AshCounter : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("label")] TMP_Text _labelComponent;

        void OnEnable()
        {
            GameEvents.AshesChanged += OnAshesChanged;
            OnAshesChanged(Progress.Ashes);
        }

        void OnDisable() => GameEvents.AshesChanged -= OnAshesChanged;

        void OnAshesChanged(int ashes) => _labelComponent.text = $"İblis Külü  {ashes}";
    }
}
