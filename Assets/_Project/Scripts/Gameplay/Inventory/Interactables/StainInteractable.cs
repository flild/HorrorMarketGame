using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interactables
{
    public class StainInteractable : InteractableObject, IInteractable
    {
        [Header("Stain Settings")]
        [SerializeField] private float _washTime = 3f;

        [Header("Visuals")]
        [SerializeField] private DecalProjector _decalProjector;

        private IEquipmentService _equipment;
        private SignalBus _signalBus;
        private CancellationTokenSource _washCts;

        private bool _isWashing;
        private string _activeToolId; // Кешируем ID, чтобы корректно остановить анимацию

        private Material _materialInstance;
        private bool _isMaterialInstanced;
        private float _currentWashProgress = 0.2f;

        private static readonly int CleanAmountProp = Shader.PropertyToID("_CleanAmount");
        private static readonly int RandomSeedProp = Shader.PropertyToID("_RandomSeed");

        [Inject]
        public void Construct(IEquipmentService equipment, SignalBus signalBus)
        {
            _equipment = equipment;
            _signalBus = signalBus;
        }

        public override string InteractionPrompt
        {
            get
            {
                // Проверяем capability через интерфейс
                if (_equipment.CurrentItem is ICleaningTool)
                    return "Отмыть пятно [Зажать E]";

                return "Грязное пятно (Нужен инструмент для уборки)";
            }
        }

        public override void Interact()
        {
            if (_isWashing) return;

            if (_equipment.CurrentItem is ICleaningTool)
            {
                // Запоминаем ID инструмента, которым начали мыть
                _activeToolId = _equipment.CurrentItem.Id;

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

            if (!_isMaterialInstanced && _decalProjector != null)
            {
                _materialInstance = new Material(_decalProjector.material);
                _decalProjector.material = _materialInstance;

                // изменять при создании пятна на уровне
               // _materialInstance.SetVector(RandomSeedProp, new Vector2(UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f)));

                _isMaterialInstanced = true;
            }

            _signalBus.Fire(new ToolActionSignal { ToolId = _activeToolId, IsActive = true });

            try
            {
                while (_currentWashProgress < 0.9f)
                {
                    _currentWashProgress += Time.deltaTime / _washTime;

                    if (_isMaterialInstanced)
                    {
                        float cleanVal = Mathf.Lerp(0f, 1.05f, _currentWashProgress);
                        _materialInstance.SetFloat(CleanAmountProp, cleanVal);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                if (_isMaterialInstanced && _materialInstance != null)
                {
                    Destroy(_materialInstance);
                }

                Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
                // Игрок отпустил кнопку
            }
            finally
            {
                _isWashing = false;
                _signalBus.Fire(new ToolActionSignal { ToolId = _activeToolId, IsActive = false });
                _activeToolId = null;
            }
        }

        private void OnDestroy()
        {
            if (_isMaterialInstanced && _materialInstance != null)
            {
                Destroy(_materialInstance);
            }
        }
    }
}