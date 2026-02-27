using UnityEngine;

public class DronePhysics : MonoBehaviour
{
    float mass;
    float k; //Rotor's torque constant
    float l; //Arm Length
    float g; //9.81 m/s2 TODO figure out how to do it with integers 
    Vector3 InertiaMatrix;
    Vector3 InertiaMAtrixInverse;
    float frameWidth; //Frame is a square
    double sqrt2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mass = 17; //g
        l = 33; //mm
        k = 0; //figure out
        g = 9.81f; //m/s2
        sqrt2 = Mathf.Sqrt(2);

        frameWidth = 83; //mm
        InertiaMatrix = new Vector3(
            mass * frameWidth * frameWidth / 12,
            mass * frameWidth * frameWidth / 12,
            mass * frameWidth * frameWidth / 6
        );
        InertiaMAtrixInverse = new Vector3(  //Since is a diagolanl matrix we can jsut represent it as a vector and the inverse is jsut the inverse of the diagonal elements.
            1 / InertiaMatrix.x,
            1 / InertiaMatrix.y,
            1 / InertiaMatrix.z
        );
    }

    // Update is called once per frame
    void Update()
    {
        DroneState state = RK4(ComputeDynamics, Time.deltaTime, ref state, controlInput);
    }

    DroneState ComputeDynamics(DroneState state, Vector4 controlInput) //TODO pre calc torque and c before this cause is the same in every runge kutta step
    {
        float c = (controlInput.x + controlInput.y + controlInput.z + controlInput.w) / mass; // Collective thrust
        Vector3 torque = new Vector3(
            l / sqrt2 * ( controlInput.x - controlInput.y - controlInput.z + controlInput.w),
            l / sqrt2 * (-controlInput.x - controlInput.y + controlInput.z + controlInput.w),
            k * (controlInput.x - controlInput.y + controlInput.z - controlInput.w)
        );

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

            angularVelocity = Vector3.Scale(InertiaMAtrixInverse, Vector3.Scale(Vector3.Cross(torque, state.angularVelocity), 1 / InertiaMatrix))
        };

        return derivative;
    }

    DroneState FRK4(Func<DroneState, Vector4, DroneState> func, float dt, ref DroneState state, Vector4 controlInput)
    {
        DroneState k1 = func(state, controlInput);
        DroneState k2 = func(state + k1 * (dt / 2), controlInput);
        DroneState k3 = func(state + k2 * (dt / 2), controlInput);
        DroneState k4 = func(state + k3 * dt, controlInput);

        state += (k1 + 2 * k2 + 2 * k3 + k4) * (dt / 6);
        state.orientation = state.orientation.normalized; // Normalize the quaternion to prevent drift
        return state;
    }
}