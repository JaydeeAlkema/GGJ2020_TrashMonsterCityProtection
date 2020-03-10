using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[SerializeField] private GameObject enemyPrefab = default;
	[SerializeField] private float timeSinceStartGame = default;
	[SerializeField] private float chanceToSpawn = 1;
	[SerializeField] private Transform spawnPoint = default;

	private void Start()
	{
		StartCoroutine(SpawnEnemy());
	}

	private IEnumerator SpawnEnemy()
	{
		while(true)
		{
			int rand = Random.Range(0, 100);
			if(rand <= chanceToSpawn)
			{
				GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
				enemyGO.GetComponent<EnemyBehaviour>().Health = enemyGO.GetComponent<EnemyBehaviour>().Health * (timeSinceStartGame / 100);
			}

			yield return new WaitForSeconds(5f);
		}
	}
}
