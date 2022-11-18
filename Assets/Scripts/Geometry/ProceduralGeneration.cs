using System.Linq;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [Range(0, 10)] [Tooltip("The probability that a ground section will spawn")]
    [SerializeField] int groundGenerationBias = 7;
    [SerializeField] GameObject[] groundSections;
    [Tooltip("ALWAYS have the end of the chasm right after the start in the array")]
    [SerializeField] GameObject[] chasms; 
    
    [Header("REQUIRED COMPONENTS")]
    [SerializeField] PlayerController playerController;

    public static int Chasm = -1;
    public static int Ground = 1;
    int[] probabilityArray = new int[10];

    static GameObject[] staticGroundSections;
    static GameObject[] staticChasms; 


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
        
        staticGroundSections = groundSections;
        staticChasms = chasms;
    }
    
    public static GameObject AddSection(int type, GameObject currentGround, GameObject[] ground){
        int sectionNum;
        if(type == Chasm){
            sectionNum = Random.Range(0, staticChasms.Length);
            if(sectionNum % 2 != 0) //since theres always 2 parts of each chasm
                sectionNum--;
            
            //Add a chasm   
        }
        else{
            //Getting the section
            do{
                sectionNum = Random.Range(0, staticGroundSections.Length);
            }while(ground.Contains<GameObject>(staticGroundSections[sectionNum]));

            staticGroundSections[sectionNum].SetActive(true);
            Path sectionPath = staticGroundSections[sectionNum].GetComponent<PathCreator>().path;
            
            //Calculating the offset of each point
            Vector2 lastPoint = currentGround.GetComponent<PathCreator>().path[currentGround.GetComponent<PathCreator>().path.NumPoints-1];
            Vector2 moveDistance = new Vector2(lastPoint.x - sectionPath[0].x, lastPoint.y - sectionPath[0].y);

            //Moving the section points to the current location
            for(int i=0; i < sectionPath.NumPoints; i++){
                if(sectionPath.AutoSetControlPoints && i % 3 != 0)
                    continue;
                sectionPath.CustomMovePoint(i, sectionPath[i] + moveDistance); 
            }
            // Cleaning up
            staticGroundSections[sectionNum].GetComponent<GroundCreator>().UpdateGround();
        }

        return staticGroundSections[sectionNum];
    }
}
