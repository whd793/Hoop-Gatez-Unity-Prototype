using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnefallGames;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public enum GameState
{
	Prepare,
	Playing,
	Revive,
	GameOver,
}

public class GameManager : MonoBehaviour {

	public static GameManager Instance { private set; get; }
	public static event System.Action<GameState> GameStateChanged = delegate { };
	public static bool isRestart = false;

	public GameState GameState
	{
		get
		{
			return gameState;
		}
		private set
		{
			if (value != gameState)
			{
				gameState = value;
				GameStateChanged(gameState);
			}
		}
	}

	[Header("Gameplay Config")]
	public int pathSpace = 5;
	[SerializeField]
	private int groundNumber = 10;
	[SerializeField]
	private float originalXPathDistance = 1f;
	[SerializeField]
	private float maxXPathDistance = 4;
	[SerializeField]
	private float xPathDistanceIncreaseFactor = 0.1f;
	[SerializeField]
	private float xPathDistanceIncreaseDuration = 1f;
	public float normalGrowingScale = 2f;
	public float centerGrowingScale = 0.2f;
	public float objectFadingTime = 1f;
	public float colorBlendingTime = 0.5f;
	public int reviveWaitTime = 3;
	public int centerPointCount = 5;
	public float moveToCenterTime = 1f;
	[SerializeField]
	[Range(0f, 1f)]
	private float coinFrequency = 0.5f;
	[SerializeField]
	[Range(0f, 1f)]
	private float obstacleFrequency = 0.2f;
	[SerializeField]
	private Color[] groundColors;
	[SerializeField]
	private Color[] colors;
	[Header("Gameplay References")]
	[SerializeField]
	private CameraController camControl;
	[SerializeField]
	private GameObject groundPrefab;
	[SerializeField]
	private GameObject firstGround;
	[SerializeField]
	private GameObject fadingGroundPrefab;
	[SerializeField]
	private GameObject coinPrefab;
	[SerializeField]
	private GameObject obstaclePrefab;
	[SerializeField]
	private GameObject coinExplodePrefab;

	public float GroundBottomPosition { private set; get; }
	public bool IsRevived { private set; get; }

	public Material grColor;

	private GameState gameState = GameState.GameOver;

	private List<PathController> listPathControl = new List<PathController>();
	private List<FadingGroundController> listFadingGroundControl = new List<FadingGroundController>();
	private List<CoinController> listCoinControl = new List<CoinController>();
	private List<GameObject> listObstacle = new List<GameObject>();
	private List<GameObject> listCoinExplode = new List<GameObject>();
	private float currentZPos = 0;
	private int previousColorIndex = 0;
	private float xPathDistance = 0;
//	private Renderer centerPointRender = null;

