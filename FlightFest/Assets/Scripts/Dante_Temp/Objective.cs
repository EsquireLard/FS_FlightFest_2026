using UnityEngine;

public class Objective : MonoBehaviour
{
    [SerializeField] float objectiveTime;
    float currentTime;

    bool playerInRange;

    void Awake()
    {
        playerInRange = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            currentTime += Time.deltaTime;
            Debug.Log("IN RANGE");
        }
        else
        {
            currentTime -= Time.deltaTime;
            Debug.Log("OUT RANGE");
        }

        currentTime = Mathf.Clamp(currentTime, 0.0f, objectiveTime);

        GameManager.instance.UpdateObjective(currentTime, objectiveTime);
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
