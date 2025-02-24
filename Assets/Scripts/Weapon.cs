using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;
    public int weaponDamage;
    [Header("Shooting")]
    // Shooting
    public bool isShooting;
    public bool readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    [Header("Burst")]
    // Burst
    public int bulletsPerBurst = 3;
    public int burstBulletsLeft;

    [Header("Spread")]
    // Spread
    public float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    [Header("Bullet")]
    // Bullet Prroperties
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;
   

    // Animations and Particles
    public GameObject muzzleEffect;
    internal Animator animator;

    [Header("Reload & Magazine")]
    // Loading
    public float reloadTime;
    public int magazineSize;
    public int bulletsLeft;
    public Boolean isReloading;

    // Spawn and rotation positiosn for picked up weapons
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    bool isADS = false;

    // Types of Shootings
    public enum ShootingMode
    {
        Single,
        Burst,
        Automatic
    }

    // Type of weapons
    public enum WeaponModel
    {
        Glock,
        AK47
    }

    public WeaponModel thisWeapon;
    public ShootingMode currentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();
        // Sets bullets to default amount
        bulletsLeft = magazineSize;

        spreadIntensity = hipSpreadIntensity;
    }
    void Update()
    {
        if (isActiveWeapon)
        {
           
            GetComponent<Outline>().enabled = false; //  Fixes bug that keeps weapon outlined after swapping fast

            // Aim-down sights
            if (Input.GetMouseButtonDown(1))
            {
                animator.SetTrigger("enterADS");
                isADS = true;
                HUDManager.Instance.middleDot.SetActive(false);
                spreadIntensity = adsSpreadIntensity;
            }
            if (Input.GetMouseButtonUp(1))
            {
                animator.SetTrigger("exitADS");
                isADS = false;
                HUDManager.Instance.middleDot.SetActive(true);
                spreadIntensity = hipSpreadIntensity;
            }


            // Empty Magazine sound
            if ((bulletsLeft <= 0 || isReloading)&& isShooting)
            {
                SoundManager.Instance.emptyMagazineSound.Play();
            }

            // Automatic shooting
            if (currentShootingMode == ShootingMode.Automatic)
            {
                // Holding down left click
                isShooting = Input.GetKey(KeyCode.Mouse0); // GetKey allows for us to hold it down
            }
            
            // Semi-Automatic or Burst
            else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
            {

                isShooting = Input.GetKeyDown(KeyCode.Mouse0); // GetKeyDown makes us have to click again
            }

            // Reloading
            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && WeaponManager.Instance.CheckAmmoLeftFor(thisWeapon)> 0) // Manual reload
            {
                Reload();
            }
            if (readyToShoot && bulletsLeft == 0 && !isShooting && !isReloading && WeaponManager.Instance.CheckAmmoLeftFor(thisWeapon) > 0)
            {
                Reload();
            }


            if (readyToShoot && isShooting && bulletsLeft > 0)
            {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }

       }
       
        
    }

   

    private void FireWeapon()
    {
        if (isReloading == false)
        {
            // Each time you fire the gun the bullets decrease
            bulletsLeft--;
            // Animationsa and particles will be applied after the weapon is fired
            muzzleEffect.GetComponent<ParticleSystem>().Play();


            if (isADS)
            {
                animator.SetTrigger("RECOIL_ADS");
            }
            else
            {

                animator.SetTrigger("RECOIL");
            }
            // Sound will be played when weapon is fired
            SoundManager.Instance.PlayShootingSound(thisWeapon);
            readyToShoot = false;

            Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

            // Instantiate the bullet
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

            Bullet aBullet = bullet.GetComponent<Bullet>();
            aBullet.bulletDamage = weaponDamage;

            // Pointing the bullet to face the shooting direction
            bullet.transform.forward = shootingDirection;

            // Shoot the bullett
            bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);

            // Destroy bullet after some time
            StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

            // Check if we are done shooting
            if (allowReset)
            {
                Invoke("ResetShot", shootingDelay);
                allowReset = false;
            }

            // Burst Mode
            if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1) // we shoot once bbefore this check
            {
                burstBulletsLeft--;
                Invoke("FireWeapon", shootingDelay);
            }
        }

        

    }

    private void Reload()
    {
        isReloading = true;
        SoundManager.Instance.PlayReloadSound(thisWeapon);
        animator.SetTrigger("RELOADING");
        Invoke("ReloadCompleted", reloadTime);


    }

    private void ReloadCompleted()
    {
        if(WeaponManager.Instance.CheckAmmoLeftFor(thisWeapon) > magazineSize)
        {
            bulletsLeft = magazineSize;
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeapon);
        }
        else
        {
            bulletsLeft = WeaponManager.Instance.CheckAmmoLeftFor(thisWeapon);
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeapon);
        }
        isReloading = false;
    }
    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        // Shooting from the middle of our screen to check where are we pointing at
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;
        if(Physics.Raycast(ray,out hit))
        {
            // Hitting something
            targetPoint = hit.point;
        }
        else
        {
            // Shooting at the air
            targetPoint = ray.GetPoint(100);
        }
        Vector3 direction = targetPoint - bulletSpawn.position;

        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        // Returning the shooting direction and spread
        return direction + new Vector3(0, y, z);

    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
