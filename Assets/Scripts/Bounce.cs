using UnityEngine;
using System.Collections;

public class Bounce : MonoBehaviour {

	Transform _t;

	void Awake()
	{
		_t = transform;
		originalPos = _t.position;
	}

	Vector3 tmpPos;
	Vector3 originalPos;
	public float YDip = -0.1f;
	public float Speed = 1.0f;
	float pct;

	float GetEasedPercent()
	{
		return (0.5f * Mathf.Cos(Mathf.PI * (pct + 1))) + 0.5f;
	}

	void PositionToPercent()
	{
		tmpPos = originalPos;
		tmpPos.y = originalPos.y + (GetEasedPercent() * YDip);
		_t.position = tmpPos;

	}

	const string BOUNCE_COROUTINE = "BounceCoroutine";
	IEnumerator BounceCoroutine()
	{
		// Bounce down
		while (pct < 1)
		{
			PositionToPercent();
			pct += (Time.deltaTime / Time.timeScale) * Speed;
			yield return null;
		}

		if (pct > 1)
			pct = 1;

		// Bounce up
		while (pct > 0)
		{
			PositionToPercent();
			pct -= (Time.deltaTime / Time.timeScale) * Speed;
			yield return null;
		}

		pct = 0;
		_t.position = originalPos;
	}

	public void TriggerBounce()
	{
		StopCoroutine(BOUNCE_COROUTINE);
		StartCoroutine(BOUNCE_COROUTINE);
	}
}
