using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isPaused;
    public float timeScaleOrig;

    GameObject player;
    Temp_PlayerDroneController playerScript;

    [Header("===Menus===")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuOptions;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [Header("===Displayed Text===")]
    [SerializeField] TMP_Text currentObjectiveTime;
    [SerializeField] Slider objectiveSlider;


    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<Temp_PlayerDroneController>();
        //objectiveSlider.value = 0.1f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                StatePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuOptions)
            {
                menuActive.SetActive(false);
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else
            {
                StateUnpause();
            }
        }
    }

    // ---- PAUSING ---- //
    public void StatePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void StateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    // ---- WIN CONDITION FEEDBACK ---- //
    public void YouLose()
    {
        StatePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void YouWin()
    {
        StatePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void UpdateObjective(float currTime, float objectiveTime)
    {
        int intObjTime = (int)currTime + 1;
        if (intObjTime == (int)objectiveTime + 1) intObjTime = (int)objectiveTime;
        if (currTime == 0.0f) intObjTime = 0;
        currentObjectiveTime.text = intObjTime.ToString();
        objectiveSlider.value = currTime / objectiveTime;
    }
}
