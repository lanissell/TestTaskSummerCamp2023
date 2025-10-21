using Player;
using UnityEngine;

namespace UI
{
    public class LeaderBoardRowCreator
    {
        private LeaderBoardRow _rowPrefab;
        private int _playerPlace;

        public LeaderBoardRowCreator()
        {
            _rowPrefab = Resources.Load<LeaderBoardRow>("Prefabs/UI/LeaderBoardRow");
        }

        public void InstanceRow(PlayerStats stats, Transform parent)
        {
            LeaderBoardRow newRow = GameObject.Instantiate(_rowPrefab, parent);
            newRow.PositionText.text = (++_playerPlace).ToString();
            newRow.NameText.text = stats.Name;
            newRow.MovesText.text = stats.MovesCount.ToString();
            newRow.FineText.text = stats.FineCount.ToString();
            newRow.BonusText.text = stats.BonusCount.ToString();
        }
    }
}