using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Stores a variety of ways to move between tiles. Called upon to handle the visuals of movement, doesn't actually know anything about the grid
/// </summary>
public class GridMovementGeneral : MonoBehaviour
{
    public float timeForLerps; //lerps use this as their timeframe
    static public GridMovementGeneral instance;
    public void Start()
    {
        instance = this;
    }
    public enum MovementTypes //this is so I can add smoove movement later, but for now I just want instant. 
    {
        Instant,
        Smooth,
        InstantRotateSmoothMove,
        SmoothTunnel //smooth and it overwrites 
    };

    /// <summary>
    /// determines which type of motion to move the object with
    /// </summary>
    public static void MovementByType(GameObject thingToMove, Vector3 positionToMoveTo, Quaternion rotationToRotateTo, MovementTypes type)
    {
        
        if (type == MovementTypes.Instant)
        {
            InstantMovement(thingToMove, positionToMoveTo, rotationToRotateTo);
        }
        else if (type == MovementTypes.InstantRotateSmoothMove)
        {
            InstantRotateSmoothMove(thingToMove, positionToMoveTo, rotationToRotateTo);
        }
        else if(type == MovementTypes.Smooth)
        {
            SmoothMovement(thingToMove, positionToMoveTo, rotationToRotateTo);
        }
        else if(type == MovementTypes.SmoothTunnel)
        {
            SmoothTunnel(thingToMove, positionToMoveTo, rotationToRotateTo);
        }
    }

    /// <summary>
    /// instantly updates the position and rotation of a gameobject
    /// </summary>
    /// <param name="thingToMove"></param>
    /// <param name="positionToMoveTo"></param>
    /// <param name="RotationToRotateTo"></param>
    static void InstantMovement(GameObject thingToMove, Vector3 positionToMoveTo, Quaternion RotationToRotateTo)
    {
        //this one is pretty easy
        thingToMove.transform.position = positionToMoveTo;
        thingToMove.transform.rotation = RotationToRotateTo;
        GridManagement.SetObjectToPosition(thingToMove, positionToMoveTo, false);
    }
    /// <summary>
    /// linearly updates position and rotation of a gameobject
    /// </summary>
    /// <param name="thingToMove"></param>
    /// <param name="positionToMoveTo"></param>
    /// <param name="RotationToRotateTo"></param>
    static void SmoothMovement(GameObject thingToMove, Vector3 positionToMoveTo, Quaternion RotationToRotateTo)
    {

        if (!GridManagement.SetObjectToPosition(thingToMove, positionToMoveTo, false)) //if theres something there dont move
        {
            instance.StartCoroutine(PositionLerp(thingToMove, positionToMoveTo));
            instance.StartCoroutine(RotationLerp(thingToMove, RotationToRotateTo));
        }
    }
    /// <summary>
    /// same as smooth movement but with overwriting turned on, used for worm retracting
    /// </summary>
    /// <param name="thingToMove"></param>
    /// <param name="positionToMoveTo"></param>
    /// <param name="RotationToRotateTo"></param>
    static void SmoothTunnel(GameObject thingToMove, Vector3 positionToMoveTo, Quaternion RotationToRotateTo)
    {
        instance.StartCoroutine(PositionLerp(thingToMove, positionToMoveTo));
        instance.StartCoroutine(RotationLerp(thingToMove, RotationToRotateTo));
        GridManagement.SetObjectToPosition(thingToMove, positionToMoveTo, false,true);
    }
    static void InstantRotateSmoothMove(GameObject thingToMove, Vector3 positionToMoveTo, Quaternion rotationToRotateTo)
    {

        instance.StartCoroutine(PositionLerp(thingToMove, positionToMoveTo));
        thingToMove.transform.rotation = rotationToRotateTo;
        GridManagement.SetObjectToPosition(thingToMove, positionToMoveTo, false);
    }
    /// <summary>
    /// moves thingtomove to positiontomoveto over a duration of timetomove seconds
    /// </summary>
    /// <param name="thingToMove">the object the movement is applied to</param>
    /// <param name="positionToMoveTo">the position to move it to</param>
    /// <param name="timeToMove">how long to take in seconds</param>
    /// <returns></returns>
    static IEnumerator PositionLerp(GameObject thingToMove, Vector3 positionToMoveTo)
    {

        float currentTime = 0;

        Vector3 originalPosish = thingToMove.transform.position;
        while (true)
        {
            currentTime += Time.deltaTime;
            thingToMove.transform.position = Vector3.Lerp(originalPosish, positionToMoveTo, currentTime / instance.timeForLerps);
            if (currentTime / instance.timeForLerps <= 1f)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                //snap it at the end to it's grid pos
                thingToMove.transform.position = GridManagement.GetPositionOfObject(thingToMove);
                //print("finished Rotation");
                break;
            }
        }
    }

    /// <summary>
    /// lerp thingtomove to rotationtomoveto over the public lerptime seconds
    /// </summary>
    /// <param name="thingToMove"></param>
    /// <param name="rotationToMoveTo"></param>
    /// <returns></returns>
    static IEnumerator RotationLerp(GameObject thingToMove,Quaternion rotationToMoveTo)
    {

        float currentTime = 0;

        Quaternion originalRotation = thingToMove.transform.rotation;
        while (true)
        {
            currentTime += Time.deltaTime;
            thingToMove.transform.rotation = Quaternion.Lerp(originalRotation, rotationToMoveTo, currentTime / instance.timeForLerps);
            if (currentTime / instance.timeForLerps <= 1f)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                //print("finished Rotation");
                break;
            }
        }
    }

    static public void StopCoroutines(GameObject ObjectStopping)
    {
      
    }
}

