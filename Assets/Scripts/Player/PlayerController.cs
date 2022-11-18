using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region PUBLIC AND SERIALIZED VARIABLES
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float initialBoost = 25f;
    [SerializeField] float minSpeed = 17f;
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRate = 0.25f;
    [Tooltip("The higher, the more accurate is the movement")]
    public float groundSpacing;

    [Space]
    [SerializeField] float distanceToGrounded = 0.1f;
    
    
    
    [Space]
    [Header("REQUIRED COMPONENTS")]
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] Transform groundBox;
    [SerializeField] GameObject spriteObject;
    //Ground generation
    [SerializeField] GameObject mainGroundObject;
    GameObject[] spawnedGroundSections = new GameObject[3];

    [SerializeField] ProceduralGeneration groundGenerator;

    [Space]
    [Header("DEBUGGING")]
    [SerializeField] bool enableLogging;
    [SerializeField] bool inEditorDrawing;
    [SerializeField] bool debugGrounded;
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
    Vector2 direction;
    Vector3 initialGroundBox;


    //CACHE
    bool leaveGroundCache;
    #endregion

    #region EXECUTION
    void Start(){
        boost = initialBoost;
        speed = boost;
        rigidbody.velocity = new Vector3(speed, 0f, 0f);

        groundPoints = mainGroundObject.GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing);

        spawnedGroundSections[0] = null;
        spawnedGroundSections[1] = mainGroundObject;
        spawnedGroundSections[2] = ProceduralGeneration.AddSection(ProceduralGeneration.Ground, mainGroundObject, spawnedGroundSections);
    }

    void Update(){
        if(debugGrounded)
            LogMessage("Grounded : " + isGrounded);

        isGrounded = GroundCheck();

        if(isGrounded){
            MoveAlongGround();
        }
        else{
            if(!rigidbody.useGravity){
                rigidbody.useGravity = true;
            }
            MoveInAir();
            UpdateGroundInAir();
        }

        //Debugging
        DisplayNextPathPoint();
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
            /*
            if(!leaveGroundCache)
                StartCoroutine(LeaveGround(0.2f));*/
            UpdateGroundPoints();
        }

        if(rigidbody.useGravity){
            transform.position = groundPoints[targetPoint];
            rigidbody.useGravity = false;
            rigidbody.velocity = Vector2.zero;
        }

        speed = CalculateSpeed(speed, minSpeed, slowDownRate);
        direction = GetDirection(targetPoint, targetPoint+1);
        transform.rotation = RotateInDirection(direction);
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if(targetPoint < groundPoints.Length-1 && transform.position.x > groundPoints[targetPoint].x){
            SetTargetToNearstFrontPoint();
            if(targetPoint-1 < groundPoints.Length){
                transform.position = groundPoints[targetPoint-1];
                targetPoint++;
            }
        }
    }

    void MoveInAir(){
        speed = CalculateSpeed(rigidbody.velocity.x, minSpeed, slowDownRate);
        rigidbody.velocity = new Vector3(speed, rigidbody.velocity.y, rigidbody.velocity.z);
    }

    void UpdateGroundInAir(){
        if(transform.position.x > groundPoints[groundPoints.Length-1].x){
            UpdateGroundPoints();
        }
    }

    float CalculateSpeed(float s, float minS, float rate){
        if(s > minS){
            s -= rate;
        }
        else if(s < minS)
            s = minS;
        return s;
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
            if(!leaveGroundCache)
                StartCoroutine(LeaveGround(0.25f));
            Jump();
        }
    }

    void Jump(){
        rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        StartCoroutine(PositiveSwitch(_ => isJustJumped = _));
    }
    #endregion

    #region UNIVERSAL
    IEnumerator LeaveGround(float resetTime){
        leaveGroundCache = true;

        float initialDist = distanceToGrounded;

        isGrounded = false;
        rigidbody.useGravity = true;
        distanceToGrounded = 0f;
        
        yield return new WaitForSeconds(resetTime);

        distanceToGrounded = initialDist;

        leaveGroundCache = false;
    }


    void UpdateGroundPoints(){
        if(spawnedGroundSections[0] != null)
            spawnedGroundSections[0].SetActive(false);
        spawnedGroundSections[0] = spawnedGroundSections[1];
        
        mainGroundObject = spawnedGroundSections[2];
        spawnedGroundSections[1] = spawnedGroundSections[2];
        groundPoints = mainGroundObject.GetComponent<PathCreator>().path.CalculateEvenlySpacedPoints(groundSpacing); 
        targetPoint = 0;

        spawnedGroundSections[2] = ProceduralGeneration.AddSection(ProceduralGeneration.Ground, mainGroundObject, spawnedGroundSections);
    }

    public void SetTargetToNearstFrontPoint(){
        while(targetPoint < groundPoints.Length && groundPoints[targetPoint].x < transform.position.x)
            targetPoint++;
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
