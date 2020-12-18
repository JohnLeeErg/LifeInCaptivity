using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {
    public bool filled=false;
    SpriteRenderer spriteComp;
    bool hasntDoneInitialCheck=true;
	// Use this for initialization
	void Start () {
        spriteComp = GetComponent<SpriteRenderer>();
        GridUpdate();//so they blue turn 1
	}
    public void GridUpdate()
    {
        GameObject CellAtPos = GridManagement.getObjectFromPosition(transform.position, true);
        if (CellAtPos&&CellAtPos.GetComponent<Cell>().alive)
        {
            filled = true;
            spriteComp.color = Color.blue;
            //print("filled");
        }
        else
        {
            filled = false;
            spriteComp.color = Color.red;
        }
    }	
	// Update is called once per frame
	void Update () {
        if (hasntDoneInitialCheck)
        {
            GridUpdate();
            hasntDoneInitialCheck = false;
        }
	}
    private void OnDestroy()
    {

        //GridDictionary.GridUpdateEvent.RemoveListener(GridUpdate);
    }
}
