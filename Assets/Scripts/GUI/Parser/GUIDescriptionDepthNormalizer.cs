using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class GUIDescriptionDepthNormalizer
{
    private static void NormalizeImpl(List<GUIControlDescription> controls, ref int depth)
    {
        foreach (GUIControlDescription control in controls)
        {
            if (control.Depth != int.MinValue)
            {
                control.AbsoluteDepth = depth++;
            }

            if (control is GUIControlConditionalDescription)
            {
                int depthToPick = depth;

                foreach (var controlList in (control as GUIControlConditionalDescription).ControlShowOnCases)
                {
                    int branchedDepth = depth;
                    NormalizeImpl(controlList.Value, ref branchedDepth);

                    depthToPick = Math.Max(branchedDepth, depthToPick);
                }

                depth = depthToPick;
            }
            else if (control is GUIControlLayerDescription)
            {
                NormalizeImpl((control as GUIControlLayerDescription).Controls, ref depth);
            }
            else if (control is GUIControlLocationDescription)
            {
                NormalizeImpl((control as GUIControlLocationDescription).Layers, ref depth);
            }
            else if (control is GUIControlMenuDescription)
            {
                NormalizeImpl((control as GUIControlMenuDescription).MenuItemControls, ref depth);
            }
        }
    }

    public static void Normalize(GUIControlSetDescription file)
    {
        int depthStart = 0;
        NormalizeImpl(file.Controls, ref depthStart);
    }
}
