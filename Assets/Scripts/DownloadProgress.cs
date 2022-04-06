using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadProgress : MonoBehaviour
{
    public int downloadProgressInput;
    private int cachedDownloadProgressInput;
    public int downloatProgressOutput;
    void Start()
    {
        downloatProgressOutput = 0;
    }

    void Update()
    {
        if(cachedDownloadProgressInput != downloadProgressInput)
        {
            downloatProgressOutput = downloadProgressInput;
            cachedDownloadProgressInput = downloadProgressInput;
        }
    }
}
