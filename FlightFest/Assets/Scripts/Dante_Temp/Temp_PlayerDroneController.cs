using UnityEngine;
using System.Collections.Generic;


public class Temp_PlayerDroneController : MonoBehaviour
{
    Vector3 moveDir;
    Vector2 walkDir;
    [SerializeField] float speedMod;
    CharacterController controller;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //movement execution
        walkDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        moveDir = walkDir.x * transform.right * 10 + walkDir.y * transform.forward * 10;
        controller.Move(moveDir * Time.deltaTime);
    }
}
