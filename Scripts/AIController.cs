using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

public class AIController : MonoBehaviour, IDamageable {

    public enum AgentState {
        Idle = 0,
        Patrolling,
        Chasing,
        Attacking,
        Dead,
        Disabled
    }

    public AgentState state;
    public Transform[] waypoints;
    private NavMeshAgent navMeshAgent;

    private Animator animator;
    private int speedHashId;

    public float distanceToStartHeadingToNextWaypoint = 1;
    public int waypointId = 0;

    public float timeSpentIdle = float.PositiveInfinity;

    public Transform target;
    public float distanceToStartChasingTarget = 15.0f;
    public float distanceToKeepChasingTarget = 30.0f;
    public float hearingProximity = 2.0f;
    public float distanceToAttack = 10.0f;

    public Vector3 WeaponHeight = new Vector3(0, 1.25f, 0);

    public float health = 50f;

    public bool playerVisible;
    private int playerLayerMask;

    public GunController currentGun;
    public float fireRate = 1f;
    private float nextTimeToFire = 0f;

    public AudioClip shootSound;
    public AudioSource audioSource;

    private AIManager aiManager;


    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        speedHashId = Animator.StringToHash("Speed");

        playerLayerMask = 1 << 11;
        playerLayerMask = ~playerLayerMask;

        aiManager = GameObject.Find("AI").GetComponent<AIManager>();

        state = AgentState.Patrolling;
    }

    void Update() {

        if (!target) {
            target = GameObject.Find("Player(Clone)").transform;
        }

        if (MPManager.gameOver) {
            return;
        }

        //Debug.DrawLine((transform.position + WeaponHeight), target.position, Color.white, 2.5f);

        if (Physics.Linecast((transform.position + WeaponHeight), target.position, playerLayerMask)) {
            playerVisible = false;
        } else {
            playerVisible = true;
        }

        if (state == AgentState.Idle) {
            Idle();
        } else if (state == AgentState.Patrolling) {
            Patrol();
        } else if (state == AgentState.Chasing) {
            Chase();
        } else if (state == AgentState.Attacking) {
            Attack();
        } else if (state == AgentState.Dead) {
            Dead();
        } else if (state == AgentState.Disabled) {
            Disabled();
        }
    }


    void Chase() {

        state = AgentState.Chasing;

        navMeshAgent.SetDestination(target.position);

        AISpeed(false, 3f, 1f);

        if (distanceToAttack > DistanceToPlayer() && playerVisible == true) {
            Attack();
        } else if (distanceToKeepChasingTarget < DistanceToPlayer()) {
            Idle();
        }
    }

    void Idle() {

        state = AgentState.Idle;
        CheckForTarget();

        AISpeed(true, 0f, 0f);

        navMeshAgent.speed = 0f;
        animator.SetFloat(speedHashId, 0.0f);


        timeSpentIdle += Time.deltaTime;
        if (timeSpentIdle > 5) {
            Patrol();
        }
    }

    void Patrol() {

        state = AgentState.Patrolling;
        CheckForTarget();

        AISpeed(false, 0.5f, 0.5f);

        navMeshAgent.SetDestination(waypoints[waypointId].position);

        if (navMeshAgent.remainingDistance < distanceToStartHeadingToNextWaypoint) {

            waypointId = (waypointId + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[waypointId].position);

            timeSpentIdle = 0;
            Idle();
        }
    }

    bool CheckForTarget() {

        Vector3 planarDifference = target.position - transform.position;
        planarDifference.y = 0;
        float actualAngle = Vector3.Angle(planarDifference, transform.forward);

        if (distanceToStartChasingTarget > DistanceToPlayer() && actualAngle < 110) {
            Chase();
            return true;

        } else if (DistanceToPlayer() < hearingProximity) {
            Chase();
            return true;
        }

        return false;
    }

    void Attack() {

        state = AgentState.Attacking;
        AISpeed(false, 0.0f, 0.0f);
        animator.SetBool("Attacking", true);

        RotateTowardsTarget();


        if (currentGun.Shoot()) {
            int chanceToHit = UnityEngine.Random.Range(0, 10);

            if (chanceToHit > 1) {
                target.GetComponent<FPSPlayerController>().TakeDamage(10);
                aiManager.score -= 10;
                audioSource.PlayOneShot(shootSound, 0.1f);
            }
        }


        if (DistanceToPlayer() > distanceToStartChasingTarget || !playerVisible) {
            animator.SetBool("Attacking", false);
            Chase();
        }
    }



    void Dead() {
        state = AgentState.Dead;
        navMeshAgent.isStopped = true;
        animator.SetBool("Dead", true);
        aiManager.HandleDeath();
        Disabled();
    }

    void Disabled() {
        state = AgentState.Disabled;
    }

    void AISpeed(bool stopped, float groundSpeed, float animationSpeed) {
        navMeshAgent.isStopped = stopped;
        navMeshAgent.speed = groundSpeed;
        animator.SetFloat(speedHashId, animationSpeed);
    }

    void RotateTowardsTarget() {
        Vector3 planarDifference = (target.position - transform.position);
        planarDifference.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(planarDifference.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
    }

    float DistanceToPlayer() {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance;
    }

    public void TakeDamage(float Amount) {

        if (state == AgentState.Disabled) {
            return;
        }

        health -= Amount;
        if (health <= 0) {
            Dead();
        }
    }

    public void SetHealth(float health) {
        this.health = health;

        TakeDamage(0);
    }


    public AIControllerRecord ToRecord() {
        return new AIControllerRecord(transform.position, transform.rotation, health, waypoints);
    }
}

[Serializable]
public struct AIControllerRecord {
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public string[] waypoints;

    public AIControllerRecord(Vector3 position, Quaternion rotation, float health, Transform[] waypoints) {
        this.position = position;
        this.rotation = rotation;
        this.health = health;

        this.waypoints = new string[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++) {
            this.waypoints[i] = waypoints[i].name;
        }
    }
}


