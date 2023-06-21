using Cysharp.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;

namespace DDEngine.Installer
{
    public static class GameDataInstaller
    {
        public const int ErrorCodeNone = 0;
        public const int ErrorCodeFileNotFound = -1;
        public const int ErrorCodeInstallFailed = -2;
        public const int ErrorCodeCorrupted = -3;
        public const int ErrorCodeDataTooLarge = -4;
        public const int ErorCodeNotDirkDagger = -5;

        private const string InstalledMarkTextFileName = "installed.txt";

        [DllImport("sisinstaller", EntryPoint = "install_dd_game_data")]
        private static extern int InstallNative(string path, string destPath);

        public static bool IsGameDataInstalled(string destPath)
        {
            return File.Exists(Path.Join(destPath, InstalledMarkTextFileName));
        }

        public static async UniTask<int> Install(string path, string destPath)
        {
#if FULL_GAME_IN_RESOURCES
            string[] copyResources = new string[]
            {
                "opes_general",
                "opes_loc-cn",
                "opes_loc-de",
                "opes_loc-en",
                "opes_loc-es",
                "opes_loc-fr",
                "opes_loc-it",
                "opes_loc-tw",
                "protected_general",
                "protected_loc-cn",
                "protected_loc-de",
                "protected_loc-en",
                "protected_loc-es",
                "protected_loc-fr",
                "protected_loc-it",
                "protected_loc-tw",
                "startup_general",
                "startup_loc-cn",
                "startup_loc-de",
                "startup_loc-en",
                "startup_loc-es",
                "startup_loc-fr",
                "startup_loc-it",
                "startup_loc-tw"
            };

            foreach (string filename in copyResources)
            {
                TextAsset result = Resources.Load<TextAsset>("Full/" + filename);
                if (result == null)
                {
                    continue;
                }

                byte[] dataBuffer = result.bytes;
                await File.WriteAllBytesAsync(Path.Join(destPath, filename + ".opes"), dataBuffer);
            }

            // Add a simple text file marking installation
            using (StreamWriter simpleTextFile = File.CreateText(Path.Join(destPath, InstalledMarkTextFileName)))
            {
                simpleTextFile.Write("Installed");
            }

            return GameDataInstaller.ErrorCodeNone;
#else
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                var extractedFilePathAndSize = await GameDataInstallStreamSearch.GetToFile(stream, destPath);
                int errCode = InstallNative(extractedFilePathAndSize.Item1, destPath);

                // Add a simple text file marking installation
                using (StreamWriter simpleTextFile = File.CreateText(Path.Join(destPath, InstalledMarkTextFileName)))
                {
                    simpleTextFile.Write("Installed");
                }

                File.Delete(extractedFilePathAndSize.Item1);
                return errCode;
            }
#endif
        }
    }
}