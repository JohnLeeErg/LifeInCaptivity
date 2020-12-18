using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonParent : MonoBehaviour {
    [SerializeField] bool canFinishDead;
    public UnityEvent OnSuccess = new UnityEvent();
    bool fulfilled,endTriggered;
    // Use this for initialization
    void Start() {
        GridDictionary.GridUpdateEvent.AddListener(GridUpdate);
    }
    void GridUpdate()
    {
        fulfilled = true; //assume true
        foreach (Button eachButton in GetComponentsInChildren<Button>())
        {
            eachButton.GridUpdate();
            if (!eachButton.filled)
            {
                fulfilled = false;
            }
        }
        if (fulfilled && !endTriggered)
        {
            //if you're still fulfilled, then you win
            OnSuccess.Invoke();
            endTriggered = true;
        }
    }
    // Update is called once per frame
    void Update() {

    }
    public void LoadNextScene()
    {
        if (!GridManagement.playerDead || canFinishDead)
        {
           StartCoroutine( PlayerMovement.instance.StartEndSequence());
        }
    }
    private void OnDestroy()
    {
        
        GridDictionary.GridUpdateEvent.RemoveListener(GridUpdate);
    }
}
