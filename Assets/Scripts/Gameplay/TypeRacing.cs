using UnityEngine;
using TMPro;

public class TypeRacing : MonoBehaviour{

    [SerializeField] int wordLength;
    [SerializeField] int longMistakesCount = 3;
    [SerializeField] TMP_Text longTargetText;
    [SerializeField] TMP_Text longDisplayText;
    [SerializeField] TMP_InputField longInputField;
    [SerializeField] TMP_Text longMistakesText;

    int mistakes;
    string prevLongText = "";
    
    void Awake() {
        mistakes = longMistakesCount;
        longMistakesText.text = mistakes.ToString();
    }

    void Start(){
        longTargetText.text = GenerateWord(wordLength);
    }

    void Update()
    {
        longInputField.ActivateInputField();
        ResetCaretPosition();
        if(EliminateUncessaryKeys())
            return;
        CheckForWordEnd();
        CheckForNewCharacters();

        if (Input.anyKeyDown){
            longDisplayText.text = longInputField.text;
        }

    }

    bool EliminateUncessaryKeys(){
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Space)){
            longInputField.text = prevLongText;
            longInputField.caretPosition = longInputField.text.Length;
            return true;
        }
        return false;
    }

    void ResetCaretPosition(){
        if (longInputField.caretPosition != longInputField.text.Length)
            longInputField.caretPosition = longInputField.text.Length;
    }

    void CheckForWordEnd(){
        if (longInputField.text.Length == longTargetText.text.Length && longInputField.text[longInputField.text.Length - 1] == longTargetText.text[longInputField.text.Length - 1]){
            // Add to speed
            ResetAll();
        }
    }

    void CheckForNewCharacters(){
        if (prevLongText.Length != longInputField.text.Length){
            longInputField.text = longInputField.text.ToUpper();
            if (longInputField.text[longInputField.text.Length - 1] != longTargetText.text[longInputField.text.Length - 1]){
                mistakes--;
                longInputField.text = prevLongText;
                longMistakesText.text = mistakes.ToString();

                if(mistakes <= 0)
                    ResetAll();
            }
            else{
                prevLongText = longInputField.text;
            }
        }
    }

    void ResetAll(){
        mistakes = longMistakesCount;
        longMistakesText.text = mistakes.ToString();
        longDisplayText.text = "";
        longInputField.text = "";
        prevLongText = "";
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
