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
    float[] kP;
    float[] kI;
    float[] kD;


    //PID cumulative terms
    float[] cumulativeI;
    float[] prevError;
    float[] prevTime;


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
        prevTime = new float[3] {Time.time, Time.time, Time.time};
        prevError = new float[3] { 0.0f, 0.0f, 0.0f };
        cumulativeI = new float[3] { 0.0f, 0.0f, 0.0f };

        //TODO we need to tune these constants
        kP = new float[3] { 2.0f, 0.6f, 0.6f };
        kI = new float[3] { 12.0f, 3.5f, 3.5f };
        kD = new float[3] { 0.0f, 0.03f, 0.03f };

        rcExpo = new float[3] { 0.1f, 0.1f, 0.1f };
        rcRates = new float[3] { 1.0f, 1.0f, 1.0f };
        rates = new float[3] { 0.7f, 0.7f, 0.7f };
    }

    // Update is called once per frame
    void Update()
    {
         throttle = controls.RCController.Throttle.ReadValue<float>();
        yaw = controls.RCController.Yaw.ReadValue<float>();
        pitch = controls.RCController.Pitch.ReadValue<float>();
        roll = controls.RCController.Roll.ReadValue<float>();
        Debug.Log("Raw: Throttle " + throttle + " Yaw " + yaw + " Pitch " + pitch + " Roll " + roll);

        float throttleSetpoint = (throttle + 1) / 2.0f;
        float yawSetpoint = ComputeBetaflightRates(0, yaw);
        float pitchSetpoint = ComputeBetaflightRates(1, pitch);
        float rollSetpoint = ComputeBetaflightRates(2, roll);
        Debug.Log("Set: Throttle " + throttleSetpoint + " Yaw " + yawSetpoint + " Pitch " + pitchSetpoint + " Roll " + rollSetpoint);

        float yawPID = PIDEquation(yawSetpoint, state.angularVelocity.y, 0) / 1000.0f;
        float pitchPID = PIDEquation(pitchSetpoint, state.angularVelocity.x, 1) / 1000.0f;
        float rollPID = PIDEquation(rollSetpoint, state.angularVelocity.z, 2) / 1000.0f;

        Debug.Log("PID: Yaw " + yawPID + " Pitch " + pitchPID + " Roll " + rollPID);

        f1 = throttleSetpoint + pitchPID - yawPID - rollPID;
        f2 = throttleSetpoint + pitchPID + yawPID + rollPID;
        f3 = throttleSetpoint - pitchPID - yawPID + rollPID;
        f4 = throttleSetpoint - pitchPID + yawPID - rollPID;

        Debug.Log("Thrusts: F1 " + f1 + " F2 " + f2 + " F3 " + f3 + " F4 " + f4);

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

    float PIDEquation(float setpoint, float measurements, int axis)
    {
        float deltaTime = Time.time - prevTime[axis]; //TODO This is not correct, we need to use the correct previous time for each axis
        prevTime[axis] = Time.time;   
        
        float error = setpoint - measurements;
        prevError[axis] = error;

        //Proportional term
        float P = kP[axis] * error;

        //Integral term
        float I = cumulativeI[axis] + kI[axis] * error * deltaTime; //TODO This is not correct, we need to integrate the error over time
        cumulativeI[axis] = I;

        if (I > 400) //TODO This is a very arbitrary threshold, we need to tune this
        {
            I = 400;
        }
        else if (I < -400)
        {
            I = -400;
        }

        //Derivative term

        float D = kD[axis] * (error - prevError[axis]) / deltaTime; //TODO This is not correct, we need to divide by the time step

        float PID = P + I + D;

        if (PID > 500) //TODO This is a very arbitrary threshold, we need to tune this
        {
            PID = 500;
        }
        else if (PID < -500)
        {
            PID = -500;
        }

        return PID;
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