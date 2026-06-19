using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Tasks.Data
{
    [CreateAssetMenu(fileName = "Task_New", menuName = "NightShift/Tasks/Task Definition")]
    public class TaskDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }

        [field: Header("Localization Keys")]
        [Tooltip("Ключ для названия квеста (напр: task_wash_name)")]
        [field: SerializeField] public string DisplayNameKey { get; private set; }

        [Tooltip("Ключ для описания (напр: task_wash_desc)")]
        [field: SerializeField] public string PhoneDescriptionKey { get; private set; }

        [field: Header("Tracking")]
        [Tooltip("ID действия из интерактива (например: 'wash_stain')")]
        [field: SerializeField] public string TargetActionId { get; private set; }

        [field: SerializeField, Range(1, 20)] public int RequiredAmount { get; private set; } = 1;

        [field: Header("Progression")]
        [field: SerializeField, Range(10, 120)] public int TimeRewardMinutes { get; private set; } = 30;
    }
}
