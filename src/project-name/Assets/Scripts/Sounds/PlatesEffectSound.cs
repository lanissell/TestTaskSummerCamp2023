using Plates;
using Player;
using UnityEngine;

namespace Sounds
{
    [RequireComponent(typeof(RandomSoundPlayer))]
    public class PlatesEffectSound : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] _bonusEffectSounds;
        [SerializeField]
        private AudioClip[] _fineEffectSounds;
        private RandomSoundPlayer _soundPlayer;

        private void OnEnable()
        {
            AddingStepPlate.StepAdding += OnStepAdding;
            MovingBackPlate.MovingBackActivating += OnMovingBackActivating;
            PlayersChanger.AllPlayerFinished += OnAllPlayersFinished;
        }

        private void OnDisable()
        {
            AddingStepPlate.StepAdding -= OnStepAdding;
            MovingBackPlate.MovingBackActivating -= OnMovingBackActivating;
            PlayersChanger.AllPlayerFinished -= OnAllPlayersFinished;
        }

        private void Start()
        {
            _soundPlayer = GetComponent<RandomSoundPlayer>();
        }

        private void OnStepAdding()
        {
            PlayBonusSound();
        }

        private void OnMovingBackActivating()
        {
            PlayFineSound();
        }

        private void OnAllPlayersFinished()
        {
            DestroyThisGameObject();
        }

        private void PlayBonusSound()
        {
            _soundPlayer.Sounds = _bonusEffectSounds;
            _soundPlayer.PlayRandomSound();
        }
        
        private void PlayFineSound()
        {
            _soundPlayer.Sounds = _fineEffectSounds;
            _soundPlayer.PlayRandomSound();
        }

        private void DestroyThisGameObject()
        {
            Destroy(gameObject);
        }

    }
}
