using System.Threading.Tasks;
using UnityEngine;

namespace APKUpdate
{
    public class DefaultAPKUpdateHandler : APKUpdateHandlerBase
    {
        public DefaultAPKUpdateHandler(string targetSHA)
        {
            TargetSHA = targetSHA;
        }

        public override async Task<bool> CheckVersionUpdate()
        {
            Debug.Log($"目前版本：{Application.version}，當前平台不是Android，不會執行下載更新apk流程...");
            return true;
        }

        public override void Install()
        {
            // Do nothing.
        }
    }
}
