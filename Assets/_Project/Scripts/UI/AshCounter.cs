using Branded.Meta;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // HUD demon ash total.
    public class AshCounter : MonoBehaviour
    {
        [SerializeField] TMP_Text label;

        void OnEnable()
        {
            Progress.AshesChanged += Show;
            Show(Progress.Ashes);
        }

        void OnDisable() => Progress.AshesChanged -= Show;

        void Show(int ashes) => label.text = $"İblis Külü  {ashes}";
    }
}
