using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using DDEngine.BaseScript;

namespace DDEngine.Action
{
    public class ActionLibrary
    {
        private Dictionary<string, Dictionary<string, ScriptBlock<ActionOpcode>>> actionHandlers;

        public ActionLibrary(Dictionary<string, Dictionary<string, ScriptBlock<ActionOpcode>>> actionHandlers)
        {
            this.actionHandlers = actionHandlers;
        }

        public IEnumerator HandleAction(ActionInterpreter interpreter, string id, string action)
        {
            // Find script to execute
            if (actionHandlers.ContainsKey(id))
            {
                var dict2 = actionHandlers[id];
                if (dict2.ContainsKey(action))
                {
                    yield return interpreter.Execute(dict2[action]);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Action \"" + action + "\" not found for id=" + id);
#endif
                    yield break;
                }
            }
            else
            {
                Debug.LogError("No action found for id=" + id);
                yield break;
            }
        }
    }
}