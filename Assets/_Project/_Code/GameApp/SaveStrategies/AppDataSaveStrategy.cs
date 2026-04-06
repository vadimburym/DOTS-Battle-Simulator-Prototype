using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using _Project._Code.Core.Abstractions;
using _Project._Code.Infrastructure;
using UnityEngine;

namespace _Project._Code.GameApp.SaveStrategies
{
    [Serializable]
    public sealed class AppDataSaveStrategy : ISaveStrategy
    {
        [SerializeField] private string _folderName = "DOTS RTS Prototype";
        [SerializeField] private string _fileName = "save";
        
        private string _folderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _folderName);
        private const string FILE_FORMAT = ".dat";
        private const int BufferSize = 65536;

        void ISaveStrategy.DeleteRepository()
        {
            var fileName = _fileName + FILE_FORMAT;
            var filePath = Path.Combine(_folderPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        async UniTask<Dictionary<string, string>> ISaveStrategy.LoadRepository()
        {
            var fileName = _fileName + FILE_FORMAT;
            var filePath = Path.Combine(_folderPath, fileName);

            if (!File.Exists(filePath))
            {
                return new();
            }

            byte[] loadedData;
            using (FileStream fs = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                loadedData = new byte[fs.Length];
                int bytesRead = 0;
                int totalBytes = (int)fs.Length;

                while (bytesRead < totalBytes)
                {
                    int read = await fs.ReadAsync(loadedData, bytesRead, totalBytes - bytesRead);
                    if (read == 0)
                        throw new IOException("Unexpected end of file");
                    bytesRead += read;
                }
            }

            string jsonData = await DataEncryptUtils.DecryptStringAsync(loadedData);

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
        }

        async UniTask ISaveStrategy.SaveRepository(Dictionary<string, string> repository)
        {
            var jsonData = JsonConvert.SerializeObject(repository);

            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            var fileName = _fileName + FILE_FORMAT;
            var filePath = Path.Combine(_folderPath, fileName);

            byte[] encryptedData = await DataEncryptUtils.EncryptStringAsync(jsonData);

            using (FileStream fs = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: BufferSize,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await fs.WriteAsync(encryptedData, 0, encryptedData.Length);
            }
        }
    }
}