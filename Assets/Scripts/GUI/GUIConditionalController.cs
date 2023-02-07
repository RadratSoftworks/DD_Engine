using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIConditionalController : MonoBehaviour
{
    private string variableName;
    private string activeValue;

    private ActionInterpreter actionInterpreter;

    private void OnDestroy()
    {
        actionInterpreter.VariableChanged -= OnVariableChanged;
    }

    private void OnVariableChanged(string variableName, string value, bool isGlobal)
    {
        if (variableName == this.variableName)
        {
            gameObject.SetActive(value == activeValue);
        }
    }

    private void OnControlSetStateChanged(bool enabled)
    {
        if (enabled)
        {
            OnVariableChanged(variableName, actionInterpreter.GetValue(variableName, out bool isGlobal), isGlobal);
        }
    }

    public void Setup(GUIControlSet controlSet, string variable, string value)
    {
        this.variableName = variable;
        this.activeValue = value;

        actionInterpreter = controlSet.ActionInterpreter;
        gameObject.SetActive(actionInterpreter.GetValue(variable, out _) == value);

        actionInterpreter.VariableChanged += OnVariableChanged;
        controlSet.StateChanged += OnControlSetStateChanged;
    }
}
