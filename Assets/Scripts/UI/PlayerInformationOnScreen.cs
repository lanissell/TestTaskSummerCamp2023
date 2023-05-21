using System;
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
                    PlayersChanger.PlayerChanged += ShowInformationOnScreen;
                    PlayersChanger.AllPlayerFinished += DestroyThisGameObject;
                }

                private void OnDisable()
                {
                    PlayersChanger.PlayerChanged -= ShowInformationOnScreen;
                    PlayersChanger.AllPlayerFinished -= DestroyThisGameObject;
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
