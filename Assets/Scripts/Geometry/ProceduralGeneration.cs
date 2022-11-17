using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] float distanceToAddSection = 45f;
    [SerializeField] float distanceToRemoveAnchor = 100f;
    [Range(0, 10)] [Tooltip("The probability that a ground section will spawn")]
    [SerializeField] int groundGenerationBias = 7;
    [SerializeField] GameObject[] groundSections;
    [Tooltip("ALWAYS have the end of the chasm right after the start in the array")]
    [SerializeField] GameObject[] chasms; 

    [Header("REQUIRED COMPONENTS")]
    [SerializeField] PlayerController playerController;

    const int Chasm = -1;
    const int Ground = 1;
    int[] probabilityArray = new int[10];

    // CACHE
    bool leaveGroundCache;

    void Awake() {
        //Filling the probability array
        for(int i=0; i < 10; i++){
            if(i < groundGenerationBias)
                probabilityArray[i] = Ground;
            else
                probabilityArray[i] = Chasm;
        }
    }

    void Update()
    {
        if(playerController.sectionsQueue.Count < 3){
            Debug.Log("Add section");
            AddSection(Ground);
        }
    }
    void AddSection(int type){
        if(type == Chasm){
            int sectionNum = Random.Range(0, chasms.Length);
            if(sectionNum % 2 != 0) //since theres always 2 parts of each chasm
                sectionNum--;
            
            //Add a chasm   
        }
        else{
            //Getting the section
            int sectionNum;
            do{
                sectionNum = Random.Range(0, groundSections.Length);
            }while(playerController.sectionsQueue.Contains(groundSections[sectionNum]));
            groundSections[sectionNum].SetActive(true);
            playerController.sectionsQueue.Enqueue(groundSections[sectionNum]);
            Path sectionPath = groundSections[sectionNum].GetComponent<PathCreator>().path;
            
            //Calculating the offset of each point
            Vector2 lastPoint = playerController.mainGround.path[playerController.mainGround.path.NumPoints-1];
            Vector2 moveDistance = new Vector2(lastPoint.x - sectionPath[0].x, lastPoint.y - sectionPath[0].y);

            //Moving the section points to the current location
            for(int i=0; i < sectionPath.NumPoints; i++){
                if(sectionPath.AutoSetControlPoints && i % 3 != 0)
                    continue;
                sectionPath.CustomMovePoint(i, sectionPath[i] + moveDistance); 
            }
            // Cleaning up
            groundSections[sectionNum].GetComponent<GroundCreator>().UpdateGround();
        }
    }
}
