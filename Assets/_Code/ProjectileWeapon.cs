using Kinemation.FPSFramework.Runtime.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : EquipmentItem
{
    [SerializeField] public WeaponAnimData gunData;
    [SerializeField] public RecoilAnimData recoilData;

    public Value<AmmoInfo> CurrentAmmoInfo = new Value<AmmoInfo>();
    protected int _ammoProperty = 30;

    // Reloading
    private int m_AmmoToAdd;
    private bool m_ReloadLoopStarted;
    private float m_ReloadLoopEndTime;
    private float m_ReloadStartTime;
    private bool m_EndReload;

    public int MagazineSize { get => _projectileWeaponInfo.Shooting.MagazineSize; }
    public bool AmmoEnabled { get => _projectileWeaponInfo.Shooting.EnableAmmo; }

    protected ProjectileWeaponInfo _projectileWeaponInfo;

    public int SelectedFireMode { get; protected set; } = 8;

    public override void Initialize(EquipmentHandler eHandler, int id)
    {
        base.Initialize(eHandler, id);

        _projectileWeaponInfo = EInfo as ProjectileWeaponInfo;
    }

    public override void Equip(EquipmentItem item)
    {
        base.Equip(item);

        Player._recoilAnimation.Init(recoilData, _projectileWeaponInfo.Shooting.RoundsPerMinute, (FireMode)SelectedFireMode);
        Player.CoreAnimComponent.OnGunEquipped(gunData);
    }

    public override bool TryUseOnce(Ray[] itemUseRays, int useType)
    {
        bool canUse = false;

        //Shooting
        if (Time.time > m_NextTimeCanUse)
        {
            canUse = (CurrentAmmoInfo.Val.CurrentInMagazine > 0 /*|| !m_PW.Shooting.EnableAmmo*/) && SelectedFireMode != (int)FireMode.Safety;

            if (canUse)
            {
                //if (SelectedFireMode == (int)fireMode.Burst)
                //    StartCoroutine(C_DoBurst());
                //else
                Shoot(itemUseRays);

                m_NextTimeCanUse = Time.time + (m_UseThreshold * Mathf.Clamp(1 / _projectileWeaponInfo.Shooting.FireRateOverTime.Evaluate(EHandler.ContinuouslyUsedTimes / (float)MagazineSize), 0.1f, 10f));

                //m_GeneralEvents.OnUse.Invoke();
            }
            else
            {
                //Play Empty/Dry fire sound
                if (!Player.Reload.Active)
                {
                    //EHandler.PlaySound(m_PW.Shooting.DryShootAudio, 1f);

                    /*if (m_PW.Shooting.HasDryFireAnim)
                    {
                        EHandler.Animator_SetFloat(animHash_FireIndex, 4);
                        EHandler.Animator_SetTrigger(animHash_Fire);
                    }

                    DryFire.Send();*/

                    m_NextTimeCanUse = Time.time + 0.1f;
                }
            }
        }

        return canUse;
    }

    public override bool TryUseContinuously(Ray[] itemUseRays, int useType)
    {

        //Used to prevent calling the Play empty/dry fire functionality in continuous mode
        if ((CurrentAmmoInfo.Val.CurrentInMagazine == 0 /*&& m_PW.Shooting.EnableAmmo*/) || SelectedFireMode == (int)FireMode.Safety)
            return false;

        if (SelectedFireMode == (int)FireMode.Full)
            return TryUseOnce(itemUseRays, useType);
        return false;
    }

    public virtual void Shoot(Ray[] itemUseRays)
    {
        // Shoot sound
        //EHandler.PlaySound(m_PW.Shooting.ShootAudio, 1f);

        // Handling sounds
        //EHandler.PlayDelayedSounds(m_PW.Shooting.HandlingAudio);

        // Shell drop sounds
        //if (Player.IsGrounded.Get() == true && m_PW.Shooting.CasingDropAudio.Length > 0)
        //EHandler.PlayDelayedSounds(m_PW.Shooting.CasingDropAudio);

        // Play Fire Animation 
        //int fireIndex;

        //if (!Player.Aim.Active)
        //{
        //    fireIndex = m_CurrentFireAnimIndex == 0 ? 0 : 2;
        //
        //    if (m_PW.Shooting.HasAlternativeFireAnim)
        //        m_CurrentFireAnimIndex = m_CurrentFireAnimIndex == 0 ? 1 : 0;
        //}
        //else
        //{
        //    fireIndex = m_CurrentFireAnimIndex == 0 ? 1 : 3;

        //    if (m_PW.Shooting.HasAlternativeFireAnim)
        //        m_CurrentFireAnimIndex = m_CurrentFireAnimIndex == 0 ? 1 : 0;
        //}

        //EHandler.Animator_SetFloat(animHash_FireIndex, fireIndex);
        //EHandler.Animator_SetTrigger(animHash_Fire);

        // Cam Forces
        //Player.Camera.Physics.PlayDelayedCameraForces(m_PW.Shooting.HandlingCamForces);

        // Ammo
        _ammoProperty--;

        UpdateAmmoInfo();
    }

    public override bool TryStartReload()
    {
        if (m_ReloadLoopEndTime < Time.time && _projectileWeaponInfo.Shooting.EnableAmmo && CurrentAmmoInfo.Val.CurrentInMagazine < _projectileWeaponInfo.Shooting.MagazineSize)
        {
            m_AmmoToAdd = _projectileWeaponInfo.Shooting.MagazineSize - CurrentAmmoInfo.Val.CurrentInMagazine;

            if (CurrentAmmoInfo.Val.CurrentInStorage < m_AmmoToAdd)
                m_AmmoToAdd = CurrentAmmoInfo.Val.CurrentInStorage;

            if (m_AmmoToAdd > 0)
            {
                //EHandler.ClearDelayedSounds();

                if (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && _projectileWeaponInfo.Reloading.HasEmptyReload)
                {
                    //Dry Reload
                    if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Once)
                        m_ReloadLoopEndTime = Time.time + _projectileWeaponInfo.Reloading.EmptyReloadDuration;
                    else if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Progressive)
                        m_ReloadStartTime = Time.time + _projectileWeaponInfo.Reloading.EmptyReloadDuration;

                    //EHandler.Animator_SetTrigger(animHash_EmptyReload);

                    //Player.Camera.Physics.PlayDelayedCameraForces(_weaponInfo.Reloading.EmptyReloadLoopCamForces);
                    //EHandler.PlayDelayedSounds(_weaponInfo.Reloading.EmptyReloadSounds);
                }
                else
                {
                    //Tactical Reload
                    if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Once)
                    {
                        m_ReloadLoopEndTime = Time.time + _projectileWeaponInfo.Reloading.ReloadDuration;
                        //EHandler.Animator_SetTrigger(animHash_Reload);

                        //Player.Camera.Physics.PlayDelayedCameraForces(_weaponInfo.Reloading.ReloadLoopCamForces);
                        //EHandler.PlayDelayedSounds(_weaponInfo.Reloading.ReloadSounds);
                    }
                    else if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Progressive)
                    {
                        m_ReloadStartTime = Time.time + _projectileWeaponInfo.Reloading.ReloadStartDuration;
                        //EHandler.Animator_SetTrigger(animHash_StartReload);

                        //Player.Camera.Physics.PlayDelayedCameraForces(_weaponInfo.Reloading.ReloadStartCamForces);
                        //EHandler.PlayDelayedSounds(_weaponInfo.Reloading.ReloadStartSounds);
                    }
                }

                if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Once)
                    UpdateAmmoInfo();

                m_GeneralEvents.OnReload.Invoke(true); // Invoke the Reload Start Unity Event

                return true;
            }
        }

        return false;
    }

    //This method is called by the 'Equipment Handler' to check if the reload is finished
    public override bool IsDoneReloading()
    {
        if (!m_ReloadLoopStarted)
        {
            if (Time.time > m_ReloadStartTime)
            {
                if (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && _projectileWeaponInfo.Reloading.HasEmptyReload)
                {
                    //Empty/Dry Reload
                    m_ReloadLoopStarted = true;

                    if (_projectileWeaponInfo.Reloading.ProgressiveEmptyReload && _projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Progressive)
                    {
                        if (m_AmmoToAdd > 1)
                        {
                            //Play the reload start State after the empty reload
                            //Player.Camera.Physics.PlayDelayedCameraForces(m_PW.Reloading.ReloadStartCamForces);
                            //EHandler.PlayDelayedSounds(m_PW.Reloading.ReloadStartSounds);

                            m_ReloadLoopEndTime = Time.time + _projectileWeaponInfo.Reloading.ReloadStartDuration;
                            //EHandler.Animator_SetTrigger(animHash_StartReload);
                        }
                        else
                        {
                            //GetAmmoFromInventory(1);

                            _ammoProperty++;
                            m_AmmoToAdd--;

                            return true;
                        }
                    }
                }
                else
                {
                    //Tactical Reload
                    m_ReloadLoopStarted = true;
                    m_ReloadLoopEndTime = Time.time + 1;

                    //Player.Camera.Physics.PlayDelayedCameraForces(m_PW.Reloading.ReloadLoopCamForces);
                    //EHandler.PlayDelayedSounds(m_PW.Reloading.ReloadSounds);

                    //EHandler.Animator_SetTrigger(animHash_Reload);
                }
            }

            return false;
        }

        if (m_ReloadLoopStarted && Time.time >= m_ReloadLoopEndTime)
        {
            if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Once || (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && !_projectileWeaponInfo.Reloading.ProgressiveEmptyReload))
            {
                _ammoProperty += m_AmmoToAdd;
                //GetAmmoFromInventory(m_AmmoToAdd);
                m_AmmoToAdd = 0;
            }
            else if (_projectileWeaponInfo.Reloading.ReloadType == ProjectileWeaponInfo.ReloadType.Progressive)
            {
                if (m_AmmoToAdd > 0)
                {
                    //GetAmmoFromInventory(1);

                    _ammoProperty++;
                    m_AmmoToAdd--;
                }

                if (m_AmmoToAdd > 0)
                {
                    //Player.Camera.Physics.PlayDelayedCameraForces(_weaponInfo.Reloading.ReloadLoopCamForces);
                    //EHandler.PlayDelayedSounds(_weaponInfo.Reloading.ReloadSounds);

                    //EHandler.Animator_SetTrigger(animHash_Reload);
                    m_ReloadLoopEndTime = Time.time + _projectileWeaponInfo.Reloading.ReloadDuration;
                }
                else if (!m_EndReload)
                {
                    //EHandler.Animator_SetTrigger(animHash_EndReload);
                    m_EndReload = true;
                    m_ReloadLoopEndTime = Time.time + _projectileWeaponInfo.Reloading.ReloadEndDuration;

                    //Player.Camera.Physics.PlayDelayedCameraForces(_weaponInfo.Reloading.ReloadEndCamForces);
                    //EHandler.PlayDelayedSounds(_weaponInfo.Reloading.ReloadEndSounds);
                }
                else
                    m_EndReload = false;
            }

            UpdateAmmoInfo();

            return !m_EndReload && m_AmmoToAdd == 0;
        }

        return false;
    }

    public override bool CanBeUsed()
    {
        return CurrentAmmoInfo.Get().CurrentInMagazine > 0 || !_projectileWeaponInfo.Shooting.EnableAmmo;
    }

    public void UpdateAmmoInfo()
    {
        if (!_projectileWeaponInfo.Shooting.EnableAmmo)
            return;

        CurrentAmmoInfo.Set(
            new AmmoInfo()
            {
                CurrentInMagazine = _ammoProperty,

                // Get the ammo count from the inventory
                CurrentInStorage = 300
            });
    }
}
