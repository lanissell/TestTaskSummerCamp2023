using UnityEngine;

namespace UI
{
    public class GamePause : MonoBehaviour
    {
        public void PauseGame()
        {
            Time.timeScale = 0;
        }

        public void PlayGame()
        {
            Time.timeScale = 1;
        }
    }
}
