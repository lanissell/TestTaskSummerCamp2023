namespace Plates.PlateCreator
{
    public class PlateCreateController
    {
        private PlateCreator _simplePlateCreator;
        
        private float _addingStepPlateRate;
        private PlateCreator _addingStepPlateCreator;
        
        private float _movingBackPlateRate;
        private PlateCreator _movingBackPlateCreator;
        
        
        public PlateCreateController(int addingStepPlateRate, int movingBackPlateRate)
        {
            _addingStepPlateRate = addingStepPlateRate;
            _movingBackPlateRate = movingBackPlateRate;
            
            _simplePlateCreator = new PlateCreator("Prefabs/Plates/SimplePlate");
            _addingStepPlateCreator = new PlateCreator("Prefabs/Plates/AddingStepPlate");
            _movingBackPlateCreator = new PlateCreator("Prefabs/Plates/MovingBackPlate");
        }
        
        public PlateCreator GetPlateCreator(int plateNum)
        {
            PlateCreator plateCreator = _simplePlateCreator;
            if (plateNum % _addingStepPlateRate == 0)
            {
                plateCreator = _addingStepPlateCreator;
            }
            else if (plateNum % _movingBackPlateRate == 0)
            {
                plateCreator = _movingBackPlateCreator;
            }
            return plateCreator;
        }
    }
}