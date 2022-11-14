using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float initialSpeed;
    [SerializeField] float minSpeed;
    [Range(0f, 2.5f)]
    [SerializeField] float slowDownRate;
    [Tooltip("The higher, the more accurate is the movement")]
    [SerializeField] float groundSpacing;

    [SerializeField] GameObject spriteObject;
    [SerializeField] PathCreator pathCreator;

    float speed;
    int targetPoint = 1;
    Vector2 direction;
    Vector2[] groundPoints;

    void Start(){
        speed = initialSpeed;
        groundPoints = pathCreator.path.CalculateEvenlySpacedPoints(groundSpacing);
        transform.position = groundPoints[0];
    }

    void Update(){
        Move();
    }

    void Move(){
        if(targetPoint == groundPoints.Length)
            return;
        speed = CalculateSpeed(speed, minSpeed, slowDownRate);
        direction = GetDirection(targetPoint-1, targetPoint);
        transform.Translate(direction * speed);

        spriteObject.transform.rotation = RotateInDirection(direction);
        
        if(GetDistanceToPoint(groundPoints[targetPoint]) < 0.005f)
            targetPoint++;
    }

    float CalculateSpeed(float s, float minS, float rate){
        if(s > minS){
            s -= rate * Time.deltaTime;
            if(s < minS)
                s = minS;
        }
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
}
