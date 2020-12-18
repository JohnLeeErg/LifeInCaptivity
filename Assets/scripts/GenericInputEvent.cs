using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericInputEvent : MonoBehaviour {
    [SerializeField] string input;
    [SerializeField] UnityEvent OnInputDown;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown(input))
        {
            OnInputDown.Invoke();
        }
	}
}
