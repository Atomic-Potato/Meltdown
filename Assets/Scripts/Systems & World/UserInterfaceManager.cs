using UnityEngine;
using TMPro;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] TMP_Text distanceText;
    [SerializeField] TMP_Text speedText;
    [SerializeField] TMP_Text timerText;
    [Space]
    [SerializeField] GameplayLoop gameplayLoop;
    [SerializeField] PlayerController playerController;

    void Update(){
        distanceText.text = gameplayLoop.currentDistance + "m";
        speedText.text = playerController.isGrounded ? (int)PlayerController.speed + "m/s" : (int)PlayerController.velocity.magnitude + "m/s";
        timerText.text = ((int)gameplayLoop.timer).ToString();
    }
}
