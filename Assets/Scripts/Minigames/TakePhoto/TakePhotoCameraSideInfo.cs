using System;
using System.Collections.Generic;
using UnityEngine;

public class TakePhotoCameraSideInfo
{
    /// <summary>
    /// The scroll distance from the target point in the layer, where the focus subject lies
    /// </summary>
    public int Distance { get; set; }

    /// <summary>
    /// The allowed distances from two side of the focus object, where the object is focused and not blurry
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// List of image that form this camera side.
    /// </summary>
    public List<TakePhotoImageDisplayInfo> ImageDisplays { get; set; } = new List<TakePhotoImageDisplayInfo>();
}
