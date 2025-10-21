using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameControl
{
    public class SceneLoader : MonoBehaviour
    {
        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
    }
}
