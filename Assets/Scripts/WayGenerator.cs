using Plates;
using Plates.PlateCreator;
using UnityEngine;


public class WayGenerator : MonoBehaviour
{
    [SerializeField] 
    private Plate _startPlate;
    [SerializeField] 
    private Plate _finishPlate;
    [SerializeField] 
    private int _addingStepPlateRate;
    [SerializeField] 
    private int _movingBackPlateRate;

    private PlateCreateController _plateCreateController;
    private Transform _transform;
    private Plate _previousPlate;
    
    private void Awake()
    {
        _plateCreateController = new PlateCreateController(_addingStepPlateRate, _movingBackPlateRate);
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
            PlateCreator plateCreator = _plateCreateController.GetPlateCreator(plateNum);
            Plate plate = plateCreator.GetPlate(plateNum, _previousPlate);
            Transform plateTransform = plate.transform;
            plateTransform.position = childrenTransforms[plateNum].position;
            _previousPlate.NextPlate = plate;
            _previousPlate = plate;
            Destroy(childrenTransforms[i].gameObject);
        }
        _previousPlate.NextPlate = _finishPlate;
    }

    
}
