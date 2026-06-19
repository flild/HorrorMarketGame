using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Phone.UI
{
    public class PhoneTaskItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;

        [Header("Colors")]
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _completedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Серый для выполненных

        public void Setup(string title, string description, bool isCompleted)
        {
            _titleText.text = title;
            _descText.text = description;

            _titleText.color = isCompleted ? _completedColor : _activeColor;
            _descText.color = isCompleted ? _completedColor : _activeColor;

            // Если хочешь зачеркивать текст у выполненных:
            if (isCompleted)
            {
                _titleText.fontStyle = FontStyles.Strikethrough;
            }
            else
            {
                _titleText.fontStyle = FontStyles.Normal;
            }
        }
    }
}
