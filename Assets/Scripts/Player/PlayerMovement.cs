using System;
using System.Collections;
using UnityEngine;
using Cube;
using Plates;


namespace Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        public static event Action<Plate> PlayerStopped;
        
        public Plate StartPlate;
        
        [SerializeField]
        private float _distanceThreshold;
        [SerializeField]
        private float _movementSpeed;
        [SerializeField]
        private float _movementHeight;
        private Plate _currentPlate;
        
        private Transform _transform;
        private PlayerStats _playerStats;
        
         private void OnEnable()
        {
            PlayingCube.CubeDropped += OnCubeDropped;
            MovingBackPlate.MovingBackActivating += OnMovingBackActivating;
        }

        private void OnDisable()
        {
            PlayingCube.CubeDropped -= OnCubeDropped;
            MovingBackPlate.MovingBackActivating -= OnMovingBackActivating;
        }

        private void Start()
        {
            _currentPlate = StartPlate;
            _transform = transform;
            _playerStats = GetComponent<PlayerStats>();
        }
    
        private void OnCubeDropped(int plateCount)
        {
            StartCoroutine(MovePlayerAlongWay(plateCount, false));
        }

        private void OnMovingBackActivating()
        {
            if (_currentPlate.PlateNum == null) return;
            int plateCount = Math.Abs((int)_currentPlate.PlateNum);
            StartCoroutine(MovePlayerAlongWay(plateCount, true));
        }

        private IEnumerator MovePlayerAlongWay(int plateCount, bool isBack)
        {
            if (!_playerStats.CanPlay) yield break;
            for (int i = 0; i < plateCount; i++)
            {
                Vector3 nextPosition = GetNextPlatePosition(isBack);
                yield return MovePlayerToTarget(nextPosition);
            }
            _transform.parent = _currentPlate.GetEmptyPosition();
            PlayerStopped?.Invoke(_currentPlate);
            _currentPlate.ActivatePlateEffect(_playerStats);
        }

        private Vector3 GetNextPlatePosition(bool isBack)
        {
            Plate nextPlate = isBack ? _currentPlate.PreviousPlate : _currentPlate.NextPlate;
            if (!nextPlate) return Vector3.zero;
            _currentPlate = nextPlate;
            return nextPlate.GetEmptyPosition().position;
        }
        
        private IEnumerator MovePlayerToTarget(Vector3 targetPosition)
        {
            if (targetPosition == Vector3.zero) yield break;
            Vector3 startPosition = _transform.position;
            float startTime = Time.time;
            while (Vector3.Distance(_transform.position, targetPosition) > _distanceThreshold)
            {
                _transform.position = GetBezierPoint(startPosition, targetPosition, 
                    (Time.time - startTime) *_movementSpeed);
                yield return new WaitForEndOfFrame();
            }
        }
 
        private Vector3 GetBezierPoint(Vector3 startPoint, Vector3 endPoint, float time)
        {
            var centerPoint = (startPoint + endPoint) / 2 + Vector3.up * _movementHeight;
            var startCenterSegment = Vector3.Lerp(startPoint, centerPoint, time);
            var centerEndSegment = Vector3.Lerp(centerPoint, endPoint, time);
            var bezierPoint = Vector3.Lerp(startCenterSegment, centerEndSegment, time);
            return bezierPoint;
        }

        

    }
}
