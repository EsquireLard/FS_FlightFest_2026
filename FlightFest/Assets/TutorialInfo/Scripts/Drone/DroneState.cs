using UnityEngine;

/// <summary>
/// Represents the full kinematic state of the quad‑copter.
/// Shared between the physics integrator, controller, rigidbody, etc.
/// </summary>
public struct DroneState
{
    public Vector3 position;
    public Quaternion orientation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public static DroneState operator +(DroneState a, DroneState b)
    {
        return new DroneState
        {
            position = a.position + b.position,
            orientation = new Quaternion(
                a.orientation.x + b.orientation.x,
                a.orientation.y + b.orientation.y,
                a.orientation.z + b.orientation.z,
                a.orientation.w + b.orientation.w
            ),
            velocity = a.velocity + b.velocity,
            angularVelocity = a.angularVelocity + b.angularVelocity
        };
    }

    public static DroneState operator *(DroneState a, float scalar)
    {
        return new DroneState
        {
            position = a.position * scalar,
            orientation = new Quaternion(
                a.orientation.x * scalar,
                a.orientation.y * scalar,
                a.orientation.z * scalar,
                a.orientation.w * scalar
            ),
            velocity = a.velocity * scalar,
            angularVelocity = a.angularVelocity * scalar
        };
    }

}
