using System;
using System.Collections.Generic;

public class ScriptBlock<T>
{
    public List<ScriptCommand<T>> Commands { get; set; }

    /// <summary>
    /// If we are either allowed to save the game during the duration of this script.
    /// </summary>
    public bool Saveable { get; set; } = true;

    public ScriptBlock()
    {
        Commands = new List<ScriptCommand<T>>();
    }
}
