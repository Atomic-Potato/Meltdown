using UnityEngine;

public class Physicsf : MonoBehaviour{
    public static float globalGravity = -9.81f;
    public static void ApplyGravity(Rigidbody rb, float gravityScale = 1f){
        if(rb.useGravity)
            rb.useGravity = false;

        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }
}
