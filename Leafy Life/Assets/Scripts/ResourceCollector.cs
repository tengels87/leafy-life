using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResourceCollector : MonoBehaviour {
    public Canvas canvas;
    public GameObject itemIconTemplate;
    public RectTransform targetUI;
    public float flyDuration = 0.6f;
    public float spawnDelay = 0.1f;
    public float spawnRandomOffset = 0.1f;

    // One simple pool
    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake() {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Prewarm a moderate default pool
        for (int i = 0; i < 20; i++) {
            GameObject obj = Instantiate(itemIconTemplate, canvasRect);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// Call this for a resource fly animation
    public void Collect(Sprite sprite, int amount, Vector3 worldPosition) {
        StartCoroutine(AnimateResources(sprite, amount, worldPosition, targetUI));
    }

    private IEnumerator AnimateResources(Sprite sprite, int amount, Vector3 worldPosition, RectTransform targetUI) {
        for (int i = 0; i < amount; i++) {
            SpawnAndFly(sprite, worldPosition, targetUI);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnAndFly(Sprite sprite, Vector3 worldPosition, RectTransform targetUI) {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        Vector2 localStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out localStartPos);

        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(itemIconTemplate, canvasRect);
        obj.transform.SetParent(canvasRect);
        obj.SetActive(true);

        // Update sprite just before showing
        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;

        obj.transform.localPosition = localStartPos + Random.insideUnitCircle * spawnRandomOffset;
        obj.transform.localScale = Vector3.one;

        Vector2 destScreenPos = RectTransformUtility.WorldToScreenPoint(null, targetUI.position);

        obj.transform.DOMove(destScreenPos, flyDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                obj.SetActive(false);
                pool.Enqueue(obj);

                PopWallet(targetUI);
            });
    }

    private bool walletAnimating = false;

    private void PopWallet(RectTransform targetUI) {
        if (walletAnimating) return;
        walletAnimating = true;

        targetUI.DOKill();
        targetUI.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                targetUI.DOScale(1f, 0.15f).SetEase(Ease.InQuad)
                    .OnComplete(() => walletAnimating = false);
            });
    }
}