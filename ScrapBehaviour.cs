using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapBehaviour : MonoBehaviour
{
	private enum ScrapState
	{
		Idle,
		MovingTowardsPlayer
	}

	[SerializeField] private ScrapState scrapState = ScrapState.Idle;
	[Space]
	[SerializeField] private float sineFrequency = 1;
	[SerializeField] private float sineMagnitude = 1;
	[Space]
	[SerializeField] private string tagToCheck = "Player";
	[Space]
	[SerializeField] private Transform targetTransform = default;
	[SerializeField] private float moveSmoothing = 7.5f;
	[SerializeField] private Vector3 velocity = Vector3.zero;

	private Vector3 pos;

	private void Start()
	{
		pos = transform.position;
	}

	private void Update()
	{
		if(scrapState == ScrapState.MovingTowardsPlayer)
			MoveTowardsPlayer();
		else
			transform.position = pos + transform.up * Mathf.Sin(Time.time * sineFrequency) * sineMagnitude;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag(tagToCheck))
		{
			targetTransform = other.transform;
			scrapState = ScrapState.MovingTowardsPlayer;
		}
	}

	private void MoveTowardsPlayer()
	{
		transform.position = Vector3.SmoothDamp(transform.position, targetTransform.position, ref velocity, moveSmoothing);
		if(Vector3.Distance(transform.position, targetTransform.position) <= 1f)
			Destroy(gameObject);
	}
}
