// Written by Jay Gunderson
// 07/03/2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    public AudioSource ShootingChannel;
    public AudioSource reloadingSoundGlock;
    public AudioSource reloadingSoundAK47;
    public AudioSource emptyMagazineSound;
    public AudioClip GlockShoot;
    public AudioClip AK47Shoot;

    public AudioSource throwablesChannel;
    public AudioClip grenadeSound;

    [Header("Zombie Sounds")]
    public AudioClip zombieWalking;
    public AudioClip zombieChase;
    public AudioClip zombieAttack;
    public AudioClip zombieHurt;
    public AudioClip zombieDeath;
    public AudioSource zombieChannel;

    [Header("Player Sounds")]
    public AudioSource playerChannel;
    public AudioClip playerHurt;
    public AudioClip playerDie;
    
    public AudioClip gameOverMusic;

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

    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Glock:
               ShootingChannel.PlayOneShot(GlockShoot);
                break;
            case WeaponModel.AK47:
                ShootingChannel.PlayOneShot(AK47Shoot);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Glock:
                reloadingSoundGlock.Play();
                break;
            case WeaponModel.AK47:
                reloadingSoundAK47.Play();
                break;
        }
    }

}
