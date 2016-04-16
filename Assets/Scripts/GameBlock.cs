using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBlock : MonoBehaviour {

	public enum Powerup
	{
		BoardClear,
		TimeStop
	}

	public static List<GameBlock> Pool = new List<GameBlock>();
	public static GameBlock prefab;

	public float SlideSpeed = 1.0f;

	public bool IsPending = false;

	[System.NonSerialized]
	public Powerup? PowerupId;

	[System.NonSerialized]
	public int SlideX;
	[System.NonSerialized]
	public int SlideY;

	[System.NonSerialized]
	public int BoardX;
	[System.NonSerialized]
	public int BoardY;

	public Sprite BlockSprite;
	public Sprite PowerupSprite;
	public int BlockType;

	SpriteRenderer _r;
	SpriteRenderer _r2;
	Transform _t;

	public void ActivatePending()
	{
		_r.enabled = true;
		_r2.enabled = true;
		IsPending = false;
	}

	void Awake()
	{
		_r = GetComponent<SpriteRenderer>();
		_t = transform;
		_r2 = _t.GetChild(0).GetComponent<SpriteRenderer>();
	}

	public void StartSliding(int x, int y)
	{
		SlideX = x;
		SlideY = y;
		StartCoroutine(SlideCoroutine());
	}

	int targetTileX;
	int targetTileY;
	Vector3 lastPos;
	Vector3 tmpPos;
	float movePct;

	public bool IsAnimating = false;
	bool wasMoved = false;
	IEnumerator SlideCoroutine()
	{
		IsAnimating = true;
		wasMoved = false;
		GameBoard.Current.CurrentState = GameBoard.GameState.Animating;

		while (true)
		{
			// Attempt to move towards the next tile
			movePct = 0;
			targetTileX = BoardX + SlideX;
			targetTileY = BoardY + SlideY;

			// Is this an acceptable move?
			if (targetTileX >= 0 && targetTileY >= 0 && targetTileX < GameBoard.Current.Cols && targetTileY < GameBoard.Current.Rows && GameBoard.Current.Board[targetTileY, targetTileX] == null)
			{
				// Then move us there now, and start animating
				GameBoard.Current.Board[BoardY, BoardX] = null;
				SetBlockPosition(targetTileX, targetTileY);
			}
			else
			{
				break;
			}

			// Animate movement
			lastPos = _t.localPosition;
			while (movePct < 1)
			{
				tmpPos = lastPos;
				tmpPos.x += (SlideX * movePct * GameBoard.Current.BlockSize);
				tmpPos.y += (SlideY * movePct * GameBoard.Current.BlockSize);
				_t.localPosition = tmpPos;

				movePct += SlideSpeed * Time.deltaTime;
				yield return null;
			}

			// Stop and finish movement
			BoardX = targetTileX;
			BoardY = targetTileY;
			_t.localPosition = GameBoard.Current.GetLocalPosition(targetTileX, targetTileY);

			wasMoved = true;
		}

		IsAnimating = false;
		GameBoard.Current.CurrentState = GameBoard.GameState.Selecting;
		GameBoard.Current.ResetSelection(this, BoardX, BoardY);
		if (wasMoved)
			GameBoard.Current.TestForClear(BoardX, BoardY);
	}

	void SetBlockPosition(int x, int y)
	{
		BoardX = x;
		BoardY = y;

		if (GameBoard.Current.Board[y, x] != null)
			Despawn(GameBoard.Current.Board[y, x]);
		GameBoard.Current.Board[y, x] = this;
	}

	void InitializeFromPool(Sprite sprite, Sprite powerup, Vector3 localPosition, int block, Powerup? powerupId, Transform parent, int x, int y, bool isDeactivated = false)
	{
		IsAnimating = false;
		BlockSprite = _r.sprite = sprite;
		_t.parent = parent;
		_t.localPosition = localPosition;
		BlockType = block;

		PowerupSprite = _r2.sprite = powerup;
		PowerupId = powerupId;

		_r2.enabled = !isDeactivated;
		_r.enabled = !isDeactivated;
		IsPending = isDeactivated;

		SetBlockPosition(x, y);
	}

	public static void Despawn(GameBlock block)
	{
		block.IsAnimating = false;
		GameBoard.Current.Board[block.BoardY, block.BoardX] = null;
		block.gameObject.SetActive(false);
	}

	static GameBlock tmp;
	public static GameBlock Spawn(Sprite sprite, Sprite powerup, Vector3 localPosition, int block, Powerup? powerupId, Transform parent, int x, int y, bool isDeactivated = false)
	{
		// Attempt to use a pooled block
		tmp = null;
		for (int i = 0; i < Pool.Count; i++)
		{
			if (Pool[i] != null && !Pool[i].gameObject.activeSelf)
			{
				tmp = Pool[i];
				tmp.gameObject.SetActive(true);
				tmp.InitializeFromPool(sprite, powerup, localPosition, block, powerupId, parent, x, y, isDeactivated);
				return tmp;
			}
		}

		// Create a new block
		tmp = (GameBlock)Instantiate(prefab);
		tmp.InitializeFromPool(sprite, powerup, localPosition, block, powerupId, parent, x, y, isDeactivated);
		Pool.Add(tmp);
		return tmp;
	}

}
