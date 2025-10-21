using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sounds
{
    [RequireComponent(typeof(AudioSource) , typeof(Rigidbody))]
    public class CollisionSound : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] _sounds;
        [SerializeField]
        private float _maxVolume;
        [SerializeField]
        private float _delay;
        [SerializeField]
        private float _dependenceOnVelocity;
        private AudioSource _audioSource;
        private Rigidbody _rigidbody;
        private bool _canPlaySound = true;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private IEnumerator OnCollisionEnter(Collision collision)
        {
            if (!_canPlaySound) yield break;
            SetVolumeDependsOnVelocity(_rigidbody.velocity.sqrMagnitude);
            PlaySound();
            yield return new WaitForSeconds(_delay);    
            _canPlaySound = true;
        }

        private void SetVolumeDependsOnVelocity(float magnitude)
        {
            _audioSource.volume = magnitude * _dependenceOnVelocity;
            if (_maxVolume < _audioSource.volume) _audioSource.volume = _maxVolume;
        }

        private void PlaySound()
        {
            _audioSource.PlayOneShot(_sounds[Random.Range(0, _sounds.Length)]);
            _canPlaySound = false;
        }

    }
}
