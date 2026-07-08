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

	[Header("Button Materials")]
	public GameObject buttonObject;
    public Material lockedMaterial;
    public Material unlockedMaterial;
    public Material completedMaterial;
	private Renderer buttonRenderer;

	private void Awake()
	{
		GameObject infoPanel = FindLoadedObjectByName("InfoPanel");
		buttonObject = this.gameObject;
		if (infoPanel != null)
		{
			infoPanelTransform = infoPanel.transform;
			Transform mainPanel = infoPanelTransform.Find("Main");
			if (mainPanel != null)
			{
				infoPanelPrefab = mainPanel.gameObject;
				infoPanelPrefab.SetActive(false);
			}
			else
			{
				Debug.LogWarning("ObjButton: InfoPanel found, but Main child was not found.");
			}
		}
		else
		{
			Debug.LogWarning("ObjButton: InfoPanel was not found in the loaded scene.");
		}

		if (buttonObject != null)
		{
			buttonRenderer = buttonObject.GetComponent<Renderer>();
		}

		originalScale = transform.localScale;
	}

	void Update()
	{
		if (buttonRenderer == null || levelData == null)
		{
			return;
		}

		if (levelData.isCompleted)
		{
			buttonRenderer.material = completedMaterial;
		}
		else if (levelData.isUnlocked)
		{
			buttonRenderer.material = unlockedMaterial;
		}
		else
		{
			buttonRenderer.material = lockedMaterial;
		}
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
		if (!levelData.isUnlocked)
		{
			return;
		}
		openLevelInfo();
		StartBounce();
	}

    public void openLevelInfo()
    {
		if (infoPanelTransform == null || infoPanelPrefab == null || levelData == null)
		{
			Debug.LogWarning("ObjButton: Cannot open level info panel because the panel or level data is missing.");
			return;
		}

		if (infoPanelTransform != null)
		{
			infoPanelTransform.gameObject.SetActive(true);
		}

        infoPanelPrefab.SetActive(true);
        infoPanelPrefab.transform.Find("WorldTxt").GetComponent<TextMeshProUGUI>().text = levelData.levelName.worldName;
        infoPanelPrefab.transform.Find("LevelTxt").GetComponent<TextMeshProUGUI>().text = levelData.level.ToString();
        infoPanelPrefab.transform.Find("LevelImg").GetComponent<Image>().sprite = levelData.levelImage;
        infoPanelPrefab.transform.Find("Continue").GetComponent<LoadLevel>().sceneName = levelData.levelSceneName;
		
		PlayerBattleSO playerBattleSO = FindObjectOfType<PlayerData>().playerBattleSO;
		playerBattleSO.currentLevel = levelData;
    }

	private GameObject FindLoadedObjectByName(string objectName)
	{
		GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach (GameObject candidate in allObjects)
		{
			if (candidate == null)
			{
				continue;
			}

			if (candidate.name != objectName)
			{
				continue;
			}

			if (candidate.scene.IsValid())
			{
				return candidate;
			}
		}

		return null;
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
