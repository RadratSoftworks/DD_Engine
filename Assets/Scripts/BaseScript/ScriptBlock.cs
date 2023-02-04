using System;
using System.Collections.Generic;

public class ScriptBlock<T>
{
    public List<ScriptCommand<T>> Commands { get; set; }

    public ScriptBlock()
    {
        Commands = new List<ScriptCommand<T>>();
    }
}
