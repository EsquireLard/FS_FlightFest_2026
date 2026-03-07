using UnityEngine;

public class FinalMarker : Objective
{
    private TrainingManager trainingManager;
    
    void Awake()
    {
        trainingManager = GetComponentInParent<TrainingManager>();
        playerInRange = false;
        Debug.Log("Final Marker Awake: " + trainingManager);   
    }

    protected override void FinishObjective()
    {
        Debug.Log("Final Marker reached!");
        trainingManager.NextExcercise();
        GameObject.Destroy(gameObject);
    }
}
