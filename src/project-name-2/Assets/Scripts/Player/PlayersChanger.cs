using System;
using System.Collections.Generic;
using Plates;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayersCreator), typeof(AudioSource))]
    public class PlayersChanger : MonoBehaviour
    {
        public static event Action<PlayerStats> PlayerChanged;
        public static event Action AllPlayerFinished;

        private PlayersCreator _playersCreator;
        private int _currentPlayerIndex;
        private AudioSource _audioSource;
        private List<PlayerStats> _playerStats;

        private void OnEnable()
        {
            PlayersCreator.PlayersCreated += OnPlayersCreated;
            PlayerMovement.PlayerStopped += OnPlayerStopped;
            FinishPlate.PlayerFinished += OnPlayerFinished;
        }

        private void OnDisable()
        {
            PlayersCreator.PlayersCreated -= OnPlayersCreated;
            PlayerMovement.PlayerStopped -= OnPlayerStopped;
            FinishPlate.PlayerFinished -= OnPlayerFinished;
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _playersCreator = GetComponent<PlayersCreator>();
        }


        private void OnPlayersCreated(List<PlayerStats> stats)
        {
            InitPlayersList(stats);
        }
        
        private void OnPlayerStopped(Plate plate)
        {
            if (plate.IsMovePlayer || _playerStats == null) return;
            ChangeCurrentPlayer();
        }

        private void OnPlayerFinished(PlayerStats playerStats)
        {
            RemovePlayerFromList(playerStats);
        }
        
        private void InitPlayersList(List<PlayerStats> stats) => _playerStats = stats;

        private void ChangeCurrentPlayer()
        {
            if (_playerStats.Count == 0) return;
            DeactivatePlayer(_playerStats[GetCurrentPlayerIndex()]);
            _currentPlayerIndex++;
            ActivatePlayer(_playerStats[GetCurrentPlayerIndex()]);
        }

        private int GetCurrentPlayerIndex()
        {
            if (_currentPlayerIndex > _playerStats.Count - 1 
                || _currentPlayerIndex < 0) 
                _currentPlayerIndex = 0;
            return _currentPlayerIndex;
        }

        private void DeactivatePlayer(PlayerStats player)
        {
            player.enabled = false;
        }

        private void ActivatePlayer(PlayerStats player)
        {
            player.enabled = true;
            PlayerChanged?.Invoke(player);
        }

        private void RemovePlayerFromList(PlayerStats playerStats)
        {
            _audioSource.Play();
            _playerStats.Remove(playerStats);
            _currentPlayerIndex--;
            if (_playerStats.Count != 0) return;
            AllPlayerFinished?.Invoke();
        }

    }
}
