using Cysharp.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

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
        }
    }
}