using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region PUBLIC AND SERIALIZED VARIABLES
    [Header("SLIDING")]
    [SerializeField] float initialBoost = 25f;
    [SerializeField] float minSpeedAir = 17f;
    [SerializeField] float minSpeedGrounded = 17f;
    [SerializeField] float maxSpeedGrounded = 125f;
    [Space]
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRateAir = 0.25f;
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRateGrounded = 0.25f;
    [Space]
    [SerializeField] float maxFallingVelocity = -60f;
    
    [Space]
    [Header("GROUND SLIPPING")]
    [SerializeField] bool groundSlipping = true;
    [Space]
    // Sorry these ones are hard to explain without visualizing them
    [Range(0, 1000)] [Tooltip("Caps the target position in measuring the difference in height"
                                + "or width between the player target position and the second"
                                + "target positon, to decide if to leave the ground or not")]
    [SerializeField] int maxTargetDifferenceNormalSlope = 125;
    [Range(0, 1000)] [Tooltip("Caps the target position in measuring the difference in height"
                                + "or width between the player target position and the second"
                                + "target positon, to decide if to leave the ground or not")]
    [SerializeField] int maxTargetDifferenceUpSlope = 215;
    [Space]
    [Range(0, 180)] [Tooltip("The angle limit changes between zero and this max. If the current"
                                + "angle difference is greater than the limit (which varies with speed),"
                                + "then the player slipps off the ground")]
    [SerializeField] float maxAngleDifferenceNormalSlope = 100f;
    [Range(0, 180)] [Tooltip("The angle limit changes between zero and this max. If the current"
                                + "angle difference is greater than the limit (which varies with speed),"
                                + "then the player slipps off the ground")]
    [SerializeField] float maxAngleDifferenceUpSlope = 75f;
    
    [Space]
    [Header("JUMPING")]
    [Tooltip("The jump force when player is neither goind down nor up a slope")]
    [SerializeField] float jumpForce = 15f;
    [Space]
    [Tooltip("The jump force when goind down a slope. (Note that the player jumps to the right instead of up)")]
    [SerializeField] float downSlopeJumpForce = 30f; 
    [Range(90f, 180f)]
    [SerializeField] float downSlopeMinAngle = 125f;
    [Space]
    [Tooltip("The jump force when going up a slope. (Note that the player jumps straight up unlike down slopes)")]
    [SerializeField] float upSlopeJumpForce = 30f; 
    [Range(0f, 90f)]
    [SerializeField] float upSlopeMaxAngle = 45f;

    [Space]
    [Header("SYSTEMS")]
    [SerializeField] float gravityScale = 1f;
    [Tooltip("The higher, the more accurate is the movement")]
    public float groundSpacing;
    [SerializeField] float distanceToGrounded = 0.1f;
    
    
    
    [Space]
    [Header("REQUIRED COMPONENTS")]
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] Transform groundBox;
    [SerializeField] GameObject spriteObject;
    [SerializeField] ProceduralGeneration proceduralGenerator; 
    //Ground generation
    [SerializeField] GameObject mainGroundObject;
    GameObject[] spawnedGroundSections = new GameObject[3];

    [SerializeField] ProceduralGeneration groundGenerator;

    [Space]
    [Header("DEBUGGING")]
    [SerializeField] bool enableLogging;
    [SerializeField] bool inEditorDrawing;
    [SerializeField] bool debugGrounded;
    [SerializeField] bool debugVelocity;
    [SerializeField] bool debugAngleWithGround;
    [SerializeField] bool debugJumpSlope;
    [SerializeField] bool debugSlopeType;
    [SerializeField] bool debugGroundSlipping;
    [SerializeField] GameObject targetObject;
    [SerializeField] GameObject tagretObjectSecond;

    // STATES
    [HideInInspector] public bool isJustJumped;
    [HideInInspector] public bool isJustLanded;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isJustLeftGround;

    // Other hidden
    [HideInInspector] public Vector2[] groundPoints;
    Vector2[] groundPointsNext;
    [HideInInspector] public int targetPoint;
    #endregion

    #region PRIVATE VARIABLES
    float speed;
    float boost;
    float angleWithGround;

    int targetPointsDiff; // How many points between the 2 targets

    bool chamsChosen;
    bool applyGravity = true;
    
    bool normalSlope;
    bool downSlope;
    bool upSlope;
    
    float velocityMagnitude = 0f; 
    Vector2 velocity = Vector2.zero;
    
    Vector2 direction = Vector2.right;
    Vector2 prevPosition = Vector2.zero;

    //CACHE
    bool leaveGroundCache;
    GameObject prevMainGround = null;
    #endregion

    #region EXECUTION
    void Start(){
        boost = initialBoost;
        speed = boost;
        velocity = direction * speed;
        rigidbody.velocity = velocity;


        spawnedGroundSections[0] = null;
        spawnedGroundSections[1] = mainGroundObject;
        spawnedGroundSections[2] = proceduralGenerator.AddGroundSection(mainGroundObject, spawnedGroundSections);

        groundPoints = mainGroundObject.GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing);
        groundPointsNext = spawnedGroundSections[2].GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing);
    }

    void Update(){
        isGrounded = GroundCheck();

        if(isGrounded){
            angleWithGround = Vector2.Angle(Vector2.up, direction);
        }
        else{
            UpdateGroundInAir();
            LimitFallingVelocity();
        }

        //Debugging
        if (debugGrounded)
            LogMessage("Grounded : " + isGrounded);
        if(debugVelocity){
            if(isGrounded)
                LogMessage($"<color=brown>[GROUND]</color> Velocity magnitude : " + velocityMagnitude + "\nVelocity vector : " + velocity);
            else{
                float rbVelMagnitude = Mathf.Sqrt(Mathf.Pow(rigidbody.velocity.x,2) + Mathf.Pow(rigidbody.velocity.y,2));
                LogMessage($"<color=cyan>[AIR]</color> Velocity magnitude : " + rbVelMagnitude + "\nVelocity vector : " + rigidbody.velocity);
            }
        }
        if(debugAngleWithGround){
            if(isGrounded)
            LogMessage($"Angle with ground: <color=magenta>" + angleWithGround + "</color>");
        }
        DisplayNextPathPoint();
        DebugSlopeType();
    }

    void FixedUpdate() {
        if(isGrounded){
            UpdateSlopeGeneralDirection();
            MoveAlongGround();
            if(CheckForGroundSlipping()){
                if(!leaveGroundCache)
                    StartCoroutine(LeaveGround(0.1f));
            }
        }

        if(applyGravity)
            Physicsf.ApplyGravity(rigidbody, gravityScale);   

        if(!isGrounded){
            if(!applyGravity){
                applyGravity = true;
            }
            MoveInAir();
        } 
    }


    void OnDrawGizmos() {
        if(!inEditorDrawing)
            return;
            
        GroundCheck();
    }
    #endregion

    #region MOVEMENT
    void MoveAlongGround(){
        if(targetPoint >= groundPoints.Length-1){
            if(Vector2.Distance(transform.position, spawnedGroundSections[2].GetComponent<PathCreator>().path[0]) > 1f){
                if(!leaveGroundCache)
                    StartCoroutine(LeaveGround(0.2f));
                return;
            }
            UpdateGroundPoints();
        }

        if(applyGravity){
            applyGravity = false;
            rigidbody.velocity = Vector2.zero;
            transform.position = groundPoints[targetPoint];
        }

        speed = CalculateSpeed(speed, minSpeedGrounded, slowDownRateGrounded);
        if(speed > maxSpeedGrounded)
            speed = maxSpeedGrounded;
        direction = GetDirection(targetPoint, targetPoint+1);
        transform.rotation = RotateInDirection(direction);
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // NOTE: We calculate the velocity here to ignore when the position is hard set later down in the if statement
        ClaculateGroundedVelocity(transform.position - (Vector3)direction * speed * Time.deltaTime, transform.position);

        if(targetPoint < groundPoints.Length-1 && transform.position.x > groundPoints[targetPoint].x){
            SetTargetToNearstFrontPoint();
            if(targetPoint < groundPoints.Length){
                transform.position = groundPoints[targetPoint-1];
                if(targetPoint < groundPoints.Length-1)
                    targetPoint++;
            }
        }
    }

    void MoveInAir(){
        rigidbody.velocity = new Vector3(CalculateSpeed(rigidbody.velocity.x, minSpeedAir, slowDownRateAir), rigidbody.velocity.y, rigidbody.velocity.z);

    }

    void UpdateGroundInAir(){
        if(transform.position.x > groundPoints[groundPoints.Length-1].x){
            UpdateGroundPoints();
        }
    }

    float CalculateSpeed(float currentSpeed, float minSpeed, float rate){
        if(currentSpeed > minSpeed){
            currentSpeed -= rate;
        }
        else if(currentSpeed < minSpeed)
            currentSpeed = minSpeed;
        return currentSpeed;
    }

    Vector2 GetDirection(int ptCurr, int ptNext){
        if(ptNext < groundPoints.Length)
            return (groundPoints[ptNext] - groundPoints[ptCurr]).normalized; 
        if(ptCurr >= groundPoints.Length)
            return (groundPointsNext[ptNext - groundPoints.Length] - groundPointsNext[ptCurr - groundPoints.Length]).normalized;
        else
            return (groundPointsNext[ptNext - groundPoints.Length] - groundPoints[ptCurr]).normalized;
    }

    Quaternion RotateInDirection(Vector2 dir){
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler( 0f, 0f, rotation);
    }
    #endregion

    #region GROUND SLIPPING
    bool CheckForGroundSlipping()
    {
        if (!isGrounded || downSlope || !groundSlipping)
            return false;

        SetTargetsDifference();
        float angleLimit = GetAngleLimit();

        Vector2 targetNextDirection = GetNextTargetDirection();
        float targetNextAngle = Vector2.Angle(Vector2.up, targetNextDirection);

        DebugGroundSlipping(angleLimit, targetNextAngle);

        if (Mathf.Abs(angleWithGround - targetNextAngle) > angleLimit)
            return true;
        return false;
    }

    private Vector2 GetNextTargetDirection()
    {
        Vector2 targetNextDirection;
        if (targetPoint + targetPointsDiff < groundPoints.Length - 1)
            targetNextDirection = GetDirection(targetPoint + targetPointsDiff, targetPoint + targetPointsDiff + 1);
        else
            targetNextDirection = GetDirection(targetPoint + targetPointsDiff - 1, targetPoint + targetPointsDiff);
        return targetNextDirection;
    }

    float GetAngleLimit(){
        float angleLimit;
        if (normalSlope)
            angleLimit = (Mathf.Abs(velocityMagnitude - maxSpeedGrounded)) * maxAngleDifferenceNormalSlope / (maxSpeedGrounded - minSpeedGrounded);
        else
            angleLimit = (Mathf.Abs(velocityMagnitude - maxSpeedGrounded)) * maxAngleDifferenceUpSlope / (maxSpeedGrounded - minSpeedGrounded);

        return angleLimit;
    }

    void SetTargetsDifference(){
        if (normalSlope)
            targetPointsDiff = (int)((velocityMagnitude - minSpeedGrounded) * (float)maxTargetDifferenceNormalSlope / (maxSpeedGrounded - minSpeedGrounded));
        else
            targetPointsDiff = (int)((velocityMagnitude - minSpeedGrounded) * (float)maxTargetDifferenceUpSlope / (maxSpeedGrounded - minSpeedGrounded));
    }
    #endregion

    #region JUMPING
    public void OnJump(InputAction.CallbackContext context){
        if(context.started){    
            if(!isGrounded)
                return;
            if(!leaveGroundCache){
                if(angleWithGround < upSlopeMaxAngle)
                    StartCoroutine(LeaveGround(0.25f)); // Normal slope
                else
                    StartCoroutine(LeaveGround(0.075f));
            }
            Jump();
        }
    }

    void Jump(){
        if(downSlope){
            rigidbody.velocity = new Vector3(rigidbody.velocity.x + downSlopeJumpForce, rigidbody.velocity.y, rigidbody.velocity.z);
            if(debugJumpSlope)
                LogMessage($"Jump slope: <color=red>Down Slope</color>");
        }
        else if(upSlope){
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y + upSlopeJumpForce, rigidbody.velocity.z);
            if(debugJumpSlope)
                LogMessage($"Jump slope: <color=green>Up Slope</color>");
        }
        else{
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y + jumpForce, rigidbody.velocity.z);
            if(debugJumpSlope)
                LogMessage($"Jump slope: <color=blue>Normal Slope</color>");
        }

        StartCoroutine(PositiveSwitch(_ => isJustJumped = _));
    }
    #endregion

    #region UNIVERSAL
    IEnumerator LeaveGround(float resetTime){
        leaveGroundCache = true;

        float initialDist = distanceToGrounded;

        isGrounded = false;
        applyGravity = true;
        distanceToGrounded = 0f;

        rigidbody.velocity = new Vector3(velocity.x, velocity.y, rigidbody.velocity.z);

        StartCoroutine(PositiveSwitch(_ =>isJustLeftGround = _));
        
        yield return new WaitForSeconds(resetTime);

        distanceToGrounded = initialDist;

        leaveGroundCache = false;
    }

    public void SetTargetToNearstFrontPoint(){
        while(targetPoint < groundPoints.Length-1 && groundPoints[targetPoint].x < transform.position.x)
            targetPoint++;
    }

    void ClaculateGroundedVelocity(){
        velocityMagnitude = Vector2.Distance(prevPosition, transform.position)/Time.deltaTime;
        velocity = new Vector2((transform.position.x - prevPosition.x)/Time.deltaTime, (transform.position.y - prevPosition.y)/Time.deltaTime);

    }

    void ClaculateGroundedVelocity(Vector2 previous, Vector2 current){
        velocityMagnitude = Vector2.Distance(previous, current)/Time.deltaTime;
        velocity = new Vector2((current.x - previous.x)/Time.deltaTime, (current.y - previous.y)/Time.deltaTime);
    }


    IEnumerator PositiveSwitch(Action<bool> key, WaitForSeconds time = null){
        key(true);
        yield return time;
        key(false);
    }
    
    IEnumerator NegativeSwitch(Action<bool> key, WaitForSeconds time = null){
        key(false);
        yield return time;
        key(true);
    }

    #endregion

    #region SYSTEMS
    bool GroundCheck(){
        Path path = null;
        
        if(prevMainGround = null)
            prevMainGround = mainGroundObject;
        else if(prevMainGround != mainGroundObject)
            path = mainGroundObject.GetComponent<PathCreator>().path;
        
        if(path == null)
            return false;

        for(int i=0; i < path.NumSegments; i++){
            Vector2[] points = path.GetPointsInSegment(i);
            float dist = HandleUtility.DistancePointBezier(transform.position, points[0], points[3], points[1], points[2]);
            if(dist <= distanceToGrounded){
                if(!isGrounded){
                    // Stop the player in place to do the calculations
                    applyGravity = false;
                    rigidbody.velocity = Vector3.zero;
                    // Set the ground target point
                    targetPoint = Path.GetNearestPoint(transform.position, groundPoints) + 1;
                    transform.position = groundPoints[targetPoint];
                    // State bool
                    StartCoroutine(PositiveSwitch(_ => isJustLanded = _));  
                }
                return true;
            }
        }

        if(isGrounded){
            StartCoroutine(PositiveSwitch(_ =>isJustLeftGround = _));
        }

        return false;
    }

    void UpdateGroundPoints(){
        if(spawnedGroundSections[0] != null)
            spawnedGroundSections[0].SetActive(false);
        spawnedGroundSections[0] = spawnedGroundSections[1];
        
        mainGroundObject = spawnedGroundSections[2];
        spawnedGroundSections[1] = spawnedGroundSections[2];
        groundPoints = groundPointsNext; 
        targetPoint = 0;

        if(!chamsChosen){
            int groundType = proceduralGenerator.GetRandomGroundType();
            if(groundType == ProceduralGeneration.Chasm){
                chamsChosen = true;
                spawnedGroundSections[2] = proceduralGenerator.AddLeftChasm(mainGroundObject, spawnedGroundSections);
            }
            else{
                spawnedGroundSections[2] = proceduralGenerator.AddGroundSection(mainGroundObject, spawnedGroundSections);
            }
        }
        else{
            spawnedGroundSections[2] = proceduralGenerator.AddRightChasm(mainGroundObject, spawnedGroundSections);
            chamsChosen = false;
        }

        groundPointsNext = spawnedGroundSections[2].GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing);
    }
    
    void UpdateSlopeGeneralDirection(){
        if(angleWithGround > downSlopeMinAngle){
            if(normalSlope == true) normalSlope = false;
            if(downSlope == false) downSlope = true;
            if(upSlope == true) upSlope = false;
        }
        else if(angleWithGround < upSlopeMaxAngle){
            if(normalSlope == true) normalSlope = false;
            if(downSlope == true) downSlope = false;
            if(upSlope == false) upSlope = true;
        }
        else{
            if(normalSlope == false) normalSlope = true;
            if(downSlope == true) downSlope = false;
            if(upSlope == true) upSlope = false;
        }
    } 

    void LimitFallingVelocity(){
        if (rigidbody.velocity.y < maxFallingVelocity)
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, maxFallingVelocity, rigidbody.velocity.z);
    }
    #endregion

    #region  DEBUGGING
    private void DrawBox(Vector2 position, Vector2 size, Color color)
    {
        if(enableLogging){
            //TOP
            Debug.DrawRay(new Vector3(position.x - size.x, position.y + size.y, 0f), new Vector3(size.x*2f, 0f, 0f), color);
            //BOTTOM
            Debug.DrawRay(new Vector3(position.x - size.x, position.y - size.y, 0f), new Vector3(size.x*2f, 0f, 0f), color);
            //LEFT
            Debug.DrawRay(new Vector3(position.x - size.x, position.y - size.y, 0f), new Vector3(0f, size.y*2f, 0f), color);
            //RIGHT
            Debug.DrawRay(new Vector3(position.x + size.x, position.y - size.y, 0f), new Vector3(0f, size.y*2f, 0f), color);
        }
    }

    void LogRay(Vector3 start, Vector3 direction, Color color){
        if(enableLogging)
            Debug.DrawRay(start, direction, color);
    }

    void LogMessage(string message){
        if(enableLogging)
            Debug.Log(message);
    }

    void DisplayNextPathPoint(){
        if(targetPoint >= groundPoints.Length)
            return;

        if(enableLogging){
            if(targetObject != null)
                targetObject.transform.position = groundPoints[targetPoint];
            if(debugGroundSlipping && tagretObjectSecond != null){
                if(targetPoint + targetPointsDiff < groundPoints.Length)
                    tagretObjectSecond.transform.position = groundPoints[targetPoint + targetPointsDiff];
                else{
                    int nextDiff = targetPoint + targetPointsDiff - groundPoints.Length; 
                    tagretObjectSecond.transform.position = nextDiff < groundPointsNext.Length ? groundPointsNext[nextDiff] : groundPointsNext[groundPointsNext.Length-1];
                }
                    
            }
        }
    }

    void DebugGroundSlipping(float angleLimit, float angle){
        if (enableLogging && debugGroundSlipping){
            if(normalSlope){
                Debug.Log($"Current angle: <color=blue>" + angleWithGround + "</color> / Next angle: <color=cyan>" + angle
                            + "</color> / Slope type: <color=magenta>Normal slope</color>" 
                            + "\n Difference: <color=green>" + Mathf.Abs(angleWithGround - angle) + "</color> / Angle limit: <color=red>" + angleLimit + "</color>");
            }
            else if(upSlope){
                Debug.Log($"Current angle: <color=blue>" + angleWithGround + "</color> / Next angle: <color=cyan>" + angle
                            + "</color> / Slope type: <color=pink>Up Slope slope</color>" 
                            + "\n Difference: <color=green>" + Mathf.Abs(angleWithGround - angle) + "</color> / Angle limit: <color=red>" + angleLimit + "</color>");
            }
        }
    }

    void DebugSlopeType(){
        if(enableLogging && debugSlopeType){
            if(normalSlope)
                Debug.Log($"<color=red>Normal</color> Slope | Angle: <color=yellow>" + angleWithGround + "</color>");
            else if(downSlope)
                Debug.Log($"<color=green>Down</color> Slope | Angle: <color=yellow>" + angleWithGround + "</color>");
            else if(upSlope)
                Debug.Log($"<color=blue>Up</color> Slope | Angle: <color=yellow>" + angleWithGround + "</color>");
        }
    }
    #endregion
}
