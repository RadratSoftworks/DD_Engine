using System;
using System.Collections.Generic;

class FilePaths
{
    public static readonly string GeneralResourceFileName = "opes_general.opes";
    public static readonly string IntroResourceFileName = "startup_general.opes";
    public static readonly string LocalizationResourceFileName = "opes_loc-{0}.opes";
    public static readonly string ProtectedLocalizationResourceFileName = "protected_loc-{0}.opes";

    // Animations
    public static readonly string MenuLensFlareAnimaionFilename = "animations/Menu_lensflare.anim";
    public static readonly string[] ArrowAnimationsPath =
    {
        "animations/arrow0.anim",
        "animations/arrow1.anim",
        "animations/arrow2.anim",
        "animations/arrow3.anim"
    };

    // GUI
    public static readonly string MainChapterGUIControlFileName = "ch1/locations/office.fwd";
    //public static readonly string MainChapterGUIControlFileName = "chapters/main.fwd";
    
    // Sound
    public static readonly string MenuOptionSwitchSFXFileName = "sound/menu_click_bullet";
}
