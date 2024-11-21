using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace APKUpdate
{
    public class APKUpdateFlow : MonoBehaviour
    {
        [SerializeField]
        private LogToScreen logToScreen;

        [SerializeField]
        private string confirmVersionUrl = "http://35.236.150.38/api/confirmVersion.php";

        [SerializeField]
        private string downloadUrl = "http://35.236.150.38/api/download.php";

        [SerializeField]
        private string testSHA = "";

        [SerializeField]
        private string testDownloadUrl = "";


        private IAPKUpdateHandler updateHandler;


        private async void Awake()
        {
            await StartAPKUpdateFlow();
        }

        private async Task StartAPKUpdateFlow()
        {
            logToScreen.enabled = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            updateHandler = new AndroidAPKUpdateHandler(confirmVersionUrl);
#else
            downloadUrl = testDownloadUrl;
            updateHandler = new DefaultAPKUpdateHandler(testSHA);
#endif

            var checkUpdate = await updateHandler.CheckVersionUpdate();

            if (checkUpdate)
            {
                if (!updateHandler.CheckApkExist())
                {
                    var downloadResult = await updateHandler.Download(downloadUrl);
                    if (!downloadResult)
                    {
                        Debug.LogError("下載失敗，結束更新流程，進入遊戲");
                        return;
                    }
                }

                if (updateHandler.VerifyApk())
                {
                    updateHandler.Install();
                }
                else
                {

                }
            }
            else
            {
                if (updateHandler.CheckApkExist())
                {
                    updateHandler.DeleteApk();
                }
                Debug.Log("已經是最新版本，進入遊戲");
            }
            logToScreen.enabled = false;
        }
    }
}
