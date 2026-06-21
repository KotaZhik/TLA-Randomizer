using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace RandomizerMod
{
    [BepInPlugin("com.KotaZhik.Randomizer", "Randomizer", "1.0")]
    public class Randomizer : BaseUnityPlugin
    {
        private bool isTLA2;
        private int maxScene;

        private void Awake()
        {
            Random.InitState((int)System.DateTime.Now.Ticks);

            string productName = Application.productName.ToLower();
            isTLA2 = productName.Contains("tla 2");
            maxScene = isTLA2 ? 81 : 110;

            Harmony harmony = new Harmony("com.KotaZhik.Randomizer.patch");
            harmony.PatchAll();

            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogInfo($"Randomizer loaded for {(isTLA2 ? "TLA2" : "TLA1")}!");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            int index = scene.buildIndex;

            if (index == 1 && !isTLA2)
            {
                var allObjects = FindObjectsOfType<MonoBehaviour>();
                foreach (var obj in allObjects)
                {
                    if (obj.GetType().Name == "w42scr" && obj.gameObject.GetComponent<RandomW42>() == null)
                        obj.gameObject.AddComponent<RandomW42>();
                }
                return;
            }

            if (index <= 1 || index >= maxScene) return;

            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allGameObjects)
            {
                if (obj.name.Contains("nextLevel") && obj.scene.isLoaded)
                {
                    var allScripts = obj.GetComponents<MonoBehaviour>();
                    foreach (var script in allScripts)
                        script.enabled = false;

                    var teleporter = obj.GetComponent<RandomTeleporter>();
                    if (teleporter == null)
                        teleporter = obj.AddComponent<RandomTeleporter>();

                    teleporter.currentScene = index;
                    teleporter.maxScene = maxScene;
                }
            }
        }
    }

    [HarmonyPatch(typeof(worldScr), "goNext")]
    class GoNextPatch
    {
        static bool Prefix()
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int max = 110;

            string productName = Application.productName.ToLower();
            if (productName.Contains("tla 2")) max = 81;

            if (currentScene <= 1 || currentScene >= max) return true;

            int randomScene;
            do
            {
                randomScene = Random.Range(2, max);
            }
            while (randomScene == currentScene);

            SceneManager.LoadScene(randomScene);
            return false;
        }
    }

    public class RandomTeleporter : MonoBehaviour
    {
        public int currentScene;
        public int maxScene;
        private bool used = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (used) return;
            if (other.name != "hero" && other.name != "Hero") return;

            used = true;
            int randomScene;
            do
            {
                randomScene = Random.Range(2, maxScene);
            }
            while (randomScene == currentScene);

            SceneManager.LoadScene(randomScene);
        }
    }

    public class RandomW42 : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                int randomScene;
                do
                {
                    randomScene = Random.Range(2, 110);
                }
                while (randomScene == 109);

                SceneManager.LoadScene(randomScene);
            }
        }
    }
}
