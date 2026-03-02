using UnityEngine;

public class ObjectiveSpawner : MonoBehaviour
{
    [SerializeField] float objectiveTime;
    [SerializeField] string objectivePopup;
    [SerializeField] ObjectiveSpawner nextObjective;
    [SerializeField] GameObject objectivePrefab;

    GameObject objectToSpawn;
    float originalObjectiveTime;
    string originalObjectivePopup;


    void Start()
    {
        originalObjectiveTime = objectivePrefab.GetComponent<Objective>().objectiveTime;
        originalObjectivePopup = objectivePrefab.GetComponent<Objective>().objectivePopup;
        objectToSpawn = objectivePrefab;
    }

    public void SpawnObjective()
    {
        objectToSpawn.GetComponent<Objective>().objectiveTime = objectiveTime;

        if (objectivePopup != "")
        {
            objectToSpawn.GetComponent<Objective>().objectivePopup = objectivePopup;
        }

        if (nextObjective != null)
        {
            objectToSpawn.GetComponent<Objective>().nextObjective = nextObjective;
        }

        GameObject.Instantiate(objectToSpawn, gameObject.transform);

        objectivePrefab.GetComponent<Objective>().objectiveTime = originalObjectiveTime;
        objectivePrefab.GetComponent<Objective>().objectivePopup = originalObjectivePopup;
        objectivePrefab.GetComponent<Objective>().nextObjective = null;
    }
}
