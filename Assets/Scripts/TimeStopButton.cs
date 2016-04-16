using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeStopButton : MonoBehaviour {

	public static TimeStopButton Current;
	Text _t;
	Image _i;

	void Awake()
	{
		Current = this;
		_i = GetComponent<Image>();
		_t = GetComponentInChildren<Text>();
	}

	int lastCount = 0;
	public void UpdateCount(int count)
	{
		if (lastCount != count)
		{
			if (count == 0)
			{
				_i.enabled = false;
				_t.enabled = false;
			}
			else
			{
				_i.enabled = true;
				_t.enabled = true;
				_t.text = count.ToString();
			}
			lastCount = count;
		}
	}

	public void Trigger()
	{
		GameBoard.Current.DoTimeStop();
	}

	public void UpdateNumber(int number)
	{
		GameBoard.Current.TimeStopItems++;
		UpdateCount(GameBoard.Current.TimeStopItems);
	}
}
