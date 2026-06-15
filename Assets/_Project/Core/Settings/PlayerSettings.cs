using UnityEngine;
using NaughtyAttributes;

namespace Project.Core.Settings
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Project/Settings/PlayerSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Movement")]
        [Min(0)] public float walkSpeed = 5f;
        [Min(0)] public float sprintSpeed = 8f;
        public float gravity = -9.81f;
        [Min(0)] public float stepInterval = 0.5f; // Для будущих шагов

        [Header("Look")]
        [Range(0.01f, 2f)] public float mouseSensitivity = 1f;
        [Min(0)] public float upperLookLimit = 80f;
        [Min(0)] public float lowerLookLimit = 80f;
    }
}