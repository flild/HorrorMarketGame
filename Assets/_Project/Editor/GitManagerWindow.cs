#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GitManagerWindow : EditorWindow
{
    [Serializable]
    private class GitFileEntry
    {
        public string Path;
        public string StatusCode;
        public string StatusLabel;
        public bool Selected;
        public bool Staged;
        public bool IsUntracked;
        public bool IsIgnored;
    }

    private sealed class GitResult
    {
        public int ExitCode;
        public string Output;
        public string Error;
        public string Combined => string.IsNullOrWhiteSpace(Error) ? Output : $"{Output}\n{Error}".Trim();
        public bool Success => ExitCode == 0;
    }

    private const int PreviewMaxLines = 250;

    private string commitMessage = "Очередной костыль";
    private string statusText = "Окно загружено. Жду команд.";
    private MessageType statusType = MessageType.Info;

    private string repoRoot;
    private string currentBranch = "unknown";
    private string branchSummary = "";
    private string gitVersion = "";
    private bool gitAvailable = true;

    private readonly List<GitFileEntry> entries = new List<GitFileEntry>();
    private List<string> localBranches = new List<string>();
    private int selectedBranchIndex = -1;

    private Vector2 filesScroll;
    private Vector2 logScroll;
    private Vector2 diffScroll;

    private string gitLog = "";
    private string fileFilter = "";
    private string diffText = "";
    private string diffTargetPath = "";

    private bool showOnlySelected = false;
    private bool showStaged = true;
    private bool showUnstaged = true;
    private bool showUntracked = true;
    private bool autoStageSelectedOnCommit = true;
    private bool rememberSelection = true;

    private readonly Dictionary<string, bool> selectionMemory = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    private GUIStyle headerStyle;
    private GUIStyle panelStyle;
    private GUIStyle smallLabelStyle;
    private GUIStyle monoStyle;
    private GUIStyle diffStyle;

    private Color accent = new Color(0.24f, 0.58f, 0.95f);
    private Color accentSoft = new Color(0.18f, 0.35f, 0.55f);
    private Color danger = new Color(0.78f, 0.22f, 0.22f);
    private Color success = new Color(0.18f, 0.62f, 0.28f);
    private Color warning = new Color(0.85f, 0.65f, 0.15f);

    [MenuItem("Tools/Git Дашборд")]
    public static void ShowWindow()
    {
        var window = GetWindow<GitManagerWindow>("Git Boss");
        window.minSize = new Vector2(800, 750);
        window.Show();
    }

    private void OnEnable()
    {
        BuildStyles();
        RefreshRepository(true);
    }

    private void BuildStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
        };

        panelStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(6, 6, 6, 6)
        };

        smallLabelStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };

        monoStyle = new GUIStyle(EditorStyles.textArea)
        {
            font = EditorStyles.label.font,
            wordWrap = false,
            richText = true
        };
    }

    private void OnGUI()
    {
        if (headerStyle == null) BuildStyles();

        DrawTopBar();

        EditorGUILayout.Space(4);
        DrawRepositoryOverview();

        EditorGUILayout.Space(4);
        DrawCommitPanel();

        EditorGUILayout.Space(4);
        DrawFilesPanel();

        EditorGUILayout.Space(4);
        DrawDiffPanel();

        EditorGUILayout.Space(4);
        DrawLogPanel();

        HandleKeyboardShortcuts();
    }

    private void DrawTopBar()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(2);
            GUILayout.Label("Git Manager", headerStyle);
            GUILayout.FlexibleSpace();

            if (gitAvailable)
                DrawChip($"Git {gitVersion}", success);
            else
                DrawChip("Git не найден", danger);
        }

        if (!gitAvailable)
        {
            EditorGUILayout.HelpBox("Git не найден. Убедись, что он добавлен в переменные среды (PATH).", MessageType.Error);
        }
    }

    private void DrawRepositoryOverview()
    {
        using (new EditorGUILayout.VerticalScope(panelStyle))
        {
            GUILayout.Label("Состояние репозитория", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawInfoBox("Корень проекта", string.IsNullOrEmpty(repoRoot) ? "Не найден" : repoRoot);
                DrawInfoBox("Статус ветки", string.IsNullOrEmpty(branchSummary) ? "Чисто" : branchSummary);
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (localBranches.Count > 0 && selectedBranchIndex >= 0 && selectedBranchIndex < localBranches.Count)
                {
                    int newIdx = EditorGUILayout.Popup("Текущая ветка", selectedBranchIndex, localBranches.ToArray(), GUILayout.ExpandWidth(true));
                    if (newIdx != selectedBranchIndex)
                    {
                        if (EditorUtility.DisplayDialog("Смена ветки", $"Точно прыгаем на {localBranches[newIdx]}?", "Да", "Отмена"))
                        {
                            RunGit($"checkout {localBranches[newIdx]}");
                            RefreshRepository(false);
                            AssetDatabase.Refresh();
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"Ветка: {currentBranch}", EditorStyles.boldLabel);
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(statusText, statusType);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Обновить статус", GUILayout.Height(28))) RefreshRepository(false);
                if (GUILayout.Button("Fetch", GUILayout.Height(28))) { RunGit("fetch --all --prune"); RefreshRepository(false); }
                if (GUILayout.Button("Pull", GUILayout.Height(28))) { RunGit("pull"); RefreshRepository(false); AssetDatabase.Refresh(); }
                if (GUILayout.Button("Stash", GUILayout.Height(28))) { RunGit("stash"); RefreshRepository(false); AssetDatabase.Refresh(); }
                if (GUILayout.Button("Pop Stash", GUILayout.Height(28))) { RunGit("stash pop"); RefreshRepository(false); AssetDatabase.Refresh(); }

                if (GUILayout.Button("Открыть папку", GUILayout.Height(28)))
                {
                    if (!string.IsNullOrEmpty(repoRoot) && Directory.Exists(repoRoot))
                        EditorUtility.RevealInFinder(repoRoot);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                autoStageSelectedOnCommit = GUILayout.Toggle(autoStageSelectedOnCommit, "Индексировать выбранные перед коммитом", GUILayout.Width(270));
                showStaged = GUILayout.Toggle(showStaged, "В индексе", GUILayout.Width(80));
                showUnstaged = GUILayout.Toggle(showUnstaged, "Изменены", GUILayout.Width(80));
                showUntracked = GUILayout.Toggle(showUntracked, "Новые", GUILayout.Width(70));
                showOnlySelected = GUILayout.Toggle(showOnlySelected, "Только выделенные", GUILayout.Width(150));
            }
        }
    }

    private void DrawCommitPanel()
    {
        using (new EditorGUILayout.VerticalScope(panelStyle))
        {
            GUILayout.Label("Коммит и Отправка", EditorStyles.boldLabel);
            commitMessage = EditorGUILayout.TextField("Сообщение", commitMessage);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Добавить в индекс (выбранные)", GUILayout.Height(34))) StageSelected();
                if (GUILayout.Button("Убрать из индекса", GUILayout.Height(34))) UnstageSelected();

                GUI.backgroundColor = success;
                if (GUILayout.Button("Commit & Push (--no-verify)", GUILayout.Height(34))) CommitAndPush();
                GUI.backgroundColor = Color.white;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("В индекс (все изменения)", GUILayout.Height(24))) StageAllChanged();
                if (GUILayout.Button("Сбросить индекс (все)", GUILayout.Height(24))) UnstageAll();
            }
        }
    }

    private void DrawFilesPanel()
    {
        using (new EditorGUILayout.VerticalScope(panelStyle))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Файлы для работы", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Отображено: {GetVisibleEntries().Count}", EditorStyles.miniLabel);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                fileFilter = EditorGUILayout.TextField("Поиск:", fileFilter, GUILayout.MinWidth(200));

                if (GUILayout.Button("Выбрать всё", GUILayout.Width(90))) SetSelectionForVisible(true);
                if (GUILayout.Button("Снять всё", GUILayout.Width(90))) SetSelectionForVisible(false);

                GUI.backgroundColor = danger;
                if (GUILayout.Button("Откатить выделенные", GUILayout.Width(150))) DiscardSelected();
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(4);

            using (var scroll = new EditorGUILayout.ScrollViewScope(filesScroll, GUILayout.Height(200)))
            {
                filesScroll = scroll.scrollPosition;
                var visible = GetVisibleEntries();

                if (visible.Count == 0)
                {
                    EditorGUILayout.HelpBox("Пусто. Либо всё закоммичено, либо фильтры отсекли все файлы.", MessageType.Info);
                }
                else
                {
                    foreach (var entry in visible) DrawFileRow(entry);
                }
            }
        }
    }

    private void DrawFileRow(GitFileEntry entry)
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            bool old = entry.Selected;
            entry.Selected = GUILayout.Toggle(entry.Selected, GUIContent.none, GUILayout.Width(18));

            if (old != entry.Selected && rememberSelection)
                selectionMemory[entry.Path] = entry.Selected;

            DrawStatusTag(entry.StatusLabel, entry.Staged ? success : (entry.IsUntracked ? warning : accentSoft));

            GUILayout.Label(entry.Path, EditorStyles.label);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Diff", GUILayout.Width(50))) LoadDiff(entry.Path);
            if (GUILayout.Button("Revert", GUILayout.Width(60))) RevertSingleFile(entry.Path);
        }
    }

    private void DrawDiffPanel()
    {
        using (new EditorGUILayout.VerticalScope(panelStyle))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Просмотр изменений (Diff)", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(string.IsNullOrEmpty(diffTargetPath) ? "Файл не выбран" : diffTargetPath, EditorStyles.miniLabel);
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(diffScroll, GUILayout.MinHeight(150)))
            {
                diffScroll = scroll.scrollPosition;
                EditorGUILayout.TextArea(string.IsNullOrEmpty(diffText) ? "Выбери файл и нажми Diff." : diffText, monoStyle, GUILayout.ExpandHeight(true));
            }
        }
    }

    private void DrawLogPanel()
    {
        using (new EditorGUILayout.VerticalScope(panelStyle))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Консоль Git", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Очистить", GUILayout.Width(90))) gitLog = "";
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(logScroll, GUILayout.Height(100)))
            {
                logScroll = scroll.scrollPosition;
                EditorGUILayout.TextArea(string.IsNullOrEmpty(gitLog) ? "Лог пуст." : gitLog, monoStyle, GUILayout.ExpandHeight(true));
            }
        }
    }

    private void DrawInfoBox(string title, string value)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinHeight(45)))
        {
            GUILayout.Label(title, EditorStyles.miniBoldLabel);
            GUILayout.Label(value, smallLabelStyle);
        }
    }

    private void DrawChip(string text, Color color)
    {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUILayout.Label(text, EditorStyles.whiteMiniLabel, GUILayout.Width(EditorGUIUtility.singleLineHeight * 7.5f));
        GUI.backgroundColor = oldColor;
    }

    private void DrawStatusTag(string text, Color color)
    {
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUILayout.Label($" {text} ", EditorStyles.whiteMiniLabel, GUILayout.Width(92));
        GUI.backgroundColor = prev;
    }

    private void HandleKeyboardShortcuts()
    {
        var e = Event.current;
        if (e.type != EventType.KeyDown) return;
        if (e.control && e.keyCode == KeyCode.R)
        {
            RefreshRepository(false);
            e.Use();
        }
    }

    private void RefreshRepository(bool initial)
    {
        repoRoot = GetRepoRoot();
        gitVersion = GetGitVersion();
        gitAvailable = !string.IsNullOrEmpty(gitVersion);

        if (!gitAvailable)
        {
            SetStatus("Git не найден. Команды работать не будут.", MessageType.Error);
            return;
        }

        if (string.IsNullOrEmpty(repoRoot))
        {
            SetStatus("Папка .git не найдена. Это вообще репозиторий?", MessageType.Warning);
            entries.Clear();
            return;
        }

        UpdateBranches();

        var status = RunGit("status --porcelain=v1 --branch");
        ParseStatus(status.Output);
        SetStatus(initial ? "Репозиторий загружен." : "Статус обновлён.", entries.Count == 0 ? MessageType.Info : MessageType.Warning);
    }

    private void UpdateBranches()
    {
        var branchOutput = RunGit("branch").Output;
        if (string.IsNullOrWhiteSpace(branchOutput)) return;

        localBranches = branchOutput.Split('\n')
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrEmpty(b))
            .Select(b => b.StartsWith("*") ? b.Substring(1).Trim() : b)
            .ToList();

        selectedBranchIndex = localBranches.IndexOf(currentBranch);
    }

    private void ParseStatus(string statusOutput)
    {
        entries.Clear();

        if (string.IsNullOrWhiteSpace(statusOutput))
        {
            currentBranch = "unknown";
            branchSummary = "";
            return;
        }

        var lines = statusOutput.Replace("\r\n", "\n").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        ParseBranchLine(lines[0]);

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.Length < 4) continue;

            var code = line.Substring(0, 2);
            var path = line.Substring(3).Trim();
            if (path.Contains(" -> ")) path = path.Split(new[] { " -> " }, StringSplitOptions.None).Last();

            var entry = new GitFileEntry
            {
                Path = path,
                StatusCode = code,
                StatusLabel = DescribeStatus(code),
                Staged = IsStaged(code),
                IsUntracked = code == "??",
                IsIgnored = code == "!!"
            };

            if (rememberSelection && selectionMemory.TryGetValue(entry.Path, out bool remembered)) entry.Selected = remembered;
            if (entry.IsUntracked && !showUntracked) continue;

            entries.Add(entry);
        }
    }

    private void ParseBranchLine(string branchLine)
    {
        if (!branchLine.StartsWith("##"))
        {
            currentBranch = "unknown";
            branchSummary = branchLine;
            return;
        }

        var raw = branchLine.Substring(2).Trim();
        branchSummary = raw;
        var branchPart = raw;
        var spaceIndex = raw.IndexOf(" [", StringComparison.Ordinal);

        if (spaceIndex >= 0) branchPart = raw.Substring(0, spaceIndex);

        if (branchPart.Contains("...")) currentBranch = branchPart.Split(new[] { "..." }, StringSplitOptions.None)[0].Trim();
        else if (branchPart.StartsWith("No commits yet on ", StringComparison.OrdinalIgnoreCase)) currentBranch = branchPart.Replace("No commits yet on ", "").Trim();
        else currentBranch = branchPart.Trim();
    }

    private string DescribeStatus(string code)
    {
        if (code == "??") return "NEW";
        if (code == "!!") return "IGNORED";
        char index = code[0], work = code[1];
        if (index == 'R' || work == 'R') return "RENAMED";
        if (index == 'A' || work == 'A') return "ADDED";
        if (index == 'D' || work == 'D') return "DELETED";
        if (index == 'C' || work == 'C') return "COPIED";
        if (index == 'U' || work == 'U') return "CONFLICT";
        if (index == 'M' || work == 'M') return "MODIFIED";
        return code.Trim();
    }

    private bool IsStaged(string code) => code != "??" && code != "!!" && code[0] != ' ';

    private List<GitFileEntry> GetVisibleEntries()
    {
        IEnumerable<GitFileEntry> res = entries;
        if (!showStaged) res = res.Where(e => !e.Staged);
        if (!showUnstaged) res = res.Where(e => e.Staged || e.IsUntracked);
        if (!showUntracked) res = res.Where(e => !e.IsUntracked);
        if (!string.IsNullOrWhiteSpace(fileFilter)) res = res.Where(e => e.Path.IndexOf(fileFilter, StringComparison.OrdinalIgnoreCase) >= 0);
        if (showOnlySelected) res = res.Where(e => e.Selected);
        return res.OrderByDescending(e => e.Selected).ThenByDescending(e => e.Staged).ThenBy(e => e.Path).ToList();
    }

    private void SetSelectionForVisible(bool value)
    {
        foreach (var entry in GetVisibleEntries())
        {
            entry.Selected = value;
            if (rememberSelection) selectionMemory[entry.Path] = value;
        }
    }

    private void StageSelected()
    {
        var selected = entries.Where(e => e.Selected).ToList();
        if (selected.Count == 0) { SetStatus("Выдели файлы сначала.", MessageType.Warning); return; }

        foreach (var file in selected) RunGit($"add -- {QuotePath(file.Path)}");
        SetStatus($"Добавлено в индекс: {selected.Count} файлов.", MessageType.Info);
        RefreshRepository(false);
    }

    private void StageAllChanged()
    {
        RunGit("add -A");
        SetStatus("Все изменения закинуты в индекс.", MessageType.Info);
        RefreshRepository(false);
    }

    private void UnstageSelected()
    {
        var selected = entries.Where(e => e.Selected).ToList();
        if (selected.Count == 0) return;

        foreach (var file in selected) RunGit($"reset HEAD -- {QuotePath(file.Path)}");
        SetStatus($"Убрано из индекса: {selected.Count} файлов.", MessageType.Info);
        RefreshRepository(false);
    }

    private void UnstageAll()
    {
        RunGit("reset HEAD -- .");
        SetStatus("Все файлы убраны из индекса.", MessageType.Info);
        RefreshRepository(false);
    }

    private void RevertSingleFile(string path)
    {
        if (EditorUtility.DisplayDialog("Откат файла", $"Точно откатить {path}?", "Снести", "Отмена"))
        {
            var res = RunGit($"restore -- {QuotePath(path)}");
            if (!res.Success) RunGit($"checkout -- {QuotePath(path)}");
            if (entries.Any(e => e.Path == path && e.IsUntracked)) RunGit($"clean -f -- {QuotePath(path)}");

            AssetDatabase.Refresh();
            RefreshRepository(false);
        }
    }

    private void CommitAndPush()
    {
        if (string.IsNullOrWhiteSpace(commitMessage))
        {
            SetStatus("Напиши сообщение коммита, не беси.", MessageType.Warning);
            return;
        }

        if (autoStageSelectedOnCommit)
        {
            var selected = entries.Where(e => e.Selected).ToList();
            if (selected.Count > 0) StageSelected(); else StageAllChanged();
        }

        RefreshRepository(false);

        if (!entries.Any(e => e.Staged))
        {
            SetStatus("Нечего коммитить. Индекс пуст.", MessageType.Warning);
            return;
        }

        // Обход хуков при коммите (если есть локальные линтеры)
        var commitRes = RunGit($"commit --no-verify -m {Quote(commitMessage)}");
        if (!commitRes.Success)
        {
            SetStatus("Ошибка коммита. Чекай лог.", MessageType.Error);
            return;
        }

        // Обход хуков при пуше (шлем LFS и прочие проверки)
        var pushRes = RunGit("push --no-verify");
        if (!pushRes.Success)
            SetStatus("Скоммитилось, но Push обосрался. Чекай лог.", MessageType.Error);
        else
        {
            SetStatus("Успешно: Commit и Push выполнены без валидации (--no-verify).", MessageType.Info);
            commitMessage = "";
        }

        AssetDatabase.Refresh();
        RefreshRepository(false);
    }

    private void DiscardSelected()
    {
        var selected = entries.Where(e => e.Selected).ToList();
        if (selected.Count == 0) return;

        if (!EditorUtility.DisplayDialog("Удаление изменений", "Точно убить все выбранные локальные изменения? Восстановить будет невозможно.", "Жги", "Я передумал"))
            return;

        foreach (var file in selected)
        {
            RunGit($"restore -- {QuotePath(file.Path)}");
            RunGit($"checkout -- {QuotePath(file.Path)}");
            if (file.IsUntracked) RunGit($"clean -f -- {QuotePath(file.Path)}");
        }

        AssetDatabase.Refresh();
        RefreshRepository(false);
        SetStatus("Выбранные изменения уничтожены.", MessageType.Warning);
    }

    private void LoadDiff(string path)
    {
        diffTargetPath = path;
        var selected = entries.FirstOrDefault(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase));

        if (selected != null && selected.IsUntracked)
        {
            diffText = "Файл новый (Untracked). Git пока не знает с чем его сравнивать.";
            return;
        }

        var result = RunGit($"diff --no-ext-diff -- {QuotePath(path)}");
        diffText = result.Combined;

        if (string.IsNullOrWhiteSpace(diffText))
        {
            var cached = RunGit($"diff --cached --no-ext-diff -- {QuotePath(path)}");
            diffText = string.IsNullOrWhiteSpace(cached.Combined) ? "Нет изменений." : cached.Combined;
        }

        var lines = diffText.Replace("\r\n", "\n").Split('\n');
        if (lines.Length > PreviewMaxLines)
            diffText = string.Join("\n", lines.Take(PreviewMaxLines)) + "\n... (Diff слишком большой, обрезан для превью)";
    }

    private void SetStatus(string msg, MessageType type)
    {
        statusText = msg;
        statusType = type;
        gitLog = $"[{DateTime.Now:HH:mm:ss}] {msg}\n{gitLog}";
        Repaint();
    }

    private string GetRepoRoot()
    {
        try
        {
            // Самый надежный способ для Unity - проверить папку .git в корне проекта
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            if (Directory.Exists(Path.Combine(projectRoot, ".git")))
                return projectRoot;

            // Фолбэк на команду git, если репо лежит выше
            var result = RunGit($"-C {QuotePath(projectRoot)} rev-parse --show-toplevel", silent: true);
            return result.Success ? result.Output.Trim() : null;
        }
        catch { return null; }
    }

    private string GetGitVersion()
    {
        try
        {
            var result = RunGit("--version", silent: true);
            return result.Success ? result.Combined.Replace("git version", "").Trim() : null;
        }
        catch { return null; }
    }

    private GitResult RunGit(string arguments, bool silent = false)
    {
        var root = string.IsNullOrEmpty(repoRoot) ? Path.GetFullPath(Path.Combine(Application.dataPath, "..")) : repoRoot;

        var info = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {
            using (var process = Process.Start(info))
            {
                if (process == null) return new GitResult { ExitCode = -1, Error = "Процесс не запущен." };

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                var result = new GitResult { ExitCode = process.ExitCode, Output = output?.Trim() ?? "", Error = error?.Trim() ?? "" };

                if (!silent && !string.IsNullOrWhiteSpace(result.Combined))
                    gitLog = $"> git {arguments}\n{result.Combined}\n{gitLog}";

                return result;
            }
        }
        catch (Exception ex)
        {
            if (!silent) gitLog = $"> git {arguments}\n{ex.Message}\n{gitLog}";
            return new GitResult { ExitCode = -1, Error = ex.Message };
        }
    }

    private static string Quote(string value) => value == null ? "\"\"" : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    private static string QuotePath(string path) => Quote(path);
}
#endif