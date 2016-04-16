using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SliderArrow : MonoBehaviour {

	public static List<SliderArrow> Pool = new List<SliderArrow>();
	public static SliderArrow prefab;

	[System.NonSerialized]
	public int DirectionX;

	[System.NonSerialized]
	public int DirectionY;

	SpriteRenderer _r;
	Collider2D _c;

	void Awake()
	{
		_r = GetComponent<SpriteRenderer>();
		_c = GetComponent<Collider2D>();
	}

	void OnMouseDown()
	{
		GameBoard.Current.SlideSelection(DirectionX, DirectionY);
	}

	public static void Despawn(SliderArrow tile)
	{
		tile.gameObject.SetActive(false);
	}

	void InitializeFromPool(Sprite sprite, Vector3 localPosition, Transform parent, int x, int y)
	{
		DirectionX = x;
		DirectionY = y;
		transform.parent = parent;
		transform.localPosition = localPosition;
		_r.sprite = sprite;
		_r.enabled = false;
		_c.enabled = false;
	}

	public void TileSelectionChanged(bool isSelected)
	{
		_r.enabled = isSelected;
		_c.enabled = isSelected;
	}

	static SliderArrow tmp;
	public static SliderArrow Spawn(Sprite sprite, Vector3 localPosition, Transform parent, int x, int y)
	{
		// Attempt to use a pooled block
		tmp = null;
		for (int i = 0; i < Pool.Count; i++)
		{
			if (Pool[i] != null && !Pool[i].gameObject.activeSelf)
			{
				tmp = Pool[i];
				tmp.gameObject.SetActive(true);
				tmp.InitializeFromPool(sprite, localPosition, parent, x, y);
				return tmp;
			}
		}

		// Create a new block
		tmp = (SliderArrow)Instantiate(prefab);
		tmp.InitializeFromPool(sprite, localPosition, parent, x, y);
		Pool.Add(tmp);
		return tmp;
	}
}
