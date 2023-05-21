using UnityEngine;

namespace Plates.PlateCreator
{
    public class PlateCreator
    {
        private Plate _simplePlatePrefab;

        public PlateCreator(string prefabPath)
        {
            _simplePlatePrefab = Resources.Load<GameObject>(prefabPath).GetComponent<Plate>();
        }

        public Plate GetPlate(int number, Plate previousPlate = null)
        {
            Plate plate = GameObject.Instantiate(_simplePlatePrefab);
            plate.PlateNum = number;
            plate.PreviousPlate = previousPlate;
            return plate;
        }
    }
}