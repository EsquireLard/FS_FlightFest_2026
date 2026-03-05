using UnityEngine;
using System;

public class DronePhysics : MonoBehaviour
{
    float maxThrustPerMotor;
    float mass;
    float k; //Rotor's torque constant
    float l; //Arm Length
    float g; //9.81 m/s2 TODO figure out how to do it with integers
    Vector3 InertiaMatrix;
    Vector3 InertiaMAtrixInverse;
    float frameWidth; //Frame is a square
    float sqrt2;
    public DroneState currentState;
    private FlightController flightController; // Reference to the FlightController script
    private Rigidbody rb; // Reference to the Rigidbody component


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        flightController = GetComponent<FlightController>(); // Get the FlightController component attached to the same GameObject
    }

    void Start()
    {
        mass = 0.017f; //kg
        l = 0.033f; //m
        k = 0.01f; //Nm TODO figure out
        g = 9.81f; //m/s2
        sqrt2 = Mathf.Sqrt(2);

        maxThrustPerMotor = mass * g / 2; //I am just making the drone able to hover at 50% throttle, so the max thrust per motor is mass * g / 4 (since we have 4 motors) and then we divide by 2 to get the max thrust at 100% throttle. This is just a starting point and we can adjust it later based on how the drone actually performs in the simulation.
        frameWidth = 0.083f; //m
        InertiaMatrix = new Vector3(
            mass * frameWidth * frameWidth / 12,
            mass * frameWidth * frameWidth / 12,
            mass * frameWidth * frameWidth / 6
        );
        InertiaMAtrixInverse = new Vector3(  //Since is a diagonal matrix we can jsut represent it as a vector and the inverse is jsut the inverse of the diagonal elements.
            1 / InertiaMatrix.x,
            1 / InertiaMatrix.y,
            1 / InertiaMatrix.z
        );
        ResetDroneState();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float[] controlInput = flightController.motorMix; // Get the motor mix from the FlightController

        float c = (controlInput[0] + controlInput[1] + controlInput[2] + controlInput[3]) / mass;
        Vector3 torque = new Vector3(
            -l / sqrt2 * (-controlInput[0] - controlInput[1] + controlInput[2] + controlInput[3]),
            -k * (controlInput[0] - controlInput[1] + controlInput[2] - controlInput[3]),
            l / sqrt2 * (controlInput[0] - controlInput[1] - controlInput[2] + controlInput[3])
        );

        rb.AddTorque(torque);
        rb.AddForce(transform.rotation *  new Vector3(0, c, 0) - new Vector3(0, g, 0));

    //     /*Debug.Log("StatePrev: Position: (" + currentState.position.x + ", " + currentState.position.y + ", " + currentState.position.z + 
    //              ") Orientation: (" + currentState.orientation.x + ", " + currentState.orientation.y + ", " + currentState.orientation.z + ", " + currentState.orientation.w + ")" + 
    //              " Velocity: (" + currentState.velocity.x + ", " + currentState.velocity.y + ", " + currentState.velocity.z + ")" + 
    //              " Angular Velocity: (" + currentState.angularVelocity.x + ", " + currentState.angularVelocity.y + ", " + currentState.angularVelocity.z + ")");
    //     */
    //     // Debug.Log("Max Thrust Per Motor: " + maxThrustPerMotor);
    //     currentState = FRK4(ComputeDynamics, Time.deltaTime, ref currentState, flightController.motorMix); //TODO state management
    //                                                                                                  //TODO figure out why ref

    //     /*Debug.Log("StateNew: Position: (" + currentState.position.x + ", " + currentState.position.y + ", " + currentState.position.z + 
    //      ") Orientation: (" + currentState.orientation.x + ", " + currentState.orientation.y + ", " + currentState.orientation.z + ", " + currentState.orientation.w + ")" + 
    //      " Velocity: (" + currentState.velocity.x + ", " + currentState.velocity.y + ", " + currentState.velocity.z + ")" + 
    //      " Angular Velocity: (" + currentState.angularVelocity.x + ", " + currentState.angularVelocity.y + ", " + currentState.angularVelocity.z + ")");
    //     */
    //     transform.position = new Vector3(currentState.position.y, currentState.position.z, currentState.position.x); //In Unity grabity is in the y axis but in our simulation is in the z axis so we need to swap them
    //     transform.rotation = new Quaternion(-currentState.orientation.y, -currentState.orientation.z, currentState.orientation.x, currentState.orientation.w);
    currentState.position = new Vector3(transform.position.y, transform.position.z, transform.position.x); //In Unity gravity is in the y axis but in our simulation is in the z axis so we need to swap them
    currentState.orientation = new Quaternion(-transform.rotation.y, -transform.rotation.z, transform.rotation.x, transform.rotation.w);
    }

    DroneState ComputeDynamics(DroneState state, float[] controlInput) //TODO pre calc torque and c before this cause is the same in every runge kutta step
    {
        float c = (controlInput[0] + controlInput[1] + controlInput[2] + controlInput[3]) / mass; // Collective thrust
        Vector3 torque = new Vector3(
            l / sqrt2 * (controlInput[0] - controlInput[1] - controlInput[2] + controlInput[3]),
            l / sqrt2 * (-controlInput[0] - controlInput[1] + controlInput[2] + controlInput[3]),
            k * (controlInput[0] - controlInput[1] + controlInput[2] - controlInput[3])
        );
        // Debug.Log("Control Input: " + controlInput[0] + " " + controlInput[1] + " " + controlInput[2] + " " + controlInput[3] + " c: " + c + "l: " + l + " k: " + k);
        // Debug.Log("x: " + l / sqrt2 * (controlInput[0] - controlInput[1] - controlInput[2] + controlInput[3]));
        // Debug.Log("y: " + l / sqrt2 * (-controlInput[0] - controlInput[1] + controlInput[2] + controlInput[3]));
        // Debug.Log("z: " + k * (controlInput[0] - controlInput[1] + controlInput[2] - controlInput[3]));
        // Debug.Log("Torque: " + torque.x + ", " + torque.y + ", " + torque.z);

        DroneState derivative = new DroneState
        {
            position = state.velocity,
            velocity = state.orientation * new Vector3(0, 0, c) - new Vector3(0, 0, g), //TODO check the * logic here

            orientation = new Quaternion(
                0.5f * (state.angularVelocity.x * state.orientation.w + state.angularVelocity.z * state.orientation.y - state.angularVelocity.y * state.orientation.z),
                0.5f * (state.angularVelocity.y * state.orientation.w - state.angularVelocity.z * state.orientation.x + state.angularVelocity.x * state.orientation.z),
                0.5f * (state.angularVelocity.z * state.orientation.w + state.angularVelocity.y * state.orientation.x - state.angularVelocity.x * state.orientation.y),
                0.5f * (-state.angularVelocity.x * state.orientation.x - state.angularVelocity.y * state.orientation.y - state.angularVelocity.z * state.orientation.z)
            ).normalized,

            angularVelocity = Vector3.Scale(InertiaMAtrixInverse, torque - Vector3.Cross(state.angularVelocity, Vector3.Scale(InertiaMatrix, state.angularVelocity))) // Since the inertia matrix is diagonal, the matrix–vector multiplication reduces to element-wise scaling of the vector components.
        };
        // Debug.Log("item 1: " + Vector3.Scale(InertiaMatrix, state.angularVelocity));
        // Debug.Log("item 2: " + Vector3.Cross(state.angularVelocity, Vector3.Scale(InertiaMatrix, state.angularVelocity)));
        // Debug.Log("torque: " + torque.x + ", " + torque.y + ", " + torque.z);
        // Debug.Log("item 3: " + (torque - Vector3.Cross(state.angularVelocity, Vector3.Scale(InertiaMatrix, state.angularVelocity))));

        // Debug.Log("Derivative: Angular Velocity: " + derivative.angularVelocity);

        return derivative;
    }

    DroneState FRK4(Func<DroneState, float[], DroneState> func, float dt, ref DroneState state, float[] controlInput)
    {
        DroneState k1 = func(state, controlInput);
        DroneState k2 = func(state + k1 * (dt / 2), controlInput);
        DroneState k3 = func(state + k2 * (dt / 2), controlInput);
        DroneState k4 = func(state + k3 * dt, controlInput);

        state += (k1 + 2.0f * k2 + 2.0f * k3 + k4) * (dt / 6.0f);
        state.orientation = state.orientation.normalized; // Normalize the quaternion to prevent drift
        return state;
    }

    void ResetDroneState()
    {
        currentState = new DroneState //TODO figure out how to set the initial state
        {
            position = new Vector3(transform.position.x, transform.position.z, transform.position.y), //In Unity gravity is in the y axis but in our simulation is in the z axis so we need to swap them
            orientation = new Quaternion(-transform.rotation.y, -transform.rotation.z, transform.rotation.x, transform.rotation.w),
            velocity = Vector3.zero,
            angularVelocity = Vector3.zero
        };
    }
}