	//	{
	//		new Color(137F, 120F, 124F, 255),
	//		new Color(47, 47, 47, 255)
	//	};

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			DestroyImmediate(Instance.gameObject);
			Instance = this;
		}
	}

	void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 60;
		ScoreManager.Instance.Reset();
		PrepareGame();
		//		centerPointRender = grColor.GetComponent<Renderer>();
		//		grColor = GetComponent<Renderer>().material;
		//		centerPointOriginalColor = centerPointRender.material.color;
	}

	public void PrepareGame()
	{
		//Fire event
		GameState = GameState.Prepare;
		gameState = GameState.Prepare;

		int coIndex = Random.Range(0, colors.Length);
		Color color2 = colors[coIndex];
		//		groundPrefab.GetComponent<Renderer>().sharedMaterial.color = color;
		grColor.color = color2;
		//Add another actions here
		//		grColor.color = new Color[Random.Range(0, colors.Length)];
		//Set continue equal to false
		IsRevived = false;

		//Set x path distance
		xPathDistance = originalXPathDistance;

		//Set path bottom position
		GroundBottomPosition = firstGround.transform.position.y;

		//Assign current z position
		currentZPos = firstGround.transform.position.z;

		//Add first path to the list
		listPathControl.Add(firstGround.GetComponent<PathController>());

		//Create some path first
		for(int i = 0; i < groundNumber; i++)
		{
			float zPos = currentZPos + pathSpace;
			float xPos = 0;
			Vector3 pathPos = new Vector3(xPos, GroundBottomPosition, zPos);
			PathController pathControl = GetPathControl();
			pathControl.gameObject.transform.position = pathPos;
			pathControl.gameObject.SetActive(true);
			currentZPos = pathControl.transform.position.z;
		}

		if (isRestart)
			PlayGame();
	}


	/// <summary>
	/// Actual start the game
	/// </summary>
	public void PlayGame()
	{
		//Fire event
		GameState = GameState.Playing;
		gameState = GameState.Playing;

		//Add another actions here

		//Change grounds color
		if (!IsRevived)
			ChangeGroundsColor();

		//Increase x path distance
		StartCoroutine(IncreaseXPathDistance());
		//		centerPointRender.material.color = new Color (148, 77, 77, 255);

	}


	/// <summary>
	/// Call Revive event
	/// </summary>
	public void Revive()
	{
		//Fire event
		GameState = GameState.Revive;
		gameState = GameState.Revive;

		//Add another actions here
	}


	/// <summary>
	/// Call GameOver event
	/// </summary>
	public void GameOver()
	{
		//Fire event
		GameState = GameState.GameOver;
		gameState = GameState.GameOver;

		//Add another actions here
		isRestart = true;
	}


	public void LoadScene(string sceneName, float delay)
	{
		StartCoroutine(LoadingScene(sceneName, delay));
	}

	IEnumerator LoadingScene(string sceneName, float delay)
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(sceneName);
	}


	//Get the inactive path object
	PathController GetPathControl()
	{
		foreach (PathController o in listPathControl)
		{
			if (!o.gameObject.activeInHierarchy)
				return o;
		}

		PathController pathControl = Instantiate(groundPrefab, Vector3.zero, Quaternion.Euler(-90, 0, 0)).GetComponent<PathController>();
		listPathControl.Add(pathControl);
		pathControl.gameObject.SetActive(false);
		return pathControl;
	}


	//Get the inactive fading ground object
	FadingGroundController GetFadingGround()
	{
		foreach(FadingGroundController o in listFadingGroundControl)
		{
			if (!o.gameObject.activeInHierarchy)
				return o;
		}

		FadingGroundController fadingGroundControl = Instantiate(fadingGroundPrefab, Vector3.zero, Quaternion.Euler(-90, 0, 0)).GetComponent<FadingGroundController>();
		listFadingGroundControl.Add(fadingGroundControl);
		fadingGroundControl.gameObject.SetActive(false);
		return fadingGroundControl;
	}


	//Get the inactive coin object
	CoinController GetCoinControl()
	{
		foreach (CoinController o in listCoinControl)
		{
			if (!o.gameObject.activeInHierarchy)
				return o;
		}

		CoinController coinControl = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity).GetComponent<CoinController>();
		listCoinControl.Add(coinControl);
		coinControl.gameObject.SetActive(false);
		return coinControl;
	}


	//Get the inactive obstacle
	GameObject GetObstacle()
	{
		foreach(GameObject o in listObstacle)
		{
			if (!o.activeInHierarchy)
				return o;
		}
		GameObject obstacle = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);
		obstacle.SetActive(false);
		listObstacle.Add(obstacle);
		return obstacle;
	}


	//Get the inactive coin explode object
	GameObject GetCoinExplode()
	{
		foreach (GameObject o in listCoinExplode)
		{
			if (!o.activeInHierarchy)
				return o;
		}
		GameObject coinExplode = Instantiate(coinExplodePrefab, Vector3.zero, Quaternion.identity);
		coinExplode.SetActive(false);
		listCoinExplode.Add(coinExplode);
		return coinExplode;
	}


	//Get the arranged list of path control
	List<PathController> ArrangedList()
	{
		List<PathController> finalList = new List<PathController>();
		List<PathController> pathsControl = FindObjectsOfType<PathController>().ToList();
		int pathNumber = pathsControl.Count;
		while (finalList.Count < pathNumber)
		{
			float min = 1000;
			PathController minZPathControl = null;
			foreach (PathController o in pathsControl)
			{
				if (o.transform.position.z < min)
				{
					min = o.transform.position.z;
					minZPathControl = o;
				}
			}
			finalList.Add(minZPathControl);
			pathsControl.Remove(minZPathControl);
		}
		return finalList;
	}



	//Change the ground color
	IEnumerator ChangingGroundColor()
	{
		yield return null;
		List<PathController> arrangedList = ArrangedList();
		int currentColorIndex = Random.Range(0, groundColors.Length);
		while (currentColorIndex == previousColorIndex)
		{
			currentColorIndex = Random.Range(0, groundColors.Length);
		}
		previousColorIndex = currentColorIndex;
		Color color = groundColors[currentColorIndex];
		groundPrefab.GetComponent<Renderer>().sharedMaterial.color = color;
		//		grColor.color = color;
		foreach (PathController o in arrangedList)
		{
			o.ChangeGroundColor(color);
			yield return new WaitForSeconds(0.08f);
		}
	}


	//Play coin explode effect
	IEnumerator PlayingCoinExplodeEffect(Vector3 pos)
	{
		GameObject coinExplode = GetCoinExplode();
		coinExplode.transform.position = pos;
		coinExplode.SetActive(true);
		ParticleSystem par = coinExplode.GetComponent<ParticleSystem>();
		par.Play();
		yield return new WaitForSeconds(par.main.startLifetimeMultiplier);
		coinExplode.SetActive(false);
	}


	//Increase x distance of the path
	IEnumerator IncreaseXPathDistance()
	{
		while (xPathDistance < maxXPathDistance)
		{
			yield return new WaitForSeconds(xPathDistanceIncreaseDuration);
			xPathDistance += xPathDistanceIncreaseFactor;
		}
	}


	IEnumerator MovePathToCenter()
	{
		List<PathController> listPath = ArrangedList();
		for(int i = 0; i < listPath.Count; i++)
		{
			listPath[i].MoveToCenter();
			yield return new WaitForSeconds(0.08f);
		}
		yield return new WaitForSeconds(moveToCenterTime - (listPath.Count * 0.08f));
		PlayGame();
	}



	////////////////////////////////////////////////////////////////Publish functions

	/// <summary>
	/// Load the saved screenshot
	/// </summary>
	/// <returns></returns>
	public Texture LoadedScrenshot()
	{
		byte[] bytes = File.ReadAllBytes(ShareManager.Instance.ScreenshotPath);
		Texture2D tx = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
		tx.LoadImage(bytes);
		return tx;
	}

	/// <summary>
	/// Continue the game
	/// </summary>
	public void SetContinueGame()
	{
		IsRevived = true;
		CoinController[] coins = FindObjectsOfType<CoinController>();

		//Disable coins
		foreach(CoinController o in coins)
		{
			o.gameObject.SetActive(false);
		}

		//Disable obstacles
		ObstacleController[] obstacles = FindObjectsOfType<ObstacleController>();
		foreach(ObstacleController o in obstacles)
		{
			o.gameObject.SetActive(false);
		}

		camControl.MoveToCenter();

		StartCoroutine(MovePathToCenter());
	}


	/// <summary>
	/// Create next path
	/// </summary>
	public void CreatePath()
	{
		List<PathController> listPath = ArrangedList();
		if (listPath.Count < groundNumber) {
			float xPos = Random.Range (-xPathDistance, xPathDistance);
			float zPos = currentZPos + pathSpace;
			Vector3 pathPos = new Vector3 (xPos, GroundBottomPosition, zPos);
			PathController pathControl = GetPathControl ();
			pathControl.transform.position = pathPos;
			currentZPos = pathControl.transform.position.z;

			if (Random.value <= coinFrequency) {
				if (Random.value <= obstacleFrequency) { //Create obstacle and coin
					if (Random.value <= 0.5f) {
						//Create coin on left
						CoinController coinControl = GetCoinControl ();
						coinControl.transform.position = pathControl.leftPoint.position;
						coinControl.gameObject.SetActive (true);

						//Create obstacle on right
						GameObject obstacle = GetObstacle ();
						obstacle.transform.position = pathControl.rightPoint.position;
						obstacle.SetActive (true);
					} else {
						//Create coin on right
						CoinController coinControl = GetCoinControl ();
						coinControl.transform.position = pathControl.rightPoint.position;
						coinControl.gameObject.SetActive (true);

						//Create obstacle on left
						GameObject obstacle = GetObstacle ();
						obstacle.transform.position = pathControl.leftPoint.position;
						obstacle.SetActive (true);
					}
				} else { //Create coin only
					//Create coin
					CoinController coinControl = GetCoinControl ();
					if (Random.value <= 0.5f) //Create on left
						coinControl.transform.position = pathControl.leftPoint.position;
					else //Create on right
						coinControl.transform.position = pathControl.rightPoint.position;
					coinControl.gameObject.SetActive (true);
				}
			} else {
				if (Random.value <= obstacleFrequency) { //Create obstacle
					if (Random.value <= 0.5f) { //Create two obstacles
						GameObject obstacle_1 = GetObstacle ();
						obstacle_1.transform.position = pathControl.leftPoint.position;
						obstacle_1.SetActive (true);

						GameObject obstacle_2 = GetObstacle ();
						obstacle_2.transform.position = pathControl.rightPoint.position;
						obstacle_2.SetActive (true);

					} else { //Create one obstacle
						GameObject obstacle = GetObstacle ();
						if (Random.value <= 0.5f)//Create obstacle on left
							obstacle.transform.position = pathControl.rightPoint.position;
						else //Create obstacle on right
							obstacle.transform.position = pathControl.rightPoint.position;
						obstacle.SetActive (true);
					}    
				}
			}

			pathControl.gameObject.SetActive (true);
		}
	}


	/// <summary>
	/// Create fading ground wit given position
	/// </summary>
	/// <param name="pos"></param>
	public void CreateFadingGround(Vector3 pos, Color color,float scaleFactor)
	{
		FadingGroundController fgCotrol = GetFadingGround();
		fgCotrol.gameObject.SetActive(true);
		fgCotrol.transform.position = pos;
		fgCotrol.FadingGround(color, scaleFactor);
	}


	/// <summary>
	/// Change the color of all grounds 
	/// </summary>
	public void ChangeGroundsColor()
	{
		StartCoroutine(ChangingGroundColor());
	}

	/// <summary>
	/// Play coin explode effect
	/// </summary>
	public void PlayCoinExplode(Vector3 pos)
	{
		StartCoroutine(PlayingCoinExplodeEffect(pos));
	}


}
