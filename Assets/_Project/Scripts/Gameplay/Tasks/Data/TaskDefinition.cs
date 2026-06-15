using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Tasks.Data
{
    [CreateAssetMenu(fileName = "Task_New", menuName = "Project/Tasks/Task Definition")]
    public class TaskDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }

        [field: Header("UI & Phone")]
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea(2, 4)] public string PhoneDescription { get; private set; }

        [field: Header("Tracking")]
        [Tooltip("ID действия, которое должен выполнить игрок (например: 'wash_stain', 'unpack_box')")]
        [field: SerializeField] public string TargetActionId { get; private set; }

        [Tooltip("Сколько раз нужно выполнить действие для закрытия задачи")]
        [field: SerializeField, Range(1, 20)] public int RequiredAmount { get; private set; } = 1;

        [field: Header("Progression")]
        [field: SerializeField, Range(10, 120)] public int TimeRewardMinutes { get; private set; } = 30;
    }
}
