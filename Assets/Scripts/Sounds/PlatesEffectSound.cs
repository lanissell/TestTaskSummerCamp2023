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

        private void Start()
        {
            _soundPlayer = GetComponent<RandomSoundPlayer>();
            GlobalEventManager.OnAddingStepActive += PlayBonusSound;
            GlobalEventManager.OnMovingBackActive += PlayFineSound;
            GlobalEventManager.OnAllPlayersFinished += DestroyThisGameObject;
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
            GlobalEventManager.OnAddingStepActive -= PlayBonusSound;
            GlobalEventManager.OnMovingBackActive -= PlayFineSound;
            GlobalEventManager.OnAllPlayersFinished -= DestroyThisGameObject;
            Destroy(gameObject);
        }

    }
}
