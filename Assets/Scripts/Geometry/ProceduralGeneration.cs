using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [Range(0, 10)] [Tooltip("The probability that a ground section will spawn")]
    [SerializeField] int groundGenerationBias = 7;
    [SerializeField] float chasmsWidth = 5f;
    [SerializeField] float chasmsHeight = 2.5f;
    [SerializeField] GameObject[] groundSections;
    [Tooltip("ALWAYS have the end of the chasm right after the start in the array")]
    [SerializeField] GameObject[] chasms;
    
    [Space]
    [Header("REQUIRED COMPONENTS")]
    [SerializeField] GameObject groundConnector;

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

    public int GetRandomGroundType(){
        return probabilityArray[Random.Range(0, 10)];
    }

    
    public GameObject AddGroundSection(GameObject currentGround, GameObject[] ground){
        int sectionNum;
        do
        {
            sectionNum = Random.Range(0, groundSections.Length);
        } while (ground.Contains<GameObject>(groundSections[sectionNum]));

        groundSections[sectionNum].SetActive(true);
        Path sectionPath = groundSections[sectionNum].GetComponent<PathCreator>().path;

        // Calculating the offset of each point
        Path currentPath = currentGround.GetComponent<PathCreator>().path;
        Vector2 lastPoint = currentPath[currentPath.NumPoints - 1];
        Vector2 moveDistance = new Vector2(lastPoint.x - sectionPath[0].x, lastPoint.y - sectionPath[0].y);

        MoveSection(sectionPath, currentPath, moveDistance);
        RenderPath(sectionPath, currentPath);

        return groundSections[sectionNum];
    }

    public GameObject AddLeftChasm(GameObject currentGround, GameObject[] ground){
        int sectionNum;
        do{
            sectionNum = Random.Range(0, chasms.Length);
            if(sectionNum % 2 != 0) //since theres always 2 parts of each chasm
                sectionNum--;
        }while(ground.Contains<GameObject>(chasms[sectionNum]));            

        // Adding first section
        chasms[sectionNum].SetActive(true);
        Path sectionPath = chasms[sectionNum].GetComponent<PathCreator>().path;

        //Calculating the offset of each point
        Path currentPath = currentGround.GetComponent<PathCreator>().path;
        Vector2 lastPoint = currentPath[currentPath.NumPoints - 1];
        Vector2 moveDistance = new Vector2(lastPoint.x - sectionPath[0].x, lastPoint.y - sectionPath[0].y);
        
        MoveSection(sectionPath, currentPath, moveDistance);
        RenderPath(sectionPath, currentPath);

        return chasms[sectionNum];
    }

    public GameObject AddRightChasm(GameObject currentGround, GameObject[] ground){
        int sectionNum;
        do{
            sectionNum = Random.Range(0, chasms.Length);
            if(sectionNum % 2 == 0) //since theres always 2 parts of each chasm
                sectionNum++;
            Debug.Log("Chasm number  : " + sectionNum + "  LENGTH: " + chasms.Length);
        }while(ground.Contains<GameObject>(chasms[sectionNum]));            

        // Adding first section
        chasms[sectionNum].SetActive(true);
        Path sectionPath = chasms[sectionNum].GetComponent<PathCreator>().path;

        //Calculating the offset of each point
        Path currentPath = currentGround.GetComponent<PathCreator>().path;
        Vector2 lastPoint = currentPath[currentPath.NumPoints - 1];
        Vector2 moveDistance = new Vector2(lastPoint.x + chasmsWidth - sectionPath[0].x, lastPoint.y + chasmsHeight - sectionPath[0].y);
        
        MoveSection(sectionPath, currentPath, moveDistance, true);
        //RenderPath(sectionPath, currentPath);
        RenderSection(sectionPath);

        return chasms[sectionNum];
    }

    static void MoveSection(Path nextPath, Path currentPath, Vector2 moveDistance, bool rightChasm = false)
    {
        for (int i = 0; i < nextPath.NumPoints; i++)
        {
            if (nextPath.AutoSetControlPoints && i % 3 != 0)
                continue;
            // Move the first handle of the next section to be at the opposite side 
            // of the last handle of the current section 
            if (i == 1 && !rightChasm)
            {
                Vector2 handleMoveDist = new Vector2(currentPath[currentPath.NumPoints - 1].x - currentPath[currentPath.NumPoints - 2].x,
                                                        currentPath[currentPath.NumPoints - 1].y - currentPath[currentPath.NumPoints - 2].y);
                nextPath.ForceMovePoint(i, currentPath[currentPath.NumPoints - 1] + handleMoveDist);
                continue;
            }

            nextPath.ForceMovePoint(i, nextPath[i] + moveDistance);
        }
    }

    void RenderPath(Path nextPath, Path currentPath)
    {
        GameObject oldRenderer = groundRenderers.Dequeue();
        if (oldRenderer != null)
            Destroy(oldRenderer);
        GameObject newRenderer = Instantiate(groundConnector);
        groundRenderers.Enqueue(newRenderer);
        Path connectionPath = newRenderer.GetComponent<PathCreator>().path;
        //Matching the first 4 points to the points in the current segment
        for (int i = 0; i < 4; i++)
            connectionPath.ForceMovePoint(i, currentPath[i]);
        //Adding the extra points in the current segment
        for (int i = 4; i < currentPath.NumPoints; i++)
            connectionPath.AddPoint(currentPath[i]);
        //Adding the points of the next segment
        for (int i = 1; i < nextPath.NumPoints; i++)
            connectionPath.AddPoint(nextPath[i]);

        newRenderer.GetComponent<GroundCreator>().UpdateGround();
    }

    void RenderSection(Path sectionPath){
        GameObject oldRenderer = groundRenderers.Dequeue();
        if (oldRenderer != null)
            Destroy(oldRenderer);

        GameObject newRenderer = Instantiate(groundConnector);
        groundRenderers.Enqueue(newRenderer);

        Path connectionPath = newRenderer.GetComponent<PathCreator>().path;
        //Matching the first 4 points to the points in the section
        for(int i=0; i < 4; i++)
            connectionPath.ForceMovePoint(i, sectionPath[i]);
        //Adding the extra points in the current section
        for (int i = 4; i < sectionPath.NumPoints; i++)
            connectionPath.AddPoint(sectionPath[i]);

        newRenderer.GetComponent<GroundCreator>().UpdateGround();
    }
}
