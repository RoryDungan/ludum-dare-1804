using Assets.Code;
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

    private void Awake()
    {
        aeroplaneController = FindObjectOfType<AeroplaneController>();
        Assert.IsNotNull(aeroplaneController);

        aeroplaneController.gameObject.SetActive(false);

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

        mainCamera = Camera.main;

        promiseTimer = PromiseTimer.Instance;
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
}
