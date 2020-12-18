using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour {
    [SerializeField] bool fadeOnStart = true;
    [SerializeField] Color fadeColor,startColor;
    [SerializeField] float fadeTime;
    SpriteRenderer sprite;
	// Use this for initialization
	void Start () {

        sprite = GetComponent<SpriteRenderer>();
        sprite.color = startColor;
        if (fadeOnStart)
        {
            StartCoroutine(FadeToColor(sprite,fadeColor, fadeTime));
        }
	}
	public static IEnumerator FadeToColor(SpriteRenderer sprite, Color targetColor,float time)
    {
        float t=0;
        Color startCol = sprite.color;
        while (t < 1)
        {
            sprite.color = Color.Lerp(startCol, targetColor, t / time);
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
