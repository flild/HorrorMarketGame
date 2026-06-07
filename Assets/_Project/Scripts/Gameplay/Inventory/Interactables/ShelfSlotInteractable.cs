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
    [RequireComponent(typeof(BoxCollider))]
    public class ShelfSlotInteractable : InteractableObject, IInteractable
    {
        [Header("Slot Settings")]
        [SerializeField] private ProductItemDefinition _allowedProduct;
        [SerializeField] private Vector2Int _gridSize = new Vector2Int(2, 2);
        [SerializeField] private float _delayBetweenItems = 0.3f;

        [Header("Visuals")]
        [SerializeField] private GameObject _highlightVisual;

        private BoxCollider _collider;
        private CancellationTokenSource _unpackCts;
        private IEquipmentService _equipment;
        private bool _isUnpacking;
        private int _currentItemsPlaced = 0;

        private int MaxCapacity => _gridSize.x * _gridSize.y;

        [Inject]
        public void Construct(IEquipmentService equipment)
        {
            _equipment = equipment;
        }

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            AlignHighlightVisual(); // Жестко ставим на место при старте
            if (_highlightVisual != null) _highlightVisual.SetActive(false);
        }

        private void OnValidate()
        {
            if (_collider == null) _collider = GetComponent<BoxCollider>();
            AlignHighlightVisual(); // Чтобы подсветка сама прыгала на дно прямо в редакторе
        }

        private void AlignHighlightVisual()
        {
            if (_highlightVisual == null || _collider == null) return;

            // Считаем локальную позицию: центр по X и Z, и самое дно по Y
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

        public override string InteractionPrompt
        {
            get
            {
                if (_currentItemsPlaced >= MaxCapacity)
                    return "Место заполнено";

                // В руках коробка
                if (_equipment.CurrentItem is BoxItemDefinition box)
                {
                    if (box.ContentItem == _allowedProduct)
                        return $"Выложить {box.ContentItem.DisplayName} [Зажать E]";
                    return $"Сюда нужно: {_allowedProduct.DisplayName}";
                }

                // В руках одиночный товар
                if (_equipment.CurrentItem == _allowedProduct)
                {
                    return $"Положить {_allowedProduct.DisplayName} [E]";
                }

                return $"Пустое место ({_allowedProduct.DisplayName})";
            }
        }

        public override void Interact()
        {
            if (_isUnpacking || _currentItemsPlaced >= MaxCapacity) return;

            // СЦЕНАРИЙ 1: Игрок держит коробку
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
            // СЦЕНАРИЙ 2: Игрок держит нужный товар в руках
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

        // ================= МАТЕМАТИКА СЕТКИ =================
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

            // Вот здесь теперь просто берем центр коллайдера по высоте
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