using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerCountSelector : MonoBehaviour
    {
        [HideInInspector]
        public int PlayersCount;
        [SerializeField]
        private TextMeshProUGUI _counterText;
        [SerializeField]
        private int _minPlayersCount;
        [SerializeField]
        private int _maxPlayersCount;
        [SerializeField]
        private PlayersNamesSelector _namesSelector;

        private void Start()
        {
            PlayersCount = _minPlayersCount;
            _counterText.text = PlayersCount.ToString();
            for (int i = 0; i < PlayersCount; i++)
            {
                _namesSelector.IncreaseInputField();
            }
        }

        public void IncreasePlayerCount()
        {
            if (PlayersCount == _maxPlayersCount) return;
            PlayersCount++;
            _counterText.text = PlayersCount.ToString();
            _namesSelector.IncreaseInputField();
        }
        
        public void DecreasePlayerCount()
        {
            if (PlayersCount == _minPlayersCount) return;
            PlayersCount--;
            _counterText.text = PlayersCount.ToString();
            _namesSelector.DecreasePlayerCount();
        }

    }
}
