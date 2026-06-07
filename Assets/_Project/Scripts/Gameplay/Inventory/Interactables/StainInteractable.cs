using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interactables
{
    public class StainInteractable : InteractableObject, IInteractable
    {
        [Header("Stain Settings")]
        [Tooltip("ID швабры, чтобы пятно нельзя было отмыть коробкой")]
        [SerializeField] private string _mopId = "mop";
        [SerializeField] private float _washTime = 3f; // За сколько секунд отмывается

        [Header("Visuals")]
        [SerializeField] private MeshRenderer _renderer;

        private IEquipmentService _equipment;
        private SignalBus _signalBus;
        private CancellationTokenSource _washCts;
        private bool _isWashing;
        private Material _materialInstance;
        private float _currentWashProgress = 0f;

        [Inject]
        public void Construct(IEquipmentService equipment, SignalBus signalBus)
        {
            _equipment = equipment;
            _signalBus = signalBus;
        }

        private void Awake()
        {
            // Создаем уникальный инстанс материала, чтобы не отмыть все пятна на уровне разом
            if (_renderer != null) _materialInstance = _renderer.material;
        }

        public override string InteractionPrompt
        {
            get
            {
                if (_equipment.CurrentItem != null && _equipment.CurrentItem.Id == _mopId)
                    return "Отмыть пятно [Зажать E]";

                return "Грязное пятно (Нужна швабра)";
            }
        }

        public override void Interact()
        {
            if (_isWashing) return;

            if (_equipment.CurrentItem != null && _equipment.CurrentItem.Id == _mopId)
            {
                _washCts = new CancellationTokenSource();
                WashRoutine(_washCts.Token).Forget();
            }
        }

        public override void EndInteract()
        {
            if (_washCts != null)
            {
                _washCts.Cancel();
                _washCts.Dispose();
                _washCts = null;
            }
        }

        private async UniTaskVoid WashRoutine(CancellationToken token)
        {
            _isWashing = true;

            // Кричим швабре: "Начинай махать!"
            _signalBus.Fire(new ToolActionSignal { ToolId = _mopId, IsActive = true });

            try
            {
                while (_currentWashProgress < 1f)
                {
                    _currentWashProgress += Time.deltaTime / _washTime;

                    // Понижаем альфу материала (исчезновение)
                    if (_materialInstance != null)
                    {
                        Color c = _materialInstance.color;
                        c.a = Mathf.Lerp(1f, 0f, _currentWashProgress);
                        _materialInstance.color = c;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                // Пятно отмыто на 100%
                Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
                // Игрок отпустил кнопку
            }
            finally
            {
                _isWashing = false;
                // Кричим швабре: "Стоп!"
                _signalBus.Fire(new ToolActionSignal { ToolId = _mopId, IsActive = false });
            }
        }
    }
}
