using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class FriendlyBehaviour : MonoBehaviour
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
	[SerializeField] private Transform target = default;    // The target transform.
	[SerializeField] private float targetMinRange = default;    // The minimum range for the target to be within range of the Agent to start tracking.
	[SerializeField] private float targetAttackDistance = default;  // The distance the Agent will attack it's target in.
	[SerializeField] private LayerMask targetMask = default;    // The target mask. 
	[SerializeField] private float targetAttackInterval = 0.1f;  // The interval between attacks.
	[Space]
	[SerializeField] private NavMeshAgent agent = default;  // Reference to own Agent Navmesh.
	[SerializeField] private float moveSpeed = default; // Speed of the agent.
	[SerializeField] private float targetSearchInterval = 0.1f;  // The time between searches.
	[SerializeField] private float moveToTargetInterval = default;  // The time between the agent destination being updated.
	[Space]
	[SerializeField] private float maxDistanceToStartingPos = default;  // Max distance the enemy can move from starting position before it will return to it's starting pos.
	[SerializeField] private Vector3 startingPos = default; // The starting pos of this agent.
	[Space]
	[SerializeField] private GunBehaviour gun = default;    // The gun of this friendly pawn.
	[SerializeField] private LayerMask collidableMask = default;    // the layermask that this pawn will check if anything is in between this and the target.
	[Space]
	[SerializeField] private Animator anim;
	[SerializeField] private bool idle, running, shooting;

	private Ray ray;

	private void Start()
	{
		targetAttackInterval = Random.Range(0.05f, 0.15f);

		startingPos = transform.position;
		agent.speed = moveSpeed;

		StartCoroutine(FindNearestTarget());
		StartCoroutine(MoveToTarget());
		StartCoroutine(MoveToStartingPos());
		StartCoroutine(Attack());
	}

	private void Update()
	{
		if(target == null)
		{
			agent.destination = transform.position;
			agentState = AgentState.Searching;
			idle = true;
		}
		else
		{
			idle = false;
		}

		anim.SetBool("Idle", idle);
		anim.SetBool("Running", running);
		anim.SetBool("Shoot", shooting);
	}

	/// <summary>
	/// Calls the FindTarget function on a timer.
	/// </summary>
	private IEnumerator FindNearestTarget()
	{
		while(true)
		{
			target = FindTarget();
			yield return new WaitForSeconds(targetSearchInterval);
		}
	}

	/// <summary>
	/// Sets the target destination for the agent destination.
	/// </summary>
	private IEnumerator MoveToTarget()
	{
		while(true)
		{
			if(agentState == AgentState.Tracking && target)
			{
				agent.destination = target.position;
				running = true;

				if(Vector3.Distance(transform.position, target.position) <= targetAttackDistance)
					agentState = AgentState.Attacking;

				if(Vector3.Distance(transform.position, startingPos) >= maxDistanceToStartingPos)
					agentState = AgentState.ReturningToStartPos;
			}
			else
			{
				running = false;
			}
			yield return new WaitForSeconds(moveToTargetInterval);
		}
	}

	/// <summary>
	/// Moves the enemy back to it's starting position.
	/// </summary>
	private IEnumerator MoveToStartingPos()
	{
		while(true)
		{
			if(agentState == AgentState.ReturningToStartPos)
			{
				agent.destination = startingPos;
				running = true;

				if(Vector3.Distance(transform.position, startingPos) <= agent.stoppingDistance)
					agentState = AgentState.Searching;

				if(Vector3.Distance(transform.position, target.position) <= targetAttackDistance)
					agentState = AgentState.Attacking;
			}
			else
			{
				running = false;
			}
			yield return new WaitForSeconds(moveToTargetInterval);
		}
	}

	/// <summary>
	/// Attacks the target when in range.
	/// </summary>
	private IEnumerator Attack()
	{
		while(true)
		{
			if(agentState == AgentState.Attacking)
			{
				if(target == null)
					break;

				shooting = true;

				agent.destination = transform.position;
				transform.LookAt(target.transform);

				gun.Shoot();

				if(Vector3.Distance(transform.position, target.position) > targetAttackDistance)
					agentState = AgentState.Tracking;
			}
			else
			{
				shooting = false;
			}
			yield return new WaitForSeconds(targetAttackInterval);
		}
	}

	/// <summary>
	/// Finds the closest target. This will only happen again when the old target is forgotten.
	/// </summary>
	/// <returns></returns>
	private Transform FindTarget()
	{
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
}
