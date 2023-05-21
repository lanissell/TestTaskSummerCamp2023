using System;

namespace Plates
{
    public class PlateMovingBack: Plate
    {
        public static event Action MovingBackActivating;

        public override bool ActivatePlateEffect()
        {
            MovingBackActivating?.Invoke();
            return true;
        }
    }
}
