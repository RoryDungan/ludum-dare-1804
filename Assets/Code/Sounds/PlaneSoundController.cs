using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

public class PlaneSoundController : MonoBehaviour
{
    private AeroplaneController aeroplane;
    private StudioEventEmitter jetSoundEmitter;

    private const string throttleParameterName = "Speed";

    void Awake()
    {
        aeroplane = GetComponent<AeroplaneController>();
        Assert.IsNotNull(aeroplane, "PlantSoundController must be placed on an object with an AeroplaneController.");

        var emitters = GetComponents<StudioEventEmitter>();

        jetSoundEmitter = emitters.FirstOrDefault(e => e.Event == "event:/jet engine 1");
        Assert.IsNotNull(aeroplane, "PlantSoundController must be placed on an object with a sound emitter.");
    }

    void Update()
    {
        jetSoundEmitter.SetParameter(throttleParameterName, aeroplane.Throttle);
    }
}
