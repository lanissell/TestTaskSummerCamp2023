using Cube;
using Plates;
using UnityEngine;

namespace Player
{
    public class PlayerStats : MonoBehaviour
    {
        public string Name;
        public int MovesCount { get; private set; }
        public int BonusCount { get; private set; }
        public int FineCount { get; private set; }
        public Color Color;

        private void OnEnable()
        {
            PlayingCube.CubeDropped += OnCubeDropped;
            AddingStepPlate.StepAdding += OnStepAdding;
            MovingBackPlate.MovingBackActivating += OnMovingBackActivating;
            FinishPlate.PlayerFinished += OnPlayerFinished;
        }

        private void OnDisable()
        {
            PlayingCube.CubeDropped -= OnCubeDropped;
            AddingStepPlate.StepAdding -= OnStepAdding;
            MovingBackPlate.MovingBackActivating -= OnMovingBackActivating;
            FinishPlate.PlayerFinished -= OnPlayerFinished;
        }

        private void OnCubeDropped(int sideNum)
        {
            AddMovesCount();
        }

        private void OnStepAdding()
        {
            AddBonusCount();
        }

        private void OnMovingBackActivating()
        {
            AddFineCount();
        }

        private void OnPlayerFinished(PlayerStats playerStats)
        {
            DisablePlayer(playerStats);
        }

        private void AddMovesCount() => MovesCount++;

        private void AddBonusCount() => BonusCount++;

        private void AddFineCount() => FineCount++;

        private void DisablePlayer(PlayerStats stats)
        {
            if (stats == this)
                enabled = false;
        }

    }
}
