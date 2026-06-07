using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory.Mop
{
    public class MopVisualController : MonoBehaviour
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

        private SignalBus _signalBus;
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

        private void OnDestroy()
        {
            _signalBus?.TryUnsubscribe<ToolActionSignal>(OnToolAction);
        }

        private void OnToolAction(ToolActionSignal signal)
        {
            // Убеждаемся, что сигнал именно для швабры (ID должен совпадать с тем, что в SO)
            if (signal.ToolId == "mop" && _animator != null)
            {
                _animator.SetBool(IsScrubbingHash, signal.IsActive);
            }
        }

        private void LateUpdate()
        {
            // Делаем анти-клип. 
            // Пускаем луч из точки HoldPoint вперед по направлению взгляда камеры.
            if (Physics.Raycast(transform.parent.position, transform.parent.forward, out RaycastHit hit, _mopLength, _obstacleLayer))
            {
                // Если луч врезался в пол ближе, чем длина швабры, мы сдвигаем саму модельку (_mopRoot) назад к игроку.
                // Таким образом, щетка всегда будет скользить ровно по поверхности пола.
                float overlap = _mopLength - hit.distance;
                _mopRoot.localPosition = new Vector3(0, 0, -(overlap - _floorOffset));
            }
            else
            {
                // Если смотрим в воздух, возвращаем швабру в дефолтное положение
                _mopRoot.localPosition = Vector3.Lerp(_mopRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }
    }
}
