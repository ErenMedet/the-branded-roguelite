using Branded.Loop;
using UnityEngine;

namespace Branded.DevTools
{
    // Temporary on-screen night clock for testing the night/morning loop.
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] NightCycle nightCycle;

        GUIStyle _style;

        void OnGUI()
        {
            if (!nightCycle) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            _style.normal.textColor = Color.white;
            GUI.Label(new Rect(20, 16, 400, 40), CycleText(), _style);
        }

        string CycleText()
        {
            switch (nightCycle.Phase)
            {
                case CyclePhase.Night:
                    int seconds = Mathf.CeilToInt(nightCycle.TimeUntilDawn);
                    return $"Gece {nightCycle.NightNumber}   Şafağa {seconds / 60}:{seconds % 60:00}";
                case CyclePhase.Dawn: return "Şafak söküyor";
                case CyclePhase.Morning: return "Sabah";
                default: return $"Gece {nightCycle.NightNumber + 1} çöküyor";
            }
        }
    }
}
