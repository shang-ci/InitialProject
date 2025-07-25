﻿/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Hotbar
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewSlotRestrictions;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using UnityEngine;
    
    /// <summary>
    /// The component that handles input per view slot. useful for a hotbar.
    /// </summary>
    public class ItemViewSlotContainerIndexedActionsHandler : MonoBehaviour
    {
        [Tooltip("The item hotbar to use when an input is pressed.")]
        [SerializeField] protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;
        [Tooltip("Use the item assigned to this slot when clicked.")]
        [SerializeField] protected ItemViewSlotsContainerItemActionBindingBase m_ItemActionsBinding;
        [Tooltip("Input to use Item Actions in the Item Hotbar.")]
        [SerializeField]
        protected IndexedInput[] m_Input = new IndexedInput[]
        {
            new IndexedInput(0, "Equip First Item", InputType.ButtonDown),
            new IndexedInput(1, "Equip Second Item",  InputType.ButtonDown),
            new IndexedInput(2, "Equip Third Item", InputType.ButtonDown),
            new IndexedInput(3, "Equip Fourth Item", InputType.ButtonDown),
            new IndexedInput(4, "Equip Fifth Item", InputType.ButtonDown),
            new IndexedInput(5, "Equip Sixth Item",  InputType.ButtonDown),
            new IndexedInput(6, "Equip Seventh Item",  InputType.ButtonDown),
            new IndexedInput(7, "Equip Eighth Item", InputType.ButtonDown),
            new IndexedInput(8, "Equip Ninth Item",  InputType.ButtonDown),
            new IndexedInput(9, "Equip Tenth Item",  InputType.ButtonDown),
        };
        protected IPlayerInput m_PlayerInput;
        
        /// <summary>
        /// Initialize and listen to events.
        /// </summary>
        protected virtual void Start()
        {
            if (m_ItemViewSlotsContainer == null) { m_ItemViewSlotsContainer = GetComponent<ItemSlotCollectionView>(); }

            if (m_ItemActionsBinding == null) { m_ItemActionsBinding = GetComponent<ItemViewSlotsContainerItemActionBindingBase>();}

            m_ItemViewSlotsContainer.OnBindInventory += OnInventoryBound;
            m_ItemViewSlotsContainer.OnUnBindInventory += OnInventoryUnbound;
            OnInventoryBound(m_ItemViewSlotsContainer.Inventory);
            
            if (m_PlayerInput == null) {
                enabled = false;
            }
        }

        /// <summary>
        /// Handle gameplay input being enabled/disabled.
        /// </summary>
        /// <param name="enable">is gameplay input enabled?</param>
        private void HandleEnableGameplayInput(bool enable)
        {
            enabled = enable;
        }

        /// <summary>
        /// Check for the input in update.
        /// </summary>
        protected virtual void Update()
        {
            for (int i = 0; i < m_Input.Length; i++) {
                if ( m_Input[i].Index < 0 || m_ItemViewSlotsContainer.SlotCount < m_Input[i].Index) {
                    continue;
                }
                if (m_Input[i].CheckInput(m_PlayerInput)) {
                    m_ItemActionsBinding.TriggerItemAction(m_Input[i].Index);
                }
            }
        }

        /// <summary>
        /// Handle a new inventory being bound.
        /// </summary>
        protected void OnInventoryBound(Inventory inventory)
        {
            if (inventory == null) { return; }

            //TODO the bound inventory might not be the player inventory (example chest) so the input might not be found there...
            //Find a better solution to get the player input.
            m_PlayerInput = inventory.gameObject.GetCachedComponent<IPlayerInput>();
            EventHandler.RegisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
            enabled = true;
        }

        /// <summary>
        /// Handle inventor being unbound.
        /// </summary>
        protected void OnInventoryUnbound(Inventory inventory)
        {
            if (m_PlayerInput == null) {
                return;
            }
            EventHandler.UnregisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
            m_PlayerInput = null;
            enabled = false;
        }
        
        /// <summary>
        /// Unregister input on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_PlayerInput == null || (m_PlayerInput is MonoBehaviour && m_PlayerInput as MonoBehaviour == null)) {
                return;
            }
            EventHandler.UnregisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
        }
    }
}