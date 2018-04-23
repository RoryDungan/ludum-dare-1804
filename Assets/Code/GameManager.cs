﻿using Assets.Code;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityStandardAssets.Vehicles.Aeroplane;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuUI;

    private Animation mainMenuAnim;

    [SerializeField]
    private GameObject inGameUI;

    private Animation gameUIAnim;

    [SerializeField]
    private GameObject cameraRig;

    private Transform cameraRigPivot;

    [SerializeField]
    private Transform cameraLookTarget;

    [SerializeField]
    private Spline introSpline;

    [SerializeField]
    private float introCinematicDuration = 3f;

    private GameObject dummyPlane;

    private Camera mainCamera;

    private AeroplaneController aeroplaneController;

    private PromiseTimer promiseTimer;


    /// <summary>
    /// The number of photos taken so far.
    /// </summary>
    public int PhotosTaken { get; private set; }

    /// <summary>
    /// The maximum number of photos.
    /// </summary>
    public int TotalPhotos { get; private set; }

    private IList<string> photoIds;

    private void Awake()
    {
        aeroplaneController = FindObjectOfType<AeroplaneController>();
        Assert.IsNotNull(aeroplaneController);

        dummyPlane = GameObject.Find("DummyPlane");
        Assert.IsNotNull(dummyPlane);

        Assert.IsNotNull(mainMenuUI);
        Assert.IsNotNull(inGameUI);
        Assert.IsNotNull(cameraRig);
        Assert.IsNotNull(cameraLookTarget);

        var cameraRigChildren = cameraRig.GetComponentsInChildren<Transform>(true);
        cameraRigPivot = cameraRigChildren.FirstOrDefault(t => t.gameObject.name == "Pivot");
        Assert.IsNotNull(cameraRigPivot);

        mainMenuAnim = mainMenuUI.GetComponent<Animation>();
        Assert.IsNotNull(mainMenuAnim);

        gameUIAnim = inGameUI.GetComponent<Animation>();
        Assert.IsNotNull(gameUIAnim);

        mainCamera = Camera.main;

        promiseTimer = PromiseTimer.Instance;
    }

    private void Start()
    {
        ResetGame();
    }

    private void ResetGame()
    {
        // Default to 9 photos
        TotalPhotos = 9;
        PhotosTaken = 0;

        inGameUI.SetActive(false);
        aeroplaneController.gameObject.SetActive(false);

        photoIds = new List<string>();
    }

    public void PlayClicked()
    {
        StartGame();
    }

    public void QuitClicked()
    {
        Application.Quit();
    }

    void StartGame()
    {
        mainMenuAnim.Play();

        promiseTimer.WaitUntil(t =>
        {
            var currentProgress = Mathf.Min(1f, t.elapsedTime / introCinematicDuration);

            mainCamera.transform.position = introSpline
                .GetPoint(Mathf.SmoothStep(0, 1, currentProgress));

            var pos = cameraLookTarget.position - mainCamera.transform.position;
            var newRot = Quaternion.LookRotation(pos);
            mainCamera.transform.rotation = 
                Quaternion.Lerp(mainCamera.transform.rotation, newRot, currentProgress);

            return t.elapsedTime >= introCinematicDuration;
        })
        .Then(() =>
        {
            mainMenuUI.SetActive(false);

            inGameUI.SetActive(true);
            gameUIAnim.Play();

            AttachCameraToRig();
            dummyPlane.SetActive(false);
            aeroplaneController.gameObject.SetActive(true);
        })
        .Done();
    }

    private void AttachCameraToRig()
    {
        mainCamera.transform.SetParent(cameraRigPivot);
        cameraRig.SetActive(true);
    }

    private void DetachCameraFromRig()
    {
        mainCamera.transform.SetParent(null);
        cameraRig.SetActive(false);
    }

    /// <summary>
    /// Called when a photo is taken.
    /// </summary>
    public void OnPhotoTaken(string photoId)
    {
        PhotosTaken++;

        photoIds.Add(photoId);

        if (PhotosTaken >= TotalPhotos)
        {
            // TODO: show end game screen.
        }
    }
}
