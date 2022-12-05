using UnityEngine;

public class GameplayLoop : MonoBehaviour
{
    [SerializeField] int distanceToWin  = 1000;

    [SerializeField] Transform startTransform;
    [SerializeField] Transform playerTransform;
    

    [HideInInspector] public int currentDistance = 0;

    void Update(){
        currentDistance = (int)Mathf.Abs(startTransform.position.x - playerTransform.position.x);

        if(currentDistance > distanceToWin)
            Debug.Log("You Win");
    }
}
