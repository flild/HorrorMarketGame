#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


    public class PhysicsSnitch : MonoBehaviour
    {
        private Rigidbody _rb;
        private Collider[] _cols;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _cols = GetComponentsInChildren<Collider>(true);
            LogState("AWAKE (Самый старт)");
        }

        void Start()
        {
            LogState("START (Перед первым кадром)");
        }

        void FixedUpdate()
        {
            // Чекаем только первые 3 физических кадра, когда обычно случается провал
            if (Time.frameCount <= 3)
            {
                LogState($"FIXED UPDATE (Кадр {Time.frameCount})");
            }
        }

        private void LogState(string phase)
        {
            if (_cols.Length == 0)
            {
                Debug.LogError($"[{phase}] <color=red>АЛЁ, ГДЕ КОЛЛАЙДЕРЫ?</color>");
                return;
            }

            bool allEnabled = true;
            foreach (var c in _cols)
            {
                if (!c.enabled || c.isTrigger) allEnabled = false;
            }

            string rbState = _rb != null
                ? $"isKin=<b>{_rb.isKinematic}</b>, detect=<b>{_rb.detectCollisions}</b>, vel=<b>{_rb.linearVelocity.y:F2}</b>" // Для Unity 6+ linearVelocity, если старая - замени на velocity
                : "НЕТ RIGIDBODY";

            string colState = allEnabled ? "<color=green>АКТИВНЫ</color>" : "<color=red>ОТКЛЮЧЕНЫ/ТРИГГЕРЫ</color>";

            Debug.Log($"[{phase}] Коллизии: {colState} | RB: {rbState} | Y-Позиция рута: {transform.position.y:F3}");
        }

        // Если она всё-таки во что-то врезалась, но провалилась
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"<color=yellow>[ПОЙМАЛ КОЛЛИЗИЮ]</color> Врезался в {collision.gameObject.name} на скорости {_rb.linearVelocity.magnitude}");
        }
    }
#endif