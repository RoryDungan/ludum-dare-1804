using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Code
{
    /// <summary>
    /// Moves this object along a spline.
    /// </summary>
    class SimpleSplineFollower : MonoBehaviour
    {
        [SerializeField]
        private Spline spline;

        [SerializeField]
        private float velocity = 1f;

        private float currentProgress;

        void Awake()
        {
            Assert.IsNotNull(spline);
        }

        void Update()
        {
            transform.position = spline.GetPoint(currentProgress);
            transform.rotation = Quaternion.LookRotation(spline.GetDirection(currentProgress));

            currentProgress += (velocity / spline.GetDeriv(currentProgress).magnitude)
                * Time.deltaTime;
        }
    }
}
