using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public bool AlignOnStart;
    public bool overWriteOnStart; //whether or not to delete whatever else when you are spawned
    public bool solid=true;
    // Use this for initialization
    void Start()
    {
        if (solid)
        {
            GameObject alreadyThere;
            //put the object in the grid at startup
            alreadyThere = GridManagement.getObjectFromPosition(transform.position);
            //if (alreadyThere!=null && alreadyThere.GetComponent<WormMovement>() != null)
            //{
            //    //if the other object has the capability to retreat, make it do that
            //    //alreadyThere.GetComponent<WormMovement>().Retract();
            //    GridManagement.SetObjectToPosition(gameObject, transform.position, true, false);
            //}
            //else
            //{

            GridManagement.SetObjectToPosition(gameObject, transform.position, true, overWriteOnStart);
            //}

            if (AlignOnStart)
            {
                transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            //nonsolids dont care they just always overwrite, since they never do normal movements
            GridManagement.SetObjectToPosition(gameObject, transform.position, true, true, false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    void GridUpdate()
    {

    }
    void OnDestroy()
    {
        //being paranoid again, make absolute sure you are removed from the grids before being destroyed
        //but this shouldn't really ever get called
        if (GridManagement.IsInGrid(gameObject))
        {
            GridManagement.ClearPosition(GridManagement.GetPositionOfObject(gameObject),solid);
        }


        GridMovementGeneral.StopCoroutines(gameObject); //and stop any movement coroutines that are on it
    }
}
