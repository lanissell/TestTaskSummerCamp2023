using Player;
using UnityEngine;

namespace UI
{
    public class LeaderBoard : MonoBehaviour
    {
        [SerializeField]
        private LeaderBoardRow _rowPrefab;
        [SerializeField]
        private GameObject _leaderBoardGameObject;
        [SerializeField]
        private Transform _tableTransform;
        private int _playerCounter;

        private void Start()
        {
            GlobalEventManager.OnPlayerFinished += AddPlayerOnBoard;
            GlobalEventManager.OnAllPlayersFinished += ActivateLeaderBoard;
        }

        private void AddPlayerOnBoard(PlayerStats stats)
        {
            var newRow = Instantiate(_rowPrefab, _tableTransform);
            newRow.PositionText.text = (++_playerCounter).ToString();
            newRow.NameText.text = stats.Name;
            newRow.MovesText.text = stats.MovesCount.ToString();
            newRow.FineText.text = stats.FineCount.ToString();
            newRow.BonusText.text = stats.BonusCount.ToString();
        }
        
        private void ActivateLeaderBoard()
        {
            _leaderBoardGameObject.SetActive(true);
            GlobalEventManager.OnPlayerFinished -= AddPlayerOnBoard;
            GlobalEventManager.OnAllPlayersFinished -= ActivateLeaderBoard;
        }

    }
}
