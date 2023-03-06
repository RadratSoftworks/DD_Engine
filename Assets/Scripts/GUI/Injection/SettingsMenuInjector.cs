using DDEngine.GUI.Parser;
using UnityEngine;

namespace DDEngine.GUI.Injection
{
    [InjectIntoSet("chapters/settings.fwd")]
    public class SettingsMenuInjector : Injector
    {
        [InjectIntoControl(GUIControlID.Menu)]
        public void InjectIntoSettingsMenu(GUIControlMenuDescription description)
        {
            description.MenuItemControls.Insert(0, new GUIControlSettingMultiValuesOptionDescription()
            {
                Position = new Vector2(0, 0),
                BackgroundImagePath = "img/menu/red_bullet",
                Id = "fps",
                SettingName = "fps",
                ValuesAndValueTextIds = new()
                {
                    new("30", "FPS 30"),
                    new("60", "FPS 60"),
#if !UNITY_ANDROID
                    new("0", "FPS Vsync")
#endif
                },
                FocusIdleAnimationPath = "animations/MenuFocusList.anim",
                Depth = 10
            });
        }
    }
}
