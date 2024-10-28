using System.IO;
using UnityEditor.Android;
using UnityEngine;

public class AndroidGradlePostProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // 將路徑調整到主Gradle的根目錄，若維持原目錄UnityLibrary的話會報錯
        string rootPath = Directory.GetParent(path).FullName;
        string gradlePropertiesPath = Path.Combine(rootPath, "gradle.properties");

        if (File.Exists(gradlePropertiesPath))
        {
            bool hasAndroidX = false;
            bool hasJetifier = false;
            var lines = File.ReadAllLines(gradlePropertiesPath);

            bool lastLineIsEmpty = string.IsNullOrWhiteSpace(lines[lines.Length - 1]);

            foreach (var line in lines)
            {
                if (line.Contains("android.useAndroidX=true"))
                    hasAndroidX = true;
                if (line.Contains("android.enableJetifier=true"))
                    hasJetifier = true;
            }

            using (var writer = new StreamWriter(gradlePropertiesPath, true))
            {
                if (!hasAndroidX)
                {
                    if (!lastLineIsEmpty) 
                        writer.WriteLine(); // 如果最後一行不是空的，寫入換行
                    writer.WriteLine("android.useAndroidX=true");
                }
                if (!hasJetifier)
                {
                    if (!lastLineIsEmpty && hasAndroidX)
                        writer.WriteLine(); // 如果最後一行不是空的且没有添加android.useAndroidX，寫入換行
                    writer.WriteLine("android.enableJetifier=true");
                }
            }

            Debug.Log($"Gradle properties updated with AndroidX and Jetifier settings in: {gradlePropertiesPath}");
        }
        else
        {
            // 如果 gradle.properties 不存在，則創建一個新的
            using (StreamWriter writer = new StreamWriter(gradlePropertiesPath, false))
            {
                writer.WriteLine("android.useAndroidX=true");
                writer.WriteLine("android.enableJetifier=true");
            }
            Debug.Log($"Gradle properties created with AndroidX and Jetifier settings in {gradlePropertiesPath}");
        }
    }
}
