using System;
using Player;

namespace Plates
{
    public class FinishPlate: Plate
    {
        public static event Action<PlayerStats> PlayerFinished;
        
        public override void ActivatePlateEffect(PlayerStats playerStats)
        {
            PlayerFinished?.Invoke(playerStats);
        }
    }
}