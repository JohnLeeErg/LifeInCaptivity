using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// an extension of dictionary with a unityevent built in. GridUpdateEvent is called whenever the grid is updated.
/// </summary>
class GridDictionary : Dictionary<Vector3, GameObject>
{
    public static UnityEvent GridUpdateEvent = new UnityEvent();
    new public void Add(Vector3 v3, GameObject gO)
    {
        //GridUpdateEvent.Invoke();
        base.Add(v3, gO);
    }
    //new public void Remove(Vector3 v3) //note that calling this on remove adds a lot lot lot of redundant calls to this but otherwise it doesn't get called on every possible update of the grid
    //{

    //    GridUpdateEvent.Invoke();
    //    base.Remove(v3);
    //}
}
/// <summary>
/// contains all basic, basic functionality of the grid. stores the positions of objects, other classes can get and set objects to positions.
/// it should also make sure that the positions are always exact, ie round them to ints
/// </summary>
public class GridManagement : MonoBehaviour
{
    public GameObject voidPrefab;
    private static GridDictionary TheGrid = new GridDictionary();
    //this is used to easily get the positions of objects
    private static Dictionary<GameObject, Vector3> InverseGrid = new Dictionary<GameObject, Vector3>();
    private static Dictionary<Vector3, GameObject> FloorGrid = new Dictionary<Vector3, GameObject>();

    //this is used to easily get the positions of objects
    private static Dictionary<GameObject, Vector3> InverseFloorGrid = new Dictionary<GameObject, Vector3>();
    
    public static Vector3 nullVector = Vector3.one / 2; //a vector that will never be one of the positions in the grid, to use as null
    public static GridManagement instance;

