using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PhotoCamera : MonoBehaviour
{
    ////////////////////////////////////////
    // Cached references to other objects
    ////////////////////////////////////////
    [SerializeField]
    private GameObject viewfinderCamera;

    [SerializeField]
    private Camera photoCamera;

    [SerializeField]
    private RenderTexture photoTexture;

    private GameObject mainCamera;

    ////////////////////////////////////////
    // State
    ////////////////////////////////////////
    private bool photoMode;

    private void Awake()
    {
        Assert.IsNotNull(viewfinderCamera, "Viewfinder Camera not assigned to PhotoCamera");
        Assert.IsNotNull(photoCamera, "Photo Camera not assigned to PhotoCamera");
        Assert.IsNotNull(photoTexture, "Photo Texture not assigned to PhotoCamera");

        mainCamera = Camera.main.gameObject;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TogglePhotoMode();
        }

        if (photoMode && Input.GetKeyDown(KeyCode.E))
        {
            TakePhoto();
        }
    }

    private void TogglePhotoMode()
    {
        photoMode = !photoMode;

        mainCamera.SetActive(!photoMode);
        viewfinderCamera.SetActive(photoMode);
    }

    private void TakePhoto()
    {
    }
}
