using System;
using System.Collections.Generic;
using Player;

public static class GlobalEventManager
{
        public static event Action<List<string>> OnStartButtonClick; 
        public static event Action<int> OnPlayerMovementStart;
        public static event Action OnPlayerStop;
        public static event Action OnAddingStepActive;
        public static event Action OnMovingBackActive;
        public static event Action<PlayerStats> OnPlayerFinished;
        public static event Action<PlayerStats> OnPlayerChanged;
        public static event Action OnAllPlayersFinished;
        public static event Action OnCubeTouchNegativeZone;
        
        public static void SendOnStartButtonClick(List<string> names) => 
                OnStartButtonClick?.Invoke(names);

        public static void SendOnPlayerMovementStart(int stepsCount) => 
                OnPlayerMovementStart?.Invoke(stepsCount);

        public static void SendOnPlayerStop() => 
                OnPlayerStop?.Invoke();
        
        public static void SendOnAddingStepActive() => 
                OnAddingStepActive?.Invoke();
        
        public static void SendOnMovingBackActive() => 
                OnMovingBackActive?.Invoke();
        
        public static void SendOnPlayerFinished(PlayerStats playerStats) => 
                OnPlayerFinished?.Invoke(playerStats);
        
        public static void SendOnPlayerChanged(PlayerStats playerStats) => 
                OnPlayerChanged?.Invoke(playerStats);
        
        public static void SendOnAllPlayersFinished() => 
                OnAllPlayersFinished?.Invoke();
        public static void SendOnCubeTouchNegativeZone() =>
                OnCubeTouchNegativeZone?.Invoke();
        
}