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

    [Space]
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject looseScreen;
    [SerializeField] GameObject winScreen;

    void Update(){
        distanceText.text = gameplayLoop.currentDistance + "m";
        speedText.text = playerController.isGrounded ? (int)PlayerController.speed + "m/s" : (int)PlayerController.velocity.magnitude + "m/s";
        timerText.text = ((int)gameplayLoop.timer).ToString();
    }

    public void DisplayLooseScreen(){
        if(!looseScreen.activeSelf){
            gameUI.SetActive(false);
            looseScreen.SetActive(true);
        }
    }

    public void DisplayWinScreen(){
        if(!winScreen.activeSelf){
            gameUI.SetActive(false);
            winScreen.SetActive(true);
        }
    }
}
