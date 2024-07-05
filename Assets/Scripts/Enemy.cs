// Written by Jay Gunderson
// 07/01/2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int HP = 100;
    private Animator animator;

    private NavMeshAgent navAgent;

    public bool isDead= false;
    private void Start()
    {
        // Gets both the Animator component and NavMeshAgent of the object that has this attached to it
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;
        if (HP <= 0)
        {

            int randomValue = Random.Range(0, 2); // 0 or 1

            if (randomValue == 0 && isDead == false)
            {
                animator.SetTrigger("DIE1");
            }
            else if (randomValue == 1 && isDead == false) // Made it an else if instead of else in case I add another animation
            {
                animator.SetTrigger("DIE2");
            }
            isDead = true;
            SoundManager.Instance.zombieChannel.PlayOneShot(SoundManager.Instance.zombieDeath);
        }
        else
        {
            animator.SetTrigger("DAMAGE");
            SoundManager.Instance.zombieChannel.PlayOneShot(SoundManager.Instance.zombieHurt);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f); // Start attacking/Stop attacking

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 100f); // Detection (Start Chasing)

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f); // Stop Chasing
    }
}
