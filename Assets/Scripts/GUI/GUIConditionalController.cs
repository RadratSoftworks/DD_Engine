using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using DDEngine.Action;

namespace DDEngine.GUI
{
    public class GUIConditionalController : MonoBehaviour
    {
        private string[] variableNames;
        private string[] activeValues;

        private ActionInterpreter actionInterpreter;

        private void OnDestroy()
        {
            actionInterpreter.VariableChanged -= OnVariableChanged;
        }

        private void OnVariableChanged(List<string> variableChangeList)
        {
            if (variableChangeList.Any(changedVarName => variableNames.Any(varName => varName == changedVarName)))
            {
                CheckAndChangeGameObjectStateIfPossible();
            }
        }

        private void CheckAndChangeGameObjectStateIfPossible()
        {
            gameObject.SetActive(actionInterpreter.GUIConditionResult(variableNames, activeValues));
        }

        private void OnControlSetStateChanged(bool enabled)
        {
            if (enabled)
            {
                CheckAndChangeGameObjectStateIfPossible();
            }
        }

        public void Setup(GUIControlSet controlSet, string variable, string value)
        {
            this.variableNames = GUIConditionHelper.GetParticipateVariablesInCondition(variable);
            this.activeValues = GUIConditionHelper.GetRequiredValues(value);

            actionInterpreter = controlSet.ActionInterpreter;
            gameObject.SetActive(actionInterpreter.GetValue(variable, out _) == value);

            actionInterpreter.VariableChanged += OnVariableChanged;
            controlSet.StateChanged += OnControlSetStateChanged;
        }
    }
}