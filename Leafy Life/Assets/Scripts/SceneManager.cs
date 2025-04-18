using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public MapController mapTreehouse;
    public MapController mapGarden;

    private Dictionary<MapController.MapType, MapController> mapsDict = new Dictionary<MapController.MapType, MapController>();
    private string currentScene = null;
    private MapController lastActiveMapController = null;

    void Awake() {
        mapsDict.Clear();
        mapsDict.Add(MapController.MapType.TREEHOUSE, mapTreehouse);
        mapsDict.Add(MapController.MapType.GARDEN, mapGarden);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            activateMap(MapController.MapType.GARDEN);
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            activateMap(MapController.MapType.TREEHOUSE);
        }
    }

    public void activateMap(MapController.MapType mapType) {
        if (mapsDict.TryGetValue(mapType, out MapController mapController)) {
            if (mapController != lastActiveMapController) {
                mapController.gameObject.SetActive(true);
                mapController.init();

                if (lastActiveMapController != null) {
                    lastActiveMapController.gameObject.SetActive(false);
                }

                // spawn player
                PlayerController player = WorldConstants.Instance.getPlayerController();
                player.setPosition2D(mapController.getSpawnPosition());

                lastActiveMapController = mapController;
            }
        }
    }

    public void loadScene(string newSceneName) {
        StartCoroutine(switchSceneCoroutine(newSceneName));
    }

    private IEnumerator switchSceneCoroutine(string sceneName) {
        print("42 start coroutine");
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone) {
            yield return null;
            print("42 loading " + sceneName + " ...");
        }
        print("42 loading " + sceneName + " done.");
        
        if (currentScene != null) {
            print("42 start unloading " + currentScene);
            asyncLoad = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentScene);
        }

        while (!asyncLoad.isDone) {
            yield return null;
            print("42 UN-loading " + currentScene + " ...");
        }
        print("42 UN-loading " + currentScene + " done.");

        currentScene = sceneName;
    }
}
