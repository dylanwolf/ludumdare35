﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelText : MonoBehaviour {

	public static LevelText Current;
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
		_t.text = GameBoard.Current.Level.ToString();
	}

	public void Reset()
	{
		Time.timeScale = 1;
		GameBoard.Current.Level = 1;
		Refresh();
	}

	public void AddLevel()
	{
		GameBoard.Current.Level++;
		Refresh();
		_b.TriggerBounce();
		Time.timeScale += GameBoard.Current.SpeedIncreasePerLevel;
    }
}
