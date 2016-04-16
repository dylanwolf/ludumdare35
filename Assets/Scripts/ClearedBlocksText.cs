using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClearedBlocksText : MonoBehaviour {

	public static ClearedBlocksText Current;
	Bounce _b;
	Text _t;

	void Awake()
	{
		Current = this;
		_b = GetComponent<Bounce>();
		_t = GetComponent<Text>();
	}

	public void Refresh()
	{
		_t.text = GameBoard.Current.ClearedBlocks.ToString();
	}

	public void Reset()
	{
		GameBoard.Current.ClearedBlocks = 0;
		nextLevel = 10;
		Refresh();
	}

	int nextLevel = 10;

	public void AddBlock()
	{
		GameBoard.Current.ClearedBlocks++;
		Refresh();
		_b.TriggerBounce();

		while (GameBoard.Current.ClearedBlocks >= nextLevel)
		{
			nextLevel += 10;
			LevelText.Current.AddLevel();
		}
    }
}
