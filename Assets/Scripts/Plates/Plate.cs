using System.Linq;
using TMPro;
using UnityEngine;

namespace Plates
{
    public abstract class Plate : MonoBehaviour
    {
        public int PlateNum
        {
            get
            { 
                return _plateNum;
            }
            set
            {
                _plateNum = value;
                _textMesh.text = _plateNum.ToString(); 
            }
        }
        private int _plateNum;
        [SerializeField]
        private PositionOnPlate[] _positionsOnPlate;
        [SerializeField]
        private TextMeshProUGUI _textMesh;

        public Transform GetEmptyPosition()
        {
            return _positionsOnPlate.Select(pos => pos.Transform).
                FirstOrDefault(posTransform => posTransform.childCount == 0);
        }
        
        public abstract bool ActivatePlateEffect();

    }
}
