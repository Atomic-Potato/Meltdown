using UnityEngine;
using TMPro;

public class TypeRacing : MonoBehaviour{

    [SerializeField] int wordLength;
    [SerializeField] TMP_Text longTargetText;
    [SerializeField] TMP_InputField longInputField;

    string prevLongText = "";

    void Start(){
        longInputField.ActivateInputField();
        longTargetText.text = GenerateWord(wordLength);
    }

    void Update() {
        if(longInputField.text.Length == longTargetText.text.Length){
            // Add to speed
            longInputField.text = "";
            prevLongText = "";
            longTargetText.text = GenerateWord(wordLength);
        }

        if(prevLongText.Length != longInputField.text.Length){
            longInputField.text = longInputField.text.ToUpper(); 
            if(longInputField.text[longInputField.text.Length-1] != longTargetText.text[longInputField.text.Length-1])
                longInputField.text = prevLongText; 
            else
                prevLongText = longInputField.text;
        }
    }

    string GenerateWord(int Length){
        string word = "";
        for(int i=0; i < Length; i++){
            word = word + (char)Random.Range(65, 90);
        }
        return word;
    }
}
