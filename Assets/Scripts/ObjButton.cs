using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjButton : MonoBehaviour
{
	[SerializeField] private float bounceDuration = 0.35f;
	[SerializeField] private Vector3 peakBounceScale = new Vector3(0.8f, 1.35f, 0.8f);

	private Coroutine bounceRoutine;
	private Vector3 originalScale;

    public LevelSO levelData;
    public Transform infoPanelTransform;
    public GameObject infoPanelPrefab;
    public Image levelImage;



	private void Awake()
	{
		GameObject infoPanel = GameObject.Find("InfoPanel");
		if (infoPanel != null)
		{
			infoPanelTransform = infoPanel.transform;
            infoPanelPrefab = infoPanelTransform.Find("Main").gameObject;
            infoPanelPrefab.SetActive(false);
		}
		originalScale = transform.localScale;
	}

	private void OnMouseEnter()
	{
		transform.localScale = originalScale * 1.05f;
	}

	private void OnMouseExit()
	{
		if (bounceRoutine == null)
		{
			transform.localScale = originalScale;
		}
	}

	private void OnMouseDown()
	{
		openLevelInfo();
		StartBounce();
	}

    public void openLevelInfo()
    {
        infoPanelPrefab.SetActive(true);
        infoPanelPrefab.transform.Find("WorldTxt").GetComponent<TextMeshProUGUI>().text = levelData.levelName.worldName;
        infoPanelPrefab.transform.Find("LevelTxt").GetComponent<TextMeshProUGUI>().text = levelData.level.ToString();
        infoPanelPrefab.transform.Find("LevelImg").GetComponent<Image>().sprite = levelData.levelImage;
        infoPanelPrefab.transform.Find("Continue").GetComponent<LoadLevel>().sceneName = levelData.levelName.sceneNames[levelData.level - 1];
    }

	private void StartBounce()
	{
		if (bounceRoutine != null)
		{
			StopCoroutine(bounceRoutine);
		}

		transform.localScale = originalScale;

		bounceRoutine = StartCoroutine(BounceRoutine());
	}

	private System.Collections.IEnumerator BounceRoutine()
	{
		float bounceTimer = 0f;

		while (bounceTimer < bounceDuration)
		{
			bounceTimer += Time.deltaTime;
			float progress = Mathf.Clamp01(bounceTimer / bounceDuration);
			float bounceArc = Mathf.Sin(progress * Mathf.PI);

			transform.localScale = new Vector3(
				Mathf.Lerp(originalScale.x, originalScale.x * peakBounceScale.x, bounceArc),
				Mathf.Lerp(originalScale.y, originalScale.y * peakBounceScale.y, bounceArc),
				Mathf.Lerp(originalScale.z, originalScale.z * peakBounceScale.z, bounceArc)
			);

			yield return null;
		}

		transform.localScale = originalScale;
		bounceRoutine = null;
	}
}
