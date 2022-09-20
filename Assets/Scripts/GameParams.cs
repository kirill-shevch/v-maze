using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameParams : MonoBehaviour
{
    public GameObject blackOutSquare;
    public TextMeshProUGUI endLabel;
    public int curLevel = 1;
    public int finalLevel = 1;
    private bool gameEnded = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(curLevel > finalLevel && !gameEnded) 
        {
            gameEnded = true;
            StartCoroutine(FadeBlackOutSquare());
        }
    }

    public IEnumerator FadeBlackOutSquare(int fadeSpeed = 5)
    {
        Color objectColor = blackOutSquare.GetComponent<Image>().color;
        Color labelColor = endLabel.color;

        float fadeAmount;

        while (blackOutSquare.GetComponent<Image>().color.a < 1)
        {
            fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackOutSquare.GetComponent<Image>().color = objectColor;

            labelColor = new Color(labelColor.r, labelColor.g, labelColor.b, fadeAmount);
            endLabel.color = labelColor;
        }

        yield return new WaitForSeconds(4f);
        Application.Quit();
    }
}
