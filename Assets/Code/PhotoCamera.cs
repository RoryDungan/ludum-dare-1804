﻿using Assets;
using RSG;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PhotoCamera : MonoBehaviour
{
    ////////////////////////////////////////
    // Cached references to other objects
    ////////////////////////////////////////
    [SerializeField]
    private GameObject viewfinderCamera;

    [SerializeField]
    private Camera photoCamera;

    private GameObject viewfinderScreen;
    private GameObject photoReviewScreen;
    private Image fadeImage;
    private Button keepButton;
    private Button deleteButton;

    [SerializeField]
    private RenderTexture photoRenderTexture;

    private GameObject mainCamera;

    private Assets.Code.PromiseTimer promiseTimer;
    private PhotoSaveManager photoManager;

    private GameManager gameManager;

    ////////////////////////////////////////
    // Configurable options
    ////////////////////////////////////////
    [SerializeField]
    private float fadeDuration = 0.3f;

    ////////////////////////////////////////
    // State
    ////////////////////////////////////////
    private bool photoMode;

    private bool takingPhoto;


    private const string ShutterClickSound = "event:/camera 2d";

    private void Awake()
    {
        var children = GetComponentsInChildren<Transform>(true);

        Assert.IsNotNull(viewfinderCamera, "No Viewfinder Camera assigned to PhotoCamera.");

        Assert.IsNotNull(photoCamera, "No PhotoCamera camera assigned to PhotoCamera script.");

        var viewfinderScreenTransform = children.FirstOrDefault(t => t.name == "ViewfinderScreen");
        Assert.IsNotNull(viewfinderScreenTransform, "Could not find object named 'ViewfinderScreen' in children");
        viewfinderScreen = viewfinderScreenTransform.gameObject;

        var photoReviewScreenTransform = children.FirstOrDefault(t => t.name == "ReviewScreen");
        Assert.IsNotNull(photoReviewScreenTransform, "Could not find object named 'ReviewScreen' in children");
        photoReviewScreen = photoReviewScreenTransform.gameObject;

        var fadeImageTransform = children.FirstOrDefault(t => t.name == "ReviewScreenFade");
        Assert.IsNotNull(fadeImageTransform, "Could not find object named 'ReviewScreenFade' in children");
        fadeImage = fadeImageTransform.GetComponent<Image>();
        Assert.IsNotNull(fadeImage, "Could not find Image component on 'ReviewScreenFade'");

        var keepButtonTransform = children.FirstOrDefault(t => t.name == "KeepButton");
        Assert.IsNotNull(keepButtonTransform, "Could not find object named 'KeepButton' in children");
        keepButton = keepButtonTransform.GetComponent<Button>();
        Assert.IsNotNull(keepButton, "Could not find Button component on 'KeepButton'");

        var deleteButtonTransform = children.FirstOrDefault(t => t.name == "DeleteButton");
        Assert.IsNotNull(deleteButtonTransform, "Could not find object named 'DeleteButton' in children");
        deleteButton = deleteButtonTransform.GetComponent<Button>();
        Assert.IsNotNull(deleteButton, "Could not find Button component on 'DeleteButton'");

        keepButton.onClick.AddListener(KeepPhotoClicked);
        deleteButton.onClick.AddListener(DeletePhotoClicked);

        mainCamera = Camera.main.gameObject;

        promiseTimer = Assets.Code.PromiseTimer.Instance;
        photoManager = PhotoSaveManager.Instance;

        Assert.IsNotNull(photoRenderTexture, "No PhotoRenderTexture assigned to PhotoCamera");

        gameManager = FindObjectOfType<GameManager>();
        Assert.IsNotNull(gameManager);
    }

    private void OnDestroy()
    {
        keepButton.onClick.RemoveListener(KeepPhotoClicked);
        deleteButton.onClick.RemoveListener(DeletePhotoClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TogglePhotoMode();
        }

        if (photoMode && !takingPhoto && Input.GetKeyDown(KeyCode.E))
        {
            TakePhoto();
        }
    }

    private void TogglePhotoMode()
    {
        photoMode = !photoMode;

        mainCamera.SetActive(!photoMode);
        viewfinderCamera.SetActive(photoMode);
        viewfinderScreen.SetActive(photoMode);
        if (!photoMode)
        {
            photoReviewScreen.SetActive(false);
            PauseManager.Instance.Unpause();
            takingPhoto = false;
        }
    }

    private void TakePhoto()
    {
        takingPhoto = true;

        PauseManager.Instance.Pause();

        viewfinderScreen.SetActive(false);
        photoReviewScreen.SetActive(true);

        fadeImage.color = Color.white;

        // Play sound
        FMODUnity.RuntimeManager.PlayOneShot(ShutterClickSound);

        CaptureImage()
            .Then(() => FadeFromWhite())
            .Done();
    }

    private IPromise FadeFromWhite()
    {
        return promiseTimer.WaitUntilUnscaled(t =>
        {
            var newColour = fadeImage.color;
            newColour.a = Mathf.Max(0f, 1f - (t.elapsedTime / fadeDuration));
            fadeImage.color = newColour;

            return t.elapsedTime >= fadeDuration;
        });
    }

    /// <summary>
    /// Updates the renderTexture with image from the photo camera.
    /// </summary>
    private IPromise CaptureImage()
    {
        photoCamera.gameObject.SetActive(true);

        return promiseTimer.WaitUntil(t => t.elapsedUpdates > 1)
            .Then(() => photoCamera.gameObject.SetActive(false));
    }

    private void KeepPhotoClicked()
    {
        keepButton.interactable = false;

        promiseTimer.DoOnEndOfFrame(() =>
            {
                var id = photoManager.SavePhoto(photoRenderTexture);
                gameManager.OnPhotoTaken(id);
            })
            .Finally(() =>
            {
                keepButton.interactable = true;

                if (photoMode)
                {
                    TogglePhotoMode();
                }
            })
            .Done();
    }

    private void DeletePhotoClicked()
    {
        if (photoMode)
        {
            TogglePhotoMode();
        }
    }
}
