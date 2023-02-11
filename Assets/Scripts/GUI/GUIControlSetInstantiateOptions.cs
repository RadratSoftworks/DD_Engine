using System;
using System.Collections.Generic;
using System.Linq;

public class GUIControlSetInstantiateOptions
{
    /// <summary>
    /// Make the view to be panned to the middle of the active when it's selected.
    /// </summary>
    public bool PanToActiveWhenSelected = true;

    public GUIControlSetInstantiateOptions(bool panToActiveWhenSelected = true)
    {
        this.PanToActiveWhenSelected = panToActiveWhenSelected;
    }
}
