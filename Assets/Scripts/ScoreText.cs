using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreText : MonoBehaviour {

	public static ScoreText Current;
	Text _t;

	void Awake()
	{
		Current = this;
	}

	void Start () {
		_t = GetComponent<Text>();
	}

	const string DIGIT1_FORMAT = "00{0}";
	const string DIGIT2_FORMAT = "0{0}";
	int lastScore = 0;
	public void SetScore(int score)
	{
		if (lastScore != score)
		{
			_t.text =
				(score < 10) ? string.Format(DIGIT1_FORMAT, score) :
				(
					(score < 100) ? string.Format(DIGIT2_FORMAT, score) :
						score.ToString()
				);
			lastScore = score;
		}
	}

	public void UpdateNumber(int number)
	{
		GameBoard.Current.Score += number;
		SetScore(GameBoard.Current.Score);
		ClearedBlocksText.Current.AddBlock();
	}
}
