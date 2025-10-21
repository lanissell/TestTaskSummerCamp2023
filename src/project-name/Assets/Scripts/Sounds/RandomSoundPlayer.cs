using UnityEngine;
using Random = UnityEngine.Random;

namespace Sounds
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomSoundPlayer : MonoBehaviour
    {
        public AudioClip[] Sounds;
        [SerializeField]
        private float _minPitch;
        [SerializeField]
        private float _maxPitch;
        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlayRandomSound()
        {
            _audioSource.pitch = Random.Range(_minPitch, _maxPitch);
            _audioSource.PlayOneShot(Sounds[Random.Range(0, Sounds.Length)]);
        }
    }
}
