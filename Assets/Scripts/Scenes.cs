namespace DDEngine
{
    public static class Scenes
    {
#if DD1_DEMO_BUILD
        public const int PlaySceneIndex = 0;
        public const int GameViewSceneIndex = 1;
        public const int InstallerSceneIndex = -1;
#else
        public const int IntroSceneIndex = 0;
        public const int InstallerSceneIndex = 1;
        public const int GameViewSceneIndex = 2;
#endif
    }
}