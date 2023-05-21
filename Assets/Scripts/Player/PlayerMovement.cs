using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        private float _distanceThreshold;
        [SerializeField]
        private float _movementSpeed;
        [SerializeField]
        private float _movementHeight;

        private int _currentPositionIndex = 0;
        private WayGenerator _generator;
        private Transform _transform;
        private PlayerStats _playerStats;
    
        private void Start()
        {
            _transform = transform;
            _generator = FindObjectOfType<WayGenerator>();
            _playerStats = GetComponent<PlayerStats>();
            GlobalEventManager.OnPlayerMovementStart += StartMoveCoroutine;
        }
    
        private void StartMoveCoroutine(int plateCount)
        {
            if (!_playerStats.CanPlay) return;
            StartCoroutine(MovePlayerOnWay(plateCount));
        }

        private IEnumerator MovePlayerOnWay(int plateCount)
        {
            Transform target = _transform;
            _transform.parent = null;
            for (int i = 1; i <= Mathf.Abs(plateCount); i++)
            {
                int positionIndex = ValidatePositionIndex(_currentPositionIndex + 
                    i * (plateCount / Mathf.Abs(plateCount)));
                target = _generator.Plates[positionIndex].GetEmptyPosition();
                yield return MovePlayerToTarget(target.position);
            }
            _transform.parent = target;
            _currentPositionIndex = ValidatePositionIndex(_currentPositionIndex + plateCount);
            if (_generator.Plates[_currentPositionIndex].ActivatePlateEffect()) yield break;
            GlobalEventManager.SendOnPlayerStop();
            
            if (_currentPositionIndex != _generator.Plates.Count - 1) yield break;
            GlobalEventManager.SendOnPlayerFinished(_playerStats);
            GlobalEventManager.OnPlayerMovementStart -= StartMoveCoroutine;
        }

        private int ValidatePositionIndex(int index)
        {
            if (index > _generator.Plates.Count - 1) return _generator.Plates.Count - 1;
            if (index < 0) return 0;
            return index;
        }
    
        private IEnumerator MovePlayerToTarget(Vector3 targetPosition)
        {
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
