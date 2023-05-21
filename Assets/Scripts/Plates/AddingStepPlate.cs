using System;
using Player;

namespace Plates
{
    public class AddingStepPlate: Plate
    {
        public static event Action StepAdding;
        
        public override void ActivatePlateEffect(PlayerStats playerStats)
        {
            StepAdding?.Invoke();
        }
    }
}
