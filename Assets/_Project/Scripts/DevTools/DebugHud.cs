using Branded.Combat;
using Branded.Loop;
using UnityEngine;

namespace Branded.DevTools
{
    // Temporary on-screen player HP and night clock until the real UI (Aşama 4).
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] HealthComponent playerHealth;
        [SerializeField] NightCycle nightCycle;

        GUIStyle _style;

        void OnGUI()
        {
            if (!playerHealth) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            _style.normal.textColor = playerHealth.IsDead ? Color.red : Color.white;
            GUI.Label(new Rect(20, 16, 400, 40), $"HP {playerHealth.currentHealth:0} / {playerHealth.maxHealth:0}", _style);

            if (!nightCycle) return;
            _style.normal.textColor = Color.white;
            GUI.Label(new Rect(20, 48, 400, 40), CycleText(), _style);
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
