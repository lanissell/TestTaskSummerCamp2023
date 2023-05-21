using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public static event Action<List<string>> GameStarted;
    
    [SerializeField]
    private PlayersNamesSelector _namesSelector;

    public void StartGame()
    {
        GameStarted?.Invoke(NamesToString(_namesSelector.InputFields));
    }

    private List<string> NamesToString(List<TMP_InputField> fields)
    {
        List<string> names = new List<string>();
        foreach (var field in fields)
        {
            names.Add(field.text.Trim());
        }
        return names;
    }

}
