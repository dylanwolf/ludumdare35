using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOver : MonoBehaviour {

	public static GameOver Current;

	MaskableGraphic[] elements;

	void Awake()
	{
		Current = this;
	}

	void Start()
	{
		elements = GetComponentsInChildren<MaskableGraphic>();
		ToggleAll(false);
	}

	void ToggleAll(bool active)
	{
		for (int i = 0; i < elements.Length; i++)
			elements[i].enabled = active;
	}

	public void Show()
	{
		ToggleAll(true);
	}

	public void PlayAgainClicked()
	{
		ToggleAll(false);
		GameBoard.Current.ResetState();
		GameBoard.Current.SetupBoard();
	}
	
}
