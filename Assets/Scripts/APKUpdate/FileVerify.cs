using UnityEngine;

namespace APKUpdate
{
    public class FileVerify : MonoBehaviour
    {
        [SerializeField]
        private string filePath = "";

        public void Start()
        {
            Debug.Log("SHA256: " + GenerateSHA256(filePath));
        }

        public static string GenerateSHA256(string filePath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}