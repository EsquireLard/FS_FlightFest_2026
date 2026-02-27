using UnityEngine;

public class Objective : MonoBehaviour
{
    bool playerInRange;

    void Awake()
    {
        playerInRange = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            GameManager.instance.objectiveSlider.value = (float)GameManager.instance.objectiveSlider.value + 1f * Time.deltaTime;
            Debug.Log("IN RANGE");
        }
        else
        {
            GameManager.instance.objectiveSlider.value = (float)GameManager.instance.objectiveSlider.value - 1f * Time.deltaTime;
            Debug.Log("OUT RANGE");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
