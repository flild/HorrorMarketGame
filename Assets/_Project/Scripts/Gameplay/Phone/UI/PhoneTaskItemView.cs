using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Phone.UI
{
    public class PhoneTaskItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _taskText;

        public void Setup(PhoneTaskData data)
        {
            string statusIcon = data.State == NightTaskState.Completed ? "<color=green>✓" : "<color=yellow>○";
            _taskText.text = $"{statusIcon} {data.Definition.DisplayName}</color>\n<size=80%>{data.Definition.PhoneDescription}</size>";
        }
    }
}
