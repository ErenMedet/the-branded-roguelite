using Branded.Loop;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.DevTools
{
    // Temporary on-screen night clock for testing the night/morning loop.
    public class DebugHud : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("nightCycle")] NightCycle _nightCycleComponent;

        GUIStyle _style;

        void OnGUI()
        {
            if (!_nightCycleComponent) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            _style.normal.textColor = Color.white;
            GUI.Label(new Rect(20, 16, 400, 40), CycleText(), _style);
        }

        string CycleText()
        {
            switch (_nightCycleComponent.Phase)
            {
                case ECyclePhase.Night:
                    int seconds = Mathf.CeilToInt(_nightCycleComponent.TimeUntilDawn);
                    return $"Gece {_nightCycleComponent.NightNumber}   Şafağa {seconds / 60}:{seconds % 60:00}";
                case ECyclePhase.Dawn: return "Şafak söküyor";
                case ECyclePhase.Morning: return "Sabah";
                default: return $"Gece {_nightCycleComponent.NightNumber + 1} çöküyor";
            }
        }
    }
}
