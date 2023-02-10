using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MinigameVariable
{
    private Dictionary<string, object> properties = new Dictionary<string, object>();
    private string name;

    public Dictionary<string, object> Properties => properties;
    public string Name => name;

    public MinigameVariable(string name)
    {
        this.name = name;
    }

    public void AddProperty(string name, object obj)
    {
        properties.Add(name, obj);
    }
}
