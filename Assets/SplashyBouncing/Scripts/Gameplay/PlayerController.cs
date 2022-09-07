using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using OnefallGames;


public enum PlayerState
{
	Prepare,
	Living,
	Die,
}

public class PlayerController : MonoBehaviour {

	public static PlayerController Instance { private set; get; }
	public static event System.Action<PlayerState> PlayerStateChanged = delegate { };

	public PlayerState PlayerState
	{
		get
		{
			return playerState;
		}

		private set
		{
			if (value != playerState)
			{
				value = playerState;
				PlayerStateChanged(playerState);
			}
		}
	}


	private PlayerState playerState = PlayerState.Die;


	[Header("Player Config")]
//	[SerializeField] private float firstBouncingDownSpeed = 17f;
//	[SerializeField] private float firstBouncingUpSpeed = 12f;
//	[SerializeField] private float limitBouncingDown = 30f;
//	[SerializeField] private float limitBouncingUp = 25f;
	[SerializeField] private float speedz = 14f;
//	[SerializeField] private float limitTop = 0;
	[SerializeField] private float bouncingSpeedIncreaseFactor = 0.1f;
	[SerializeField] private float bouncingSpeedIncreaseDuration = 2f;
	[SerializeField] private float thresholdSpeed = 30f;

	[Header("Player References")]
	[SerializeField] private CameraController camControl;
	[SerializeField] private GameObject playerExplodePrefab;
	[SerializeField] private MeshRenderer meshRender = null;

	private Rigidbody rigid = null;
	private SphereCollider sphereCollider = null;
//	private float bouncingDownSpeed = 0;
//	private float bouncingUpSpeed = 0;
	private float firstX = 0;
	private int centerPointCount = 0;
	private float originalYPos = 0;
	public GameObject plusPrefab;

	public GameObject canvas;

	private void OnEnable()
	{
		GameManager.GameStateChanged += GameManager_GameStateChanged;
	}
	private void OnDisable()
	{
		GameManager.GameStateChanged -= GameManager_GameStateChanged;
	}

	private void GameManager_GameStateChanged(GameState obj)
	{
		if (obj == GameState.Playing)
		{
			PlayerLiving();
		}
		else if (obj == GameState.Prepare)
		{
			PlayerPrepare();
		}
	}



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

