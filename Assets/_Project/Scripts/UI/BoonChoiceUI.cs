using System.Collections;
using System.Collections.Generic;
using Branded.Boons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // Morning boon choice: one card per offer, waits for a click.
    public class BoonChoiceUI : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("panel")] GameObject _panel;
        [SerializeField, FormerlySerializedAs("cards")] BoonCard[] _cardComponents;

        public int Capacity => _cardComponents.Length;

        BoonData _picked;

        void Awake() => _panel.SetActive(false);

        public IEnumerator Choose(IReadOnlyList<BoonData> offers, UnityAction<BoonData> onPicked)
        {
            if (offers == null || offers.Count == 0) yield break;

            _picked = null;
            _panel.SetActive(true);
            for (int i = 0; i < _cardComponents.Length; i++)
            {
                bool used = i < offers.Count;
                _cardComponents[i].gameObject.SetActive(used);
                if (used) _cardComponents[i].Show(offers[i], OnCardPicked);
            }

            while (!_picked) yield return null;
            _panel.SetActive(false);
            onPicked?.Invoke(_picked);
        }

        void OnCardPicked(BoonData boon) => _picked = boon;
    }
}
