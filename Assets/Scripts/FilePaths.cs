namespace DDEngine
{
    class FilePaths
    {
        public static readonly string GeneralResourceFileName = "opes_general.opes";
        public static readonly string IntroResourceFileName = "startup_general.opes";
        public static readonly string ProtectedGeneralResourceFileName = "protected_general.opes.ngdat";
        public static readonly string LocalizationResourceFileName = "opes_loc-{0}.opes";
        public static readonly string IntroLocalizedResourceFileName = "startup_loc-{0}.opes";
        public static readonly string ProtectedLocalizationResourceFileName = "protected_loc-{0}.opes";

        public static readonly string GeneralDemoResourceFilename = "demo_general.bytes";
        public static readonly string LocalizationDemoResourceFilename = "demo_loc-{0}.bytes";

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
        //public static readonly string MainChapterGUIControlFileName = "ch2/locations/Corridor.fwd";
        public static readonly string MainChapterGUIControlFileName = "chapters/main.fwd";
        public static readonly string IntroMenuGadgetScript = "intro/splash.gdg";
        public static readonly string IntroGameGadgetScript = "intro/splashStartGame.gdg";

        // Sound
        public static readonly string MenuOptionClickSFXFileName = "sound/menu_click_bullet";
        public static readonly string MenuOptionSwitchSFXFileName = "sound/menu_select";
        public static readonly string MenuOptionSwitchToMultipleSFXFileName = "sound/menu_select_multiple";

        // Extension
        public static readonly string MinigameFileExtension = ".mini";
        public static readonly string GadgetScriptFileExtension = ".gdg";
    }
}