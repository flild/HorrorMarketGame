using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Interactables
{
    public class BoxStateController : MonoBehaviour
    {
        [Header("Meshes")]
        [SerializeField] private GameObject _closedMesh;
        [SerializeField] private GameObject _openMesh;

        [Header("Fill Visuals (Fake Items)")]
        [Tooltip("Quad или плоскость с нарисованными крышками банок")]
        [SerializeField] private Transform _fillVisualTransform;

        [Tooltip("Локальная высота Y, когда коробка заполнена на 100%")]
        [SerializeField] private float _fullLocalY = 0.5f;

        [Tooltip("Локальная высота Y, когда в коробке остался минимум (около дна)")]
        [SerializeField] private float _emptyLocalY = 0.05f;

        // Текущее количество товаров в этой конкретной коробке
        public int CurrentItemsCount { get; set; }

        private void Start()
        {
            // По дефолту коробка закрыта
            SetOpenState(false);
        }

        public void SetOpenState(bool isOpen)
        {
            if (_closedMesh != null) _closedMesh.SetActive(!isOpen);
            if (_openMesh != null) _openMesh.SetActive(isOpen);
        }

        public void UpdateFillVisuals(float percentage)
        {
            if (_fillVisualTransform == null) return;

            // Защита от кривых значений (жестко ограничиваем от 0 до 1)
            percentage = Mathf.Clamp01(percentage);

            // Если коробка пустая — прячем фейковое дно к чертям, чтобы не отсвечивало
            if (percentage <= 0f)
            {
                _fillVisualTransform.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (!_fillVisualTransform.gameObject.activeSelf)
                {
                    _fillVisualTransform.gameObject.SetActive(true);
                }
            }

            // Высчитываем новую высоту через линейную интерполяцию (Lerp)
            float newY = Mathf.Lerp(_emptyLocalY, _fullLocalY, percentage);

            // Применяем позицию, не трогая оригинальные X и Z
            Vector3 currentLocalPos = _fillVisualTransform.localPosition;
            _fillVisualTransform.localPosition = new Vector3(currentLocalPos.x, newY, currentLocalPos.z);
        }
    }
}