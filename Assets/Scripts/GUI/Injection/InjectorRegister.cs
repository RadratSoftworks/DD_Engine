using System;

namespace DDEngine.GUI.Injection
{
    public static class InjectorRegister
    {
        public static void Run()
        {
            Injector.AddInjector<SettingsMenuInjector>();
        }
    }
}
