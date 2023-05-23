using System;
using Plates;
using Player;
using UnityEngine;

namespace UI
{
    public class LeaderBoard : MonoBehaviour
    {
        [SerializeField]
        private GameObject _leaderBoardGameObject;
        [SerializeField]
        private Transform _tableTransform;
        private int _playerCounter;
        private LeaderBoardRowCreator _rowCreator;

        private void OnEnable()
        {
            FinishPlate.PlayerFinished += OnPlayerFinished;
            PlayersChanger.AllPlayerFinished += OnAllPlayerFinished;
        }

        private void OnDisable()
        {
            FinishPlate.PlayerFinished -= OnPlayerFinished;
            PlayersChanger.AllPlayerFinished -= OnAllPlayerFinished;
        }

        private void Start()
        {
            _rowCreator = new LeaderBoardRowCreator();
        }

        private void OnPlayerFinished(PlayerStats playerStats)
        {
            AddPlayerOnBoard(playerStats);
        }

        private void OnAllPlayerFinished()
        {
            ActivateLeaderBoard();
        }

        private void AddPlayerOnBoard(PlayerStats stats)
        {
            _rowCreator.InstanceRow(stats, _tableTransform);
        }
        
        private void ActivateLeaderBoard()
        {
            _leaderBoardGameObject.SetActive(true);
        }

    }
}
