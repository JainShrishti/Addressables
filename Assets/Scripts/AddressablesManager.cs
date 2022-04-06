using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Cinemachine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
{
    public AssetReferenceAudioClip(string guid) : base(guid) { }
}
public class AddressablesManager : MonoBehaviour
{
    [SerializeField]
    private AssetReference playerArmatureAssetReference;

    [SerializeField]
    private AssetReference musicAssetReference;

    [SerializeField]
    private AssetReferenceTexture2D unityLogoAssetReference;

    [SerializeField]
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    //UI Components
    [SerializeField]
    private RawImage rawImageUnityLogo;

    private GameObject playerController;
    private float progress;
    private List<string> catalogsToUpdate = new List<string>();
    private List<string> keys = new List<string>();

    public DownloadProgress downloadProgressScript;
    void Start()
    {
        keys.Add("Prefab");
        keys.Add("Material");
        keys.Add("Texture");
        keys.Add("Audio");
        StartCoroutine(UpdateCatalogs());
        Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
    }

    IEnumerator UpdateCatalogs()
    {
        catalogsToUpdate.Clear();
        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
        checkForUpdateHandle.Completed += op =>
        {
            catalogsToUpdate.AddRange(op.Result);
        };
        yield return checkForUpdateHandle;
        if (catalogsToUpdate.Count > 0)
        {
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return updateHandle;
        }

        StartCoroutine(CheckDownloadDependencies());
    }

    IEnumerator CheckDownloadDependencies()
    {
        foreach(string key in keys)
        {
            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(key);
            yield return getDownloadSize;

            //If the download size is greater than 0, download all the dependencies.
            if (getDownloadSize.Result > 0)
            {
                AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(key);
                yield return downloadDependencies;
            }
        }
    }

    private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        StartCoroutine(LoadPlayerAndAudio());
    }

    IEnumerator LoadPlayerAndAudio()
    {
        var playerAsset = playerArmatureAssetReference.LoadAssetAsync<GameObject>();
        playerAsset.Completed += (playerArmatureAsset) =>
        {
            playerArmatureAssetReference.InstantiateAsync().Completed += (playerArmatureGameObject) =>
            {
                playerController = playerArmatureGameObject.Result;
                cinemachineVirtualCamera.Follow = playerController.transform.Find("PlayerCameraRoot");
            };
        };
        var musicAsset = musicAssetReference.LoadAssetAsync<AudioClip>();
        musicAsset.Completed += (clip) =>
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip.Result;
            audioSource.playOnAwake = true;
            audioSource.loop = true;
            audioSource.Play();
        };
        var imageAsset = unityLogoAssetReference.LoadAssetAsync<Texture2D>();
        while (!playerAsset.IsDone || !musicAsset.IsDone || !imageAsset.IsDone)
        {
            var playerStatus = playerAsset.GetDownloadStatus();
            var musicStatus = musicAsset.GetDownloadStatus();
            var imageStatus = imageAsset.GetDownloadStatus();
            progress = (playerStatus.Percent + musicStatus.Percent + imageStatus.Percent) / 3;
            downloadProgressScript.downloadProgressInput = (int)(progress * 100);
            yield return null;
        }
        downloadProgressScript.downloadProgressInput = 100;
    }
   
    void Update()
    {
        if (unityLogoAssetReference.Asset != null && rawImageUnityLogo.texture == null)
        {
            rawImageUnityLogo.texture = unityLogoAssetReference.Asset as Texture2D;
            Color currentColor = rawImageUnityLogo.color;
            currentColor.a = 1.0f;
            rawImageUnityLogo.color = currentColor;
        }
    }
    private void OnDestroy()
    {
        playerArmatureAssetReference.ReleaseInstance(playerController);
        unityLogoAssetReference.ReleaseAsset();
        musicAssetReference.ReleaseAsset();
    }
}
