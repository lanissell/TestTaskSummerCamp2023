using UnityEngine;

namespace Plates.PlateCreator
{
    public class PlateCreator
    {
        private Plate _platePrefab;

        public PlateCreator(string prefabPath)
        {
            _platePrefab = Resources.Load<Plate>(prefabPath);
        }

        public Plate GetPlate(int number, Plate previousPlate = null)
        {
            Plate plate = GameObject.Instantiate(_platePrefab);
            plate.PlateNum = number;
            plate.PreviousPlate = previousPlate;
            plate.gameObject.isStatic = true;
            return plate;
        }
    }
}