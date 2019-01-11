using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Log : MonoBehaviour {

    public CanvasGroup cg;
    public RectTransform rt;
    public TextMeshProUGUI text;
    public float life;
    public float fadeOutSpeed = .25f;

    private void Start() {
        StartCoroutine(LogRoutine());
    }

    IEnumerator LogRoutine() {
        yield return null;
        yield return new WaitForSecondsRealtime(life);

        while (cg.alpha > 0) {
            cg.alpha -= Time.unscaledDeltaTime * fadeOutSpeed;
            yield return null;
        }

        Destroy(gameObject);

    }



}
