using UnityEngine;

public class FlightController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    DroneState state;

    // I think this is going to be a raw measurement of the input, and then we will apply the rate transformation
    double throttle;
    double yaw;
    double pitch;
    double roll;

    //PID constants
    double kP;
    double kI;
    double kD;

    //PID cumulative error terms
    double cumulativeIPitch; 
    double cumulativeIYaw;
    double cumulativeIRoll;

    double prevErrorPitch;
    double prevErrorYaw;
    double prevErrorRoll;

    //Rotor Thrusts
    double f1;
    double f2;
    double f3;
    double f4;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        double throttleSetpoint = ComputeRate(throttle);
        double yawSetpoint = ComputeRate(yaw);
        double pitchSetpoint = ComputeRate(pitch);
        double rollSetpoint = ComputeRate(roll);

        PIDresult yawResult = PIDEquation(yawSetpoint, state.angularVelocity.y, cumulativeIYaw, prevErrorYaw);
        cumulativeIYaw = yawResult.cumulativeI;
        prevErrorYaw = yawResult.prevError;

        PIDresult pitchResult = PIDEquation(pitchSetpoint, state.angularVelocity.x, cumulativeIPitch, prevErrorPitch);
        cumulativeIPitch = pitchResult.cumulativeI;
        prevErrorPitch = pitchResult.prevError;

        PIDresult rollResult = PIDEquation(rollSetpoint, state.angularVelocity.z, cumulativeIRoll, prevErrorRoll);
        cumulativeIRoll = rollResult.cumulativeI;
        prevErrorRoll = rollResult.prevError;
    
        f1 = throttleSetpoint + pitchResult.PID - yawResult.PID - rollResult.PID;
        f2 = throttleSetpoint + pitchResult.PID + yawResult.PID + rollResult.PID;
        f3 = throttleSetpoint - pitchResult.PID - yawResult.PID + rollResult.PID;
        f4 = throttleSetpoint - pitchResult.PID + yawResult.PID - rollResult.PID;

        //TODO thrust clamp

    }

    double ComputeRate(double rawInput) //TODO implement
    {
        
    }

    PIDresult PIDEquation(double setpoint, double measurement, double cumulativeI, double prevError)
    {
        double error = setpoint - measurement;
        
        //Proportional term
        double P = kP * error;
        
        //Integral term
        double I = cumulativeI + kI * error; //TODO This is not correct, we need to integrate the error over time

        if(I > 400) //TODO This is a very arbitrary threshold, we need to tune this
        {
           I = 400;
        }
        else if(I < -400)
        {
            I = -400;
        }

        //Derivative term

        double D = kD * (error - prevError); //TODO This is not correct, we need to divide by the time step
    
        double PID = P + I + D;
        
         if(PID > 400) //TODO This is a very arbitrary threshold, we need to tune this
        {
           PID = 400;
        }
        else if(PID < -400)
        {
            PID = -400;
        }

        return new PIDresult(PID, I, error);
    }
}   

struct PIDresult
{
    public double PID;
    public double cumulativeI;
    public double prevError;
}