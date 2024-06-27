using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{

    // Ammo Hud
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;


    // Weapon
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    // Throwables
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;

    public Sprite emptySlot;

    public GameObject middleDot;
    public static HUDManager Instance { get; set; }
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

    private void Update()
    {
        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>(); // Retrieves our active weapon and stores in a variable
        Weapon unActiveWeapon = GetUnActiveWeaponSlot().GetComponentInChildren<Weapon>(); // Gets the unactive weapon

        if (activeWeapon)
        {
            magazineAmmoUI.text = $"{activeWeapon.bulletsLeft / activeWeapon.bulletsPerBurst}"; // The ammo we have in our magazine
            totalAmmoUI.text = $"{WeaponManager.Instance.CheckAmmoLeftFor(activeWeapon.thisWeapon)}";  // Our total ammo outside the magazine

            Weapon.WeaponModel model = activeWeapon.thisWeapon;
            
            // Sets the sprites
            ammoTypeUI.sprite = GetAmmoSprite(model);
            activeWeaponUI.sprite = GetWeaponSprite(model);
            if (unActiveWeapon)
            {
                unActiveWeaponUI.sprite = GetWeaponSprite(unActiveWeapon.thisWeapon);
            }
        }
        else
        {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";

            ammoTypeUI.sprite = emptySlot;

            activeWeaponUI.sprite = emptySlot;
            unActiveWeaponUI.sprite = emptySlot;
        }
    }

    private Sprite GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Glock:
                return (Resources.Load<GameObject>("Glock_Weapon")).GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.AK47:
                return (Resources.Load<GameObject>("AK47_Weapon")).GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private Sprite GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Glock:
                return (Resources.Load<GameObject>("Pistol_Ammo")).GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.AK47:
                return (Resources.Load<GameObject>("Rifle_Ammo")).GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private GameObject GetUnActiveWeaponSlot()
    {
        foreach(GameObject weaponSlot in WeaponManager.Instance.weaponSlots)
        {
            if(weaponSlot != WeaponManager.Instance.activeWeaponSlot) // If our weapon in our weapon list does not amtch the active one, then it is the unactive weapon
            {
                return weaponSlot;
            }
        }
        return null;
    }
}
