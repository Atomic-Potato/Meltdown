using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float initialBoost;
    [SerializeField] float minSpeed;
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRate;
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

    float speed;
    float boost;
    int targetPoint;
    Vector2 direction;
    Vector2[] groundPoints;

    bool isGrounded;


    int currentEnd;

    void Start(){
        boost = initialBoost;
        speed = boost;
        rigidbody.velocity = new Vector3(speed, 0f, 0f);
        groundPoints = pathCreator.path.CalculateEvenlySpacedPoints(groundSpacing);
    }

    void Update(){
        targetObject.transform.position = groundPoints[targetPoint];

        LogMessage("Grounded:" + isGrounded);

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
    }


    private void OnDrawGizmos() {
        if(!inEditorDrawing)
            return;
            
        GroundCheck(groundBoxSizeAir);
        GroundCheck(groundBoxSizeGround);
    }

    void MoveAlongGround(){
        if(targetPoint == groundPoints.Length - 1){
            // Let go of the ground ()
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

    float GetDistanceToPoint(Vector2 target){
        return Mathf.Abs(Vector2.Distance((Vector2)transform.position,target));
    }

    Quaternion RotateInDirection(Vector2 dir){
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler( 0f, 0f, rotation);
    }


    bool GroundCheck(Vector3 checkSize){
        Collider[] cols = Physics.OverlapBox(groundBox.position, checkSize);
        DrawBox(groundBox.position, checkSize, Color.blue);
        if(cols.Length > 0){
            if(!isGrounded){
                targetPoint = Path.GetNearestPoint(transform.position, groundPoints) + 1;
                transform.position = groundPoints[targetPoint];
            }
            return true;
        }
        return false;
        /*
        bool hit = Physics.Raycast(groundRay.position, Vector3.back * groundRayLength, groundMask);
        LogRay(groundRay.position, Vector3.back * groundRayLength, Color.blue);
        if(hit){
            LogMessage("Ground hit");
            return true;
        }
        return false;
        */
    }

    IEnumerator GetDirection(){
        Vector2 posPrev = transform.position;
        yield return null;
        direction = ((Vector2)transform.position - posPrev).normalized;
    }

    void SetTargetToNearstFrontPoint(){
        while(groundPoints[targetPoint].x < transform.position.x && targetPoint < groundPoints.Length)
            targetPoint++;
    }

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
    #endregion
}
