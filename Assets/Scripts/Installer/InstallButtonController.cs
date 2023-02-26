using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_ANDROID
using NativeFilePickerNamespace;
#endif

namespace DDEngine.Installer
{
    public class InstallButtonController : MonoBehaviour
    {
#if !UNITY_ANDROID
        [DllImport("sisinstaller", EntryPoint = "open_fallback_pick_ngage_game_window")]
        private static extern IntPtr OpenFallbackPickNGageGameWindow();

        [DllImport("sisinstaller", EntryPoint = "free_fallback_pick_ngage_game_window_path")]
        private static extern void FreeFallbackPickNGageGameWindowPath();
#endif

        private InstallSceneController sceneController;

        public void Setup(InstallSceneController sceneController)
        {
            this.sceneController = sceneController;
        }

        public void OnButtonClicked()
        {
#if UNITY_ANDROID
            // Don't really care about permission
            NativeFilePicker.PickFile(result =>
            {
                if (result != null)
                {
                    sceneController.GiveData(new States.NGageInstallPathIntent(result));
                }
            }, new string[] { "application/octet-stream" });
#else
            IntPtr result = OpenFallbackPickNGageGameWindow();
            if (result == null)
            {
                return;
            }

            string path = Marshal.PtrToStringUTF8(result);
            FreeFallbackPickNGageGameWindowPath();

            sceneController.GiveData(new States.NGageInstallPathIntent(path));
#endif
        }
    }
}