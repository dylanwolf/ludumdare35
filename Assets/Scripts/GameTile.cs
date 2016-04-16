using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTile : MonoBehaviour {

	public static List<GameTile> Pool = new List<GameTile>();
	public static GameTile prefab;

	public Sprite SelectedRowColSprite;
	public Sprite UnselectedSprite;
	public Sprite SelectedCellSprite;

	[System.NonSerialized]
	public int BlockX;

	[System.NonSerialized]
	public int BlockY;

	SpriteRenderer _r;

	void Awake()
	{
		_r = GetComponent<SpriteRenderer>();
	}

	public void SetSelectedRowCol()
	{
		_r.sprite = SelectedRowColSprite;
	}

	public void SetUnselected()
	{
		_r.sprite = UnselectedSprite;
	}

	public void SetSelectedCell()
	{
		_r.sprite = SelectedCellSprite;
	}

	void OnMouseDown()
	{
		GameBoard.Current.SelectBlock(BlockX, BlockY);
	}

	public static void Despawn(GameTile tile)
	{
		GameBoard.Current.Tiles[tile.BlockY, tile.BlockX] = null;
		tile.gameObject.SetActive(false);
	}

	void InitializeFromPool(Vector3 localPosition, Transform parent, int x, int y)
	{
		BlockX = x;
		BlockY = y;
		transform.parent = parent;
		transform.localPosition = localPosition;
	}

	static GameTile tmp;
	public static GameTile Spawn(Vector3 localPosition, Transform parent, int x, int y)
	{
		// Attempt to use a pooled block
		tmp = null;
		for (int i = 0; i < Pool.Count; i++)
		{
			if (Pool[i] != null && !Pool[i].gameObject.activeSelf)
			{
				tmp = Pool[i];
				tmp.gameObject.SetActive(true);
				tmp.InitializeFromPool(localPosition, parent, x, y);
				return tmp;
			}
		}

		// Create a new block
		tmp = (GameTile)Instantiate(prefab);
		tmp.InitializeFromPool(localPosition, parent, x, y);
		Pool.Add(tmp);
		return tmp;
	}
}
