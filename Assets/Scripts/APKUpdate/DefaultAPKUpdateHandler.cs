using System.Threading.Tasks;
using UnityEngine;

namespace APKUpdate
{
    public class DefaultAPKUpdateHandler : IAPKUpdateHandler
    {
        public async Task<bool> CheckVersionUpdate()
        {
            Debug.Log($"目前版本：{Application.version}，當前平台不是Android，不會執行下載更新apk流程...");
            return false;
        }

        public async Task<bool> Download(string downloadUrl)
        {
            return false;
        }

        public void Install()
        {

        }
    }
}
