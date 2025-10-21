using System;
using UnityEngine;

namespace Cube
{
    public class CubeNegativeZone : MonoBehaviour
    {
        public static event Action Touched; 

        private void OnTriggerEnter(Collider other)
        {
            Touched?.Invoke();
        }
    }
}
