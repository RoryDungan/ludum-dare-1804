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
    private GameObject viewfinderCamera;
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
    private PhotoManager photoManager;

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

    private void Awake()
    {
        var children = GetComponentsInChildren<Transform>(true);

        var viewfinderCameraTransform = children.FirstOrDefault(t => t.name == "ViewfinderCamera"); 
        Assert.IsNotNull(viewfinderCameraTransform, "Could not find object named 'ViewfinderCamera' in children");
        viewfinderCamera = viewfinderCameraTransform.gameObject;

        var photoCameraTransform = children.FirstOrDefault(t => t.name == "PhotoCamera");
        Assert.IsNotNull(photoCameraTransform, "Could not find object named 'PhotoCamera' in children");
        photoCamera = photoCameraTransform.GetComponent<Camera>();
        Assert.IsNotNull(photoCamera, "Could not find Camera component on 'PhotoCamera'");

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
        photoManager = PhotoManager.Instance;

        Assert.IsNotNull(photoRenderTexture, "No PhotoRenderTexture assigned to PhotoCamera");
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
            Time.timeScale = 1.0f;
            takingPhoto = false;
        }
    }

    private void TakePhoto()
    {
        takingPhoto = true;

        // Pause the game
        Time.timeScale = 0f;

        viewfinderScreen.SetActive(false);
        photoReviewScreen.SetActive(true);

        fadeImage.color = Color.white;

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

        promiseTimer.DoOnEndOfFrame(() => photoManager.SavePhoto(photoRenderTexture))
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
