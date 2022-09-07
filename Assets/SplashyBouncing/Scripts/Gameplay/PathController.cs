using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathController : MonoBehaviour {

	[SerializeField]
	private GameObject centerPoint;
	public Transform rightPoint;
	public Transform leftPoint;

	public float PathSpeed { private set; get; }
	public Material g2Color;
	private Renderer groundRender = null;
	private Renderer centerPointRender = null;
	private Color centerPointOriginalColor = Color.white;
	private Color groundOriginalColor = Color.white;
	private void Start()
	{
		groundRender = GetComponent<Renderer>();
		centerPointRender = centerPoint.GetComponent<Renderer>();
		centerPointOriginalColor = centerPointRender.material.color;
		groundOriginalColor = groundRender.material.color;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Respawn"))
		{
			//Reset ground original color
			groundRender.material.color = groundOriginalColor;

			//Reset center point
			centerPoint.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			centerPointRender.material.color = centerPointOriginalColor;

			//Reset this object
			gameObject.SetActive(false);
			transform.position = Vector3.zero;
		}
	}

	public void FadeCenterPoint()
	{
		StartCoroutine(FadingCenterPoint());
	}

	IEnumerator FadingCenterPoint()
	{
		Vector3 startScale = transform.localScale;
		Vector3 endScale = startScale * GameManager.Instance.centerGrowingScale;
		Color startColor = centerPointRender.material.color;
		Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
		float t = 0;
		while (t < GameManager.Instance.objectFadingTime)
		{
			t += Time.deltaTime;
			float factor = t / GameManager.Instance.objectFadingTime;
			centerPoint.transform.localScale = Vector3.Lerp(startScale, endScale, factor);
			centerPointRender.material.color = Color.Lerp(startColor, endColor, factor);
			yield return null;
		}
	}




	public void ChangeGroundColor(Color targetColor)
	{
		groundOriginalColor = targetColor;
		StartCoroutine(ChangingGroundColor(targetColor));
	}

	IEnumerator ChangingGroundColor(Color targetColor)
	{
		Color startColor = groundRender.material.color;
		float t = 0;
		while (t < GameManager.Instance.colorBlendingTime)
		{
			t += Time.deltaTime;
			float factor = t / GameManager.Instance.colorBlendingTime;
			groundRender.material.color = Color.Lerp(startColor, targetColor, factor);

			yield return null;
		}
	}


	public void MoveToCenter()
	{
		StartCoroutine(MovingToCenter());
	}
	IEnumerator MovingToCenter()
	{
		float startX = transform.position.x;
		float endX = 0;
		float t = 0;
		while (t < GameManager.Instance.moveToCenterTime)
		{
			t += Time.deltaTime;
			float factor = t / GameManager.Instance.moveToCenterTime;
			Vector3 newPos = transform.position;
			newPos.x = Mathf.Lerp(startX, endX, factor);
			transform.position = newPos;
			yield return null;
		}
	}

}
