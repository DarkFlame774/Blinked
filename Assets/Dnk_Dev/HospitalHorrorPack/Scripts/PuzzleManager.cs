using System.Collections;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] player pl;
    [Header("Raycast Settings")]
    public float rayDistance = 3f;
    public LayerMask interactableLayer;

    [Header("Zoom Animation")]
    public float zoomDuration = 0.5f;
    public Vector3 zoomScale = new Vector3(2f, 2f, 2f);

    [Header("UI Indicator")]
    public Texture2D indicatorTexture; // Drag a small circle texture here
    public float indicatorSize = 30f;

    private Camera cam;
    private bool showIndicator = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            showIndicator = true;

            if (hit.collider.CompareTag("Key") && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(PickupKey(hit.collider.gameObject));
            }
        }
        else
        {
            showIndicator = false;
        }
    }

    void OnGUI()
    {
        if (showIndicator && indicatorTexture != null)
        {
            float x = (Screen.width - indicatorSize) / 2;
            float y = (Screen.height - indicatorSize) / 2;
            GUI.DrawTexture(new Rect(x, y, indicatorSize, indicatorSize), indicatorTexture);
        }
    }

    IEnumerator PickupKey(GameObject key)
    {
        Vector3 originalScale = key.transform.localScale;
        Vector3 targetScale = zoomScale;
        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            key.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        key.transform.localScale = targetScale;

        yield return new WaitForSeconds(0.2f);
        Destroy(key);
        pl.keypick = true;
    }

    void OnDrawGizmosSelected()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * rayDistance);
    }




}
