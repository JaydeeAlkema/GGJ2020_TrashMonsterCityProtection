using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;


public class EnemyBehaviour : MonoBehaviour
{
	/// <summary>
	/// The state of the Enemy.
	/// Searching = Searching for a target (a.k.a. the Player).
	/// Tracking = Moving after the target (a.k.a. the Player).
	/// ReturningToStartPos = Returning to the starting position of this Agent.
	/// Attacking = Attacking the target when inside a radius.
	/// </summary>
	enum AgentState
	{
		Searching,
		Tracking,
		ReturningToStartPos,
		Attacking
	}

	[SerializeField] private AgentState agentState = AgentState.Searching;  // The agent starts of with searching for a target.
    [Space]
    [SerializeField] private float attackDamage = 2;
    [SerializeField] private Transform target = default;    // The target transform.
	[SerializeField] private float targetMinRange = default;    // The minimum range for the target to be within range of the Agent to start tracking.
	[SerializeField] private float targetAttackDistance = default;  // The distance the Agent will attack it's target in.
	[SerializeField] private LayerMask targetMask = default;    // The target mask. 
	[Space]
	[SerializeField] private NavMeshAgent agent = default;  // Reference to own Agent Navmesh.
	[SerializeField] private float moveSpeed = default; // Speed of the agent.
	[SerializeField] [Range(1, 100)] private float health = 100;
	[SerializeField] private float targetSearchInterval = default;  // The time between searches.
	[SerializeField] private float moveToTargetInterval = default;  // The time between the agent destination being updated.
	[Space]
	[SerializeField] private float maxDistanceToStartingPos = default;  // Max distance the enemy can move from starting position before it will return to it's starting pos.
	[SerializeField] private Vector3 startingPos = default; // The starting pos of this agent.
	[Space]
	[SerializeField] private Animator anim = default;   // The animation controller of the enemy
	[Space]
	[SerializeField] private GameObject scrapPrefab = default;  // The scrap prefab gameobject.
	[SerializeField] private float scrapSpawnDistance = 2f;
	[SerializeField] private int minimumScrapToSpawn = 10;
	[SerializeField] private int maximumScrapToSpawn = 100;
	[Space]
	[SerializeField] private GameObject onDeathParticles = default; // The particles that spawn when dying.

	private float _SearchInterval = default;
	private float _MoveToTargetInterval = default;
	private Vector3 pos;

	public float Health { get => health; set => health = value; }

	private void Start()
	{
		_SearchInterval = targetSearchInterval;
		_MoveToTargetInterval = moveToTargetInterval;

		startingPos = transform.position;
		agent.speed = moveSpeed;
	}

	private void Update()
	{
		if(agentState == AgentState.Searching)
			FindNearestTarget();

		if(agentState == AgentState.Tracking)
		{
			MoveToTarget();

			if(target && Vector3.Distance(transform.position, startingPos) >= maxDistanceToStartingPos)
				agentState = AgentState.ReturningToStartPos;

			if(target && Vector3.Distance(transform.position, target.position) <= targetAttackDistance)
				agentState = AgentState.Attacking;
		}

		if(agentState == AgentState.ReturningToStartPos)
			MoveToStartingPos();

		if(agentState == AgentState.Attacking)
			Attack();
		if(health < 0)
		{
			Die();
		}
	}

	/// <summary>
	/// Calls the FindTarget function on a timer.
	/// </summary>
	private void FindNearestTarget()
	{
		_SearchInterval -= Time.time;
		if(_SearchInterval <= 0f)
		{
			_SearchInterval = targetSearchInterval;
			target = FindTarget();
		}
	}

	/// <summary>
	/// Sets the target destination for the agent destination.
	/// </summary>
	private void MoveToTarget()
	{
		if(target)
		{
			anim.SetFloat("Motion", 0.5f);
			_MoveToTargetInterval -= Time.time;
			if(_MoveToTargetInterval <= 0f)
			{
				_MoveToTargetInterval = moveToTargetInterval;
				agent.destination = target.position;
			}
		}
	}

	/// <summary>
	/// Moves the enemy back to it's starting position.
	/// </summary>
	private void MoveToStartingPos()
	{
		anim.SetFloat("Motion", 0.5f);
		agentState = AgentState.ReturningToStartPos;
		agent.destination = startingPos;

		if(Vector3.Distance(transform.position, startingPos) <= 1f)
			agentState = AgentState.Searching;
	}

	/// <summary>
	/// Attacks the target when in range.
	/// </summary>
	private void Attack()
	{
		anim.SetFloat("Motion", 1f);
		agent.destination = transform.position;
		if(target && Vector3.Distance(transform.position, target.position) > targetAttackDistance)
			agentState = AgentState.Tracking;
		if (target != null && target.gameObject.GetComponent<PlayerControllerBehaviour>())
		{
            target.gameObject.GetComponent<PlayerControllerBehaviour>().Health -= attackDamage;

        }
	}

	/// <summary>
	/// Finds the closest target. This will only happen again when the old target is forgotten.
	/// </summary>
	/// <returns></returns>
	private Transform FindTarget()
	{
		anim.SetFloat("Motion", 0f);
		Collider[] objectsInRange = Physics.OverlapSphere(transform.position, targetMinRange, targetMask);

		Transform bestTarget = null;
		float closestDistanceSqr = Mathf.Infinity;
		Vector3 currentPos = transform.position;
		foreach(Collider potentialTarget in objectsInRange)
		{
			Vector3 directionToTarget = potentialTarget.transform.position - currentPos;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if(dSqrToTarget < closestDistanceSqr)
			{
				closestDistanceSqr = dSqrToTarget;
				bestTarget = potentialTarget.transform;
				agentState = AgentState.Tracking;
			}
		}
		return bestTarget;
	}

	/// <summary>
	/// Gizmos for debugging purposes.
	/// </summary>
	private void OnDrawGizmosSelected()
	{
		// Enemy Search Range.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, targetMinRange);

		// Transform Starting Position Range. (Home)
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, maxDistanceToStartingPos);

		// Player Detection Radius.
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, targetAttackDistance);

		// Line & Cube to startingPos.
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(transform.position, startingPos);
		Gizmos.DrawCube(startingPos, new Vector3(0.25f, 0.25f, 0.25f));

		// When target is found draw a line and place a cube at the target pos.
		if(target)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(transform.position, agent.destination);
			Gizmos.color = Color.red;
			Gizmos.DrawCube(agent.destination, new Vector3(0.25f, 0.25f, 0.25f));
		}
	}

	void Die()
	{
		int i = Random.Range(minimumScrapToSpawn, maximumScrapToSpawn);

		for(int x = 0; x < i; x++)
		{
			Vector3 pos = new Vector3(Random.Range(transform.position.x - scrapSpawnDistance, transform.position.x + scrapSpawnDistance), 1f, Random.Range(transform.position.z - scrapSpawnDistance, transform.position.z + scrapSpawnDistance));
			Instantiate(scrapPrefab, pos, Quaternion.identity);
		}

		Instantiate(onDeathParticles, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));

		Destroy(gameObject);
	}

}
