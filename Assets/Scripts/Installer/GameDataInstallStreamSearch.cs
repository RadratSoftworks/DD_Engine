using System.Threading.Tasks;
using System.IO;
using HttpMultipartParser;
using UnityEngine;
using System;

namespace DDEngine.Installer
{
    public class GameDataInstallStreamSearch
    {
        private const string ContentIdStringName = "Content-ID";

        public static async Task<Tuple<string, UInt64>> GetToFile(Stream baseStream, string basePath = null)
        {
            FileStream stream = null;
            string filePath = Path.Join(basePath ?? Application.persistentDataPath, "tempGameData.sis");

            StreamingMultipartFormDataParser binData = new StreamingMultipartFormDataParser(baseStream, boundary: "KBoundary", encoding: System.Text.Encoding.UTF8);
            binData.FileHandler += (name, fileName, type, disposition, buffer, bytes, partNumber, additionalProperties) =>
            {
                if (additionalProperties.TryGetValue(ContentIdStringName, out string contentId) && contentId.Equals("<Game>"))
                {
                    if (stream == null)
                    {
                        stream = new FileStream(filePath, FileMode.OpenOrCreate);
                    }

                    stream.Write(buffer, 0, bytes);
                }
            };

            try
            {
                await binData.RunAsync();
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    stream.Dispose();
                    throw ex;
                }
            }

            if (stream != null)
            {
                var returnResult = new Tuple<string, UInt64>(filePath, (UInt64)stream.Length);
                stream.Dispose();

                return returnResult;
            }

            return null;
        }
    }
}