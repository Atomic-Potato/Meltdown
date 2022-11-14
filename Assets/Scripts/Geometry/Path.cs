using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path{
    [SerializeField, HideInInspector] List<Vector2> points;
    [SerializeField, HideInInspector] bool autoSetControlPoints;

    public Path(Vector2 center){
        points = new List<Vector2>{
            center + Vector2.left,
            center + new Vector2(-1f,1f) * .5f,
            center + new Vector2(1f,-1f) * .5f,
            center + Vector2.right,
        };
    }

    //Indexer: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/
    //Indexers allow instances of a class or struct to be indexed just like arrays.
    public Vector2 this[int i]{
        get{
            return points[i];
        }
    }

    public int NumPoints{
        get{
            return points.Count;
        }
    }

    public int NumSegments{
        get{
            return (points.Count - 4) / 3 + 1;
        }
    }

    public bool AutoSetControlPoints{
        get{
            return autoSetControlPoints;
        }
        set{
            if(autoSetControlPoints != value){
                autoSetControlPoints = value;
                if(autoSetControlPoints)
                    AutoSetAllControlPoints();
            }
        }
    }

    public void AddSegment(Vector2 anchorPos){
        //Last point becomes an anchor, so it has 2 handles, this is the new hanle
        points.Add(points[points.Count-1] * 2 - points[points.Count - 2]); 
        //New anchor handle, half the distance between the new added handle(previous anchor) and the anchor
        points.Add((points[points.Count-1] + anchorPos) * .5f);
        points.Add(anchorPos);  

        if(autoSetControlPoints)
            AutoSetAffectedControlPoints(points.Count - 1);
    }

    public void RemoveFirstSegment(){
        if(NumSegments > 2)
            points.RemoveRange(0,3);
    }

    public Vector2[] GetPointsInSegment(int i){
        return new Vector2[]{points[i*3], points[i*3 + 1], points[i*3 + 2], points[i*3 + 3]};
    }

    public void MovePoint(int i, Vector2 position){
        Vector2 deltaMove = position - points[i]; // distance between the old and new position
        if(i%3==0 || !autoSetControlPoints)
            points[i] = position;
        else
            return;

        if(autoSetControlPoints){
            AutoSetAffectedControlPoints(i);
        }
        else{
            //if this point is an anchor point
            if(i%3 == 0) {
                // if there exist a handle after the anchor / there another segment
                if(i+1<points.Count){
                    points[i+1] += deltaMove; //Move this handle the same amount
                }
                if(i-1>0){
                    points[i-1] += deltaMove;
                }
            }
            else{ //if we're moving one of the handles of the anchor
                bool nextPointISAnchor = (i+1) % 3 == 0;
                int correspondingControlIndex = nextPointISAnchor ? i+2 : i-2; // Or the handle on the opposite side
                int anchorIndex = nextPointISAnchor ? i+1 : i-1;

                if(correspondingControlIndex >= 0 && correspondingControlIndex < points.Count){
                    float distance = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
                    Vector2 direction = (points[anchorIndex] - position).normalized;
                    points[correspondingControlIndex] = points[anchorIndex] + direction * distance;
                }
            }
        }
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1){
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float distSinceLastEvenPoint = 0;

        for(int segment=0; segment < NumSegments; segment++){
            Vector2 [] pts = GetPointsInSegment(segment);

            //Getting an approximate lenght of the curve / segment
            float controlNet = Vector2.Distance(pts[0], pts[1]) + Vector2.Distance(pts[1], pts[2]) + Vector2.Distance(pts[2], pts[3]);
            float curveLength = (controlNet + Vector2.Distance(pts[0], pts[3])) / 2f;
            //Dividing the curve into even bits
            int divisions = Mathf.CeilToInt(curveLength * resolution * 10);

            float t=0;
            while(t<=1){
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(pts[0], pts[1], pts[2], pts[3], t);
                distSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                //If we overshot the spacing then we go back that distance and that point
                //And if spacing is really small, we could overshoot multiple points
                //So we use while to keep adding these points
                while(distSinceLastEvenPoint >= spacing){
                    float overShootDist = distSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overShootDist;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    distSinceLastEvenPoint = overShootDist;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }            
        }

        return evenlySpacedPoints.ToArray();
    }

    public static int GetNearestPoint(Vector2 pos, Vector2[] pts){
        float leastDistance = float.MaxValue;
        int distIndex = -1;

        for(int i=0; i < pts.Length; i++){
            float dist = Mathf.Abs(Vector2.Distance(pos, pts[i])); 
            if(dist < leastDistance){
                leastDistance = dist;
                distIndex = i; 
            }
        }

        return distIndex;
    }

    void AutoSetAffectedControlPoints(int updatedAnchorIndex){
        for(int i = updatedAnchorIndex-3; i < updatedAnchorIndex + 3; i+=3){
            if(i >=0 && i < points.Count)
                AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControls();
    }

    void AutoSetAllControlPoints(){
        for(int i=0; i<points.Count; i++)
            AutoSetAnchorControlPoints(i);
        AutoSetStartAndEndControls();
    }

    /* Get the distance between the anchor and its neighbors
    *  Normalize these distance vectors
    *  Subtract them from eachother and you get the direction of the handles
    *  Set each control point distance from the anchor as half the distance from the anchor to the neighbor
    */
    void AutoSetAnchorControlPoints(int anchorIndex){
        Vector2 anchorPos = points[anchorIndex];
        Vector2 direction = Vector2.zero;
        float[] neighbourDistances = new float[2];

        //First neighbour
        if(anchorIndex - 3 >= 0){
            Vector2 offset = points[anchorIndex-3] - anchorPos;
            direction += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        //Second neighbour
        if(anchorIndex + 3 <= points.Count){
            Vector2 offset = points[anchorIndex+3] - anchorPos;
            direction -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        direction.Normalize();

        for(int i=0; i<2; i++){
            int controlIndex = anchorIndex + i*2 - 1;
            if(controlIndex >=0 && controlIndex < points.Count){
                points[controlIndex] = anchorPos + direction * neighbourDistances[i] * .5f;
            }
        }
    }

    /* Set the end and start control points half way 
    *  between the neighbouring anchor control points
    */
    void AutoSetStartAndEndControls(){
        points[1] = (points[0] + points[2]) * 0.5f;
        points[points.Count-2] = (points[points.Count-1] + points[points.Count-3]) * 0.5f;
    }
}
