using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace APKUpdate
{
    public abstract class APKUpdateHandlerBase : IAPKUpdateHandler
    {
        protected string FilePath => Path.Combine(Application.persistentDataPath, Application.productName) + ".apk";

        public string TargetSHA { get; protected set; }

        public bool CheckApkExist()
        {
            return File.Exists(FilePath);
        }

        public async Task<bool> Download(string downloadUrl)
        {
            try
            {
                downloadUrl = ModifyDownloadUrl(downloadUrl);

                // 使用 UnityWebRequest 下載文件，並設置使用流式寫入到文件
                UnityWebRequest request = UnityWebRequest.Get(downloadUrl);

                // 使用流式文件處理器
                request.downloadHandler = new DownloadHandlerFile(FilePath);

                // 開始下載並等待完成
                var operation = request.SendWebRequest();

                // 當下載進行中時
                while (!operation.isDone)
                {
                    float progress = request.downloadedBytes;
                    Debug.Log($"[DOWNLOAD]已下載：{progress}");

                    // 等待一小段時間以打印進度
                    await Task.Delay(100);
                }

                // 檢查下載結果
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[DOWNLOAD]APK 下載完成，存放在: " + FilePath);
                    return true;
                }

                Debug.LogError("[DOWNLOAD]下載失敗: " + request.error);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DOWNLOAD]下載過程中發生錯誤: {e.Message}");
                return false;
            }
        }

        protected virtual string ModifyDownloadUrl(string downloadUrl) { return downloadUrl; }

        public bool VerifyApk()
        {
            try
            {
                Debug.Log($"[VERIFY_APK]Check apk SHA...");
                using (FileStream stream = File.OpenRead(FilePath))
                {
                    // 使用 SHA256 算法
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(stream);
                        // 將雜湊值轉換為十六進位字串
                        string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                        Debug.Log($"[VERIFY_APK]FileSHA:{fileHash}");
                        Debug.Log($"[VERIFY_APK]TargetSHA:{TargetSHA}");
                        return fileHash == TargetSHA;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"產生 SHA-256 雜湊值時出現錯誤: {ex.Message}");
                return false;
            }
        }

        public void DeleteApk()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Debug.Log($"[DELETE_APK]刪除 APK 檔案: {FilePath}");
            }
        }

        public abstract void Install();
        public abstract Task<bool> CheckVersionUpdate();
    }
}
