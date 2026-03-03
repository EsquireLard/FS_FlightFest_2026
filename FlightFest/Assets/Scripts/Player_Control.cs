using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Control : MonoBehaviour
{
    Vector2 leftStick;
    Vector2 rightStick;

    [SerializeField]float rotateSpeed;
    [SerializeField]float flySpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        leftStick = new Vector2(Input.GetAxis("Yaw"), Input.GetAxis("Throttle"));
        rightStick = new Vector2(Input.GetAxis("Roll"), Input.GetAxis("Pitch"));

        Debug.Log("Throttle" + leftStick.y + " Yaw" + leftStick.x  + " Pitch" + rightStick.y + " Roll" + rightStick.x);
        transform.Rotate(Vector3.left, rightStick.y * rotateSpeed);
        transform.Rotate(Vector3.back, rightStick.x * rotateSpeed);
        transform.Rotate(Vector3.up, leftStick.x * rotateSpeed);
        transform.Translate(Vector3.up * (leftStick.y + 1) * flySpeed, Space.Self);
    }
}
