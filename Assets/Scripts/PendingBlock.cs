using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PendingBlock : MonoBehaviour {
	public static List<PendingBlock> Pool = new List<PendingBlock>();
	public static PendingBlock prefab;

	[System.NonSerialized]
	public GameBlock RealBlock;

	public float Timer = 0;

	[System.NonSerialized]
	float timer;

	RectTransform _t;
	Image _i;

	void Awake()
	{
		_t = (RectTransform)transform;
		_i = GetComponent<Image>();
	}

	const string TIMER_COROUTINE = "TimerCoroutine";

	Color tmpColor;
	IEnumerator TimerCoroutine()
	{
		timer = 0;

		while (timer < Timer)
		{
			if (GameBoard.Current.isTimeStopped)
			{
				yield return null;
				continue;
			}

			timer += Time.deltaTime * GameBoard.Current.DifficultyTimeScale;
			_i.fillAmount = timer / Timer;
			tmpColor = _i.color;
			tmpColor.a = timer / Timer;
			_i.color = tmpColor;
			yield return null;
		}

		RealBlock.ActivatePending();
		GameBoard.Current.StartCoroutine(GameBoard.SPAWN_TIMER_COROUTINE);
		GameBoard.Current.TestForFailure();
		Despawn(this);
	}

	public static void Despawn(PendingBlock tile)
	{
		tile.StopAllCoroutines();
		tile.gameObject.SetActive(false);
	}

	Vector3 tmpPos;
	void InitializeFromPool(Sprite sprite, GameBlock realBlock, Vector3 localPosition, Canvas canvas, Transform parent)
	{
		StopAllCoroutines();
		_i.sprite = sprite;
		RealBlock = realBlock;
		_t.SetParent(canvas.transform, true);
		_t.localPosition = Vector3.zero;
		_t.localScale = Vector3.one;
		_t.position = parent.transform.position + localPosition;
		timer = 0;
		StartCoroutine(TIMER_COROUTINE);
	}

	static PendingBlock tmp;
	public static PendingBlock Spawn(Sprite sprite, GameBlock realBlock, Vector3 localPosition, Canvas canvas, Transform parent, int x, int y)
	{
		// Attempt to use a pooled block
		tmp = null;
		for (int i = 0; i < Pool.Count; i++)
		{
			if (Pool[i] != null && !Pool[i].gameObject.activeSelf)
			{
				tmp = Pool[i];
				tmp.gameObject.SetActive(true);
				tmp.InitializeFromPool(sprite, realBlock, localPosition, canvas, parent);
				return tmp;
			}
		}

		// Create a new block
		tmp = (PendingBlock)Instantiate(prefab);
		tmp.InitializeFromPool(sprite, realBlock, localPosition, canvas, parent);
		Pool.Add(tmp);
		return tmp;
	}
}
