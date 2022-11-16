using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region PUBLIC AND SERIALIZED VARIABLES
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float initialBoost = 25f;
    [SerializeField] float minSpeed = 17f;
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRate = 0.25f;
    [SerializeField] float groundRayLength;
    [Tooltip("The higher, the more accurate is the movement")]
    [SerializeField] float groundSpacing;

    [Space]
    [SerializeField] LayerMask groundMask;
    [SerializeField] Vector3 groundBoxSizeAir;
    [SerializeField] Vector3 groundBoxSizeGround;
    
    
    
    [Space]
    [Header("REQUIRED COMPONENTS")]
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] Transform groundBox;
    [SerializeField] GameObject spriteObject;
    [SerializeField] PathCreator pathCreator;

    [Space]
    [Header("DEBUGGING")]
    [SerializeField] bool enableLogging;
    [SerializeField] bool inEditorDrawing;
    [SerializeField] GameObject targetObject;

    // STATES
    [HideInInspector] public bool isJustJumped;
    [HideInInspector] public bool isJustLanded;
    [HideInInspector] public bool isGrounded;

    #endregion

    #region PRIVATE VARIABLES
    float speed;
    float boost;
    int targetPoint;
    Vector2 direction;
    Vector2[] groundPoints;
    Vector3 initialGroundBox;

    //CACHE
    bool leaveGroundCache;
    #endregion

    #region EXECUTION
    void Start(){
        boost = initialBoost;
        speed = boost;
        rigidbody.velocity = new Vector3(speed, 0f, 0f);
        groundPoints = pathCreator.path.CalculateEvenlySpacedPoints(groundSpacing);
        initialGroundBox = groundBoxSizeAir;
    }

    void Update(){
        if(isGrounded){
            isGrounded = GroundCheck(groundBoxSizeGround);
            MoveAlongGround();
        }
        else{
            isGrounded = GroundCheck(groundBoxSizeAir);
            if(!rigidbody.useGravity){
                rigidbody.useGravity = true;
            }  
            MoveInAir();
        }

        //Debugging
        DisplayNextPathPoint();
    }


    void OnDrawGizmos() {
        if(!inEditorDrawing)
            return;
            
        GroundCheck(groundBoxSizeAir);
        GroundCheck(groundBoxSizeGround);
    }
    #endregion

    #region MOVEMENT
    void MoveAlongGround(){
        if(targetPoint == groundPoints.Length - 1){
            if(!leaveGroundCache)
                StartCoroutine(LeaveGround(0.2f));
            return;
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

        if(transform.position.x > groundPoints[targetPoint].x){
            SetTargetToNearstFrontPoint();
            transform.position = groundPoints[targetPoint-1];
            targetPoint++;
        }
    }

    void MoveInAir(){
        speed = CalculateSpeed(rigidbody.velocity.x, minSpeed, slowDownRate);
        rigidbody.velocity = new Vector3(speed, rigidbody.velocity.y, rigidbody.velocity.z);
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
        return (groundPoints[ptNext] - groundPoints[ptCurr]).normalized; 
    }

    Quaternion RotateInDirection(Vector2 dir){
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler( 0f, 0f, rotation);
    }

    void SetTargetToNearstFrontPoint(){
        while(groundPoints[targetPoint].x < transform.position.x && targetPoint < groundPoints.Length)
            targetPoint++;
    }
    #endregion

    #region JUMPING
    public void OnJump(InputAction.CallbackContext context){
        if(context.started){    
            if(!isGrounded)
                return;
            if(!leaveGroundCache)
                StartCoroutine(LeaveGround(0.01f));
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

        isGrounded = false;
        rigidbody.useGravity = true;
        groundBoxSizeAir = Vector3.zero;
        groundBox.position = new Vector3( groundBox.position.x,
                                          groundBox.position.y + 1,
                                          groundBox.position.z );

        yield return new WaitForSeconds(resetTime);
        groundBoxSizeAir = initialGroundBox;
        groundBox.position = new Vector3( groundBox.position.x,
                                          groundBox.position.y - 1,
                                          groundBox.position.z );


        leaveGroundCache = false;
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
    bool GroundCheck(Vector3 checkSize){
        Collider[] cols = Physics.OverlapBox(groundBox.position, checkSize);
        DrawBox(groundBox.position, checkSize, Color.blue);
        if(cols.Length > 0){
            if(!isGrounded){
                //Set the ground target point
                targetPoint = Path.GetNearestPoint(transform.position, groundPoints) + 1;
                transform.position = groundPoints[targetPoint];
                //State bool
                StartCoroutine(PositiveSwitch(_ => isJustLanded = _));
            }
            return true;
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
        if(enableLogging && targetObject != null)
            targetObject.transform.position = groundPoints[targetPoint];
    }
    #endregion
}
