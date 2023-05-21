using UnityEngine;

namespace PlayingCube
{
    public class CubeNegativeZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            GlobalEventManager.SendOnCubeTouchNegativeZone();
        }
    }
}
