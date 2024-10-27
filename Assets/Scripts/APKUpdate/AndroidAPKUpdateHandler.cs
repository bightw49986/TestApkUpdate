using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


namespace APKUpdate
{
    public class AndroidAPKUpdateHandler : IAPKUpdateHandler
    {
        private string FilePath => Path.Combine(Application.persistentDataPath, Application.productName) + ".apk";

        private readonly string versionApiUrl;
        private string latestVersion;

        public AndroidAPKUpdateHandler(string confirmVersionUrl)
        {
            versionApiUrl = confirmVersionUrl;
        }

        public async Task<bool> CheckVersionUpdate()
        {
            var currentVersion = Application.version;
            latestVersion = currentVersion;

            try
            {
                UnityWebRequest request = UnityWebRequest.Get(versionApiUrl);
                await request.SendWebRequest();

                // 檢查是否有錯誤
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[CONFIRM_VERSION]Error getting version: " + request.error);
                    return false;
                }
                else
                {
                    // 取得回傳的JSON字串
                    string jsonResponse = request.downloadHandler.text;

                    Debug.Log("[CONFIRM_VERSION]Response: " + jsonResponse);

                    // 使用 Newtonsoft.Json 解析 JSON
                    VersionResponse versionResponse = JsonConvert.DeserializeObject<VersionResponse>(jsonResponse);

                    // 檢查 ErrorCode 並處理結果
                    if (versionResponse.ErrorCode == 0)
                    {
                        latestVersion = versionResponse.Version;
                        Debug.Log($"[CONFIRM_VERSION]Latest version: {latestVersion}/ Current version: {currentVersion}");

                        return latestVersion != currentVersion;
                    }
                    else
                    {
                        Debug.LogError("[CONFIRM_VERSION]Error in response: ErrorCode = " + versionResponse.ErrorCode);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[CONFIRM_VERSION]檢查版本過程發生錯誤: " + e.Message);
                return false;
            }
        }

        public async Task<bool> Download(string downloadUrl)
        {
            try
            {
                downloadUrl = downloadUrl + "?version=" + latestVersion;

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

        public void Install()
        {
            try
            {
                Debug.Log("[INSTALL]開始安裝新版本...");
                AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");

                // 使用 Intent.ACTION_VIEW 來設置打開 APK 文件
                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_VIEW"));

                // 設置 FileProvider 路徑
                AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

                // 使用 FileProvider 獲取安全的 URI
                AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
                AndroidJavaObject apkUri = fileProviderClass.CallStatic<AndroidJavaObject>(
                    "getUriForFile",
                    currentActivity,
                    currentActivity.Call<string>("getPackageName") + ".provider", // FileProvider authorities
                    new AndroidJavaObject("java.io.File", FilePath)
                );

                // 設置 Data 和 Type
                intent.Call<AndroidJavaObject>("setDataAndType", apkUri, "application/vnd.android.package-archive");

                // 設置 flags 允許 URI 訪問權限
                intent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
                intent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK"));

                // 啟動 APK 安裝
                currentActivity.Call("startActivity", intent);
            }
            catch (Exception e)
            {
                Debug.LogError("[INSTALL]Error during APK installation: " + e.Message);
            }
        }

        [System.Serializable]
        public class VersionResponse
        {
            public int ErrorCode;
            public string Version;
        }
    }
}