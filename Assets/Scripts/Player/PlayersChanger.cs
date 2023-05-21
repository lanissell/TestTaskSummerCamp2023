using System;
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

        private void OnEnable()
        {
            PlayerMovement.PlayerStopped += OnPlayerStopped;
            FinishPlate.PlayerFinished += RemovePlayerFromList;
        }

        private void OnDisable()
        {
            PlayerMovement.PlayerStopped -= OnPlayerStopped;
            FinishPlate.PlayerFinished -= RemovePlayerFromList;
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _playersCreator = GetComponent<PlayersCreator>();
        }

        private void OnPlayerStopped(Plate plate)
        {
            if (plate.IsMovePlayer) return;
            ChangeCurrentPlayer();
        }

        private void ChangeCurrentPlayer()
        {
            if (_playersCreator.Players.Count == 0) return;
            DeactivatePlayer(_playersCreator.Players[GetCurrentPlayerIndex()]);
            _currentPlayerIndex++;
            ActivatePlayer(_playersCreator.Players[GetCurrentPlayerIndex()]);
        }

        private int GetCurrentPlayerIndex()
        {
            if (_currentPlayerIndex > _playersCreator.Players.Count - 1 
                || _currentPlayerIndex < 0) 
                _currentPlayerIndex = 0;
            return _currentPlayerIndex;
        }

        private void DeactivatePlayer(PlayerStats player)
        {
            player.AddMovesCount();
            player.CanPlay = false;
        }

        private void ActivatePlayer(PlayerStats player)
        {
            player.CanPlay = true;
            PlayerChanged?.Invoke(player);
        }

        private void RemovePlayerFromList(PlayerStats playerStats)
        {
            _audioSource.Play();
            var players = _playersCreator.Players;  
            players.Remove(playerStats);
            _currentPlayerIndex--;
            if (players.Count != 0) return;
            AllPlayerFinished?.Invoke();
        }

    }
}
