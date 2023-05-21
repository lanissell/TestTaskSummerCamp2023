namespace Plates
{
    public class PlateAddingStep: Plate
    {
        public override bool ActivatePlateEffect()
        {
            GlobalEventManager.SendOnAddingStepActive();
            return true;
        }
    }
}
