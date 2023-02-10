using System;
using System.IO;
using System.Collections.Generic;

public class MinigameInfo
{
    private Dictionary<string, MinigameVariable> variables = new Dictionary<string, MinigameVariable>();
    
    public MinigameVariable AddOrGetVariable(string name)
    {
        if (variables.ContainsKey(name))
        {
            return variables[name];
        } else
        {
            MinigameVariable variable = new MinigameVariable(name);
            variables.Add(name, variable);

            return variable;
        }
    }
}