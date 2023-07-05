using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : PlayerComponent
{
    public EquipmentHandler activeEHandler;

    protected EquipmentItem _attachedEquipmentItem;

    [SerializeField]
    private bool m_AimWhileReloading;

    [SerializeField]
    private bool m_AutoReloadOnEmpty = true;

    private float m_NextTimeCanAutoReload;
    private float m_NextTimeToEquip;
    private bool m_WaitingToEquip;

    private int _index;

    private void Awake()
    {
        Player.SwapItem.SetTryer(TrySwapItems);
        Player.ScrollValue.AddChangeListener(ChangeWeapon);
        Player.EquipItem.SetTryer(TryChangeItem);

        Player.DestroyEquippedItem.SetTryer(TryDestroyHeldItem);

        Player.UseItem.AddStopListener(() => Player._recoilAnimation.Stop());

        Player.Aim.SetStartTryer(TryStartAim);
        Player.Aim.AddStopListener(OnAimStop);

        Player.UseItem.SetStartTryer(TryStartUseItem);
        Player.Reload.SetStartTryer(TryStartReload);

        Player.Lean.SetStartTryer(TryStartLean);
        Player.ChangeScope.SetTryer(TrySwitchScope);

        Player.PointAim.SetStartTryer(TryStartPointAiming);
        Player.PointAim.AddStopListener(OnPointAimingStop);

        Player.Holster.SetStartTryer(TryStartHolster);
        Player.Holster.AddStopListener(OnHolsterStop);

        Player.UseItemHeld.SetTryer(TryUse);
    }

    private void Update()
    {
        if (Player.UseItem.Active)
        {
            Player.UseItemHeld.Try(true, 0);
        }

        if (Player.Reload.Active)
        {
            bool endedReloading = activeEHandler.EquipmentItem.IsDoneReloading();

            if (endedReloading)
                Player.Reload.ForceStop();
        }

        //Equip the new item after the previous one has been unequipped
        if (m_WaitingToEquip && Time.time > m_NextTimeToEquip)
        {
            Equip(Player.EquippedItem.Get());
            m_WaitingToEquip = false;
        }
    }

    private void Equip(EquipmentItem item)
    {
        if (Player.Aim.Active)
            Player.Aim.ForceStop();

        if (Player.Reload.Active)
            Player.Reload.ForceStop();

        activeEHandler.EquipItem(item);

        Player.ActiveEquipmentItem.Set(activeEHandler.EquipmentItem);
        //m_FPCamera.fieldOfView = activeEHandler.EquipmentItem.EModel.TargetFOV;
    }

    private void ChangeWeapon(int index)
    {
        int newIndex = _index;
        newIndex += index;

        if (newIndex > activeEHandler._equipmentItems.Count - 1)
        {
            newIndex = 0;
        }
        if (newIndex < 0)
        {
            newIndex = activeEHandler._equipmentItems.Count - 1;
        }

        _index = newIndex;

        Player.SwapItem.Try(activeEHandler._equipmentItems[_index]);
    }

    private bool TryChangeItem(EquipmentItem item, bool instantly)
    {
        if (Player.EquippedItem.Get() == item && item != null)
            return false;

        ChangeItem(item, instantly);

        return true;
    }

    private void ChangeItem(EquipmentItem item, bool instantly)
    {
        // Register the equipment item for equipping
        m_WaitingToEquip = true;

        // Register the current equipped item for disabling
        if (activeEHandler.EquipmentItem != null)
        {
            if (activeEHandler.UsingItem.Active)
            {
                activeEHandler.UsingItem.ForceStop();
                activeEHandler.EquipmentItem.OnUseEnd();
            }

            if (Player.Aim.Active)
                Player.Aim.ForceStop();

            if (Player.Reload.Active)
                Player.Reload.ForceStop();

            activeEHandler.UnequipItem();

            if (!instantly)
                m_NextTimeToEquip = Time.time + activeEHandler.EquipmentItem.EInfo.Unequipping.Duration;
        }

        Player.EquippedItem.Set(item);
    }


    private bool TrySwapItems(EquipmentItem item)
    {
        EquipmentItem currentItem = Player.EquippedItem.Get();

        if (currentItem != null && activeEHandler.ContainsEquipmentItem(item)) // Check if the passed item is swappable
        {
            Player.DestroyEquippedItem.Try();
            Player.EquipItem.Try(item, true);

            return true;
        }

        return false;
    }

    private bool TryDestroyHeldItem()
    {
        if (Player.EquippedItem.Get() == null)
            return false;
        else
        {
            //Player.Inventory.RemoveItem(Player.EquippedItem.Get());
            Player.EquippedItem.Get().gameObject.SetActive(false);
            Player.EquipItem.Try(null, true);
            return true;
        }
    }

    public virtual bool TryStartReload() => _attachedEquipmentItem.TryStartReload();

    private bool TryUse(bool continuously, int useIndex)
    {
        EquipmentItem eItem = activeEHandler.EquipmentItem;
        //float staminaTakePerUse = eItem.EInfo.General.StaminaTakePerUse;
        bool eItemCanBeUsed = eItem.CanBeUsed();

        // Interrupt the reload if possible
        if (!continuously && Player.Reload.Active /*&& eItem.EInfo.General.CanStopReloading*/ && eItemCanBeUsed)
            Player.Reload.ForceStop();

        if (CanUseItem(eItem))
        {
            bool usedSuccessfully = activeEHandler.TryUse(continuously, useIndex);

            if (usedSuccessfully)
            {
                //if (staminaTakePerUse > 0f)
                //    Player.Stamina.Set(Mathf.Max(Player.Stamina.Get() - staminaTakePerUse, 0f));

                m_NextTimeCanAutoReload = Time.time + 0.35f;
            }

            if (!eItemCanBeUsed)
            {
                Player._recoilAnimation.Stop();
            }

            //Try reload the item if the item can't be used (e.g. out of ammo) and 'Reload on empty' is active
            //if (!eItemCanBeUsed && m_AutoReloadOnEmpty && !continuously && m_NextTimeCanAutoReload < Time.time)
            //Player.Reload.TryStart();

            return usedSuccessfully;
        }

        return false;
    }

    private bool CanUseItem(EquipmentItem eItem)
    {
        if (eItem != null)
        {
            //float staminaTakePerUse = eItem.EInfo.General.StaminaTakePerUse;

            bool airborneCondition = Player.IsGrounded.Get();// || eItem.EInfo.General.UseWhileAirborne;
            bool runningCondition = !Player.Sprint.Active;// || eItem.EInfo.General.UseWhileRunning;
            //bool staminaCondition = staminaTakePerUse == 0f || Player.Stamina.Get() > staminaTakePerUse;

            return airborneCondition /*&& staminaCondition*/ && runningCondition && !Player.Reload.Active;
        }

        return false;
    }

    public void SetActiveEquipment(EquipmentItem equipment)
    {
        _attachedEquipmentItem = equipment;
    }

    private bool TryStartAim()
    {
        if (Player.Sprint.Active ||
            Player.Reload.Active ||
            (!m_AimWhileReloading && Player.Aim.Active))
            return false;

        return activeEHandler.TryStartAim();
    }

    private void OnAimStop() => activeEHandler.OnAimStop();

    private bool TryStartUseItem()
    {
        return _attachedEquipmentItem.CanBeUsed();
    }

    private bool TryStartLean(float lean)
    {
        return true;
    }

    private bool TrySwitchScope()
    {
        if (!Player.Aim.Active)
            return false;

        return true;
    }

    private bool TryStartPointAiming()
    {
        if (!Player.Aim.Active)
            return false;

        return activeEHandler.TryStartPointAiming();
    }

    private void OnPointAimingStop() => activeEHandler.OnPointAimingStop();

    private bool TryStartHolster()
    {
        if (Player.Aim.Active)
            return false;

        return activeEHandler.TryStartHolster();
    }

    private void OnHolsterStop() => activeEHandler.OnHolsterStop();
}
