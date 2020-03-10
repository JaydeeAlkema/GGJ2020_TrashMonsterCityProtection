using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
	private Vector3 cameraTarget = default;

	[SerializeField]
	float zoomChangeAmount = 80;
	float height = 10f;

	[SerializeField]
	float maxZoomDistance = default;
	[SerializeField]
	float minZoomDistance = default;

	[SerializeField]
	private Transform target = default;

	private void Start()
	{
		height = transform.position.y;
	}

	void Update()
	{
		cameraTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
		transform.position = Vector3.Lerp(transform.position, target.position + new Vector3(0, height, 0), Time.deltaTime * 8);

		if(Input.mouseScrollDelta.y > 0)
		{
			if(height > minZoomDistance)
			{
				height -= zoomChangeAmount * Time.deltaTime;
			}
		}
		if(Input.mouseScrollDelta.y < 0)
		{
			if(height < maxZoomDistance)
			{
				height += zoomChangeAmount * Time.deltaTime;
			}
		}
	}
}
