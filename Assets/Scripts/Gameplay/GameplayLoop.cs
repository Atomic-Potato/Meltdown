using UnityEngine;

public class GameplayLoop : MonoBehaviour
{
    [SerializeField] int distanceToWin  = 1000;

    [SerializeField] Transform startTransform;
    [SerializeField] Transform playerTransform;
    

    [HideInInspector] public float timer = 20f;
    [HideInInspector] public int currentDistance = 0;

    void Update(){
        currentDistance = (int)Mathf.Abs(startTransform.position.x - playerTransform.position.x);

        if(timer > 0){
            timer -= Time.deltaTime;
            if(timer < 0)
                timer = 0;
        }

        if(currentDistance > distanceToWin && timer > 0){
            Debug.Log("You Win");
        }
        else if(timer <= 0) {
            Debug.Log("You Loose!");
        }   
    }
}
