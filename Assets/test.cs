using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float str;

    Vector2 velocity;
    float velocityMagnitude;

    Vector2 prevPosition;

    void Start()
    {
        rb.velocity = Vector3.right * 5f;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("[GROUND] Velocity magnitude : " + velocityMagnitude + "\nVelocity vector : " + velocity);
    }

    private void FixedUpdate() {
        ClaculateGroundedVelocity();
    }

    void ClaculateGroundedVelocity(){
        velocityMagnitude = Vector2.Distance(prevPosition, transform.position)/Time.deltaTime;
        velocity = new Vector2((transform.position.x - prevPosition.x)/Time.deltaTime, (transform.position.y - prevPosition.y)/Time.deltaTime);
        prevPosition = transform.position;
    }
}
