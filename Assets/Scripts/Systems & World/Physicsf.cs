using UnityEngine;

public class Physicsf : MonoBehaviour{
    public static float globalGravity = -9.81f;
    public static void ApplyGravity(Rigidbody rb, float gravityScale = 1f){
        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        rb.velocity += gravity * Time.deltaTime;
    }

    public static Vector2 GetNormalForce(float angle, float gravity){
        return new Vector2(gravity * Mathf.Cos(angle), gravity * Mathf.Sin(angle));
    }
}
