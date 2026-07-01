using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Juul
{
    [BepInPlugin(guid, title, version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public const string guid = "Juul";
        public const string title = "Juul";
        public const string version = "4.0.0";
        private GameObject coreObject;

        void Awake()
        {
            Instance = this;
            try
            {
                var harmony = new Harmony(guid);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {

            }
            coreObject = new GameObject("JuulCore");
            UnityEngine.Object.DontDestroyOnLoad(coreObject);
            StartCoroutine(DelayedInitialize());

        }

        private void Update()
        {
        }

        private IEnumerator DelayedInitialize()
        {
            float waitTime = 0f;
            while (GorillaTagger.Instance == null && waitTime < 30f)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
            }

            if (GorillaTagger.Instance == null)
            {
                yield break;
            }

            try
            {
                Buttons.Initialize();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                }
                yield break;
            }
            try
            {
                coreObject.AddComponent<Core>();
            }
            catch (Exception e)
            {

                yield break;
            }
            try
            {
                Debug.Log("[Juul] Total Mods: " + ExtraButtons.CountAllButtons());
                Audios.Play("Loaded");

            }
            catch (Exception e)
            {
            }

        }

        void OnDestroy()
        {
            if (coreObject != null)
            {
                UnityEngine.Object.Destroy(coreObject);
            }
        }
    }
}