	// Update is called once per frame
	void Update () {

		if (playerState == PlayerState.Living)
		{
			if (Input.GetMouseButtonDown(0))
			{
				firstX = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)).x;
			}
			else if (Input.GetMouseButton(0))
			{
				float currentX = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)).x;
				float amount = Mathf.Abs(Mathf.Abs(currentX) - Mathf.Abs(firstX));

				if (currentX > firstX)
				{
					transform.position += new Vector3(amount * thresholdSpeed * Time.deltaTime, 0, 0);
				}
				else
				{
					transform.position -= new Vector3(amount * thresholdSpeed * Time.deltaTime, 0, 0);
				}

				firstX = currentX;
			}

			if (Input.GetKeyDown(KeyCode.O))
			{
				GameManager.Instance.GameOver();
			}


			transform.Translate(Vector3.forward * Time.deltaTime * speedz);
		}
	}

	private void PlayerPrepare()
	{
		//Fire event
		PlayerState = PlayerState.Prepare;
		playerState = PlayerState.Prepare;

		//Add another function here
		meshRender = GetComponent<MeshRenderer>();
		rigid = GetComponent<Rigidbody>();
		sphereCollider = GetComponent<SphereCollider>();

		//Replace player with current character
		GameObject currentChar = CharacterManager.Instance.characters[CharacterManager.Instance.SelectedIndex];
		GetComponent<MeshFilter>().mesh = currentChar.GetComponent<MeshFilter>().sharedMesh;
		meshRender.material = currentChar.GetComponent<MeshRenderer>().sharedMaterial;

//		bouncingDownSpeed = firstBouncingDownSpeed;
//		bouncingUpSpeed = firstBouncingUpSpeed;
		originalYPos = transform.position.y;
	}

	private void PlayerLiving()
	{
		//Fire event
		PlayerState = PlayerState.Living;
		playerState = PlayerState.Living;

		//Add another actions here

		meshRender.enabled = true;
		rigid.isKinematic = true;
		sphereCollider.isTrigger = true;
		transform.position = new Vector3(0, originalYPos, Mathf.RoundToInt(transform.position.z));
		transform.eulerAngles = Vector3.zero;

		StartCoroutine(IncreaseBouncingSpeed());
	}

	private void PlayerDie()
	{
		//Fire event
		PlayerState = PlayerState.Die;
		playerState = PlayerState.Die;

		//Add another actions here
	}


	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Finish"))
		{
			SoundManager.Instance.PlaySound(SoundManager.Instance.explode);

			//Call events
			ShareManager.Instance.CreateScreenshot();
			StartCoroutine(SetGamestate());

			//Player particle
			ParticleSystem par = Instantiate(playerExplodePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
			par.Play();
			Destroy(par.gameObject, par.main.startLifetimeMultiplier);

			//Disabe mesh render
			meshRender.enabled = false;
		}

		//
		if (other.CompareTag("CenterPoint")) //Hit center point
		{
			//			Instantiate(plusPrefab, Vector3.zero, Quaternion.identity);
						StartCoroutine (plusCoroutine());
			Color groundColor = other.transform.parent.GetComponent<Renderer>().material.color;
			//				SoundManager.Instance.PlaySound(SoundManager.Instance.hitCenterPoint);
			ScoreManager.Instance.AddScore(2);
			other.transform.parent.GetComponent<PathController>().FadeCenterPoint();
			//				GameManager.Instance.CreateFadingGround(other.transform.position, groundColor, GameManager.Instance.centerGrowingScale);
			centerPointCount++;
			if (centerPointCount == GameManager.Instance.centerPointCount)
			{
				GameManager.Instance.ChangeGroundsColor();
				centerPointCount = 0;
			}
		} else  //Hit ground
		{
			Color groundColor = other.GetComponent<Collider>().GetComponent<Renderer>().material.color;
			SoundManager.Instance.PlaySound(SoundManager.Instance.hitGround);
			ScoreManager.Instance.AddScore(1);
			GameManager.Instance.CreatePath();
			//				GameManager.Instance.CreateFadingGround(other.transform.position, groundColor, GameManager.Instance.normalGrowingScale);
		}


		if (other.CompareTag ("Pillar")) { //Hit center point
			SoundManager.Instance.PlaySound(SoundManager.Instance.explode);

			//Call events
			ShareManager.Instance.CreateScreenshot();
			StartCoroutine(SetGamestate());

			//Player particle
			ParticleSystem par = Instantiate(playerExplodePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
			par.Play();
			Destroy(par.gameObject, par.main.startLifetimeMultiplier);

			//Disabe mesh render
			meshRender.enabled = false;
			//
			//Get component
			Rigidbody rigid = GetComponent<Rigidbody>();
			SphereCollider sphereCollider = GetComponent<SphereCollider>();

			//Disable trigger and kinematic
			sphereCollider.isTrigger = false;
			rigid.isKinematic = false;
			//			            StartCoroutine(ForcePlayerFallDown(transform.position.z)); 
		}
	}





	private IEnumerator IncreaseBouncingSpeed()
	{
		while (playerState == PlayerState.Living)
		{
			yield return new WaitForSeconds(bouncingSpeedIncreaseDuration);


			if (speedz < 26f) {
				speedz += bouncingSpeedIncreaseFactor;
			}
		}
	}

	private IEnumerator SetGamestate()
	{
		yield return null; //Wait to capture screenshot
		PlayerDie();
		if (!GameManager.Instance.IsRevived)
		{
			if (AdManager.Instance.IsRewardedVideoAdReady())
			{
				GameManager.Instance.Revive();
			}
			else
			{
				GameManager.Instance.GameOver();
			}
		}
		else
		{
			GameManager.Instance.GameOver();
		}
	}

	private IEnumerator plusCoroutine()
	{
		GameObject plusObj = null;
//		GameObject plusAwesomeObj = null;
		plusObj = UnityEngine.Object.Instantiate<GameObject>(this.plusPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, this.canvas.transform);
		plusObj.GetComponent<Animator>().Play("Base Layer.plus", 0, 0f);
		//		plusObj.transform.GetChild(0).GetComponent<Text>().text = "+3".ToString();

		yield return new WaitForSeconds(0.35f);
		//回收对象
		Destroy(plusObj);


	}
}
