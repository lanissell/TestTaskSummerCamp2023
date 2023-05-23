using System;
using System.Collections.Generic;
using GameControl;
using Plates;
using UnityEngine;
namespace Player
{
    public class PlayersCreator : MonoBehaviour
    {
        public static event Action<List<PlayerStats>> PlayersCreated;
        
        private List<PlayerStats> _players;
        [SerializeField]
        private Plate _startPlate;
        [SerializeField]
        private PlayerStats _playerPrefab;
        [SerializeField]
        private Color[] _newPlayerColors;

        private void OnEnable()
        {
            GameStarter.GameStarted += OnGameStarted;
        }

        private void OnDisable()
        {
            GameStarter.GameStarted -= OnGameStarted;
        }

        private void Start()
        {
            _players = new List<PlayerStats>();
        }

        private void OnGameStarted(List<string> names)
        {
            CreatePlayers(names);
        }

        private void CreatePlayers(List<string> names)
        {
            int playersCount = names.Count;
            int colorIndex = 0;
            for (int i = 0; i < playersCount; i++)
            {
                var newPlayer = CreateOnePlayer();
                if (newPlayer.TryGetComponent(out Renderer playerRenderer))
                {
                    newPlayer.Color = _newPlayerColors[colorIndex];
                    SetColor(ref colorIndex, playerRenderer);
                }
                _players.Add(newPlayer);
                SetPlayerName(i, names[i]);
            }
            _players[0].enabled = true;
            PlayersCreated?.Invoke(_players);
        }

        private PlayerStats CreateOnePlayer()
        {
            Transform emptyPoint = _startPlate.GetEmptyPosition();
            PlayerStats newPlayer = Instantiate(_playerPrefab, emptyPoint.position, 
                Quaternion.identity);
            newPlayer.transform.parent = emptyPoint;
            newPlayer.GetComponent<PlayerMovement>().StartPlate = _startPlate;
            newPlayer.enabled = false;
            return newPlayer;
        }

        private void SetPlayerName(int playerIndex, string playerName)
        {
            if (playerName.Equals(""))
            {
                _players[playerIndex].Name = $"Player {playerIndex + 1}";
                return;
            }
            _players[playerIndex].Name = playerName;
        }

        private void SetColor(ref int colorIndex, Renderer playerRenderer)
        {
            playerRenderer.material.color = _newPlayerColors[colorIndex];
            colorIndex ++;
            if (colorIndex > _newPlayerColors.Length - 1) colorIndex = 0;
        }
        
        

    }
}
