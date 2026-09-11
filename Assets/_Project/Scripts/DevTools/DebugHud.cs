using Branded.Combat;
using UnityEngine;

namespace Branded.DevTools
{
    // Temporary on-screen player HP until the real UI (Aşama 4).
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] HealthComponent playerHealth;

        GUIStyle _style;

        void OnGUI()
        {
            if (!playerHealth) return;
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            _style.normal.textColor = playerHealth.IsDead ? Color.red : Color.white;
            GUI.Label(new Rect(20, 16, 400, 40), $"HP {playerHealth.currentHealth:0} / {playerHealth.maxHealth:0}", _style);
        }
    }
}
