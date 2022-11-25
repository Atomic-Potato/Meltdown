using UnityEngine;
using TMPro;

public class TypeRacing : MonoBehaviour{

    
    [SerializeField] TMP_InputField longInputField;

    void Start(){
        // MOVE THIS TO WHEN THE PLAYER JUMPS ONCE IMPLEMENTED
        longInputField.ActivateInputField();
    }
}
