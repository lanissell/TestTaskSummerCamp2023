using System;
using System.Collections.Generic;
using Plates;
using UnityEngine;
namespace Player
{
    public class PlayersCreator : MonoBehaviour
    {
        [HideInInspector]
        public List<PlayerStats> Players;
        [SerializeField]
        private Plate _startPlate;
        [SerializeField]
        private PlayerStats _playerPrefab;
        [SerializeField]
        private Color[] _newPlayerColors;

        private void OnEnable()
        {
            GameStarter.GameStarted += CreatePlayers;
        }

        private void OnDisable()
        {
            GameStarter.GameStarted -= CreatePlayers;
        }

        private void Start()
        {
            Players = new List<PlayerStats>();
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
                Players.Add(newPlayer);
                SetPlayerName(i, names[i]); 
            }
            Players[0].CanPlay = true;
        }

        private PlayerStats CreateOnePlayer()
        {
            Transform emptyPoint = _startPlate.GetEmptyPosition();
            PlayerStats newPlayer = Instantiate(_playerPrefab, emptyPoint.position, 
                Quaternion.identity);
            newPlayer.transform.parent = emptyPoint;
            return newPlayer;
        }

        private void SetPlayerName(int playerIndex, string playerName)
        {
            if (playerName.Equals(""))
            {
                Players[playerIndex].Name = $"Player {playerIndex + 1}";
                return;
            }
            Players[playerIndex].Name = playerName;
        }

        private void SetColor(ref int colorIndex, Renderer playerRenderer)
        {
            playerRenderer.material.color = _newPlayerColors[colorIndex];
            colorIndex ++;
            if (colorIndex > _newPlayerColors.Length - 1) colorIndex = 0;
        }
        
        

    }
}
