using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RewardButtons : MonoBehaviour
{
    [Header("Sahnedeki Butonlar")]
    public List<GameObject> buttons;

    [Header("Süreler")]
    public float interval = 5f;
    public float showDuration = 5f;
    public float fadeDuration = 0.3f;

    GameObject currentButton;
    Coroutine routine;
    public static RewardButtons Instance;

    void Start()
    {
        Instance = this;
        foreach (var btn in buttons)
            btn.SetActive(false);

        routine = StartCoroutine(ButtonRoutine());
    }

    IEnumerator ButtonRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            currentButton = buttons[Random.Range(0, buttons.Count)];
            currentButton.SetActive(true);

            CanvasGroup cg = currentButton.GetComponent<CanvasGroup>();
            if (cg != null)
                yield return StartCoroutine(Fade(cg, 0, 1));

            yield return new WaitForSeconds(showDuration);

            HideCurrent();
        }
    }

    public void HideCurrent()
    {
        if (currentButton == null) return;

        CanvasGroup cg = currentButton.GetComponent<CanvasGroup>();
        if (cg != null)
            StartCoroutine(FadeAndDisable(currentButton, cg));
        else
            currentButton.SetActive(false);

        currentButton = null;
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to)
    {
        cg.alpha = from;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        cg.alpha = to;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    IEnumerator FadeAndDisable(GameObject obj, CanvasGroup cg)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 0;
        obj.SetActive(false);
    }
}
