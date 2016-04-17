using UnityEngine;
using System.Collections;

public class Sky : MonoBehaviour {

	public SpriteRenderer DaySky;
	public SpriteRenderer NightSky;

	SkyCloud[] Clouds;

	public float CycleTimer = 5.0f;
	public float TransitionTime = 1.0f;
	float timer;
	CycleStage stage;

	enum CycleStage
	{
		Day,
		DayToNight,
		Night,
		NightToDay
	}

	Color SetColorAlpha(Color original, float alpha)
	{
		tmpColor = original;
		tmpColor.a = alpha;
		return tmpColor;
	}

	Color tmpColor;
	void Start()
	{
		Clouds = GetComponentsInChildren<SkyCloud>();

		DaySky.color = SetColorAlpha(DaySky.color, 1);
		NightSky.color = SetColorAlpha(NightSky.color, 1);
		stage = CycleStage.Day;

		for (int i = 0; i < Clouds.Length; i++)
			Clouds[i].SetAlpha(1);
	}

	// Update is called once per frame
	void Update () {

		timer += Time.deltaTime;

		if (stage == CycleStage.Day || stage == CycleStage.Night)
		{
			if (timer > CycleTimer)
			{
				timer = timer % CycleTimer;
				if (stage == CycleStage.Day)
				{
					stage = CycleStage.DayToNight;
					DaySky.sortingOrder = -100;
					NightSky.sortingOrder = -200;
					NightSky.color = SetColorAlpha(NightSky.color, 1);
				}
				else
				{
					stage = CycleStage.NightToDay;
					DaySky.sortingOrder = -200;
					NightSky.sortingOrder = -100;
					DaySky.color = SetColorAlpha(DaySky.color, 1);
				}
			}
		}
		else if (stage == CycleStage.DayToNight)
		{
			if (timer < TransitionTime)
			{
				DaySky.color = SetColorAlpha(DaySky.color, 1 - (timer / TransitionTime));

				for (int i = 0; i < Clouds.Length; i++)
					Clouds[i].SetAlpha(1 - (timer * 0.5f / TransitionTime));
			}
			else
			{
				timer = timer % CycleTimer;
				DaySky.color = SetColorAlpha(DaySky.color, 0);
				stage = CycleStage.Night;

				for (int i = 0; i < Clouds.Length; i++)
					Clouds[i].SetAlpha(0.5f);
			}
		}
		else if (stage == CycleStage.NightToDay)
		{
			if (timer < TransitionTime)
			{
				NightSky.color = SetColorAlpha(NightSky.color, 1 - (timer / TransitionTime));

				for (int i = 0; i < Clouds.Length; i++)
					Clouds[i].SetAlpha(0.5f + (timer * 0.5f / TransitionTime));
			}
			else
			{
				timer = timer % CycleTimer;
				NightSky.color = SetColorAlpha(NightSky.color, 0);
				stage = CycleStage.Day;

				for (int i = 0; i < Clouds.Length; i++)
					Clouds[i].SetAlpha(1);
			}
		}

	}
}
