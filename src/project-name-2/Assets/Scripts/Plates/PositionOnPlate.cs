using UnityEngine;

namespace Plates
{
    public class PositionOnPlate : MonoBehaviour
    {
        [HideInInspector]
        public Transform Transform;

        private void Awake()
        {
            Transform = transform;
        }
    }
}
