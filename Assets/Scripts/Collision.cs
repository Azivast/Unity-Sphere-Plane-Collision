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
    // User Input Variables
    public Vector3 ballTravelDirection = new Vector3(1, -1, 0);
    public Vector3 ballPosition;
    public float ballRadius = 1;
    
    [Tooltip("Plane rotation in Euler degrees")]
    public Vector3 PlaneRotation;
    [Tooltip("Distance from plane normal to world origin")]
    public float planeDistance;
    
    // Internal Variables
    [NonSerialized] 
    private VectorRenderer vectors;
    
    private Transform ball;
    private Transform plane;
    private Transform ballImpact;
    private Transform ballFinal;
    

    private Vector3 newBallPosition;
    private Vector3 impact;
    private Vector3 aVec;
    private Vector3 bVec;


    // Fetch children
    void Start()
    {
        ball = GameObject.Find("Ball (Initial)").transform;
        plane = GameObject.Find("Plane").transform;
        ballImpact = GameObject.Find("Ball (Impact)").transform;
        ballFinal = GameObject.Find("Ball (Final)").transform;
    }
    
    void OnEnable() {
        vectors = GetComponent<VectorRenderer>();
    }

    private void UpdateFromUserInput()
    {
        plane.transform.rotation = Quaternion.Euler(PlaneRotation.x, PlaneRotation.y, PlaneRotation.z);
        ball.position = ballPosition;
        plane.position = plane.up * planeDistance;
        ballTravelDirection.Normalize();
        
        Vector3 ballScale = new Vector3(ballRadius*2f, ballRadius*2f, ballRadius*2f);
        ball.localScale = ballScale;
        ballImpact.localScale = ballScale;
        ballFinal.localScale = ballScale;
    }

    private void IllustrateModel()
    {
        // Illustrate ball positions
        ballImpact.position = impact;
        ballFinal.position = newBallPosition;
        
        // Illustrate vectors
        using (vectors.Begin())
        {
            vectors.Draw(ballPosition, ballPosition+ballTravelDirection, Color.red);
            vectors.Draw(plane.position, plane.position+plane.up, Color.green);
            
            vectors.Draw(ballPosition, impact, Color.magenta);
            vectors.Draw(impact, impact-aVec, Color.yellow);
            vectors.Draw(impact-aVec, impact-aVec-aVec, Color.yellow);
            vectors.Draw(impact-aVec-aVec, impact-aVec-aVec+bVec, Color.yellow);
            vectors.Draw(impact, newBallPosition, Color.magenta);
            
            vectors.Draw(ballPosition, ballPosition+aVec, Color.black);
            vectors.Draw(ballPosition, ballPosition+aVec.normalized*ballRadius, Color.white);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFromUserInput();
        CalculateBounce();
        IllustrateModel();
    }

    private void CalculateBounce()
    {
        //              ( ) ball
        //              /\
        //---___     a /  \  b
        //      ---___/    \
        //            ---___\
        //                  ---___plane
        
        // Calculate a
        // ||a|| = ballPosition*planeNormal-ballRadius-planeDistance
        float aLength =
            (ballPosition.x * plane.up.x +
            ballPosition.y * plane.up.y +
            ballPosition.z * plane.up.z)    // dot product
            - planeDistance
            - ballRadius;
        Vector3 aDir = -plane.up;
        aVec = aDir*aLength;

        // Calculate b
        //  ballRadius / a = b / ballDirection.magnitude*ballRadius =>
        //  b = ballDirection.magnitude*(ballRadius^2) / a
        Vector3 bDir = ballTravelDirection;
        //Vector3 bDir = ballDirection * ballRadius * ballRadius / a;
        float bLength = aLength*aLength /
                        (aVec.x * ballTravelDirection.x +
                        aVec.y * ballTravelDirection.y +
                        aVec.z * ballTravelDirection.z);    // dot product
        bVec = bDir * bLength;

        impact = ballPosition + bVec;
        newBallPosition = impact - aVec - aVec + bVec; 
    }
}
