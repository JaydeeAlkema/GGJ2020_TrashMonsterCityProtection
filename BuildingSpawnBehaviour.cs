using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// The state of the building.
/// </summary>
enum BuildingState
{
	Rundown,
	Repaired,
	Done
}

public class BuildingSpawnBehaviour : MonoBehaviour
{
	#region Variables
	[SerializeField] private BuildingState buildingState = BuildingState.Rundown;   // The state of this building.
	[SerializeField] private float buildingStateCheckInterval = 0.1f;   // The amount of seconds between state checks.
	[Space]
	[SerializeField] private BuildingPrefab[] buildingPrefabs = default;    // A list with all the Building Prefabs. These hold the repared variant and the Rundown variant.
	[SerializeField] private GameObject spawnedBuildingPrefab = default;    // This is just a container for the spawned prefab.
	[Space]
	[SerializeField] private Vector3 buildingPrefabSpawnRotation = default;     // the starting rotation of this building.
	[SerializeField] private Transform buildingPrefabSpawnTransform = default;  // The transform position where the building spawns.
	[SerializeField] private Transform buildingPrefabParent = default;    // The parent of the spawned building.
	[SerializeField] private int buildingIndex = 0;     // The index of the buildingPrefabsArray.
	[Space(20)]
	[SerializeField] private int amountOfScrapNeeded = default; // Amount of scrap needed for the building to be repaired.
	[SerializeField] private int amountOfScrapCollected = 0;    // Amount of scrap collected.

	[SerializeField] private Image progressImage = default;
	[SerializeField] private GameObject textAndIconParent = default;
	[SerializeField] private float textUpdateInterval = 0.1f;   // The amount of seconds between each text update.
	[Space]
	[SerializeField] private float scrapCollectionInterval = 0.1f;  // The delay between collecting scrap from the player to the building.
	[SerializeField] private BoxCollider scrapCollectionCollider = default; // the collider with which the player has to enter to trigger the scrapCollection event.
	[SerializeField] private string tagToCheck = default;   // The tag to check for player within the radius.
	[SerializeField] private bool collectingScrap = false;  // Gets set to true when the player goes within radius.

	[SerializeField] private GameObject friendlyAIPrefab;   // The prefab of the friendly AI.
	[SerializeField] private Transform[] friendlyAIPrefabSpawnPos;  // The position where the friendly AI will spawn.
	[SerializeField] private Transform friendlyAIPrefabSpawnPosPivot;   // The pivot of the ai spawn points.

	[SerializeField] private int repairSpeed = 1;

	[SerializeField] private PlayerControllerBehaviour player; //The player

    internal BuildingState BuildingState { get => buildingState; set => buildingState = value; }
    #endregion

    private void Start()
	{
		player = FindObjectOfType<PlayerControllerBehaviour>();
		SpawnBuilding();
		StartCoroutine(repairProgressUpdate());
		StartCoroutine(AddScrapWhenPlayerWithinRange());
		StartCoroutine(CheckBuildingState());

		textAndIconParent.transform.position = new Vector3(progressImage.transform.position.x, buildingPrefabs[buildingIndex].ScrapTextHeight, progressImage.transform.position.z);
		scrapCollectionCollider.size = buildingPrefabs[buildingIndex].ScrapCollectingDetectionRadius;

		friendlyAIPrefabSpawnPosPivot.rotation = Quaternion.Euler(buildingPrefabSpawnRotation);
	}

