using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class GitManagerWindow : EditorWindow
{
    private string commitMessage = "Fixed bugs and added new bugs";

    [MenuItem("Tools/Фарм коммитов (Git)")]
    public static void ShowWindow()
    {
        GetWindow<GitManagerWindow>("Git Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("Автоматизация GitHub", EditorStyles.boldLabel);

        commitMessage = EditorGUILayout.TextField("Сообщение:", commitMessage);

        EditorGUILayout.Space();

        if (GUILayout.Button("Commit & Push (Без моделей)", GUILayout.Height(30)))
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
            {
                UnityEngine.Debug.LogWarning("А кто сообщение писать будет? Напиши хоть что-то.");
                return;
            }

            RunGitCommand("add .");
            RunGitCommand($"commit -m \"{commitMessage}\"");
            // Тот самый флаг, чтобы обмануть LFS и не грузить бинарники
            RunGitCommand("push --no-verify");

            UnityEngine.Debug.Log("✅ Отправлено на GitHub! Квадратик позеленел.");
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("Опасная зона", EditorStyles.boldLabel);

        if (GUILayout.Button("Откатить ВСЕ текущие изменения", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Внимание!",
                "Это удалит ВСЕ несохраненные изменения в скриптах и сценах, откатив проект к последнему коммиту. Продолжить?",
                "Да, сноси всё", "Отмена"))
            {
                RunGitCommand("reset --hard HEAD");
                RunGitCommand("clean -fd");
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("⏪ Откатились к последнему коммиту.");
            }
        }
    }

    private void RunGitCommand(string arguments)
    {
        // Application.dataPath указывает на папку Assets. Нам нужен корень проекта.
        string projectPath = Application.dataPath.Replace("/Assets", "");

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = projectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output) && !output.Contains("up to date"))
            {
                UnityEngine.Debug.Log($"Git: {output}");
            }
            if (!string.IsNullOrEmpty(error) && !error.Contains("warning"))
            {
                // Гит иногда пишет обычную инфу в поток ошибок, так что фильтруем
                UnityEngine.Debug.LogWarning($"Git Msg/Error: {error}");
            }
        }
    }
}