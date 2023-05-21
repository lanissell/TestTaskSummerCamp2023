namespace Plates
{
    public class PlateMovingBack: Plate
    {
        public override bool ActivatePlateEffect()
        {
            GlobalEventManager.SendOnPlayerMovementStart(PlateNum);
            GlobalEventManager.SendOnMovingBackActive();
            return true;
        }
    }
}