	/// <summary>
	/// Triggers when a colider enters the trigger.
	/// </summary>
	/// <param name="other"></param>
	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag(tagToCheck))
		{
			collectingScrap = true;
		}
	}

	/// <summary>
	/// Triggers when a colider exits the trigger.
	/// </summary>
	/// <param name="other"></param>
	private void OnTriggerExit(Collider other)
	{
		if(other.CompareTag(tagToCheck))
		{
			collectingScrap = false;
		}
	}

	/// <summary>
	/// Spawns a random building from the buildingsPrefab array.
	/// </summary>
	private void SpawnBuilding()
	{
		buildingIndex = Random.Range(0, buildingPrefabs.Length);
		spawnedBuildingPrefab = Instantiate(buildingPrefabs[buildingIndex].BuildingModelBroken, buildingPrefabSpawnTransform.position, Quaternion.Euler(buildingPrefabSpawnRotation), buildingPrefabParent);
		amountOfScrapNeeded = buildingPrefabs[buildingIndex].ScrapNeededToRepair;
	}

	/// <summary>
	/// Checks for building state. This get's changed if the building has enough scrap to be repaired.
	/// </summary>
	/// <returns>nothing</returns>
	private IEnumerator CheckBuildingState()
	{
		while(true)
		{
			if(buildingState == BuildingState.Repaired)
			{
				Destroy(spawnedBuildingPrefab);
				spawnedBuildingPrefab = Instantiate(buildingPrefabs[buildingIndex].BuildingModelRepaired, buildingPrefabSpawnTransform.position, Quaternion.Euler(buildingPrefabSpawnRotation), buildingPrefabParent);
				buildingState = BuildingState.Done;
				StartCoroutine(BuildingStateDoneEvent());
			}
			yield return new WaitForSeconds(buildingStateCheckInterval);
		}
	}

	/// <summary>
	/// The update function for the TMPro text Gameobject.
	/// </summary>
	/// <returns></returns>
	private IEnumerator repairProgressUpdate()
	{
		while(true)
		{
			progressImage.fillAmount = ((float)amountOfScrapCollected / (float)amountOfScrapNeeded);
			//amountOfScrapNeededAndLeftText.text = amountOfScrapCollected + " / " + amountOfScrapNeeded;
			yield return new WaitForSeconds(textUpdateInterval);
		}
	}

	/// <summary>
	/// Adds scrap to the building and removes it from the player when player is within range.
	/// </summary>
	/// <returns></returns>
	private IEnumerator AddScrapWhenPlayerWithinRange()
	{
		while(true)
		{
			if(buildingState == BuildingState.Rundown)
			{
				if(collectingScrap)
				{
					if(player.ScrapCollected >= 2)
					{
						player.ScrapCollected -= repairSpeed;
						int newScrapAmount = Mathf.Clamp(amountOfScrapCollected += repairSpeed, 0, amountOfScrapNeeded);
						amountOfScrapCollected = newScrapAmount;
					}
				}
				if(amountOfScrapCollected >= amountOfScrapNeeded)
				{
					buildingState = BuildingState.Repaired;
				}
			}
			yield return new WaitForSeconds(scrapCollectionInterval);
		}
	}

	/// <summary>
	/// The event that triggers when the building state is done. The timer is hardcoded because, well... It's to end this script from updating pretty much.
	/// </summary>
	/// <returns></returns>
	private IEnumerator BuildingStateDoneEvent()
	{
		if(buildingState == BuildingState.Done)
		{
			Instantiate(friendlyAIPrefab, friendlyAIPrefabSpawnPos[0].position, Quaternion.Euler(buildingPrefabSpawnRotation));
			Instantiate(friendlyAIPrefab, friendlyAIPrefabSpawnPos[1].position, Quaternion.Euler(buildingPrefabSpawnRotation));

			Destroy(this, 1f);
			yield return null;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, buildingPrefabs[buildingIndex].ScrapCollectingDetectionRadius);
	}
}

/// <summary>
/// BuildingPrefab struct. Very easy way to add more building prefabs to the array.
/// </summary>
[System.Serializable]
public struct BuildingPrefab
{
	[SerializeField] private GameObject buildingModelBroken;
	[SerializeField] private GameObject buildingModelRepaired;
	[Space]
	[SerializeField] private Vector3 scrapCollectingDetectionRadius;
	[SerializeField] private float scrapTextHeight;
	[SerializeField] private int scrapNeededToRepair;

	public GameObject BuildingModelBroken { get => buildingModelBroken; set => buildingModelBroken = value; }
	public GameObject BuildingModelRepaired { get => buildingModelRepaired; set => buildingModelRepaired = value; }
	public Vector3 ScrapCollectingDetectionRadius { get => scrapCollectingDetectionRadius; set => scrapCollectingDetectionRadius = value; }
	public float ScrapTextHeight { get => scrapTextHeight; set => scrapTextHeight = value; }
	public int ScrapNeededToRepair { get => scrapNeededToRepair; set => scrapNeededToRepair = value; }
}
