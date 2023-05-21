using System;

namespace Plates
{
    public class PlateAddingStep: Plate
    {
        public static event Action StepAdding; 

        public override bool ActivatePlateEffect()
        {
            StepAdding?.Invoke();
            return true;
        }
    }
}
