using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class ItemPhysicsController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider[] _colliders;

        public void SetPhysicsState(bool enable)
        {
            if (_rb != null)
            {
                _rb.isKinematic = !enable;
                _rb.detectCollisions = enable;
            }

            foreach (var col in _colliders)
            {
                if (col != null)
                {
                    // Если это триггер взаимодействия, его можно пропускать, 
                    // но если у тебя отдельный слой, то можно отключать всё.
                    col.enabled = enable;
                }
            }
        }

        [ContextMenu("Auto-Find Components")]
        private void AutoFindComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
        }
    }
}
