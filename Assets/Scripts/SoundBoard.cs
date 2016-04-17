using UnityEngine;
using System.Collections;

public class SoundBoard : MonoBehaviour {

	public AudioClip[] BlockCollect;
	public AudioClip[] BlockMatch;
	public AudioClip[] BlockSelect;
	public AudioClip[] BlockShift;
	public AudioClip[] BoardClear;
	public AudioClip[] ItemCollect;
	public AudioClip[] LevelUp;
	public AudioClip[] TimeStopStart;
	public AudioClip[] TimeStopTick;
	public AudioClip[] BlockSettle;
	public AudioClip[] BlockAppear;
	public AudioClip[] TimeStopEnd;

	public static SoundBoard Current;

	AudioSource source;

	void Awake()
	{
		Current = this;
		source = GetComponent<AudioSource>();
	}

	void PlayRandomSound(AudioClip[] clips)
	{
		PlaySound(clips[Mathf.Clamp(Random.Range(0, clips.Length), 0, clips.Length - 1)]);
	}

	void PlaySound(AudioClip clip)
	{
		source.PlayOneShot(clip);
	}

	public static void PlayBlockCollect()
	{
		Current.PlayRandomSound(Current.BlockCollect);
	}

	public static void PlayBlockMatch()
	{
		Current.PlayRandomSound(Current.BlockMatch);
	}

	public static void PlayBlockSelect()
	{
		Current.PlayRandomSound(Current.BlockSelect);
	}

	public static void PlayBlockShift()
	{
		Current.PlayRandomSound(Current.BlockShift);
	}

	public static void PlayBlockSettle()
	{
		Current.PlayRandomSound(Current.BlockSettle);
	}

	public static void PlayBlockAppear()
	{
		Current.PlayRandomSound(Current.BlockAppear);
	}

	public static void PlayBoardClear()
	{
		Current.PlayRandomSound(Current.BoardClear);
	}

	public static void PlayItemCollect()
	{
		Current.PlayRandomSound(Current.ItemCollect);
	}

	public static void PlayLevelUp()
	{
		Current.PlayRandomSound(Current.LevelUp);
	}

	public static void PlayTimeStopStart()
	{
		Current.PlayRandomSound(Current.TimeStopStart);
	}

	public static void PlayTimeStopTick()
	{
		Current.PlayRandomSound(Current.TimeStopTick);
	}

	public static void PlayTimeStopEnd()
	{
		Current.PlayRandomSound(Current.TimeStopEnd);
	}
}
