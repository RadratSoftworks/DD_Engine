using System;
using System.Collections.Generic;
using System.Linq;

public class GUIControlSetInstantiateOptions
{
    /// <summary>
    /// Make the view to be panned to the middle of the active when it's selected.
    /// </summary>
    public bool PanToActiveWhenSelected = true;

    /// <summary>
    /// Destroy this control set immediately when it's disabled
    /// </summary>
    public bool DestroyWhenDisabled = false;

    public GUIControlSetInstantiateOptions(bool panToActiveWhenSelected = true, bool destroyWhenDisabled = false)
    {
        this.PanToActiveWhenSelected = panToActiveWhenSelected;
        this.DestroyWhenDisabled = destroyWhenDisabled;
    }
}
