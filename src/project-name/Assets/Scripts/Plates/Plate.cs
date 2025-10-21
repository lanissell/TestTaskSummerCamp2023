using System.Linq;
using JetBrains.Annotations;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plates
{
    public abstract class Plate : MonoBehaviour
    {
        [field: SerializeField]
        public bool IsMovePlayer { get; private set; }
        
        [CanBeNull] public Plate NextPlate { get; set; } 
        [CanBeNull] public Plate PreviousPlate { get; set; } 
        public int? PlateNum
        {
            get
            {
                return _plateNum;
            }
            set
            {
                if (_plateNum != null) return;
                _plateNum = value;
                _textMesh.text = _plateNum.ToString();
            }
        }
        private int? _plateNum;
        
        [FormerlySerializedAs("_positionsOnPlate")] [SerializeField]
        protected PositionOnPlate[] positionsOnPlate;
        [SerializeField]
        private TextMeshProUGUI _textMesh;

        public Transform GetEmptyPosition()
        {
            return positionsOnPlate.Select(pos => pos.Transform).
                FirstOrDefault(posTransform => posTransform.childCount == 0);
        }
        
        public abstract void ActivatePlateEffect(PlayerStats playerStats);

    }
}
