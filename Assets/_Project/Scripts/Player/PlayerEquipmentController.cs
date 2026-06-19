using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Project.Core.Input;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Player
{
    public class PlayerEquipmentController : MonoBehaviour
    {
        private IInputService _input;
        private IEquipmentService _equipment;

        [Inject]
        public void Construct(IInputService input, IEquipmentService equipment)
        {
            _input = input;
            _equipment = equipment;
        }

        private void Start()
        {
            // Подписываемся на кнопку сброса
            _input.OnDropTriggered += HandleDrop;
        }

        private void OnDestroy()
        {
            // Обязательно отписываемся, чтобы не словить утечку памяти при выгрузке сцены
            if (_input != null)
            {
                _input.OnDropTriggered -= HandleDrop;
            }
        }

        private void HandleDrop()
        {
            Debug.Log($"drop item from hands {_equipment.CurrentItem?.DisplayNameKey ?? "null"}"); 
            if (_equipment.IsHandsBusy)
            {
                _equipment.Unequip();
            }
        }
    }
}
