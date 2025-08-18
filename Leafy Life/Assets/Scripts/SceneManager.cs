using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using DG.Tweening;

public class SceneManager : MonoBehaviour
{
    public static UnityAction<MapController.MapType> MapChangedEvent;

    public SpriteRenderer loadingScreen;

    private Dictionary<MapController.MapType, string> mapsDict = new Dictionary<MapController.MapType, string>();
    private List<KeyValuePair<MapController.MapType, string>> mapsList;
    private string lastLoadedScene = null;
    private MapController.MapType lastLoadedMap;
    private Coroutine loadSceneCoroutine = null;
    private bool isLoading = false;

    void Awake() {
        mapsDict.Clear();
        mapsDict.Add(MapController.MapType.FOREST_STARTER, "scene_forest_starter");
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
        if (lastLoadedScene == null && loadSceneCoroutine == null && isLoading == false) {
            MapController.MapType starterMap = MapController.MapType.FOREST_STARTER;
            lastLoadedMap = starterMap;
            activateMap(starterMap);
        }
    }

    public void activateMap(MapController.MapType mapType) {
        if (mapsDict.TryGetValue(mapType, out string sceneName)) {
            if (isLoading == false) {
                isLoading = true;

                if (loadSceneCoroutine == null) {
                    loadingScreen.DOFade(1f, 2f).onComplete = () => {
                        loadSceneCoroutine = StartCoroutine(switchSceneCoroutine(sceneName));
                    };
                }
            }
        }
    }

    private IEnumerator switchSceneCoroutine(string sceneName) {
        AsyncOperation asyncLoad;

        MapController.MapType lastMapType = mapsDict.FirstOrDefault(kv => kv.Value == sceneName).Key;
        MapChangedEvent?.Invoke(lastMapType);

        // unload last scene
        if (lastLoadedScene != null) {
            asyncLoad = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(lastLoadedScene);

            while (!asyncLoad.isDone) {
                yield return null;
            }
        }

        // load new scene
        asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone) {
            yield return null;
        }

        // spawn player
        PlayerController player = WorldConstants.Instance.getPlayerController();
        MapController mapController = WorldConstants.Instance.getMapController();

        player.setPosition2D(mapController.getSpawnPosition(lastLoadedMap));

        lastLoadedScene = sceneName;
        lastLoadedMap = mapController.getMapType();

        lastLoadedScene = sceneName;

        loadingScreen.DOFade(0, 2f);

        isLoading = false;
        loadSceneCoroutine = null;
    }
}
