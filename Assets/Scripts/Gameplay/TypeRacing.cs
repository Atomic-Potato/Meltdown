using UnityEngine;
using TMPro;

public class TypeRacing : MonoBehaviour{

    [SerializeField] int wordLength;
    [SerializeField] TMP_Text longTargetText;
    [SerializeField] TMP_Text longDisplayText;
    [SerializeField] TMP_InputField longInputField;

    string prevLongText = "";

    void Awake() {
    }

    void Start(){
        longTargetText.text = GenerateWord(wordLength);
    }

    void Update() {
        //set the position of the caret at the end if it was changed        

        longInputField.ActivateInputField();

        if(Input.GetKeyDown(KeyCode.Backspace)){
            longInputField.text = prevLongText;
            // Set back the caret position to the end
            return;
        }

        if(longInputField.text.Length == longTargetText.text.Length){
            // Add to speed
            longDisplayText.text = "";
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
        
        if(Input.anyKeyDown){
            longDisplayText.text = longInputField.text;
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
