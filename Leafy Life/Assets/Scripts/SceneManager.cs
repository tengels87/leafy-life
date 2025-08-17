using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static UnityAction<MapController.MapType> MapChangedEvent;

    public MapController mapTreehouse;
    public MapController mapGarden;

    private Dictionary<MapController.MapType, string> mapsDict = new Dictionary<MapController.MapType, string>();
    private List<KeyValuePair<MapController.MapType, string>> mapsList;
    private string lastLoadedScene = null;
    private MapController.MapType lastLoadedMap;
    private Coroutine loadSceneCoroutine = null;

    void Awake() {
        mapsDict.Clear();
        mapsDict.Add(MapController.MapType.GARDEN, "scene_garden");
        mapsDict.Add(MapController.MapType.TREEHOUSE, "scene_treehouse");

        mapsList = new List<KeyValuePair<MapController.MapType, string>>(mapsDict);
    }

    void Start()
    {
        
    }

    void Update()
    {
        // begin on garden map
        if (lastLoadedScene == null && loadSceneCoroutine == null) {
            MapController.MapType starterMap = MapController.MapType.GARDEN;
            lastLoadedMap = starterMap;
            activateMap(starterMap);
        }
    }

    public void activateMap(MapController.MapType mapType) {
        if (mapsDict.TryGetValue(mapType, out string sceneName)) {
            if (sceneName != lastLoadedScene) {
                if (loadSceneCoroutine == null) {
                    loadSceneCoroutine = StartCoroutine(switchSceneCoroutine(sceneName));
                }
            }
        }
    }

    private IEnumerator switchSceneCoroutine(string sceneName) {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone) {
            yield return null;
        }

        if (lastLoadedScene != null) {
            asyncLoad = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(lastLoadedScene);
        }

        while (!asyncLoad.isDone) {
            yield return null;
        }

        // spawn player
        PlayerController player = WorldConstants.Instance.getPlayerController();
        MapController mapController = WorldConstants.Instance.getMapController();

        player.setPosition2D(mapController.getSpawnPosition(lastLoadedMap));

        lastLoadedScene = sceneName;
        lastLoadedMap = mapController.getMapType();
        loadSceneCoroutine = null;

        MapChangedEvent?.Invoke(mapController.getMapType());

        lastLoadedScene = sceneName;
    }
}
