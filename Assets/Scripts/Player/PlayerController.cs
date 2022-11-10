using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float initialSpeed;
    [SerializeField] float minSpeed;
    [SerializeField] float slowDownRate;
    [Space]
    [SerializeField] LayerMask rayCollisionMask;
    [SerializeField] Transform groundRayLeft;
    [SerializeField] Transform groundRayRight;

    [Space]
    [SerializeField] Transform groundCheckTransform;
    [SerializeField] Vector2 checkBoxSize;

    [Space]
    [SerializeField] Rigidbody2D rigidbody;

    [Space]
    [SerializeField] bool enableDebugging = true;

    float currentSpeed;
    float gravityScale;
    bool isGrounded;

    private void Start() {
        currentSpeed = initialSpeed;
        gravityScale = rigidbody.gravityScale;
    }

    private void Update() {
        GroundCheck();
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, int.MaxValue);
        Log("Grounded: " + isGrounded);
    }

    private void FixedUpdate() {
        Move();

        if(!isGrounded)
            ApplyGravity(gravityScale);
        else
            ApplyGravity(0f);
    }


    private void Move(){
        currentSpeed -= Time.deltaTime * slowDownRate;

        DrawRay(groundRayLeft.transform.position, Vector3.left * 0.2f, Color.blue);
        DrawRay(groundRayRight.transform.position, Vector3.right * 0.2f, Color.blue);

        RaycastHit2D leftHit = Physics2D.Raycast(groundRayLeft.transform.position, Vector2.down, 0.2f, rayCollisionMask);
        RaycastHit2D rightHit = Physics2D.Raycast(groundRayRight.transform.position, Vector2.down, 0.2f, rayCollisionMask);
        RaycastHit2D hit = leftHit ? leftHit : rightHit;

        if(!isGrounded || !hit){
            transform.position = new Vector3(transform.position.x + currentSpeed * Time.deltaTime, transform.position.y, transform.position.z);
            return;
        }

        if(hit){
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            slopeAngle = leftHit ? -slopeAngle : slopeAngle; //when going down slopes
            Log("Angle: " + slopeAngle);
            Vector2 direction = new Vector2(currentSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad), currentSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad));
            direction = direction.normalized;
            transform.position = new Vector3(transform.position.x + direction.x * currentSpeed * Time.deltaTime, 
                                            transform.position.y + direction.y * currentSpeed * Time.deltaTime);    
        }
    }

    private void ApplyGravity(float scale){
        rigidbody.gravityScale = scale;
        //Removing any leftover force
        if(scale == 0f) 
            rigidbody.velocity = new Vector2(0f, 0f);
    }

    private void GroundCheck()
    {
        DrawBox(groundCheckTransform.position, checkBoxSize, Color.black);
        DrawBox(groundCheckTransform.position, checkBoxSize*2, Color.gray);
        RaycastHit2D hitIn = Physics2D.BoxCast(groundCheckTransform.position, checkBoxSize, 0f, Vector2.zero, 0f, rayCollisionMask);
        RaycastHit2D hitOut = Physics2D.BoxCast(groundCheckTransform.position, checkBoxSize*2, 0f, Vector2.zero, 0f, rayCollisionMask);
        if(hitIn) 
            isGrounded = true;
        if(!hitOut)
            isGrounded = false;
    }

    #region TOOLS
    private void DrawRay(Vector3 origin, Vector3 direction, Color color)
    {
        if(Debugger.enableGlobalDebugging && enableDebugging){
            Debug.DrawRay(origin, direction, color);
        }
    }

    private void Log(string message)
    {
        if(Debugger.enableGlobalDebugging && enableDebugging){
            Debug.Log(message);
        }
    }

    private void DrawBox(Vector2 position, Vector2 size, Color color)
    {
        if(Debugger.enableGlobalDebugging && enableDebugging){
            //TOP
            Debug.DrawRay(new Vector3(position.x - size.x/2f, position.y + size.y/2f, 0f), new Vector3(size.x, 0f, 0f), color);
            //BOTTOM
            Debug.DrawRay(new Vector3(position.x - size.x/2f, position.y - size.y/2f, 0f), new Vector3(size.x, 0f, 0f), color);
            //LEFT
            Debug.DrawRay(new Vector3(position.x - size.x/2f, position.y - size.y/2f, 0f), new Vector3(0f, size.y, 0f), color);
            //RIGHT
            Debug.DrawRay(new Vector3(position.x + size.x/2f, position.y - size.y/2f, 0f), new Vector3(0f, size.y, 0f), color);
        }
    }

    #endregion
}
