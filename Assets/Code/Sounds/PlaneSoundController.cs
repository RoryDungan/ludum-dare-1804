using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityStandardAssets.Vehicles.Aeroplane;

public class PlaneSoundController : MonoBehaviour
{
    private AeroplaneController aeroplane;
    private StudioEventEmitter jetSoundEmitter;
    private StudioEventEmitter windSoundEmitter;

    [SerializeField]
    [Tooltip("Multiplier to convert the speed value in meters per second to 0-1")]
    private float speedMultiplier = 0.005f;

    private const string throttleParameterName = "Speed";
    private const string windSpeedParameterName = "Speed";

    private Vector3 previousPosition;

    private float speed;

    void Awake()
    {
        aeroplane = GetComponent<AeroplaneController>();
        Assert.IsNotNull(aeroplane, "PlantSoundController must be placed on an object with an AeroplaneController.");

        var emitters = GetComponents<StudioEventEmitter>();

        jetSoundEmitter = emitters.FirstOrDefault(e => e.Event == "event:/jet engine 1");
        Assert.IsNotNull(aeroplane, "PlantSoundController must be placed on an object with a 'jet engine 1' sound emitter.");

        windSoundEmitter = emitters.FirstOrDefault(e => e.Event == "event:/wind");
        Assert.IsNotNull(aeroplane, "PlantSoundController must be placed on an object with a 'wind' sound emitter.");

        previousPosition = transform.position;
    }

    void Update()
    {
        var normalisedSpeed = Mathf.Clamp01(speed * speedMultiplier); 

        jetSoundEmitter.SetParameter(throttleParameterName, aeroplane.Throttle);

        windSoundEmitter.SetParameter(windSpeedParameterName, normalisedSpeed);
    }

    private void FixedUpdate()
    {
        var newPosition = transform.position;
        speed = (previousPosition - newPosition).magnitude / Time.deltaTime;
        
        previousPosition = newPosition;
    }
}
