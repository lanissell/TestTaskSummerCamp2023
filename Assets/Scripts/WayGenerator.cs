using System.Collections.Generic;
using Plates;
using UnityEngine;

public class WayGenerator : MonoBehaviour
{
    [SerializeField] 
    private Plate _startPlate;
    [SerializeField] 
    private Plate _finishPlate;
    
    [Header("SimplePlate")]
    [SerializeField]
    private Plate _simplePlate;
    
    [Header("PlateAddingStep")]
    [SerializeField]
    private PlateAddingStep _plateAddingStep;
    [SerializeField]
    private float _plateAddingStepRate;
    
    [Header("PlateMovingBack")]
    [SerializeField]
    private PlateMovingBack _plateMovingBack;
    [SerializeField]
    private float _plateMovingBackRate;
    
    private Transform _transform;
    private Plate _previousPlate;

    private void Awake()
    {
        _previousPlate = _startPlate;
        _transform = transform;
        GenerateWayByChildPositions();
    }

    private void GenerateWayByChildPositions()
    {
        Transform[] childrenTransforms = _transform.GetComponentsInChildren<Transform>();
        for (int i = 1; i < childrenTransforms.Length; i++)
        {
            int plateNum = i;
            Plate plate = Instantiate(ChosePlatePrefab(ref plateNum), 
                childrenTransforms[i].position, Quaternion.identity, _transform);
            plate.PlateNum = plateNum;
            plate.PreviousPlate = _previousPlate;
            _previousPlate.NextPlate = plate;
            _previousPlate = plate;
            Destroy(childrenTransforms[i].gameObject);
        }
        _previousPlate.NextPlate = _finishPlate;
    }

    private Plate ChosePlatePrefab(ref int plateNum)
    {
        Plate platePrefab = _simplePlate;
        if (plateNum % _plateAddingStepRate == 0)
        {
            platePrefab = _plateAddingStep;
        }
        else if (plateNum % _plateMovingBackRate == 0)
        {
            platePrefab = _plateMovingBack;
        }
        return platePrefab;
    }
}
