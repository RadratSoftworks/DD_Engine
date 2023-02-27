#if UNITY_ANDROID
using RDG;
#else
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

using UnityEngine;

namespace DDEngine.Utils
{
    public static class Vibrator
    {
        private const int MinAmplitude = 1;
        private const int MaxAmplitude = 100;

#if !UNITY_ANDROID
        private static CancellationTokenSource cancelPreviousRumbleToken = new CancellationTokenSource();

        private static async UniTask StopRumble(float duration, CancellationToken cancelToken)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: cancelToken);
            Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
        }
#endif

        /// <summary>
        /// Vibrate the device/controller.
        /// </summary>
        /// <param name="durationInSeconds">The duration of vibrate in seconds.</param>
        /// <param name="amplitude">The amplitude of the vibration, in the range of 1 to 100.</param>
        public static void Vibrate(float durationInSeconds, int amplitude)
        {
            if (amplitude < MinAmplitude)
            {
                return;
            }

#if UNITY_ANDROID
            Vibration.Vibrate((long)(durationInSeconds * 1000), (int)(Mathf.Clamp(amplitude, MinAmplitude, MaxAmplitude) * 255.0f / MaxAmplitude));
#else
            cancelPreviousRumbleToken.Cancel();

            float rightAmplitude = (float)(amplitude - 1) / MaxAmplitude;
            float leftAmplitude = 1.0f - rightAmplitude;

            Gamepad.current.SetMotorSpeeds(leftAmplitude, rightAmplitude);

            StopRumble(durationInSeconds, cancelPreviousRumbleToken.Token).Forget();
#endif
        }
    }
}
