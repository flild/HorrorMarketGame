using Assets._Project.Scripts.Gameplay.Interactables;
using Assets._Project.Scripts.Gameplay.Inventory.Data;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interactables
{
    public class ShelfInteractable : InteractableObject, IInteractable
    {
        [Header("Shelf Settings")]
        [SerializeField] private Transform[] _itemSlots;
        [SerializeField] private float _delayBetweenItems = 0.3f;

        private CancellationTokenSource _unpackCts;
        private IEquipmentService _equipment;
        private bool _isUnpacking;

        [Inject]
        public void Construct(IEquipmentService equipment)
        {
            _equipment = equipment;
        }

        public override string InteractionPrompt
        {
            get
            {
                if (_equipment.CurrentItem is BoxItemDefinition box)
                {
                    return $"Выложить {box.ContentItem.DisplayName} [E]";
                }
                return "Полка (Нужна коробка с товаром)";
            }
        }

        public override void Interact()
        {
            if (_isUnpacking) return;

            if (_equipment.CurrentItem is BoxItemDefinition boxDef && _equipment.CurrentInstance != null)
            {
                var boxState = _equipment.CurrentInstance.GetComponent<BoxStateController>();
                if (boxState != null && boxState.CurrentItemsCount > 0)
                {
                    _unpackCts = new CancellationTokenSource();
                    // Теперь мы нормально передаем все нужные ссылки в таску
                    UnpackRoutine(boxDef, boxState, _unpackCts.Token).Forget();
                }
            }
        }

        public override void EndInteract()
        {
            if (_unpackCts != null)
            {
                _unpackCts.Cancel();
                _unpackCts.Dispose();
                _unpackCts = null;
            }
        }

        private async UniTaskVoid UnpackRoutine(BoxItemDefinition boxDef, BoxStateController boxState, CancellationToken token)
        {
            _isUnpacking = true;

            try
            {
                boxState.SetOpenState(true);

                foreach (var slot in _itemSlots)
                {
                    if (slot.childCount > 0) continue;
                    if (boxState.CurrentItemsCount <= 0) break;

                    // TODO в будущем: заменить Instantiate на Object Pool от Zenject для максимальной производительности
                    var itemInstance = Instantiate(boxDef.ContentItem.ShelfPrefab, slot.position, slot.rotation, slot);

                    // Жесткая оптимизация: отрубаем физику товару на полке
                    if (itemInstance.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Destroy(rb);
                    }

                    boxState.CurrentItemsCount--;

                    // Передаем в коробку процент заполненности, чтобы она могла опустить свой фейковый Quad с текстурой
                    float fillPercentage = (float)boxState.CurrentItemsCount / boxDef.MaxCapacity;
                    boxState.UpdateFillVisuals(fillPercentage);

                    // Звук выкладки товара проигрывать здесь

                    // Ждем. Если прилетит Cancel(), прервет выполнение и выкинет в catch
                    await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenItems), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[Shelf] Игрок отвернулся или отпустил кнопку. Процесс прерван.");
            }
            finally
            {
                // finally гарантирует, что переменная сбросится даже при ошибке или прерывании
                _isUnpacking = false;
            }
        }
    }
}