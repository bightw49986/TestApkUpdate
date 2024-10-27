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
            //updateHandler = new DefaultAPKUpdateHandler();
#endif
            updateHandler = new AndroidAPKUpdateHandler(confirmVersionUrl);

            var checkUpdate = await updateHandler.CheckVersionUpdate();
            if (checkUpdate)
            {
                var downloadResult = await updateHandler.Download(downloadUrl);
                //if (downloadResult)
                //{
                //    updateHandler.Install();
                //}
            }
            logToScreen.enabled = false;
        }
    }
}
