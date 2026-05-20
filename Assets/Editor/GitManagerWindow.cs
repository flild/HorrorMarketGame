using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class GitManagerWindow : EditorWindow
{
    private string commitMessage = "Очередной фикс багов";
    private string statusText = "Готов к работе. Жду команд.";
    private MessageType statusType = MessageType.Info;
    private string gitLog = "";
    private Vector2 logScroll;

    [MenuItem("Tools/Git Дашборд")]
    public static void ShowWindow()
    {
        var window = GetWindow<GitManagerWindow>("Git Boss");
        window.minSize = new Vector2(450, 550);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // --- Блок статуса ---
        EditorGUILayout.HelpBox(statusText, statusType);
        GUILayout.Space(10);

        // --- Блок повседневной рутины ---
        GUILayout.Label("Отправка на GitHub", EditorStyles.boldLabel);
        commitMessage = EditorGUILayout.TextField("Сообщение коммита:", commitMessage);

        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f); // Зеленоватая кнопка
        if (GUILayout.Button("Commit & Push (Без тяжелых моделей)", GUILayout.Height(40)))
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
            {
                SetStatus("Напиши текст коммита, не ленись!", MessageType.Warning);
                return;
            }

            SetStatus("Добавляю файлы и пушу на сервер...", MessageType.Info);

            RunGitCommand("add .");
            string commitRes = RunGitCommand($"commit -m \"{commitMessage}\"");
            string pushRes = RunGitCommand("push --no-verify");

            if (pushRes.Contains("error") || pushRes.Contains("fatal"))
            {
                SetStatus("Ошибка при пуше! Смотри лог ниже.", MessageType.Error);
            }
            else
            {
                SetStatus($"Успешно запушено! Квадратик позеленел.\n{commitRes}", MessageType.Info);
                commitMessage = ""; // Очищаем поле после успеха
            }
        }
        GUI.backgroundColor = Color.white; // Возвращаем цвет

        GUILayout.Space(15);
        DrawUILine(Color.gray);
        GUILayout.Space(15);

        // --- Блок полезных утилит ---
        GUILayout.Label("Утилиты (на будущее)", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Проверить статус (Status)", GUILayout.Height(30)))
        {
            string res = RunGitCommand("status -s");
            SetStatus(string.IsNullOrEmpty(res) ? "Нет измененных файлов. Работай давай." : $"Измененные файлы:\n{res}", MessageType.Info);
        }

        if (GUILayout.Button("Стянуть изменения (Pull)", GUILayout.Height(30)))
        {
            SetStatus("Стягиваю с GitHub...", MessageType.Info);
            string res = RunGitCommand("pull");
            SetStatus($"Результат Pull:\n{res}", MessageType.Info);
            AssetDatabase.Refresh(); // Обновляем юнити, если прилетели новые скрипты
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);
        DrawUILine(Color.gray);
        GUILayout.Space(15);

        // --- ОПАСНАЯ ЗОНА ---
        GUILayout.Label("Опасная зона (Резет и откат)", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f); // Красная кнопка
        if (GUILayout.Button("УДАЛИТЬ несохраненное (Hard Reset)", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Внимание! Ты уверен?",
                "Это безвозвратно удалит ВСЕ твои текущие изменения в скриптах и сценах, откатив проект к последнему коммиту. Точно сносим?",
                "Да, гори оно всё", "Стой, отмена!"))
            {
                RunGitCommand("reset --hard HEAD");
                RunGitCommand("clean -fd");
                AssetDatabase.Refresh();
                SetStatus("Откатились к последнему коммиту. Начинай сначала.", MessageType.Warning);
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(20);

        // --- Лог консоли Git ---
        GUILayout.Label("Консоль Git:", EditorStyles.boldLabel);
        logScroll = EditorGUILayout.BeginScrollView(logScroll, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
        GUILayout.TextArea(gitLog, GUI.skin.label);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Очистить лог"))
        {
            gitLog = "";
        }
    }

    private void SetStatus(string msg, MessageType type)
    {
        statusText = msg;
        statusType = type;
        // Добавляем в лог для истории
        gitLog = $"[{System.DateTime.Now:HH:mm:ss}] {msg}\n{gitLog}";
    }

    private string RunGitCommand(string arguments)
    {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string outputText = "";

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = projectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            outputText = output + error; // Гит часто пишет инфу в поток ошибок

            if (!string.IsNullOrWhiteSpace(outputText))
            {
                gitLog = $"> git {arguments}\n{outputText}\n{gitLog}";
            }
        }
        return outputText.Trim();
    }

    // Вспомогательный метод для рисования полосочки
    private void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
}