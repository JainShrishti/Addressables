using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingSpinner : MonoBehaviour
{
    public GameObject joystickControls;
    public RectTransform rectTransform;
    public float speed;
    public TextMeshProUGUI displayPercent;
    private int percentComplete;
    public DownloadProgress downloadProgressScript;
    void Start()
    {
        percentComplete = 0;
    }
    private void OnEnable()
    {
        percentComplete = 0;
    }
    void Update()
    {
        if(percentComplete != downloadProgressScript.downloatProgressOutput)
        {
            rectTransform.Rotate(new Vector3(0, 0, -speed * Time.deltaTime));
            displayPercent.text = downloadProgressScript.downloatProgressOutput.ToString() + "%";
            percentComplete = downloadProgressScript.downloatProgressOutput;
        }
        else if(percentComplete == 100)
        {
            StartCoroutine(DeactivateGO()); 
        }
    }

    IEnumerator DeactivateGO()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        joystickControls.SetActive(true);
    }
}
