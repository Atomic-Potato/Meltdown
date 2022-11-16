using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GroundCreator : MonoBehaviour{

    [Range(.05f, 1.5f)]
    [SerializeField] float spacing = 1f;
    [SerializeField] float groundHeight = 5f;
    [SerializeField] float tiling = 1f;
    public bool autoUpdate;

    [Space]
    [SerializeField] PathCreator pathCreator;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;

    public void UpdateGround(){
        Vector2[] points = pathCreator.path.CalculateEvenlySpacedPoints(spacing);
        meshFilter.mesh = CreateGroundMesh(points);
        
        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * .05f);
        meshRenderer.sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    /* To Create a mesh we get the evenly spaced points along a curve
    *  Get the direction of each point
    *  Set 2 vertecies prependicular to the direction of the point
    *  (However for our case the vertex on the left will be the same as the point)
    *  Then connect them with the next point vertecies to form 2 triangles
    */
    Mesh CreateGroundMesh(Vector2[] points){
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] triangles = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0;
        int triIndex = 0;

        //Getting the direction of every point
        //Note: the points that are not first or last 
        //      take the average direction with the one before it and in front
        for(int i=0; i < points.Length; i++){
            Vector2 forward = Vector2.zero;
            if(i < points.Length - 1)
                forward += points[i+1] - points[i];
            if(i > 0)
                forward += points[i] - points[i-1];
            forward.Normalize();

            //Adding vertecies
            //Left prependicular forward(-y, x), but we want it to be on the point
            //Right prependicular forward(y, -x)
            Vector2 right = new Vector2(forward.y, -forward.x);
            verts[vertIndex] = points[i];
            verts[vertIndex+1] = points[i] + right * groundHeight * .5f;
            
            //i dont get UVs yet
            float completionPercent = i / (float)(points.Length - 1);
            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            //Adding the triangles
            if(i < points.Length - 1){
                triangles[triIndex] = vertIndex; 
                triangles[triIndex + 1] = vertIndex + 2;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 5] = vertIndex + 3;
                 
            }
            
            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }
}
