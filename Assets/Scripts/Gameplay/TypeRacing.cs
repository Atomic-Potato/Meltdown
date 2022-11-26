using UnityEngine;
using TMPro;

public class TypeRacing : MonoBehaviour{

    [Header("UNDEFINED")]
    [SerializeField] TMP_InputField nuteralInput;

    [Space]
    [SerializeField] TMP_Text boostText;
    [SerializeField] GameObject boostObject;

    [Space]
    [Header("SHORT WORD")]
    [SerializeField] float shortWordBoost = 2f;
    [SerializeField] int shortWordLength = 2;
    [SerializeField] int shortMistakesCount = 3;
    [SerializeField] TMP_Text shortTargetText;
    [SerializeField] TMP_Text shortDisplayText;
    [SerializeField] TMP_InputField shortInputField;
    [SerializeField] TMP_Text shortMistakesText;
    [SerializeField] GameObject shortObject;

    [Space]
    [Header("LONG WORD")]
    [SerializeField] float longWordBoost = 8f;
    [SerializeField] int longWordLength = 5;
    [SerializeField] int longMistakesCount = 3;
    [SerializeField] TMP_Text longTargetText;
    [SerializeField] TMP_Text longDisplayText;
    [SerializeField] TMP_InputField longInputField;
    [SerializeField] TMP_Text longMistakesText;
    [SerializeField] GameObject longObject;

    [Space]
    [Header("REQUIRED COMPONENETS")]
    [SerializeField] PlayerController playerController;

    int boostAmount = 0;
    int currentType = 0; // 0: pick a type | 1: short word | -1: long word
    int mistakes;
    string prevLongText = "";
    string prevShortText = "";
    


    void Awake() {
        longMistakesText.text = longMistakesCount.ToString();
        shortMistakesText.text = shortMistakesCount.ToString();
    }

    void Start(){
        longTargetText.text = GenerateWord(longWordLength);
        do{
            shortTargetText.text = GenerateWord(shortWordLength);
        }while(shortTargetText.text[0] == longTargetText.text[0]);
    }

    void Update(){
        
        if(playerController.isGrounded){
            if(longObject.activeSelf)
                longObject.SetActive(false);
            if(shortObject.activeSelf)
                shortObject.SetActive(false);
            if(boostObject.activeSelf)
                boostObject.SetActive(false);
        }

        if(playerController.isJustLeftGround){
            if(!longObject.activeSelf)
                longObject.SetActive(true);
            if(!shortObject.activeSelf)
                shortObject.SetActive(true);
            if(!boostObject.activeSelf)
                boostObject.SetActive(true);

            ResetAll(-1);
            ResetAll( 1);
            ResetBoost();
            currentType = 0;
        }

        if(currentType == 0){
            nuteralInput.Select();
            
            if(nuteralInput.text.Length == 0)
                return;
            
            nuteralInput.text = nuteralInput.text.ToUpper();

            if(nuteralInput.text[0] == longTargetText.text[0]){
                currentType = -1;
                longDisplayText.text = longTargetText.text[0].ToString();
                longInputField.text = longTargetText.text[0].ToString();
                longInputField.caretPosition = longInputField.text.Length;
            }
            else if(nuteralInput.text[0] == shortTargetText.text[0]){
                currentType = 1;
                shortDisplayText.text = shortTargetText.text[0].ToString();
                shortInputField.text = shortTargetText.text[0].ToString();
                shortInputField.caretPosition = shortInputField.text.Length;
            }
            
            nuteralInput.text = "";
            nuteralInput.caretPosition = 0;
        }

        if(currentType == -1){
            UpdateLong();
        }
        else if(currentType == 1){
            UpdateShort();
        }
    }

    void UpdateLong(){
        longInputField.Select();
        ResetCaretPosition(longInputField);
        if(EliminateUncessaryKeys(longInputField, prevLongText))
            return;
        CheckForWordEnd(-1, longInputField, longTargetText);
        prevLongText = CheckForNewCharacters(-1, longInputField, prevLongText, longTargetText, longMistakesText);

        if (Input.anyKeyDown){
            longDisplayText.text = longInputField.text;
        }
    }

    void UpdateShort(){
        shortInputField.Select();
        ResetCaretPosition(shortInputField);
        if(EliminateUncessaryKeys(shortInputField, prevShortText))
            return;
        CheckForWordEnd(1, shortInputField, shortTargetText);
        prevLongText = CheckForNewCharacters(1, shortInputField, prevShortText, shortTargetText, shortMistakesText);

        if (Input.anyKeyDown){
            shortDisplayText.text = shortInputField.text;
        }
    }

    bool EliminateUncessaryKeys(TMP_InputField inputField, string prevText){
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Space)){
            inputField.text = prevText;
            inputField.caretPosition = inputField.text.Length;
            return true;
        }
        return false;
    }

    void ResetCaretPosition(TMP_InputField inputField){
        if (inputField.caretPosition != inputField.text.Length)
            inputField.caretPosition = inputField.text.Length;
    }

    void CheckForWordEnd(int type, TMP_InputField inputField, TMP_Text target){
        if (inputField.text.Length == target.text.Length && inputField.text[inputField.text.Length - 1] == target.text[inputField.text.Length - 1]){
            float boost = 0;
            if(type == -1)
                boost = longWordBoost;
            else if(type == 1)
                boost = shortWordBoost;

            playerController.speed += boost;
            boostAmount += (int)boost;
            boostText.text = $"+" + boostAmount.ToString() + " speed";
            ResetAll(type);
        }
    }

    string CheckForNewCharacters(int type, TMP_InputField inputField, string prevText, TMP_Text target, TMP_Text mistakesText){
        if (prevText.Length != inputField.text.Length){
            inputField.text = inputField.text.ToUpper();
            if (inputField.text[inputField.text.Length - 1] != target.text[inputField.text.Length - 1]){
                mistakes--;
                inputField.text = prevText;
                mistakesText.text = mistakes.ToString();

                if(mistakes <= 0)
                    ResetAll(type);
            }
            else{
                return inputField.text;
            }
        }

        return prevText;
    }

    void ResetAll(int type){
        // -1 for long  | 1 for short
        if(type == -1){
            mistakes = longMistakesCount;
            longMistakesText.text = mistakes.ToString();
            longDisplayText.text = "";
            longInputField.text = "";
            prevLongText = "";
            do{
                longTargetText.text = GenerateWord(longWordLength);
            }while(shortTargetText.text.Length != 0 && longTargetText.text[0] == shortTargetText.text[0]);

            currentType = 0;
        }
        else if(type == 1){
            mistakes = shortMistakesCount;
            shortMistakesText.text = mistakes.ToString();
            shortDisplayText.text = "";
            shortInputField.text = "";
            prevShortText = "";
            do{
                shortTargetText.text = GenerateWord(shortWordLength);
            }while(longTargetText.text.Length != 0 && shortTargetText.text[0] == longTargetText.text[0]);

            currentType = 0;
        }
    }

    void ResetBoost(){
        boostAmount = 0;
        boostText.text = "+0 speed";
    }

    string GenerateWord(int Length){
        string word = "";
        for(int i=0; i < Length; i++){
            word = word + (char)Random.Range(65, 90);
        }
        return word;
    }
}
