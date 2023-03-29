using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vectors;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways] //Run in Edit Mode 
[RequireComponent(typeof(VectorRenderer))]
public class Collision : MonoBehaviour
{
    // Input variables
    public Vector3 ballDirection = new Vector3(3, 0, 0);
    public float ballRadius;
    public Vector3 ballPosition = new Vector3(10, 10 ,3);

    public Vector3 planeNormal = new Vector3(0.3f, 1, 0);
    public float planeDistance = 1;
    
    // Other Variables
    [NonSerialized] 
    private VectorRenderer vectors;

    public Vector3 newBallPosition;
    public Vector3 impact;


    // Start is called before the first frame update
    void Start()
    {
        ballDirection.Normalize();
    }
    
    void OnEnable() {
        vectors = GetComponent<VectorRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateBounce();
        using (vectors.Begin())
        {
            vectors.Draw(ballPosition, ballDirection, Color.red);
            vectors.Draw(Vector3.zero, planeNormal, Color.green);
            
            vectors.Draw(ballPosition, impact, Color.magenta);
            vectors.Draw(impact, newBallPosition, Color.yellow);
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
        
        // â = -planeNormal
        Vector3 aHat = -planeNormal;
        // ||a|| = ballPosition*nHat -planeDistance -ballRadius
        float a = ballPosition.x * planeNormal.x + 
                  ballPosition.y * planeNormal.y +
                  ballPosition.z * planeNormal.z    // dot product
                  
                  - ballRadius
                  - planeDistance;
        
        //  r / a = b / ballDirection.magnitude*r =>
        //  b = ballDirection.magnitude*(r^2) / a
        float b = ballDirection.magnitude * ballRadius * ballRadius / a;
        
        // Move ball
        Vector3 bVec = b * ballDirection;
        Vector3 aVec = a * aHat;

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
        var a = Handles.PositionHandle(ex.ballPosition, Quaternion.identity);
        var b = Handles.PositionHandle(ex.impact, Quaternion.identity);
        var c = Handles.PositionHandle(ex.newBallPosition, Quaternion.identity);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Vector Positions");
            ex.ballPosition = a;
            ex.impact = b;
            ex.newBallPosition = c;
            EditorUtility.SetDirty(target);
        }
    }
}
