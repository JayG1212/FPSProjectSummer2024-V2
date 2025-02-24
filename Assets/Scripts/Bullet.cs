using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public int bulletDamage;
    private void OnCollisionEnter(Collision objectWeHit)
    {
        if (objectWeHit.gameObject.CompareTag("Target"))
        {
            print("hit " + objectWeHit.gameObject.name + "!");
            Destroy(gameObject);
            CreateBulletImpactEffect(objectWeHit);
        }
        if (objectWeHit.gameObject.CompareTag("Wall"))
        {
            print("hit a wall!");

            CreateBulletImpactEffect(objectWeHit);

            Destroy(gameObject);
        }

        if (objectWeHit.gameObject.CompareTag("Bottle"))
        {
            print("Hit a bottle");
            objectWeHit.gameObject.GetComponent<BeerBottle>().Shatter();
        }

        if (objectWeHit.gameObject.CompareTag("Enemy"))
        {
            print("Hit a Enemy!");

            if (objectWeHit.gameObject.GetComponent<Enemy>().isDead == false)
            {
                objectWeHit.gameObject.GetComponent<Enemy>().TakeDamage(bulletDamage); // Stops from having the enemy having negative health and stops the animation from looping
                
            }
            
            CreateBloodSpreadEffect(objectWeHit);
            
            Destroy(gameObject);
        }
    }


    private void CreateBloodSpreadEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];

        GameObject bloodSprayPrefab = Instantiate(GlobalReferences.Instance.bloodSprayEffect, contact.point, Quaternion.LookRotation(contact.normal));

        bloodSprayPrefab.transform.SetParent(objectWeHit.gameObject.transform);
        Destroy(gameObject);
    }

    void CreateBulletImpactEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];

        GameObject hole = Instantiate(GlobalReferences.Instance.bulletImpactEffectPrefab, contact.point,Quaternion.LookRotation(contact.normal));

        hole.transform.SetParent(objectWeHit.gameObject.transform);
    }
    
}
