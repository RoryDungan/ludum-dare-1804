using RSG;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Code
{
    /// <summary>
    /// Singleton wrapper around the promise timer that automatically updates it every 
    /// frame.
    /// </summary>
    class PromiseTimer : UnitySingleton<PromiseTimer>
    {
        private IPromiseTimer timer;
        private IPromiseTimer unscaledTimer;

        private void Awake()
        {
            timer = new RSG.PromiseTimer();
            unscaledTimer = new RSG.PromiseTimer();

            Promise.UnhandledException += Promise_UnhandledException;
        }

        private void Promise_UnhandledException(object sender, ExceptionEventArgs e)
        {
            Debug.LogException(e.Exception);
        }

        new private void OnDestroy()
        {
            Promise.UnhandledException -= Promise_UnhandledException;

            base.OnDestroy();
        }

        private void Update()
        {
            timer.Update(Time.deltaTime);
            unscaledTimer.Update(Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Resolve the returned promise once the time has elapsed
        /// </summary>
        public IPromise WaitFor(float t)
        {
            return timer.WaitFor(t);
        }

        /// <summary>
        /// Resolve the returned promise once the predicate evalutes to true
        /// </summary>
        public IPromise WaitUntil(Func<TimeData, bool> p)
        {
            return timer.WaitUntil(p);
        }
        /// <summary>
        /// Resolve the returned promise once the predicate evaluates to false
        /// </summary>
        public IPromise WaitWhile(Func<TimeData, bool> p)
        {
            return timer.WaitWhile(p);
        }

        /// <summary>
        /// Resolve the returned promise once the time has elapsed
        /// </summary>
        public IPromise WaitForUnscaled(float t)
        {
            return unscaledTimer.WaitFor(t);
        }

        /// <summary>
        /// Resolve the returned promise once the predicate evalutes to true
        /// </summary>
        public IPromise WaitUntilUnscaled(Func<TimeData, bool> p)
        {
            return unscaledTimer.WaitUntil(p);
        }
        /// <summary>
        /// Resolve the returned promise once the predicate evaluates to false
        /// </summary>
        public IPromise WaitWhileUnscaled(Func<TimeData, bool> p)
        {
            return unscaledTimer.WaitWhile(p);
        }

        public IPromise DoOnEndOfFrame(Action a)
        {
            var promise = new Promise();
            StartCoroutine(WaitForEndOfFrame(a, promise));
            return promise;
        }

        private IEnumerator WaitForEndOfFrame(Action a, IPendingPromise p)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                a();
                p.Resolve();
            }
            catch (Exception ex)
            {
                p.Reject(ex);
            }
        }
    }
}
