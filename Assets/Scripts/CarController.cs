using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    private NNet network;

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentbreakForce;
    private bool isBreaking;
    private bool isReversing;
    private float timeSinceStart = 0.0f;
    private float reverseScore = 0.0f;

    [Header("Constraints")]
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float motorForce;
    [SerializeField]
    private float breakForce;
    [SerializeField]
    private float maxSteerAngle;

    [Header("Colliders")]
    [SerializeField]
    private WheelCollider frontLeftWheelCollider;
    [SerializeField]
    private WheelCollider frontRightWheelCollider;
    [SerializeField]
    private WheelCollider rearLeftWheelCollider;
    [SerializeField]
    private WheelCollider rearRightWheelCollider;

    [Header("Transforms")]
    [SerializeField]
    private Transform frontLeftWheelTransform;
    [SerializeField]
    private Transform frontRightWheelTransform;
    [SerializeField]
    private Transform rearLeftWheelTransform;
    [SerializeField]
    private Transform rearRightWheelTransform;
    [SerializeField]
    private Rigidbody car;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.7f;
    public float sensorMultiplier = 0.35f;
    public float reverseMultiplier = -0.7f;

    [Header("Network Options")]
    public int Layers = 1;
    public int Neurons = 10;

    private Vector3 lastPosition, startPosition;
    private Vector3 startRotation;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float theta0Sensor, theta0DashSensor, theta1Sensor, theta1DashSensor, theta2Sensor, theta2DashSensor, theta3Sensor, theta4Sensor;
    private float GlobalSpeedMultiplier;
    
    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        GlobalSpeedMultiplier = avgSpeedMultiplier;
        network = GetComponent<NNet>();

        //TestCode
        //network.Initialise(Layers, Neurons);
    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
    }

    public void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

    public void Reset()
    {
        transform.eulerAngles = startRotation;
        timeSinceStart = 0.0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition; 
        overallFitness = 0f;
        transform.position = startPosition;
        reverseScore = 0f;
        
        //TestCode
        //network.Initialise(Layers, Neurons);
    }
   
    private void FixedUpdate()
    {
        Sensors();
        GetInput();
        (verticalInput, horizontalInput) = network.RunNetwork(theta0Sensor, theta0DashSensor, theta1Sensor, theta1DashSensor, theta2Sensor, theta2DashSensor, theta3Sensor, theta4Sensor);
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        timeSinceStart +=Time.deltaTime;
        CalculateFitness();
        lastPosition = transform.position;
    }

    private void CalculateFitness()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);
        totalDistanceTravelled += distance;
        if (!isReversing)
        {
            avgSpeed = (totalDistanceTravelled / timeSinceStart) * 3.6f;
        }
        if (avgSpeed > 40)
        {
            avgSpeedMultiplier = -0.2f;
        }
        else
        {
            avgSpeedMultiplier = GlobalSpeedMultiplier;
        }
        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier) + (((theta0Sensor + theta0DashSensor + theta1Sensor + theta1DashSensor + theta2Sensor + theta2DashSensor + theta3Sensor + theta4Sensor) / 8) * sensorMultiplier) + (reverseScore*reverseMultiplier);
        if ((timeSinceStart > 20) && (overallFitness < 20))
        {
            Death();
        }
        if (overallFitness > 100000)
        {
            //Save network to JSON
            Death();
        }
    }
    private void Sensors()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        Vector3 rgt = transform.TransformDirection(Vector3.right);
        Vector3 lft = transform.TransformDirection(-(Vector3.right));
        Vector3 rgtfwd = transform.TransformDirection(Vector3.forward + Vector3.right);
        Vector3 lftfwd = transform.TransformDirection(Vector3.forward - Vector3.right);
        Vector3 rrr = transform.TransformDirection(-(Vector3.forward));
        Vector3 rgtrrr = transform.TransformDirection(-(Vector3.forward) + (Vector3.right));
        Vector3 lftrrr = transform.TransformDirection(-(Vector3.forward) - (Vector3.right));
        RaycastHit hit;
        Ray r = new Ray(transform.position, fwd);
        r.origin = transform.position;
        if (Physics.Raycast(transform.position, fwd, out hit, 10))
        {
            theta0Sensor = hit.distance / 10;
        }
        if (Physics.Raycast(transform.position, rgt, out hit, 10))
        {
            theta3Sensor = hit.distance / 10;
        }
        if (Physics.Raycast(transform.position, lft, out hit, 10))
        {
            theta4Sensor = hit.distance / 10;
        }

        if (Physics.Raycast(transform.position, rgtfwd, out hit, 10))
        {
            theta1Sensor = hit.distance / 10;
        }
        if (Physics.Raycast(transform.position, lftfwd, out hit, 10))
        {
            theta2Sensor = hit.distance / 10;
        }
        if (Physics.Raycast(transform.position, rrr, out hit, 10))
        {
            theta0DashSensor = hit.distance / 10;
        }

        if (Physics.Raycast(transform.position, rgtrrr, out hit, 10))
        {
            theta1DashSensor = hit.distance / 10;
        }
        if (Physics.Raycast(transform.position, lftrrr, out hit, 10))
        {
            theta2DashSensor = hit.distance / 10;
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        if ((verticalInput * motorForce) <= maxSpeed)
        {
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce;
            
        }
        else
        {
            frontLeftWheelCollider.motorTorque = maxSpeed;
            frontRightWheelCollider.motorTorque = maxSpeed;
        }
        if (verticalInput < 0)
        {
            reverseScore += 0.07f;
            isReversing = true;
        }
        else
        {
            isReversing = false;
        }
        frontRightWheelCollider.brakeTorque = 0f;
        frontLeftWheelCollider.brakeTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;
        currentbreakForce = isBreaking ? breakForce : 0f;
        if (isBreaking)
        {
            ApplyBreaking();
        }
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
