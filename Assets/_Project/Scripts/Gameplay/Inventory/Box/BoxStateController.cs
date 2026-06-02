using UnityEngine;

public class BoxStateController : MonoBehaviour
{
    [SerializeField] private GameObject _closedMesh;
    [SerializeField] private GameObject _openMesh;

    // Текущее количество товаров в этой конкретной коробке
    public int CurrentItemsCount { get; set; }

    private void Start()
    {
        // По дефолту коробка закрыта
        SetOpenState(false);
    }

    public void SetOpenState(bool isOpen)
    {
        _closedMesh.SetActive(!isOpen);
        _openMesh.SetActive(isOpen);
    }
}