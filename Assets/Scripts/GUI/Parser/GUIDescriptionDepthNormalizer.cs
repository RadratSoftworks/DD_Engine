using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.U2D.Path.GUIFramework;

using GUIControlDescriptionDepthSortInfo = System.Tuple<GUIControlDescription, bool>;

public static class GUIDescriptionDepthNormalizer
{
    private static void UnpackControlsFromConditionals(List<GUIControlDescription> controls, List<GUIControlDescriptionDepthSortInfo> infos, bool inCond)
    {
        foreach (GUIControlDescription description in controls)
        {
            if (description is GUIControlConditionalDescription)
            {
                GUIControlConditionalDescription descriptionCond = description as GUIControlConditionalDescription;

                foreach (var condBlock in descriptionCond.ControlShowOnCases)
                {
                    UnpackControlsFromConditionals(condBlock.Item2, infos, true);
                }
            } else
            {
                infos.Add(new GUIControlDescriptionDepthSortInfo(description, inCond));
            }
        }
    }

    private static void NormalizeImpl(List<GUIControlDescription> controls, ref int depth)
    {
        List<GUIControlDescriptionDepthSortInfo> sortControls = new List<GUIControlDescriptionDepthSortInfo>();
        UnpackControlsFromConditionals(controls, sortControls, false);

        sortControls.Sort((lhs, rhs) => lhs.Item1.Depth.CompareTo(rhs.Item1.Depth));

        int previousDepth = -1;

        foreach (GUIControlDescriptionDepthSortInfo controlAndInCondInfo in sortControls)
        {
            GUIControlDescription control = controlAndInCondInfo.Item1;
            bool inConditionSoShouldKeepNextDepth = controlAndInCondInfo.Item2;

            if (control.Depth != int.MinValue)
            {
                if (inConditionSoShouldKeepNextDepth)
                {
                    if ((previousDepth != -1) && (previousDepth < control.Depth))
                    {
                        depth++;
                    }
                } else
                {
                    previousDepth = -1;
                }

                control.AbsoluteDepth = inConditionSoShouldKeepNextDepth ? depth : ++depth;
                previousDepth = control.Depth;
            }

            if (control is GUIControlLayerDescription)
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

                // For side-menu image
                control.AbsoluteDepth = ++depth;
            } else if (control is GUIControlBackgroundLabelDescription)
            {
                // Two objects
                depth++;
            }
        }
    }

    public static void Normalize(GUIControlSetDescription file)
    {
        int depthStart = 0;
        NormalizeImpl(file.Controls, ref depthStart);
    }
}
