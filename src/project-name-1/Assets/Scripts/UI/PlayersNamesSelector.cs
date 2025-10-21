using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayersNamesSelector : MonoBehaviour
    {
        public List<TMP_InputField> InputFields;
        [SerializeField]
        private TMP_InputField _inputFieldPrefab;
        private Transform _transform;

        private void Start()
        {
            InputFields = new List<TMP_InputField>();
            _transform = transform;
        }
        
        public void IncreaseInputField()
        {
            var newFiled = Instantiate(_inputFieldPrefab, _transform);
            InputFields.Add(newFiled);
        }
        
        public void DecreasePlayerCount()
        {
            Destroy(InputFields[^1].gameObject);
            InputFields.Remove(InputFields[^1]);
        }
        

    }
}

