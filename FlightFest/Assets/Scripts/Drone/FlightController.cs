using UnityEngine;
using UnityEngine.InputSystem;

public class FlightController : MonoBehaviour
{
    DroneState state;
    private Controls controls;

    // I think this is going to be a raw measurement of the input, and then we will apply the rate transformation
    float throttle;
    float yaw;
    float pitch;
    float roll;

    [SerializeField] float rotateSpeed;
    [SerializeField] float flySpeed;


    //PID constants
    float kPPitch; // Same as Roll
    float kPYaw;

    float kIPitch; // Same as Roll
    float kIYaw;

    float kDPitch; // Same as Roll
    float kDYaw;


    //PID cumulative error terms
    float cumulativeIPitch;
    float cumulativeIYaw;
    float cumulativeIRoll;

    float prevErrorPitch;
    float prevErrorYaw;
    float prevErrorRoll;

    //Rates
    float[] rcExpo;
    float[] rcRates;
    float[] rates;


    //OutputRotor Thrusts
    float f1;
    float f2;
    float f3;
    float f4;

    void Awake()
    {
        controls = new Controls();
        controls.RCController.Enable();
    }

    void Start()
    {


        //TODO we need to tune these constants
        kPPitch = 0.6f;
        kIPitch = 3.5f;
        kDPitch = 0.03f;

        kPYaw = 2.0f;
        kIYaw = 12.0f;
        kDYaw = 0.0f;

        rcExpo = new float[3] { 0.1f, 0.1f, 0.1f };
        rcRates = new float[3] { 1.0f, 1.0f, 1.0f };
        rates = new float[3] { 0.7f, 0.7f, 0.7f };
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log((controls == null) ? "controls is NULL" : "controls is OK");
        Debug.Log((controls.RCController.Throttle == null) ? "Throttle is NULL" : "Throttle is OK");

        throttle = controls.RCController.Throttle.ReadValue<float>();
        yaw = controls.RCController.Yaw.ReadValue<float>();
        pitch = controls.RCController.Pitch.ReadValue<float>();
        roll = controls.RCController.Roll.ReadValue<float>();
        Debug.Log("Raw: Throttle " + throttle + " Yaw " + yaw + " Pitch " + pitch + " Roll " + roll);

        float throttleSetpoint = throttle;
        float yawSetpoint = ComputeBetaflightRates(0, yaw);
        float pitchSetpoint = ComputeBetaflightRates(1, pitch);
        float rollSetpoint = ComputeBetaflightRates(2, roll);
        Debug.Log("Set: Throttle " + throttle + " Yaw " + yawSetpoint + " Pitch " + pitchSetpoint + " Roll " + rollSetpoint);

        PIDresult yawResult = PIDEquation(yawSetpoint, state.angularVelocity.y, cumulativeIYaw, prevErrorYaw, kPYaw, kIYaw, kDYaw);
        cumulativeIYaw = yawResult.cumulativeI;
        prevErrorYaw = yawResult.prevError;

        PIDresult pitchResult = PIDEquation(pitchSetpoint, state.angularVelocity.x, cumulativeIPitch, prevErrorPitch, kPPitch, kIPitch, kDPitch);
        cumulativeIPitch = pitchResult.cumulativeI;
        prevErrorPitch = pitchResult.prevError;

        PIDresult rollResult = PIDEquation(rollSetpoint, state.angularVelocity.z, cumulativeIRoll, prevErrorRoll, kPPitch, kIPitch, kDPitch);
        cumulativeIRoll = rollResult.cumulativeI;
        prevErrorRoll = rollResult.prevError;


        Debug.Log("PID: Yaw " + yawResult.PID + " Pitch " + pitchResult.PID + " Roll " + rollResult.PID);

        f1 = throttleSetpoint + pitchResult.PID - yawResult.PID - rollResult.PID;
        f2 = throttleSetpoint + pitchResult.PID + yawResult.PID + rollResult.PID;
        f3 = throttleSetpoint - pitchResult.PID - yawResult.PID + rollResult.PID;
        f4 = throttleSetpoint - pitchResult.PID + yawResult.PID - rollResult.PID;

        //TODO thrust clamp

    }

    float ComputeBetaflightRates(int axis, float input)
    {
        float inputAbs = Mathf.Abs(input);

        input = input * inputAbs * inputAbs * inputAbs * rcExpo[axis] + input * (1 - rcExpo[axis]);

        float angleRate = 200.0f * rcRates[axis] * input;

        float rcSuperfactor = 1.0f - (inputAbs * rates[axis]);

        if (rcSuperfactor < 0.01f)
        {
            rcSuperfactor = 0.01f;
        }
        else if (rcSuperfactor > 1.00f)
        {
            rcSuperfactor = 1.00f;
        }

        rcSuperfactor = 1.0f / rcSuperfactor;
        angleRate *= rcSuperfactor;

        return angleRate;
    }

    PIDresult PIDEquation(float setpoint, float measurement, float cumulativeI, float prevError, float kP, float kI, float kD)
    {
        float error = setpoint - measurement;

        //Proportional term
        float P = kP * error;

        //Integral term
        float I = cumulativeI + kI * error; //TODO This is not correct, we need to integrate the error over time

        if (I > 400) //TODO This is a very arbitrary threshold, we need to tune this
        {
            I = 400;
        }
        else if (I < -400)
        {
            I = -400;
        }

        //Derivative term

        float D = kD * (error - prevError); //TODO This is not correct, we need to divide by the time step

        float PID = P + I + D;

        if (PID > 400) //TODO This is a very arbitrary threshold, we need to tune this
        {
            PID = 400;
        }
        else if (PID < -400)
        {
            PID = -400;
        }

        return new PIDresult(PID, I, error);
    }
}

struct PIDresult
{
    public float PID;
    public float cumulativeI;
    public float prevError;

    public PIDresult(float PID, float cumulativeI, float prevError)
    {
        this.PID = PID;
        this.cumulativeI = cumulativeI;
        this.prevError = prevError;
    }
}