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
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRateAir = 0.25f;
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRateGrounded = 0.25f;
    
    [Space]
    [Header("JUMPING")]
    [Tooltip("The jump force when player is neither goind down nor up a slope")]
    [SerializeField] float jumpForce = 30f;
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
    [SerializeField] GameObject targetObject;

    // STATES
    [HideInInspector] public bool isJustJumped;
    [HideInInspector] public bool isJustLanded;
    [HideInInspector] public bool isGrounded;

    // Other hidden
    [HideInInspector] public Vector2[] groundPoints;
    [HideInInspector] public int targetPoint;
    #endregion

    #region PRIVATE VARIABLES
    float speed;
    float boost;
    float angleWithGround;
    bool chamsChosen = false;
    bool applyGravity = true;
    
    float velocityMagnitude = 0f; 
    Vector2 velocity = Vector2.zero;
    
    Vector2 direction = Vector2.right;
    Vector2 prevPosition = Vector2.zero;

    //CACHE
    bool leaveGroundCache;
    #endregion

    #region EXECUTION
    void Start(){
        boost = initialBoost;
        speed = boost;
        velocity = direction * speed;
        rigidbody.velocity = velocity;

        groundPoints = mainGroundObject.GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing);

        spawnedGroundSections[0] = null;
        spawnedGroundSections[1] = mainGroundObject;
        spawnedGroundSections[2] = proceduralGenerator.AddGroundSection(mainGroundObject, spawnedGroundSections);
    }

    void Update(){
        isGrounded = GroundCheck();

        if(isGrounded){
            angleWithGround = Vector2.Angle(Vector2.up, direction);
        }

        //Debugging
        if(debugGrounded)
            LogMessage("Grounded : " + isGrounded);
        if(debugVelocity){
            if(isGrounded)
                LogMessage("[GROUND] Velocity magnitude : " + velocityMagnitude + "\nVelocity vector : " + velocity);
            else{
                float rbVelMagnitude = Mathf.Sqrt(Mathf.Pow(rigidbody.velocity.x,2) + Mathf.Pow(rigidbody.velocity.y,2));
                LogMessage("[AIR] Velocity magnitude : " + rbVelMagnitude + "\nVelocity vector : " + rigidbody.velocity);
            }
        }
        if(debugAngleWithGround){
            if(isGrounded)
            LogMessage($"Angle with ground: <color=magenta>" + angleWithGround + "</color>");
        }
        DisplayNextPathPoint();
    }

    void FixedUpdate() {
        if(isGrounded){
            MoveAlongGround();
        }

        if(applyGravity)
            Physicsf.ApplyGravity(rigidbody, gravityScale);   

        if(!isGrounded){
            if(!applyGravity){
                applyGravity = true;
            }
            MoveInAir();
            UpdateGroundInAir();
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
        direction = GetDirection(targetPoint, targetPoint+1);
        transform.rotation = RotateInDirection(direction);
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // NOTE: We calculate the velocity here to ignore when the position is hard set later down in the if statement
        ClaculateGroundedVelocity(transform.position - (Vector3)direction * speed * Time.deltaTime, transform.position);

        if(targetPoint < groundPoints.Length-1 && transform.position.x > groundPoints[targetPoint].x){
            SetTargetToNearstFrontPoint();
            if(targetPoint-1 < groundPoints.Length){
                transform.position = groundPoints[targetPoint-1];
                targetPoint++;
            }
        }
    }

    void MoveInAir(){
        rigidbody.velocity = new Vector3(CalculateSpeed(rigidbody.velocity.x, minSpeedAir, slowDownRateAir), rigidbody.velocity.y, rigidbody.velocity.z);
        //speed = CalculateSpeed(velocityMagnitude, minSpeedAir, slowDownRateAir);
        //transform.position += (Vector3)direction * speed * Time.deltaTime;
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
        if(ptCurr >= groundPoints.Length || ptNext >= groundPoints.Length)
            return Vector2.zero;
        return (groundPoints[ptNext] - groundPoints[ptCurr]).normalized; 
    }

    Quaternion RotateInDirection(Vector2 dir){
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler( 0f, 0f, rotation);
    }

    #endregion

    #region JUMPING
    public void OnJump(InputAction.CallbackContext context){
        if(context.started){    
            if(!isGrounded)
                return;
            if(!leaveGroundCache){
                if(angleWithGround < upSlopeMaxAngle)
                    StartCoroutine(LeaveGround(0.25f));
                else
                    StartCoroutine(LeaveGround(0.075f));
            }
            Jump();
        }
    }

    void Jump(){
        if(angleWithGround > downSlopeMinAngle){
            rigidbody.velocity = new Vector3(rigidbody.velocity.x + downSlopeJumpForce, rigidbody.velocity.y, rigidbody.velocity.z);
            if(debugJumpSlope)
                LogMessage($"Jump slope: <color=red>Down Slope</color>");
        }
        else if(angleWithGround < upSlopeMaxAngle){
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
        
        yield return new WaitForSeconds(resetTime);

        distanceToGrounded = initialDist;

        leaveGroundCache = false;
    }

    public void SetTargetToNearstFrontPoint(){
        while(targetPoint < groundPoints.Length && groundPoints[targetPoint].x < transform.position.x)
            targetPoint++;
    }

    void ClaculateGroundedVelocity(){
        velocityMagnitude = Vector2.Distance(prevPosition, transform.position)/Time.deltaTime;
        velocity = new Vector2((transform.position.x - prevPosition.x)/Time.deltaTime, (transform.position.y - prevPosition.y)/Time.deltaTime);
        prevPosition = transform.position;
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
        for(int i=0; i < mainGroundObject.GetComponent<PathCreator>().path.NumSegments; i++){
            Vector2[] points = mainGroundObject.GetComponent<PathCreator>().path.GetPointsInSegment(i);
            float dist = HandleUtility.DistancePointBezier(transform.position, points[0], points[3], points[1], points[2]);
            if(dist <= distanceToGrounded){
                if(!isGrounded){
                    //Set the ground target point
                    targetPoint = Path.GetNearestPoint(transform.position, groundPoints) + 1;
                    transform.position = groundPoints[targetPoint];
                    //State bool
                    StartCoroutine(PositiveSwitch(_ => isJustLanded = _));  
                }
                return true;
            }
        }
        return false;
    }

    void UpdateGroundPoints(){
        if(spawnedGroundSections[0] != null)
            spawnedGroundSections[0].SetActive(false);
        spawnedGroundSections[0] = spawnedGroundSections[1];
        
        mainGroundObject = spawnedGroundSections[2];
        spawnedGroundSections[1] = spawnedGroundSections[2];
        groundPoints = mainGroundObject.GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing); 
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
    }
    
    
    #endregion

    #region  LOGGING
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

        if(enableLogging && targetObject != null)
            targetObject.transform.position = groundPoints[targetPoint];
    }
    #endregion
}
