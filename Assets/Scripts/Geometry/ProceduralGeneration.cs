using System.Collections;
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
    [SerializeField] GroundCreator creator;
    
    const int Chasm = -1;
    const int Ground = 1;
    int[] probabilityArray = new int[10];

    void Awake() {
        creator.UpdateCollision();

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
        if(GetDistanceToLastAnchor() < distanceToAddSection){
            AddSection(Ground);
        }
        if(GetDistanceToFirstAnchor() > distanceToRemoveAnchor){
            playerController.mainGround.path.RemoveFirstSegment();
            creator.UpdateGround();
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
            int sectionNum = Random.Range(0, groundSections.Length);
            Path section = groundSections[sectionNum].GetComponent<PathCreator>().path;
            
            //Calculating the offset of each point
            Vector2 lastPoint = playerController.mainGround.path[playerController.mainGround.path.NumPoints-1];
            float distX = Mathf.Abs(section[0].x - lastPoint.x);
            float distY = Mathf.Abs(section[0].y - lastPoint.y);

            //Moving points and adding them
            for(int i=0; i < section.NumPoints; i++){
                if(i%3 != 0)
                    continue;
                Vector2 newPoint = new Vector2(0f, 0f);
                newPoint.x = section[i].x < lastPoint.x ? section[i].x + distX : section[i].x - distX;
                newPoint.y = section[i].y < lastPoint.y ? section[i].y + distY : section[i].y - distY;


            }
                Vector2 testPoint = new Vector2(Random.Range(lastPoint.x + 15f, lastPoint.x + 25f),
                                                Random.Range(lastPoint.y, lastPoint.y - 15f));
                playerController.mainGround.path.AddSegment(testPoint);

            //Cleaning up
            creator.UpdateCollision();
            creator.UpdateGround();
            playerController.groundPoints = playerController.mainGround.path.CalculateEvenlySpacedPoints(playerController.groundSpacing);
            playerController.SetTargetToNearstFrontPoint();
        }
    }

    float GetDistanceToLastAnchor(){
        return Vector2.Distance(playerController.transform.position, playerController.mainGround.path[playerController.mainGround.path.NumPoints-1]);
    }
    float GetDistanceToFirstAnchor(){
        return Vector2.Distance(playerController.transform.position, playerController.mainGround.path[0]);
    }
}
