using System;
using Player;
using UnityEngine;

namespace Plates
{
    public class PlateMovingBack: Plate
    {
        public static event Action MovingBackActivating;

        [SerializeField] 
        private int _moveBackStepsCount = -3;

        private void Awake()
        {
            PlateNum = _moveBackStepsCount;
        }

        public override void ActivatePlateEffect(PlayerStats playerStats)
        {
            MovingBackActivating?.Invoke();
        }
    }
}
