using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vectors;
using Plane = UnityEngine.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways] //Run in Edit Mode 
[RequireComponent(typeof(VectorRenderer))]
public class Collision : MonoBehaviour
{
    // Input variables
    public Vector3 ballDirection = new Vector3(3, 0, 0);

    public float ballRadius = 1;
    public Vector3 ballPosition = new Vector3(-5, 5, 0);

    // TODO: Ska kunna ställas exakt med grader
    public Vector3 planeNormal = new Vector3(0.3f, 1, 0);
    public float planeDistance;
    
    // Other Variables
    [NonSerialized] 
    private VectorRenderer vectors;
    public Transform ball;
    public Transform plane;
    public Transform ballImpact;
    public Transform ballFinal;
    

    public Vector3 newBallPosition;
    public Vector3 impact;
    public Vector3 aDir;
    public float aLength;
    public Vector3 aVec;
    public Vector3 bDir;
    public float bLength;
    public Vector3 bVec;


    // Start is called before the first frame update
    void Start()
    {
        ballRadius = 1;
    }
    
    void OnEnable() {
        vectors = GetComponent<VectorRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update user input
        ballPosition = ball.position;
        planeNormal = plane.up;
        planeDistance = plane.position.y; // TODO: Fix breaking when moving
        planeNormal.Normalize();
        ballDirection.Normalize();
        // set scale
        Vector3 tmp = new Vector3(ballRadius, ballRadius, ballRadius);
        tmp = tmp / 2;
        ball.localScale = tmp;
        ballImpact.localScale = tmp;
        ballFinal.localScale = tmp;

        
        CalculateBounce();
        
        // Illustrate ball positions
        ballImpact.position = impact;
        ballFinal.position = newBallPosition;
        
        // Illustrate vectors
        using (vectors.Begin())
        {
            vectors.Draw(ballPosition, ballPosition+ballDirection, Color.red);
            vectors.Draw(plane.position, plane.position+planeNormal, Color.green);
            
            vectors.Draw(ballPosition, impact, Color.magenta);
            vectors.Draw(impact, impact-aVec, Color.yellow);
            vectors.Draw(impact-aVec, impact-aVec-aVec, Color.yellow);
            vectors.Draw(impact-aVec-aVec, impact-aVec-aVec+bVec, Color.yellow);
            vectors.Draw(impact, newBallPosition, Color.magenta);
            
            vectors.Draw(ballPosition+aDir*ballRadius, ballPosition+aVec, Color.white);
            vectors.Draw(ballPosition, ballPosition+aDir*ballRadius, Color.white);
            //vectors.Draw(planeDistance, ballPosition+planeDistance+aDir*ballRadius, Color.white);
        }

    }

    private void CalculateBounce()
    {
        //              ( ) ball
        //              /\
        //---___     a /  \  b
        //      ---___/    \
        //            ---___\
        //                  ---___plane
        
        // ||a|| = ballPosition*nHat-ballRadius-planeDistance
        aLength =
            ballPosition.x * planeNormal.x +
            ballPosition.y * planeNormal.y +
            ballPosition.z * planeNormal.z    // dot product
            - ballRadius - planeDistance;
        aDir = -planeNormal;
        aVec = aLength * aDir;

        //  r / a = b / ballDirection.magnitude*r =>
        //  b = ballDirection.magnitude*(r^2) / a
        bDir = ballDirection;
        //Vector3 bDir = ballDirection * ballRadius * ballRadius / a;
        bLength = aLength*aLength /
                        (aVec.x * ballDirection.x +
                        aVec.y * ballDirection.y +
                        aVec.z * ballDirection.z);    // dot product
        bVec = bDir * bLength;

        impact = ballPosition + bVec;
        newBallPosition = impact - aVec - aVec + bVec; 
    }
}


// Draw Vectors
[CustomEditor(typeof(Collision))]
public class ExampleGUI : Editor {
    void OnSceneGUI() {
        var ex = target as Collision;
        if (ex == null) return;

        EditorGUI.BeginChangeCheck();
        var a = Handles.PositionHandle(ex.ballDirection, Quaternion.identity);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Vector Positions");
            ex.ballDirection = a;
            EditorUtility.SetDirty(target);
        }
    }
}
