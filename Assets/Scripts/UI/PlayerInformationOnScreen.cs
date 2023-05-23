using System;
using System.Collections.Generic;
using GameControl;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
        public class PlayerInformationOnScreen : MonoBehaviour
        {
                [SerializeField]
                private TextMeshProUGUI _nameText;
                [SerializeField]
                private Image _coloredPanel;

                private void OnEnable()
                {
                    PlayersCreator.PlayersCreated += OnPlayersCreated;
                    PlayersChanger.PlayerChanged += OnPlayerChanged;
                    PlayersChanger.AllPlayerFinished += OnAllPlayerFinished;
                }

                private void OnDisable()
                {
                    PlayersCreator.PlayersCreated -= OnPlayersCreated;
                    PlayersChanger.PlayerChanged -= OnPlayerChanged;
                    PlayersChanger.AllPlayerFinished -= OnAllPlayerFinished;
                }

                private void OnPlayersCreated(List<PlayerStats> stats)
                {
                    ShowInformationOnScreen(stats[0]);
                }

                private void OnPlayerChanged(PlayerStats playerStats)
                {
                    ShowInformationOnScreen(playerStats);
                }

                private void OnAllPlayerFinished()
                {
                    DestroyThisGameObject();
                }

                private void ShowInformationOnScreen(PlayerStats playerStats)
                {
                    _nameText.text = playerStats.Name;
                    var playerColor = playerStats.Color;
                    _coloredPanel.color = new Color(playerColor.r, 
                        playerColor.g, playerColor.b, 1f);
                }

                private void DestroyThisGameObject()
                {
                    Destroy(gameObject);
                }
        }
}
