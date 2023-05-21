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

                private void Awake()
                {
                    GlobalEventManager.OnPlayerChanged += ShowInformationOnScreen;
                    GlobalEventManager.OnAllPlayersFinished += DestroyThisGameObject;
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
                    GlobalEventManager.OnPlayerChanged -= ShowInformationOnScreen;
                    GlobalEventManager.OnAllPlayersFinished -= DestroyThisGameObject;
                    Destroy(gameObject);
                }
        }
}
