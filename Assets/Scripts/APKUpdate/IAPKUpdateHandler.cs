using System.Threading.Tasks;

namespace APKUpdate
{
    public interface IAPKUpdateHandler
    {
        Task<bool> CheckVersionUpdate();
        Task<bool> Download(string downloadUrl);
        bool CheckApkExist();
        bool VerifyApk();
        void Install();
        string TargetSHA { get; }
        void DeleteApk();
    }
}