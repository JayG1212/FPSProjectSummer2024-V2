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
}