    int thisScene;
    public static bool playerDead = false;
    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            thisScene = gameObject.scene.buildIndex;
        }
        else
        {
            Destroy(gameObject);
        }
        playerDead = false;
    }
    public static void ResetScene()
    {
        Cell.allCells.Clear();
        SceneManager.LoadSceneAsync(instance.thisScene);
    }
    /// <summary>
    /// simple test to see if the object is in the grid
    /// </summary>
    /// <param name="objectToTest"></param>
    /// <returns></returns>
    public static bool IsInGrid(GameObject objectToTest)
    {
        if (TheGrid.ContainsValue(objectToTest))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static GameObject getObjectFromPosition(Vector3 position, bool solid = true)
    {
        ///start with a round
        position = RoundVectorToInts(position);
        if (solid)
        {
            if (TheGrid.ContainsKey(position))
            {
                return TheGrid[position];
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (FloorGrid.ContainsKey(position))
            {
                return FloorGrid[position];
            }
            else
            {
                return null;
            }
        }
    }

    public static Vector3 GetPositionOfObject(GameObject TheObject)
    {

        if (InverseGrid.ContainsKey(TheObject))
        {
            return InverseGrid[TheObject];
        }
        else
        {
            return nullVector; //return my fake vector equiv of null
        }
    }


    /// <summary>
    /// I just wanna make sure these vectors are always ints, im gonna call this a every time a vector comes in, just to be really careful.
    /// </summary>
    /// <param name="vectorToRound"></param>
    /// <returns></returns>
    public static Vector3 RoundVectorToInts(Vector3 vectorToRound)
    {
        return new Vector3(Mathf.RoundToInt(vectorToRound.x), Mathf.RoundToInt(vectorToRound.y), Mathf.RoundToInt(vectorToRound.z));
    }

    /// <summary>
    /// basic, no logic setting of a position. careful using this outside of setposition in grid
    /// </summary>
    /// <param name="objectToPut"></param>
    /// <param name="PlaceToPut"></param>
    static void FillPosition(GameObject objectToPut, Vector3 PlaceToPut)
    {
        ///start with a round
        PlaceToPut = RoundVectorToInts(PlaceToPut);


        InverseGrid.Add(objectToPut, PlaceToPut);
        TheGrid.Add(PlaceToPut, objectToPut);
        

        //print(TheGrid.Count + " " + InverseGrid.Count);
    }
    static void FillFloorPosition(GameObject obj, Vector3 plc)
    {
        ///start with a round
        plc = RoundVectorToInts(plc);

        //im bein lazy with the floor, just always clear it, always fill 
        if (FloorGrid.ContainsKey(plc))
        {
            GameObject dedBoi = FloorGrid[plc];
            InverseFloorGrid.Remove(obj);
            FloorGrid.Remove(plc);
            Destroy(dedBoi);
        }
        FloorGrid.Add(plc, obj);
        InverseFloorGrid.Add(obj, plc);
    }
    /// <summary>
    /// basic no logic emptying of a position
    /// </summary>
    /// <param name="positionToClear"></param>
    public static void ClearPosition(Vector3 positionToClear, bool solid = true, bool deleteShit = true)
    {
        ///start with a round
        positionToClear = RoundVectorToInts(positionToClear);
        if (solid)
        {
            if (TheGrid.ContainsKey(positionToClear))
            {

                //this should remove the position and the thing at that position from the grid
                InverseGrid.Remove(TheGrid[positionToClear]);
                TheGrid.Remove(positionToClear);

            }
        }
        else
        {
            if (FloorGrid.ContainsKey(positionToClear))
            {
                GameObject deadBoi = FloorGrid[positionToClear];
                InverseFloorGrid.Remove(FloorGrid[positionToClear]);
                FloorGrid.Remove(positionToClear);
                if (deleteShit)
                {
                    if (deadBoi)
                    {
                        if (deadBoi.name.Contains("Player"))
                        {
                            return;

                        }
                        else
                        {
                            Destroy(deadBoi, .2f);
                        }
                    }
                }
            }
        }
        //print(TheGrid.Count + " " + InverseGrid.Count); useful debug thing to make sure im handling the lists well
    }

    /// <summary>
    /// emptying of a position and deleting the object that was there
    /// </summary>
    /// <param name="objectToDelete"> the object you pass in is removed from the game</param>
    public static void DeleteObject(GameObject objectToDelete)
    {
        ClearPosition(InverseGrid[objectToDelete]);


        Destroy(objectToDelete, .2f);

    }

    /// <summary>
    /// set an object to a position in the grid. Returns an object that is in the way if overwrite is false. returns null when it all worked. 
    /// </summary>
    /// <param name="objectToSet"> this is the object you are moving to the position</param>
    /// <param name="newPosition"> this is said position. will be rounded to ints</param>
    /// <param name="overWrite"> false by default, if true it will destroy any object in that spot</param>
    /// <param name="handleMovement"> true by default, this lets setObjectToPosition instantly snap the object to the position. if false, that implies you would have to do the physical movement yourself</param>
    /// <returns></returns>
    public static GameObject SetObjectToPosition(GameObject objectToSet, Vector3 newPosition, bool handleMovement = true, bool overWrite = false, bool solid = true)
    {
        ///start with a round
        newPosition = RoundVectorToInts(newPosition);


        if (solid)
        {


            GameObject ObjectInNextPosition = getObjectFromPosition(newPosition);
            if (ObjectInNextPosition != null && ObjectInNextPosition.GetComponent<GridObject>().solid)
            {
                //if theres something in the way and overwrite is true:
                if (overWrite)
                {
                    if (ObjectInNextPosition.name.Contains("Player") || ObjectInNextPosition.tag == "Wall")
                    {

                        return ObjectInNextPosition;

                    }
                    //kill it
                    DeleteObject(ObjectInNextPosition);

                }
                else
                {
                    //dont kill it, return it
                    //print("there's something in the way!");
                    //delete this guy
                    //Destroy(objectToSet);
                    return ObjectInNextPosition;
                }
            }

            //after that, only get rid of old object if you are actually going to move
            Vector3 previousPosition = GetPositionOfObject(objectToSet);
            if (previousPosition != nullVector)
            {
                //if the object was somewhere else in the grid originally, clear those positions on the grids
                ClearPosition(previousPosition, true);



            }
            //write floor to the space you were previously
            ClearPosition(previousPosition, false);
            Instantiate(instance.voidPrefab, previousPosition, Quaternion.identity);

            //and remove space from the position you are going to
            ClearPosition(newPosition, false);

            //actual movement
            FillPosition(objectToSet, newPosition);

            if (handleMovement)
            {
                //do the most basic physical movement
                GridMovementGeneral.MovementByType(objectToSet, newPosition, objectToSet.transform.rotation, GridMovementGeneral.MovementTypes.Instant);
            }



            return null; //on a successful movement, return null I guess, peeps would probably wanna check if it worked
        }
        else
        {
            FillFloorPosition(objectToSet, newPosition);
            return null;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
