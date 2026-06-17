using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Zenject;

namespace Assets._Project.Core
{
    public enum Language
    {
        RU,
        EN
    }

    public interface ILocalizationService
    {
        Language CurrentLanguage { get; }
        void SetLanguage(Language language);
        string GetText(string key, params object[] args);
    }

    public class LocalizationService : ILocalizationService, IInitializable
    {
        private readonly SignalBus _signalBus;

        private Language _currentLanguage = Language.RU;
        private Dictionary<string, string[]> _dictionary = new();

        public Language CurrentLanguage => _currentLanguage;

        public LocalizationService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            LoadDictionary();
            //todo доделать
            // По-хорошему, здесь нужно читать язык из PlayerPrefs (настроек игры)
        }

        public void SetLanguage(Language language)
        {
            if (_currentLanguage == language) return;

            _currentLanguage = language;
            // Кидаем сигнал, чтобы все открытые окна (Телефон, HUD) обновили тексты
            _signalBus.Fire<LanguageChangedSignal>();
        }

        public string GetText(string key, params object[] args)
        {
            if (!_dictionary.TryGetValue(key, out var translations))
            {
                Debug.LogWarning($"[Localization] Не найден ключ: {key}");
                return $"[{key}]";
            }

            int langIndex = (int)_currentLanguage;
            if (langIndex >= translations.Length || string.IsNullOrEmpty(translations[langIndex]))
            {
                // Фолбек на английский, если перевода нет
                langIndex = (int)Language.EN;
            }

            string text = translations[langIndex];

            // Если переданы аргументы (названия кнопок, имена предметов) - форматируем
            if (args != null && args.Length > 0)
            {
                try
                {
                    text = string.Format(text, args);
                }
                catch (FormatException)
                {
                    Debug.LogError($"[Localization] Ошибка формата в ключе {key}. Текст: {text}");
                }
            }

            return text;
        }

        private void LoadDictionary()
        {
            _dictionary.Clear();

            TextAsset csvFile = Resources.Load<TextAsset>("localization");
            if (csvFile == null)
            {
                Debug.LogError("[Localization] Файл localization.csv не найден в Resources!");
                return;
            }

            string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Регулярка для CSV, которая игнорирует запятые внутри кавычек
            Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            for (int i = 1; i < lines.Length; i++) // Пропускаем строку с заголовками (i = 1)
            {
                string[] row = csvParser.Split(lines[i]);
                if (row.Length < 3) continue; // Key, RU, EN

                string key = row[0].Trim();

                // Убираем кавычки, если текст был обернут в них из-за запятых в экселе
                string ruText = row[1].Trim().Trim('"');
                string enText = row[2].Trim().Trim('"');

                _dictionary[key] = new[] { ruText, enText };
            }

            Debug.Log($"[Localization] Загружено ключей: {_dictionary.Count}");
        }
    }

    public struct LanguageChangedSignal { }
}