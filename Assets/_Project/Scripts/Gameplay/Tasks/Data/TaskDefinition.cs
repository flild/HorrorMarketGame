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

        [field: Header("Progression")]
        [Tooltip("Сколько игровых минут 'промотает' таймер при завершении этой задачи в первой фазе смены")]
        [field: SerializeField, Range(10, 120)] public int TimeRewardMinutes { get; private set; } = 30;
    }
}
