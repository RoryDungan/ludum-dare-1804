using Assets.Code;
using RSG;
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

    [SerializeField]
    private RenderTexture photoTexture;

    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private RawImage photoReviewImage;

    private GameObject mainCamera;

    private Assets.Code.PromiseTimer promiseTimer;

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
        Assert.IsNotNull(viewfinderCamera, "Viewfinder Camera not assigned to PhotoCamera");
        Assert.IsNotNull(photoCamera, "Photo Camera not assigned to PhotoCamera");
        Assert.IsNotNull(photoTexture, "Photo Texture not assigned to PhotoCamera");
        Assert.IsNotNull(fadeImage, "Fade Image not assigned to PhotoCamera");
        Assert.IsNotNull(photoReviewImage, "Photo Review Image not assigned to PhotoCamera");

        mainCamera = Camera.main.gameObject;

        promiseTimer = Assets.Code.PromiseTimer.Instance;
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
        takingPhoto = true;

        FadeOut()
            .Then(() => CaptureImage())
            .Then(() => 
            {

                photoReviewImage.texture = photoTexture;
                photoReviewImage.gameObject.SetActive(true);
            })
            .Done();
    }

    private IPromise FadeOut()
    {
        return promiseTimer.WaitUntil(t =>
        {
            var newColour = fadeImage.color;
            newColour.a = Mathf.Min(1f, (t.elapsedTime / fadeDuration));
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

        return promiseTimer.WaitUntil(t => t.elapsedUpdates > 0)
            .Then(() => photoCamera.gameObject.SetActive(false));
    }
}
