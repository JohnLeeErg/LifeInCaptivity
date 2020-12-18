using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// store data about the surrounding 8 tiles for iterating cell automata
/// </summary>
public class Cell : MonoBehaviour
{
    int liveNeighbours;
    public bool alive;
    bool aliveNextTick;
    GridObject gridComp;
    [HideInInspector] public SpriteRenderer spriteComp;
    Vector3[] neighbourPositions;
    public static List<Cell> allCells =new List<Cell>();
    // Use this for initialization
    void Start()
    {
        gridComp = GetComponent<GridObject>();
        spriteComp = GetComponent<SpriteRenderer>();
        alive = gridComp.solid; //these correspond
        if (!allCells.Contains(this))
        {
            allCells.Add(this);
        }
        UpdateNeighbourCounts();
    }
    public void UpdateNeighbourCounts()
    {
        liveNeighbours = 0;

        neighbourPositions = new Vector3[] {transform.position - transform.up - transform.right, //bottomleft
            transform.position - transform.up, //bottommid
           transform.position - transform.up + transform.right, //bottomright
            transform.position - transform.right, //midleft
            transform.position + transform.right, //midright
            transform.position + transform.up - transform.right, //topleft
            transform.position + transform.up, //topmid
            transform.position + transform.up + transform.right, //topright
        };

        foreach(Vector3 neigb in neighbourPositions)
        {
            CheckForLife(neigb);
        }
        if (alive)
        {
            if (liveNeighbours < 2 || liveNeighbours > 3)
            {
                aliveNextTick = false;
            }
            else
            {
                aliveNextTick = true;
            }
        }
        else
        {
            if (liveNeighbours == 3)
            {
                aliveNextTick = true;
            }
            else
            {
                aliveNextTick = false;
            }
        }
    }
    void CheckForLife(Vector3 place)
    {
        GameObject thing = GridManagement.getObjectFromPosition(place);

        if (thing && thing.tag == "Pushable")
        {
            liveNeighbours++;
        }
        else if (!thing)
        {
            if (alive) //only generate new friends if you're alive
            {
                //check if there's a dead cell there, if not make one, so the grid can expand
                if (!GridManagement.getObjectFromPosition(place, false))
                {
                    Instantiate(GridManagement.instance.voidPrefab, place, Quaternion.identity);
                }
            }
        }
        
    }

    public void Tick()
    {
        if (alive)
        {
            if (!aliveNextTick)
            {
                //new dead one
                BecomeDead();
            }
        }
        else
        {
            if (aliveNextTick)
            {
                //new live one
                BecomeAlive();
            }
        }
    }

    void BecomeAlive()
    {
        if (name.Contains("Player"))
        {
            GridManagement.playerDead = false;
        }
        else
        {
            GridManagement.ClearPosition(transform.position, false, false); //get rid of what was there 

            if(GridManagement.SetObjectToPosition(gameObject, transform.position, true, true, true))
            {
                //if this returns an object, its the player in a mode where the player should not be killed, so lose the block instead
                //stay dead i guess it would be
                return;
            }
            //track the fact that you are now solid and alive
            gridComp.solid = true;
            alive = true;
            tag = "Pushable";
            name = "Live Cell";
            //visually become alive

            spriteComp.sortingOrder = 1;
            spriteComp.color = Color.green;
        }
    }

    void BecomeDead()
    {
        if (name.Contains("Player"))
        {
            GridManagement.playerDead = true;
        }
        else
        {
            GridManagement.ClearPosition(transform.position, true, false); //get rid of what was there

            GridManagement.SetObjectToPosition(gameObject, transform.position, true, true, false); //spawn a dead there
                                                                                                   //track that you are now dead
            gridComp.solid = false;
            alive = false;
            tag = "Untagged";

            name = "Dead Cell";
            //viz
            spriteComp.sortingOrder = 0;
            spriteComp.color = Color.clear;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnDestroy()
    {
        allCells.Remove(this);
    }
}
