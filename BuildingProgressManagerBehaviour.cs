using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingProgressManagerBehaviour : MonoBehaviour
{
    [SerializeField]
    private List<BuildingSpawnBehaviour> buildings = new List<BuildingSpawnBehaviour>();
    private int numberOfBuildingsFinished = 0;
    [SerializeField]
    private TextMeshProUGUI numberOfBuildingsLeftText = default;

    private void Start()
    {
        StartCoroutine(checkBuildings());
    }

    public IEnumerator checkBuildings()
    {
        while (true)
        {
            buildings = new List<BuildingSpawnBehaviour>();
            numberOfBuildingsFinished = 0;
            GameObject[] arrbuildings = GameObject.FindGameObjectsWithTag("Buildings");

            for (int i = 0; i < arrbuildings.Length; i++)
            {
                BuildingSpawnBehaviour b = arrbuildings[i].GetComponent<BuildingSpawnBehaviour>();

                buildings.Add(b);
                if (b != null)
                {
                    if (b.BuildingState == BuildingState.Rundown)
                    {
                        numberOfBuildingsFinished++;
                    }
                }
            }
            numberOfBuildingsLeftText.text = numberOfBuildingsFinished.ToString();
            yield return new WaitForSeconds(1f);
        }
    }
}

