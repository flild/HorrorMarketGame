using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory.Mop
{
    public class MopVisualController : MonoBehaviour, IToolVisual
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _mopRoot; // Пустышка внутри префаба, в которой лежит сама моделька

        [Header("Anti-Clip Settings")]
        [Tooltip("Слой пола/стен")]
        [SerializeField] private LayerMask _obstacleLayer;
        [Tooltip("Физическая длина швабры в метрах (от рук до щетки)")]
        [SerializeField] private float _mopLength = 1.8f;
        [Tooltip("Отступ от пола, чтобы щетка не утопала в текстурах")]
        [SerializeField] private float _floorOffset = 0.1f;

        [SerializeField]
        private Rigidbody _rootRb;
        private SignalBus _signalBus;
        private string _myToolId; // Сюда будем сохранять ID из SO
        private static readonly int IsScrubbingHash = Animator.StringToHash("IsScrubbing");

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            _signalBus.Subscribe<ToolActionSignal>(OnToolAction);
        }
        public void Initialize(string toolId)
        {
            _myToolId = toolId;
        }

        private void OnDestroy()
        {
            _signalBus?.TryUnsubscribe<ToolActionSignal>(OnToolAction);
        }

        private void OnToolAction(ToolActionSignal signal)
        {
            // Теперь префаб реагирует только на свой динамический ID
            if (signal.ToolId == _myToolId && _animator != null)
            {
                _animator.SetBool(IsScrubbingHash, signal.IsActive);
            }
        }

        private void LateUpdate()
        {
            // ФИКС: Если физика включена (швабра валяется), вырубаем логику анти-клипа
            if (_rootRb != null && !_rootRb.isKinematic)
            {
                // Опционально: плавно возвращаем сетку в центр, чтобы она не застыла криво
                _mopRoot.localPosition = Vector3.Lerp(_mopRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
                return;
            }

            // Твой старый код анти-клипа:
            if (Physics.Raycast(transform.parent.position, transform.parent.forward, out RaycastHit hit, _mopLength, _obstacleLayer))
            {
                float overlap = _mopLength - hit.distance;
                _mopRoot.localPosition = new Vector3(0, 0, -(overlap - _floorOffset));
            }
            else
            {
                _mopRoot.localPosition = Vector3.Lerp(_mopRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }
    }
}
