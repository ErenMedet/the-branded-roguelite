using System;
using System.Collections;
using System.Collections.Generic;
using Branded.Boons;
using UnityEngine;

namespace Branded.UI
{
    // Morning boon choice: one card per offer, waits for a click.
    public class BoonChoiceUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] BoonCard[] cards;

        public int Capacity => cards.Length;

        BoonData _picked;

        void Awake() => panel.SetActive(false);

        public IEnumerator Choose(IReadOnlyList<BoonData> offers, Action<BoonData> onPicked)
        {
            if (offers == null || offers.Count == 0) yield break;

            _picked = null;
            panel.SetActive(true);
            for (int i = 0; i < cards.Length; i++)
            {
                bool used = i < offers.Count;
                cards[i].gameObject.SetActive(used);
                if (used) cards[i].Show(offers[i], boon => _picked = boon);
            }

            while (!_picked) yield return null;
            panel.SetActive(false);
            onPicked?.Invoke(_picked);
        }
    }
}
