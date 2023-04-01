using Kinemation.FPSFramework.Runtime.Layers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentHandler : PlayerComponent
{
    [Serializable]
    public struct UseRaySpread
    {
        [Range(0.01f, 10f)]
        public float JumpSpreadMod,
                     RunSpreadMod,
                     CrouchSpreadMod,
                     ProneSpreadMod,
                     WalkSpreadMod,
                     AimSpreadMod;
    }

    public EquipmentItem EquipmentItem { get { return _attachedEquipmentItem; } }

    public int ContinuouslyUsedTimes { get => _continuouslyUsedTimes; }
    public Message OnChangeItem = new Message();
    public Activity UsingItem = new Activity();

    [SerializeField]
    [Group("Inverse of Accuracy - ", true)]
    protected UseRaySpread _useRaySpread = new UseRaySpread();

    [SerializeField]
    protected Transform _itemUseTransform = null;

    protected EquipmentItem _attachedEquipmentItem;
    protected EquipmentItem _attachedItem;

    protected int _continuouslyUsedTimes = 0;
    protected float _nextTimeCanUseItem = -1f;

    public List<EquipmentItem> _equipmentItems = new List<EquipmentItem>();

    protected void Start()
    {
        //m_Unarmed = GetComponentInChildren<Unarmed>(true);

        //m_EquipmentPhysicsHandler = GetComponent<EquipmentPhysicsHandler>();

        EquipmentItem[] equipmentItems = GetComponentsInChildren<EquipmentItem>(true);
        //ItemInfo itemInfo;

        for (int i = 0; i < equipmentItems.Length; i++)
        {
            //Initialize the equipment items
            _equipmentItems.Add(equipmentItems[i]);

            equipmentItems[i].Initialize(this, i);

            equipmentItems[i].gameObject.SetActive(false);
        }

        EquipItem(equipmentItems[0]);

        // Equipment Items AudioSource (For Overall first person items audio)
        //m_AudioSource = AudioUtils.CreateAudioSource("Audio Source", transform, Vector3.zero, false, 1f, 1f);
        //m_AudioSource.bypassEffects = m_AudioSource.bypassListenerEffects = m_AudioSource.bypassReverbZones = false;
        //m_AudioSource.maxDistance = 500f;

        // Persistent AudioSource (e.g. used for the fire tail sounds)
        //m_PersistentAudioSource = AudioUtils.CreateAudioSource("Persistent Audio Source", transform, Vector3.zero, true, 1f, 2.5f);
        //m_PersistentAudioSource.bypassEffects = m_PersistentAudioSource.bypassListenerEffects = m_PersistentAudioSource.bypassReverbZones = false;
        //m_PersistentAudioSource.maxDistance = 500f;
    }

    public bool ContainsEquipmentItem(EquipmentItem item) => _equipmentItems.Contains(item);

    public virtual void EquipItem(EquipmentItem item)
    {
        //ClearDelayedSounds();
        Gun gun = item as Gun;

        // Disable previous equipment item
        if (_attachedEquipmentItem != null)
            _attachedEquipmentItem.gameObject.SetActive(false);

        // Enable next equipment item
        _attachedEquipmentItem = item;
        _attachedEquipmentItem.gameObject.SetActive(true);

        //animator.Play(gun.poseName);
        Player.EquipmentController.SetActiveEquipment(gun);
        gun.gameObject.SetActive(true);

        // Notify the item components (e.g. animation, physics etc.) present on the Equipment Item object
        /*IEquipmentComponent[] itemComponents = _attachedEquipmentItem.GetComponents<IEquipmentComponent>();

        if (itemComponents.Length > 0)
        {
            foreach (var component in itemComponents)
                component.OnSelected();
        }*/

        //SetCharacterMovementSpeed(Player.Aim.Active ? _attachedEquipmentItem.EInfo.Aiming.AimMovementSpeedMod : 1f);
        _nextTimeCanUseItem = Time.time + _attachedEquipmentItem.EInfo.Equipping.Duration;

        OnChangeItem.Send();
        Player.EquippedItem.Set(item);
        _attachedEquipmentItem.Equip(item);
    }

    public virtual void UnequipItem()
    {
        if (_attachedEquipmentItem == null)
            return;

        _attachedItem = null;
        _nextTimeCanUseItem = Time.time + _attachedEquipmentItem.EInfo.Unequipping.Duration;

        EquipmentItem.Unequip();
    }

    public virtual bool TryStartAim()
    {
        if (_nextTimeCanUseItem > Time.time ||
            (!_attachedEquipmentItem.EInfo.Aiming.AimWhileAirborne && !Player.IsGrounded.Get()) || // Can this item be aimed while airborne?
            !_attachedEquipmentItem.EInfo.Aiming.Enabled || !_attachedEquipmentItem.CanAim()) // Can this item be aimed?
            return false;
        //SetCharacterMovementSpeed(m_AttachedEquipmentItem.EInfo.Aiming.AimMovementSpeedMod);
        Player.actionState.Set(FPSActionState.Aiming);
        Player.adsLayer.SetAdsAlpha(1f);
        Player.swayLayer.SetFreeAimEnable(false);
        Player._recoilAnimation.isAiming = true;

        _attachedEquipmentItem.OnAimStart();

        return true;
    }

    public virtual void OnAimStop()
    {
        //SetCharacterMovementSpeed(1f);
        Player.actionState.Set(FPSActionState.None);
        Player.adsLayer.SetAdsAlpha(0f);
        Player.adsLayer.SetPointAlpha(0f);
        Player.swayLayer.SetFreeAimEnable(true);
        Player._recoilAnimation.isAiming = false;

        if (_attachedEquipmentItem != null)
            _attachedEquipmentItem.OnAimStop();
    }

    public virtual bool TryStartPointAiming()
    {
        Player.adsLayer.SetPointAlpha(0f);
        Player.actionState.Set(FPSActionState.Aiming);

        return true;
    }

    public virtual void OnPointAimingStop()
    {
        Player.adsLayer.SetPointAlpha(1f);
        Player.actionState.Set(FPSActionState.PointAiming);
    }

    public virtual bool TryStartHolster()
    {
        Player.locoLayer.SetReadyWeight(0f);
        Player.lookLayer.SetLayerAlpha(1f);
        Player.actionState.Set(FPSActionState.None);

        return true;
    }

    public virtual void OnHolsterStop()
    {
        Player.locoLayer.SetReadyPose(ReadyPose.LowReady);
        Player.locoLayer.SetReadyWeight(1f);
        Player.lookLayer.SetLayerAlpha(.5f);
        Player.actionState.Set(FPSActionState.Ready);
    }

    public virtual bool TryUse(bool continuously, int useType)
    {
        bool usedSuccessfully = false;

        if (_nextTimeCanUseItem < Time.time)
        {
            // Use Rays (E.g Weapons with more projectiles per shot will need more rays - Shotguns)
            Ray[] itemUseRays = GenerateItemUseRays(Player, _itemUseTransform, _attachedEquipmentItem.GetUseRaysAmount(), _attachedEquipmentItem.GetUseRaySpreadMod());

            if (continuously)
                usedSuccessfully = _attachedEquipmentItem.TryUseContinuously(itemUseRays, useType);
            else
                usedSuccessfully = _attachedEquipmentItem.TryUseOnce(itemUseRays, useType);

            if (usedSuccessfully)
            {
                if (!UsingItem.Active)
                {
                    UsingItem.ForceStart();
                    EquipmentItem.OnUseStart();
                }

                //Increment the 'm_ContinuouslyUsedTimes' variable, which shows how many times the weapon has been used consecutively
                if (UsingItem.Active)
                    _continuouslyUsedTimes++;
                else
                    _continuouslyUsedTimes = 1;
            }
        }

        return usedSuccessfully;
    }

    public Ray[] GenerateItemUseRays(Humanoid humanoid, Transform anchor, int raysAmount, float equipmentSpreadMod)
    {
        var itemUseRays = new Ray[raysAmount];

        float spreadMod = 1f;

        if (humanoid != null)
        {
            if (humanoid.Jump.Active)
                spreadMod *= _useRaySpread.JumpSpreadMod;
            else if (humanoid.Sprint.Active)
                spreadMod *= _useRaySpread.RunSpreadMod;
            else if (humanoid.Crouch.Active)
                spreadMod *= _useRaySpread.CrouchSpreadMod;
            else if (humanoid.Prone.Active)
                spreadMod *= _useRaySpread.ProneSpreadMod;
            else if (humanoid.Walk.Active)
                spreadMod *= _useRaySpread.WalkSpreadMod;

            if (humanoid.Aim.Active)
                spreadMod *= _useRaySpread.AimSpreadMod;
        }

        float raySpread = equipmentSpreadMod * spreadMod;

        for (int i = 0; i < itemUseRays.Length; i++)
        {
            Vector3 raySpreadVector = anchor.TransformVector(new Vector3(UnityEngine.Random.Range(-raySpread, raySpread), UnityEngine.Random.Range(-raySpread, raySpread), 0f));
            Vector3 rayDirection = Quaternion.Euler(raySpreadVector) * anchor.forward;

            itemUseRays[i] = new Ray(anchor.position, rayDirection);
        }

        return itemUseRays;
    }
}
