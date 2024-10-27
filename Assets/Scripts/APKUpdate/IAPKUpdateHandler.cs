using System.Threading.Tasks;

namespace APKUpdate
{
    public interface IAPKUpdateHandler
    {
        Task<bool> CheckVersionUpdate();
        Task<bool> Download(string downloadUrl);
        void Install();
    }
}