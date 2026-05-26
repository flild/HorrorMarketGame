using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private SpawnLocationId _locationId;
    public SpawnLocationId LocationId => _locationId;

    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    // Рисуем зеленую сферу и луч в редакторе, чтобы ты видел, где спавн и куда игрок будет смотреть
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}
