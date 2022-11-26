using UnityEngine;
using UnityEditor;

public class CameraController : MonoBehaviour{

    [SerializeField] Vector2 offset;
    [SerializeField] GameObject player;

    void Update() {
        transform.position = new Vector3(player.transform.position.x + offset.x, player.transform.position.y + offset.y, transform.position.z);
    }

    void OnDrawGizmos() {
        SceneView.RepaintAll();
        transform.position = new Vector3(player.transform.position.x + offset.x, player.transform.position.y + offset.y, transform.position.z);
    }
}
