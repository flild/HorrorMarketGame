using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Interactables
{
    public class BoxStateController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private Animator _animator;
        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

        [Header("Fill Visuals (Fake Items)")]
        [SerializeField] private Transform _fillVisualTransform;
        [SerializeField] private float _fullLocalY = 0.1f;
        [SerializeField] private float _emptyLocalY = -0.15f;

        [Header("Capacity")]
        [field: SerializeField] public int MaxItemsCapacity { get; set; } = 10;
        [field: SerializeField] public int CurrentItemsCount { get; private set; } = 10;

        [Header("Debug in Editor")]
        [Range(0, 10)]
        [SerializeField] private int _debugCurrentItems = 10;

        private void Start()
        {
            SetOpenState(false);
            UpdateVisualsInternally();
        }

        private void OnValidate()
        {
            CurrentItemsCount = Mathf.Clamp(_debugCurrentItems, 0, MaxItemsCapacity);
            UpdateVisualsInternally();
        }

        // Вызывается извне, когда игрок начинает распаковку
        public void BeginUnpack()
        {
            if (CurrentItemsCount > 0)
            {
                SetOpenState(true);
            }
        }

        // Вызывается извне, когда игрок отпускает кнопку или процесс прерван
        public void EndUnpack()
        {
            SetOpenState(false);
        }

        // Вызывается полкой, чтобы забрать один предмет. 
        // Коробка сама решает, отдавать его или нет, и сама обновляет свой визуал.
        public bool TryExtractItem()
        {
            if (CurrentItemsCount <= 0) return false;

            CurrentItemsCount--;
            UpdateVisualsInternally();

            // Если вытащили последнее - захлопываемся
            if (CurrentItemsCount <= 0)
            {
                SetOpenState(false);
            }

            return true;
        }

        private void SetOpenState(bool isOpen)
        {
            if (_animator != null)
            {
                _animator.SetBool(IsOpenHash, isOpen);
            }
        }

        private void UpdateVisualsInternally()
        {
            if (_fillVisualTransform == null) return;

            float percentage = MaxItemsCapacity > 0 ? (float)CurrentItemsCount / MaxItemsCapacity : 0f;
            percentage = Mathf.Clamp01(percentage);

            if (percentage <= 0f)
            {
                _fillVisualTransform.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (!_fillVisualTransform.gameObject.activeSelf)
                    _fillVisualTransform.gameObject.SetActive(true);
            }

            float newY = Mathf.Lerp(_emptyLocalY, _fullLocalY, percentage);
            Vector3 currentLocalPos = _fillVisualTransform.localPosition;
            _fillVisualTransform.localPosition = new Vector3(currentLocalPos.x, newY, currentLocalPos.z);
        }
    }
}