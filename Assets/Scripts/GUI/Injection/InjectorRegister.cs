using System;

namespace DDEngine.GUI.Injection
{
    public static class InjectorRegister
    {
        private static bool ran = false;

        public static void Run()
        {
            if (ran)
            {
                return;
            }
            
            Injector.AddInjector<SettingsMenuInjector>();
            ran = true;
        }
    }
}
