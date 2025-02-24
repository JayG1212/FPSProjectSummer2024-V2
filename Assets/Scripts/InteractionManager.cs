using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; set; }
    public Weapon hoveredWeapon = null;
    public AmmoBox hoveredAmmoBox;
    public Throwable hoveredThrowable = null;

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
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHitByRaycast = hit.transform.gameObject;
            if (objectHitByRaycast.GetComponent<Weapon>() && objectHitByRaycast.GetComponent<Weapon>().isActiveWeapon == false)
            {
                print("Press F to equip");
                hoveredWeapon = objectHitByRaycast.gameObject.GetComponent<Weapon>();
                hoveredWeapon.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickUpWeapon(objectHitByRaycast.gameObject);
                }
                else
                {
                    if (hoveredWeapon)
                    {
                        hoveredWeapon.GetComponent<Outline>().enabled = true;
                    }

                }
            }
            else
            {
                if (hoveredWeapon) 
                { 
                    hoveredWeapon.GetComponent<Outline>().enabled = false; 
                }

            }
            // Ammo Box
            if (objectHitByRaycast.GetComponent<AmmoBox>())
            {
                print("Press F to pick up");
                hoveredAmmoBox = objectHitByRaycast.gameObject.GetComponent<AmmoBox>();
                hoveredAmmoBox.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickUpAmmo(hoveredAmmoBox);
                    Destroy(objectHitByRaycast.gameObject);
                }
                else
                {
                    if (hoveredAmmoBox)
                    {
                        hoveredAmmoBox.GetComponent<Outline>().enabled = true;
                    }

                }
            }
            else
            {
                if (hoveredAmmoBox) 
                { 
                    hoveredAmmoBox.GetComponent<Outline>().enabled = false; 
                }

            }


            // Throwable
            if (objectHitByRaycast.GetComponent<Throwable>())
            {
                print("Press F to pick up");
                hoveredThrowable = objectHitByRaycast.gameObject.GetComponent<Throwable>();
                hoveredThrowable.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickUpThrowable(hoveredThrowable);
                }
                else
                {
                    if (hoveredThrowable)
                    {
                        hoveredThrowable.GetComponent<Outline>().enabled = true;
                    }

                }
            }
            else
            {
                if (hoveredThrowable)
                {
                    hoveredThrowable.GetComponent<Outline>().enabled = false;
                }

            }

        }
    }
}

