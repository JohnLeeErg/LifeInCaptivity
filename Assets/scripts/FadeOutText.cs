using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutText : MonoBehaviour {
    [SerializeField] bool fadeOnStart = true;
    [SerializeField] Color fadeColor,startColor;
    [SerializeField] float fadeTime,delayTime;
    TextMesh sprite;
	// Use this for initialization
	void Start () {

        sprite = GetComponent<TextMesh>();
        sprite.color = startColor;
        if (fadeOnStart)
        {
            StartCoroutine(FadeToColor(sprite,fadeColor, fadeTime));
        }
	}
	public IEnumerator FadeToColor(TextMesh sprite, Color targetColor,float time)
    {
        float t=0;
        Color startCol = sprite.color;
        yield return new WaitForSeconds(delayTime);
        while (t/time < 1)
        {
            sprite.color = Color.Lerp(startCol, targetColor, t / time);
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
        sprite.color = targetColor;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
