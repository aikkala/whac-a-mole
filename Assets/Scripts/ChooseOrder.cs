using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChooseOrder : MonoBehaviour
{
    public SequenceManager sequenceManager;
    private TMP_Dropdown _dropdown;
    
    void Start()
    {
        // Get the component
        _dropdown = GetComponent<TMP_Dropdown>();
        
        // Get all options
        _dropdown.AddOptions(new List<string> (sequenceManager.conditionOrders.Keys));
        
        // Set default option
        sequenceManager.UpdateExperimentOrder(_dropdown.options[_dropdown.value].text);

        // Add a listener to detect when value is changed
        _dropdown.onValueChanged.AddListener(delegate { 
            DropdownValueChanged(_dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        // Update condition order for this experiment
        sequenceManager.UpdateExperimentOrder(_dropdown.options[change.value].text);
    }
}
