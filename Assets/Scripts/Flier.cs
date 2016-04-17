using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Flier : MonoBehaviour {

	public static List<Flier> Pool = new List<Flier>();
	public static Flier prefab;

	void Awake()
	{
		_r = GetComponent<SpriteRenderer>();
		_t = transform;
	}

	public enum AudioToPlay
	{
		BlockCollect,
		ItemCollect
	}

	public float TotalTime = 0.5f;
	float time = 0;

	SpriteRenderer _r;
	Transform _t;
	Vector3 tmpPos;
	Vector3 originalPos;
	Vector3 targetPos;
	float distance = 0;
	GameObject trgt;
	int targetMessageNumber;
	AudioToPlay? audioToPlay;

	const string FLY_COROUTINE = "FlyCoroutine";
	IEnumerator FlyCoroutine()
	{
		time = 0;
		while (time < (TotalTime * distance))
		{
			time += (Time.deltaTime / Time.timeScale);
			tmpPos = Vector3.Lerp(
				originalPos,
				targetPos,
				time / (TotalTime * distance));
			tmpPos.z = _t.position.z;
			_t.position = tmpPos;
			yield return null;
		}

		if (trgt != null)
		{
			trgt.SendMessage(BOUNCE_MESSAGE);
			trgt.SendMessage(UPDATE_NUMBER_MESSAGE, targetMessageNumber);
		}

		if (audioToPlay.HasValue)
		{
			switch (audioToPlay.Value)
			{
				case AudioToPlay.BlockCollect:
					SoundBoard.PlayBlockCollect();
					break;

				case AudioToPlay.ItemCollect:
					SoundBoard.PlayItemCollect();
					break;
			}
		}

		Despawn(this);
	}

	const string BOUNCE_MESSAGE = "TriggerBounce";
	const string UPDATE_NUMBER_MESSAGE = "UpdateNumber";

	public void Fly(Vector3 original, Vector3 target)
	{
		StopAllCoroutines();

		originalPos = original;
		originalPos.z = _t.position.z;

		targetPos = target;
		targetPos.z = _t.position.z;

		distance = Vector3.Distance(originalPos, targetPos);

		StartCoroutine(FLY_COROUTINE);
	}

	public static void Despawn(Flier flier)
	{
		flier.gameObject.SetActive(false);
	}

	void InitializeFromPool(Sprite sprite, Vector3 originalPos, Vector3 targetPos, GameObject targetObject, int number, AudioToPlay? audio)
	{
		_t.position = originalPos;
		_r.sprite = sprite;
		Fly(originalPos, targetPos);
		trgt = targetObject;
		targetMessageNumber = number;
		audioToPlay = audio;
	}

	static Flier tmp;
	public static Flier Spawn(Sprite sprite, Vector3 originalPos, Vector3 targetPos, GameObject targetObject, int number, AudioToPlay? audio)
	{
		tmp = null;

		// Attempt to use a pooled object
		for (int i = 0; i < Pool.Count; i++)
		{
			if (Pool[i] != null && !Pool[i].gameObject.activeSelf)
			{
				tmp = Pool[i];
				tmp.gameObject.SetActive(true);
				tmp.InitializeFromPool(sprite, originalPos, targetPos, targetObject, number, audio);
				return tmp;
			}
		}

		// Create a new object
		tmp = (Flier)Instantiate(prefab);
		tmp.InitializeFromPool(sprite, originalPos, targetPos, targetObject, number, audio);
		Pool.Add(tmp);
		return tmp;
	}
}
