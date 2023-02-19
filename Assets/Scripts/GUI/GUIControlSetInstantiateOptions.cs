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

    /// <summary>
    /// Preferred to use D-Pad for touch control. Note that some controls may override this option (menu).
    /// </summary>
    public bool PreferredDpad = false;

    public GUIControlSetInstantiateOptions(bool panToActiveWhenSelected = true, bool destroyWhenDisabled = false, bool preferredDpad = false)
    {
        this.PanToActiveWhenSelected = panToActiveWhenSelected;
        this.DestroyWhenDisabled = destroyWhenDisabled;
        this.PreferredDpad = preferredDpad;
    }
}
