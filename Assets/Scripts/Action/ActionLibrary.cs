using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ActionLibrary
{
    private Dictionary<string, Dictionary<string, ScriptBlock<ActionOpcode>>> actionHandlers;

    public ActionLibrary(Dictionary<string, Dictionary<string, ScriptBlock<ActionOpcode>>> actionHandlers)
    {
        this.actionHandlers = actionHandlers;
    }

    public bool HandleAction(string id, string action)
    {
        // Find script to execute
        if (actionHandlers.ContainsKey(id))
        {
            var dict2 = actionHandlers[id];
            if (dict2.ContainsKey(action))
            {
                ActionInterpreter interpreter = new ActionInterpreter(dict2[action]);
                interpreter.Execute();

                return true;
            } else
            {
                return false;
            }
        } else
        {
            return false;
        }
    }
}
