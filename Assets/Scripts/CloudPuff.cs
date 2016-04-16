using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudPuff : MonoBehaviour {

	public static List<CloudPuff> Pool = new List<CloudPuff>();
	public static CloudPuff prefab;

	public ParticleSystem LargeClouds;
	public ParticleSystem SmallClouds;

	IEnumerator PlayingCheckCoroutine()
	{
		while (LargeClouds.isPlaying || SmallClouds.isPlaying)
		{
			yield return null;
		}

		Despawn(this);
	}

	public void PlayParticles()
	{
		LargeClouds.Clear();
		SmallClouds.Clear();

		LargeClouds.randomSeed = (uint)Random.Range(0, int.MaxValue);
		SmallClouds.randomSeed = (uint)Random.Range(0, int.MaxValue);

		LargeClouds.Play();
		SmallClouds.Play();

		StartCoroutine(PlayingCheckCoroutine());
	}

	public static void Despawn(CloudPuff puff)
	{
		puff.gameObject.SetActive(false);
	}

	void InitializeFromPool(Vector3 position)
	{
		transform.position = position;
		PlayParticles();
	}

	static CloudPuff tmp;

	public static CloudPuff Spawn(Vector3 position)
	{
		// Attempt to use pooled smoke
		tmp = null;
		for (int i = 0; i < Pool.Count; i++)
		{
			if (Pool[i] != null && !Pool[i].gameObject.activeSelf)
			{
				tmp = Pool[i];
				tmp.gameObject.SetActive(true);
				tmp.InitializeFromPool(position);
				return tmp;
			}
		}

		// Create a new smoke
		tmp = (CloudPuff)Instantiate(prefab);
		tmp.InitializeFromPool(position);
		Pool.Add(tmp);
		return tmp;
	}
}
