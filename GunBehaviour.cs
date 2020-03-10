using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBehaviour : MonoBehaviour
{
	[SerializeField] private Transform spawn;
	[SerializeField] private LayerMask detectionMask;
	[SerializeField] private float damage = 15;
	[Space]
	[SerializeField] private Transform muzzleFlashPosition = default;
	[SerializeField] private GameObject muzzleFlash = default;
	[SerializeField] private GameObject gunTrail = default;
	[Space]
	[SerializeField] private Transform parent;

	RaycastHit hit;

	private void Start()
	{
		parent = gameObject.GetComponentInParent<Transform>();
		muzzleFlash.SetActive(false);
	}

	public void Shoot()
	{
		if(Time.timeScale != 0f)
		{
			Ray ray = new Ray(spawn.position, spawn.forward);

			float shotDistance = Mathf.Infinity;
			if(Physics.Raycast(ray, out hit, shotDistance, detectionMask))
			{
				if(hit.transform.tag == "Enemy")
				{
					//GameObject enemy = hit.transform.gameObject;
					hit.transform.gameObject.GetComponent<EnemyBehaviour>().Health -= damage;
				}
			}

			StartCoroutine(ShowMuzzleFlash());
			GameObject muzzleFlashGO = Instantiate(muzzleFlash, muzzleFlashPosition.position, Quaternion.identity);
			muzzleFlashGO.transform.rotation = parent.rotation;
			Destroy(muzzleFlashGO, 0.1f);

			Debug.DrawRay(ray.origin, ray.direction * shotDistance, Color.red, 2);
		}
	}

	private IEnumerator ShowMuzzleFlash()
	{
		muzzleFlash.SetActive(true);
		gunTrail.SetActive(true);
		gunTrail.transform.rotation = parent.rotation;
		gunTrail.GetComponent<LineRenderer>().SetPosition(0, spawn.position);
		gunTrail.GetComponent<LineRenderer>().SetPosition(1, hit.point);
		yield return new WaitForSeconds(0.1f);
		muzzleFlash.SetActive(false);
		gunTrail.SetActive(false);
		yield return null;
	}
}
