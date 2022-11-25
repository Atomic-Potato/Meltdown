using UnityEngine;
using TMPro;

public class TypeRacing : MonoBehaviour{

    [SerializeField] int wordLength;
    [SerializeField] TMP_Text longTargetText;
    [SerializeField] TMP_InputField longInputField;

    void Start(){
        longInputField.ActivateInputField();
        longTargetText.text = GenerateWord(wordLength);
    }

    string GenerateWord(int Length){
        string word = "";
        for(int i=0; i < Length; i++){
            word = word + (char)Random.Range(65, 90);
        }
        return word;
    }
}
