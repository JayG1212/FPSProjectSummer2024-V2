// Written by Jay Gunderson
// 07/02/2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class ZombieChaseState : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;

    public float chaseSpeed = 6f;

    public float stopChasingDistance = 21;
    public float attackingDistance = 2.5f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Initialization
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();

        agent.speed = chaseSpeed;

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance.zombieChannel.isPlaying == false)
        {
            SoundManager.Instance.zombieChannel.PlayOneShot(SoundManager.Instance.zombieChase);
        }
        agent.SetDestination(player.position);
        animator.transform.LookAt(player);

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        // If the player should stop chasing

        if(distanceFromPlayer > stopChasingDistance)
        {
            animator.SetBool("isChasing", false);
        }

        // If the agent should attack

        if(distanceFromPlayer < attackingDistance)
        {
            animator.SetBool("isAttacking", true);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateinfo, int layerindex)
    {
        agent.SetDestination(animator.transform.position);

        SoundManager.Instance.zombieChannel.Stop();
    }
}
