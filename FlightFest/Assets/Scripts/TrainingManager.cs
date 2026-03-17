using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    private GameObject player;
    private GameObject[] excercises;
    int currentExcercise;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        excercises = new GameObject[4];
        excercises[0] = GameObject.Find("Excercise1");
        excercises[1] = GameObject.Find("Excercise2");
        excercises[2] = GameObject.Find("Excercise3");
        excercises[3] = GameObject.Find("Excercise4");

        foreach (var excercise in excercises)
        {
            excercise.SetActive(false);
        }
        
        currentExcercise = 0;
        excercises[currentExcercise].SetActive(true);
        
    }
    public void NextExcercise()
    {
        Debug.Log("Next Excercise");
        excercises[currentExcercise].SetActive(false);

        currentExcercise++;
        if(currentExcercise >= excercises.Length)
        {
            currentExcercise = 0;
        }
        
        excercises[currentExcercise].SetActive(true);
        player.GetComponents<DronePhysics>()[0].ResetDroneState();
    }
}
