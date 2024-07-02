using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static Weapon;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }
    public List<GameObject> weaponSlots;
    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;
    


    [Header("Throwables")]
    public float throwForce = 10f;
    public GameObject grenadePrefab;
    public GameObject throwableSpawn;
    public float forceMultiplier  = 0;
    public float forceMultiplierLimit = 3f;



    [Header("Lethals")]
    public int maxLethals = 2;
    public int lethalsCount = 0;
    public Throwable.ThrowableType equippedLethalType;


    [Header("Tacticals")]
    public int maxTacticals = 2;
    public int tacticalCount = 0;
    public Throwable.ThrowableType equippedTacticalType;
    public GameObject smokeGrenadePrefab;
    private void Awake()
    {


        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

    }

    private void Start()
    {
        activeWeaponSlot = weaponSlots[0];

        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;
    }

    private void Update()
    {
        foreach (GameObject weaponSlot in weaponSlots)
        {
            if (weaponSlot == activeWeaponSlot)
            {
                weaponSlot.SetActive(true);
            }
            else
            {
                weaponSlot.SetActive(false);
            }
        }

        if (UnityEngine.Input.GetAxisRaw("Mouse ScrollWheel") > 0f || UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveSlot(1);
            
        }
        if (UnityEngine.Input.GetAxisRaw("Mouse ScrollWheel") < 0f || UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchActiveSlot(0);
        }


        #region Lethals
        
        if (UnityEngine.Input.GetKey(KeyCode.G))
        {
            forceMultiplier += Time.deltaTime;
            if(forceMultiplier > forceMultiplierLimit)
            {
                forceMultiplier = forceMultiplierLimit;
            }
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.G))
        {
            if(lethalsCount > 0)
            {
                ThrowLethal();
            }
            forceMultiplier = 1f;
        }
        
        #endregion


        #region Tacticals
       
        if (UnityEngine.Input.GetKey(KeyCode.T))
        {
            forceMultiplier += Time.deltaTime;
            if (forceMultiplier > forceMultiplierLimit)
            {
                forceMultiplier = forceMultiplierLimit;
            }
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.T))
        {
            if (tacticalCount > 0)
            {
                ThrowTactical();
            }
            forceMultiplier = 1f;
        }
       
        #endregion

    }


    public void PickUpWeapon(GameObject pickedUpWeapon)
    {
        AddWeaponIntoActiveSlot(pickedUpWeapon);

        
    }

    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon)
    {
        DropCurrentWeapon(pickedUpWeapon);
        pickedUpWeapon.transform.SetParent(activeWeaponSlot.transform, false);

        Weapon weapon = pickedUpWeapon.GetComponent<Weapon>();
        pickedUpWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        pickedUpWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);
        weapon.isActiveWeapon = true;
        weapon.animator.enabled = true;
    }

    private void DropCurrentWeapon(GameObject pickedUpWeapon)
    {
        if(activeWeaponSlot.transform.childCount > 0) // If the active weapon slot has a weapon already attached
        {
           
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject; // Saves current weapon in slot to a variable
            weaponToDrop.GetComponent<Weapon>().isActiveWeapon = false; // Disables it
            weaponToDrop.GetComponent<Weapon>().animator.enabled = false;
            weaponToDrop.transform.SetParent(pickedUpWeapon.transform.parent); // Sets the parent of the wweapon as the same weapon we disabled 
            weaponToDrop.transform.localPosition = pickedUpWeapon.transform.localPosition; // Sets the parent to the same position
            weaponToDrop.transform.localRotation = pickedUpWeapon.transform.localRotation; // Sets the parent to the same rotation
        }
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if(activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActiveWeapon = false;
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActiveWeapon = true;
        }
    }

    internal void PickUpAmmo(AmmoBox hoveredAmmoBox)
    {
        switch (hoveredAmmoBox.ammoType)
        {
            case (AmmoBox.AmmoType.RifleAmmo):
                totalRifleAmmo += hoveredAmmoBox.ammoAmount;
                break;
            case (AmmoBox.AmmoType.PistolAmmo):
                totalPistolAmmo += hoveredAmmoBox.ammoAmount;
                break;




        }
        print("Picked up Ammo");
    }

    internal void DecreaseTotalAmmo(int bulletsToDecrease, Weapon.WeaponModel thisWeapon)
    {
        switch (thisWeapon)
        {
            case Weapon.WeaponModel.AK47:
                totalRifleAmmo -= bulletsToDecrease;
                break;
            case Weapon.WeaponModel.Glock:
                totalPistolAmmo -= bulletsToDecrease;
                break;

        
        }
    }

    public int CheckAmmoLeftFor(Weapon.WeaponModel thisWeapon)
    {
        switch (thisWeapon)
        {
            case (Weapon.WeaponModel.AK47):
                return totalRifleAmmo;
            case (Weapon.WeaponModel.Glock):
                return totalPistolAmmo;
            default:
                return 0;
        }
    }

    public void PickUpThrowable(Throwable hoveredThrowable)
    {
       switch (hoveredThrowable.throwableType)
        {
            case Throwable.ThrowableType.Grenade:
                PickupThrowableAsLethal(Throwable.ThrowableType.Grenade);
                break;
            case Throwable.ThrowableType.SmokeGrenade:
                PickupThrowableAsTactical(Throwable.ThrowableType.SmokeGrenade);
                break;
        }
    }

    private void PickupThrowableAsTactical(Throwable.ThrowableType tactical)
    {
        if (equippedTacticalType == tactical || equippedTacticalType == Throwable.ThrowableType.None) // If the lethal we are picking up is the same as the one in our slot, or we have no lethal picked up yet
        {
            equippedTacticalType = tactical;
            if (tacticalCount < maxTacticals) // Limit of tacticals held
            {
                tacticalCount += 1;
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowablesUI();

            }

            else
            {
                print("Tacticals limit reached");
            }
        }
        else
        {

        }
    }

    private void PickupThrowableAsLethal(Throwable.ThrowableType lethal)
    {
        if(equippedLethalType == lethal || equippedLethalType == Throwable.ThrowableType.None) // If the lethal we are picking up is the same as the one in our slot, or we have no lethal picked up yet
        {
            equippedLethalType = lethal;
            if(lethalsCount < maxLethals) // Limit of lethals held
            {
                lethalsCount += 1;
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowablesUI();

            }

            else
            {
                print("Lethals limit reached");
            }
        }
        else
        {

        }
    }

  

    private void ThrowLethal()
    {
        GameObject lethalPrefab = GetThrowablePrefab(equippedLethalType);

        GameObject throwable = Instantiate(lethalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();

        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<Throwable>().hasBeenThrown = true;

        lethalsCount -= 1;


        if(lethalsCount <= 0)
        {
            equippedLethalType = Throwable.ThrowableType.None;
        }
        HUDManager.Instance.UpdateThrowablesUI();
    }


    private void ThrowTactical()
    {
        GameObject tacticalPrefab = GetThrowablePrefab(equippedTacticalType);

        GameObject throwable = Instantiate(tacticalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();

        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);

        throwable.GetComponent<Throwable>().hasBeenThrown = true;

        tacticalCount -= 1;


        if (tacticalCount <= 0)
        {
            equippedTacticalType = Throwable.ThrowableType.None;
        }
        HUDManager.Instance.UpdateThrowablesUI();
    }


    private GameObject GetThrowablePrefab(Throwable.ThrowableType equippedType)
    {
        switch (equippedType)
        {
            case Throwable.ThrowableType.Grenade:
                return grenadePrefab;
            case Throwable.ThrowableType.SmokeGrenade:
                return smokeGrenadePrefab;
        }
        return new();
    }
}
