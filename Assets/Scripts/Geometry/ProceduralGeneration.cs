using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [Range(0, 10)] [Tooltip("The probability that a ground section will spawn")]
    [SerializeField] int groundGenerationBias = 7;
    [SerializeField] float groundHeight;
    [SerializeField] GameObject[] groundSections;
    [Tooltip("ALWAYS have the end of the chasm right after the start in the array")]
    [SerializeField] GameObject[] chasms;
    
    [Space]
    [Header("REQUIRED COMPONENTS")]
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject groundConnector;
    [SerializeField] Material groundMaterial;

    [Space]
    [Header("DEBUGGING")]

    public static int Chasm = -1;
    public static int Ground = 1;
    int[] probabilityArray = new int[10];
    Queue<GameObject> groundRenderers = new Queue<GameObject>();

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
        groundRenderers.Enqueue(new GameObject("NULL"));
        groundRenderers.Enqueue(new GameObject("NULL"));
    }

    void Start(){
    }
    
    public GameObject AddSection(int type, GameObject currentGround, GameObject[] ground){
        int sectionNum;
        if(type == Chasm){
            sectionNum = Random.Range(0, chasms.Length);
            if(sectionNum % 2 != 0) //since theres always 2 parts of each chasm
                sectionNum--;
            
            //Add a chasm   
        }
        else{
            //Getting the section
            do{
                sectionNum = Random.Range(0, groundSections.Length);
            }while(ground.Contains<GameObject>(groundSections[sectionNum]));

            groundSections[sectionNum].SetActive(true);
            Path sectionPath = groundSections[sectionNum].GetComponent<PathCreator>().path;
            
            //Calculating the offset of each point
            Path currentPath = currentGround.GetComponent<PathCreator>().path;
            Vector2 lastPoint = currentPath[currentPath.NumPoints-1];
            Vector2 moveDistance = new Vector2(lastPoint.x - sectionPath[0].x, lastPoint.y - sectionPath[0].y);

            //Moving the section points to the current location
            for(int i=0; i < sectionPath.NumPoints; i++){
                if(sectionPath.AutoSetControlPoints && i % 3 != 0)
                    continue;
                // Move the first handle of the next section to be at the opposite side 
                // of the last handle of the current section 
                if(i == 1){
                    Vector2 handleMoveDist = new Vector2(   currentPath[currentPath.NumPoints - 1].x - currentPath[currentPath.NumPoints-2].x, 
                                                            currentPath[currentPath.NumPoints - 1].y - currentPath[currentPath.NumPoints-2].y   );
                    sectionPath.ForceMovePoint(i, currentPath[currentPath.NumPoints-1] + handleMoveDist); 
                    continue;
                }

                sectionPath.ForceMovePoint(i, sectionPath[i] + moveDistance); 

            }

            // Rendering the path
            GameObject oldRenderer = groundRenderers.Dequeue();
            if(oldRenderer != null){
                Destroy(oldRenderer);
            }
            GameObject newRenderer = Instantiate(groundConnector);
            groundRenderers.Enqueue(newRenderer);
            Path connectionPath = newRenderer.GetComponent<PathCreator>().path;
            //Matching the first 4 points to the points in the current segment
            for(int i=0; i < 4; i++){
                connectionPath.ForceMovePoint(i, currentPath[i]);
            }
            //Adding the extra points in the current segment
            for(int i=4; i < currentPath.NumPoints; i++){
                connectionPath.AddPoint(currentPath[i]);
            }
            //Adding the points of the next segment
            for(int i=1; i < sectionPath.NumPoints; i++){
                connectionPath.AddPoint(sectionPath[i]);
            }
            
            newRenderer.GetComponent<GroundCreator>().UpdateGround();

            // Cleaning up
            //groundSections[sectionNum].GetComponent<GroundCreator>().UpdateGround();

            // Vector2[] connectionPoints = new Vector2[currentPath.NumPoints + sectionPath.NumPoints];
            // Vector2[] currentEvens = currentPath.CalculateEvenlySpacedPoints(0.05f); 
            // Vector2[] sectionEvens = sectionPath.CalculateEvenlySpacedPoints(0.05f);
            // connectionPoints.Concat(currentEvens);
            // connectionPoints.Concat(sectionEvens);
            // currentEvens.CopyTo(connectionPoints, 0);
            // sectionEvens.CopyTo(connectionPoints, sectionEvens.Length-1);

            // GameObject connection = Instantiate(groundConnector);
            // GroundCreator.UpdateCustomGround(connectionPoints, 0.05f, groundHeight, connection.GetComponent<MeshRenderer>(), connection.GetComponent<MeshFilter>());
        }
        return groundSections[sectionNum];
    }
}
