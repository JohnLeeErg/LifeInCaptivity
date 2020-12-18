using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    Vector3 dir;
    bool moving;
    [SerializeField] bool player1; //only player 1 can do zoomb and automation
    [SerializeField] float inputThresh;
    [SerializeField] float maxZoom, minZoom, zoomSpeed;
    [SerializeField] bool partOfTheSystem, canPush,canActuallyReset=true;
    [SerializeField] string hInput, vInput;
    [Header("end transition")]
    [SerializeField] GameObject plane;
    float transitionTime=1, transitionRotateSpeed=360;
    Camera[] camComps = new Camera[2];
    Cell cellRef;
    int thisScene;
    public static PlayerMovement instance; //thats right this ho a singleton now
    SpriteRenderer spriteComp;
    Text iterationText,parText;
    int iterations;
    bool skipped;
    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        if (partOfTheSystem)
        {
            gameObject.tag = "Pushable";
            gameObject.AddComponent<Cell>();
            cellRef = GetComponent<Cell>();
        }
        camComps[0] = GameObject.Find("Main Camera").GetComponent<Camera>();
        camComps[1] = GameObject.Find("No bloom layer").GetComponent<Camera>();
        spriteComp = GetComponent<SpriteRenderer>();
        iterationText = GetComponentsInChildren<Text>()[0];
        parText = GetComponentsInChildren<Text>()[1];
        thisScene = gameObject.scene.buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (player1)
        {
            if (Input.GetButtonDown("Reset") && canActuallyReset)//some levels used to use reset to progress 
            {
                AudioManager.instance.RunItBack();
                GridManagement.ResetScene();
            } else if (Input.GetKeyDown(KeyCode.N))
            {
                skipped = true;
                StartCoroutine(StartEndSequence());
            }
            foreach (Camera camComp in camComps)
            {
                if (Input.GetAxis("Scroll") < 0)
                {

                    camComp.orthographicSize = Mathf.Clamp(camComp.orthographicSize + zoomSpeed, minZoom, maxZoom);

                }
                else if (Input.GetAxis("Scroll") > 0)
                {
                    camComp.orthographicSize = Mathf.Clamp(camComp.orthographicSize - zoomSpeed, minZoom, maxZoom);

                }
            }
        }
        if (player1 && !moving && Input.GetButtonDown("Fire"))
        {
            iterations++;
            iterationText.text = "steps: " + iterations;
            Iterate();
        }
        else if(!GridManagement.playerDead) //if the player is dead you can still scroll and iterate but you can't win or move
        {
            if (Input.GetAxis(hInput) < -inputThresh)
            {
                dir = -transform.right;
            }
            else if (Input.GetAxis(hInput) > inputThresh)
            {
                dir = transform.right;
            }
            else if (Input.GetAxis(vInput) > inputThresh)
            {
                dir = transform.up;
            }
            else if (Input.GetAxis(vInput) < -inputThresh)
            {
                dir = -transform.up;
            }
            else
            {
                return;
            }
            if (!moving)
            {
                GameObject thingInTheWay = GridManagement.getObjectFromPosition(transform.position + dir);
                if (thingInTheWay)
                {
                    if (canPush)
                    {
                        if (thingInTheWay.tag == "Pushable")
                        {
                            Move(thingInTheWay);
                            //update the grid to account for the push
                            Invoke("UpdateCellCounts", GridMovementGeneral.instance.timeForLerps + .1f);
                        }
                    }
                }

                moving = true;
                Move(gameObject);
                Invoke("ResetMovin", GridMovementGeneral.instance.timeForLerps + .1f);
            }
        }
    }
    /// <summary>
    /// updates the game of life step on all cells in the level
    /// </summary>
    void Iterate()
    {
        //every cell determines what it's next state should be...
        foreach (Cell cell in Cell.allCells)
        {
            cell.UpdateNeighbourCounts();

        }

        //...and then they all update
        foreach (Cell cell in Cell.allCells)
        {
            cell.Tick();
        }

        //update buttons
        GridDictionary.GridUpdateEvent.Invoke();

        if (tag == "Pushable" && GridManagement.playerDead)
        {
            //if you died from the tick, reset
            //GridManagement.ResetScene();
            cellRef.spriteComp.color = Color.white;
            cellRef.spriteComp.sortingOrder = 5;
            cellRef.alive = false;
            //and probably do some cool audiovisuals
        }
        else if (cellRef.spriteComp.color == Color.white && !GridManagement.playerDead)
        {
            //if you undied and undeath is allowed turn back normal colored
            cellRef.spriteComp.color = Color.yellow;
            cellRef.spriteComp.sortingOrder = 2;
            cellRef.alive = true;
        }

        //do the update at the end of the thing
        foreach (Cell cell in Cell.allCells)
        {
            cell.UpdateNeighbourCounts();

        }
    }
    void UpdateCellCounts()
    {
        foreach (Cell cell in Cell.allCells)
        {
            cell.UpdateNeighbourCounts();

        }

    }
    void Move(GameObject thing)
    {
        GridMovementGeneral.MovementByType(thing, thing.transform.position + dir, Quaternion.identity, GridMovementGeneral.MovementTypes.Smooth);

    }
    void ResetMovin()
    {

        GridDictionary.GridUpdateEvent.Invoke(); //wait till here to update buttons and such
        moving = false;
    }
    private void OnDestroy()
    {

    }
    public IEnumerator StartEndSequence()
    {
        if (skipped)
        {
            iterationText.color = Color.red;
            parText.color = Color.red;
            iterationText.gameObject.layer = LayerMask.NameToLayer("Default");
            parText.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            int par;
            int.TryParse(parText.text.Split(' ')[1], out par);
            print(par + " " + iterations);
            if (iterations < par)
            {
                iterationText.color = Color.yellow;
                parText.color = Color.yellow;
                iterationText.gameObject.layer = LayerMask.NameToLayer("Default");
                parText.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
        GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(1);
        camComps[0].clearFlags = CameraClearFlags.Nothing;
        camComps[1].enabled = false;
        plane.layer = LayerMask.NameToLayer("Default");
        Vector3 randomAxis = new Vector3(Random.Range(-1,1), Random.Range(-1, 1), Random.Range(-1, 1));
        Color randomCol = Random.ColorHSV();
        AudioManager.instance.Skip(transitionTime);
        for (float i = 0; i < transitionTime; i+=Time.deltaTime)
        {
            spriteComp.color =Color.Lerp(spriteComp.color, randomCol,i/transitionTime);
            camComps[0].transform.Rotate(randomAxis, transitionRotateSpeed * Time.deltaTime);
            //camComps[0].orthographicSize += 1 * Time.deltaTime;
            camComps[0].transform.position += Vector3.one * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        if (SceneManager.sceneCountInBuildSettings > SceneManager.GetActiveScene().buildIndex + 1)
        {
            //this really needs delay and effects
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadSceneAsync(0);
        }
    }
}
