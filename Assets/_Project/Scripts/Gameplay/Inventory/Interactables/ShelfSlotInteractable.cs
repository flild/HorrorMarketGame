using Assets._Project.Scripts.Gameplay.Interactables;
using Assets._Project.Scripts.Gameplay.Inventory.Data;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Assets._Project.Scripts.Gameplay.Phone; // Добавили для PlayerActionSignal
using Cysharp.Threading.Tasks;
using Project.Core.Input;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interactables
{
    [RequireComponent(typeof(BoxCollider))]
    public class ShelfSlotInteractable : InteractableObject, IInteractable
    {
        [Header("Slot Settings")]
        [SerializeField] private ProductItemDefinition _allowedProduct;
        [SerializeField] private Vector2Int _gridSize = new Vector2Int(2, 2);
        [SerializeField] private float _delayBetweenItems = 0.3f;

        [Header("Quest Tracking")]
        [Tooltip("ID действия, которое улетит в трекер квестов, когда коробка полностью опустеет")]
        [SerializeField] private string _actionId = "unpack_box";

        [Header("Visuals")]
        [SerializeField] private GameObject _highlightVisual;

        private BoxCollider _collider;
        private CancellationTokenSource _unpackCts;
        private IEquipmentService _equipment;
        private SignalBus _signalBus; // Добавили шину
        private bool _isUnpacking;
        private int _currentItemsPlaced = 0;

        private int MaxCapacity => _gridSize.x * _gridSize.y;

        private IInputService _inputService;

        [Inject]
        public void Construct(IEquipmentService equipment, SignalBus signalBus, IInputService inputService)
        {
            _equipment = equipment;
            _signalBus = signalBus;
            _inputService = inputService;
        }

        public override PromptData InteractionPrompt // МЕНЯЕМ string НА PromptData
        {
            get
            {
                if (_currentItemsPlaced >= MaxCapacity)
                    return new PromptData("ui_prompt_shelf_full"); // "Место заполнено"

                if (_equipment.CurrentItem is BoxItemDefinition box)
                {
                    if (box.ContentItem == _allowedProduct)
                    {
                        string interactBind = _inputService.GetBindingName("Interact");
                        // "Выложить {0} [{1}]"
                        return new PromptData("ui_prompt_unpack", box.ContentItem.DisplayName, interactBind);
                    }

                    // "Сюда нужно: {0}"
                    return new PromptData("ui_prompt_shelf_need", _allowedProduct.DisplayName);
                }

                if (_equipment.CurrentItem == _allowedProduct)
                {
                    string interactBind = _inputService.GetBindingName("Interact");
                    // "Положить {0} [{1}]"
                    return new PromptData("ui_prompt_shelf_place", _allowedProduct.DisplayName, interactBind);
                }

                // "Пустое место ({0})"
                return new PromptData("ui_prompt_shelf_empty", _allowedProduct.DisplayName);
            }
        }

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            AlignHighlightVisual();
            if (_highlightVisual != null) _highlightVisual.SetActive(false);
        }

        private void OnValidate()
        {
            if (_collider == null) _collider = GetComponent<BoxCollider>();
            AlignHighlightVisual();
        }

        private void AlignHighlightVisual()
        {
            if (_highlightVisual == null || _collider == null) return;

            Vector3 bottomLocalPos = new Vector3(
                _collider.center.x,
                _collider.center.y - (_collider.size.y / 2f),
                _collider.center.z
            );

            _highlightVisual.transform.localPosition = bottomLocalPos;
        }

        protected override void EnableOutline(bool enable)
        {
            if (_highlightVisual != null) _highlightVisual.SetActive(enable);
        }

        public override void Interact()
        {
            if (_isUnpacking || _currentItemsPlaced >= MaxCapacity) return;

            if (_equipment.CurrentItem is BoxItemDefinition boxDef &&
                boxDef.ContentItem == _allowedProduct &&
                _equipment.CurrentInstance != null)
            {
                var boxState = _equipment.CurrentInstance.GetComponent<BoxStateController>();
                if (boxState != null && boxState.CurrentItemsCount > 0)
                {
                    _unpackCts = new CancellationTokenSource();
                    UnpackRoutine(boxDef, boxState, _unpackCts.Token).Forget();
                }
            }
            else if (_equipment.CurrentItem == _allowedProduct)
            {
                PlaceSingleItem();
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

            if (_equipment.CurrentInstance != null)
            {
                var boxState = _equipment.CurrentInstance.GetComponent<BoxStateController>();
                if (boxState != null) boxState.EndUnpack();
            }
        }

        private async UniTaskVoid UnpackRoutine(BoxItemDefinition boxDef, BoxStateController boxState, CancellationToken token)
        {
            _isUnpacking = true;
            boxState.BeginUnpack();

            try
            {
                for (int i = _currentItemsPlaced; i < MaxCapacity; i++)
                {
                    if (!boxState.TryExtractItem())
                        break;

                    SpawnProductModel(boxDef.ContentItem);

                    // ПРОВЕРКА НА ОПУСТОШЕНИЕ КОРОБКИ
                    if (boxState.CurrentItemsCount == 0)
                    {
                        Debug.Log($"[ShelfSlotInteractable] Коробка {boxDef.Id} пуста! Кидаем сигнал {_actionId}");
                        _signalBus.Fire(new PlayerActionSignal
                        {
                            ActionId = _actionId,
                            Amount = 1
                        });

                        // Коробка пустая, дальше крутить цикл смысла нет
                        break;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenItems), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { /* Игрок отпустил E */ }
            finally
            {
                boxState.EndUnpack();
                _isUnpacking = false;
            }
        }

        private void PlaceSingleItem()
        {
            SpawnProductModel(_allowedProduct);
            _equipment.Unequip();
        }

        private void SpawnProductModel(ProductItemDefinition productDef)
        {
            Vector3 spawnPos = GetPositionForIndex(_currentItemsPlaced);
            var itemInstance = Instantiate(productDef.ShelfPrefab, spawnPos, transform.rotation, transform);

            if (itemInstance.TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);
            if (itemInstance.TryGetComponent<Collider>(out var col)) Destroy(col);

            _currentItemsPlaced++;
        }

        private Vector3 GetPositionForIndex(int index)
        {
            if (_collider == null) _collider = GetComponent<BoxCollider>();

            int col = index % _gridSize.x;
            int row = index / _gridSize.x;

            Vector3 center = _collider.center;
            Vector3 size = _collider.size;

            float stepX = size.x / _gridSize.x;
            float stepZ = size.z / _gridSize.y;

            float startX = center.x - (size.x / 2f) + (stepX / 2f);
            float startZ = center.z - (size.z / 2f) + (stepZ / 2f);

            float centerY = center.y;

            Vector3 localPos = new Vector3(startX + (col * stepX), centerY, startZ + (row * stepZ));
            return transform.TransformPoint(localPos);
        }

        private void OnDrawGizmos()
        {
            if (_collider == null) _collider = GetComponent<BoxCollider>();
            if (_collider == null || _gridSize.x <= 0 || _gridSize.y <= 0) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(_collider.center, _collider.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_collider.center, _collider.size);

            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.identity;

            int capacity = MaxCapacity;
            for (int i = 0; i < capacity; i++)
            {
                Gizmos.DrawWireSphere(GetPositionForIndex(i), 0.05f);
            }
        }
    }
}