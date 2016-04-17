using UnityEngine;
using System.Collections;

public class SkyCloud : MonoBehaviour {

	SpriteRenderer _r;
	BoxCollider2D _c;
	Transform _t;

	public Vector2 FloatSpeed;

	float Margin = 0.5f;

	Vector2 start;
	Vector2 end;

	void Awake()
	{
		_r = GetComponent<SpriteRenderer>();
		_c = GetComponent<BoxCollider2D>();
		_t = transform;
	}

	void Start()
	{
		CalculateStartAndEnd();
	}

	void CalculateStartAndEnd()
	{
		start.x = -(Camera.main.orthographicSize * Camera.main.aspect) - (_c.bounds.size.x / 2.0f) - Margin;
		start.y = -(Camera.main.orthographicSize) - (_c.bounds.size.y / 2.0f) - Margin;
		end.x = (Camera.main.orthographicSize * Camera.main.aspect) + (_c.bounds.size.x / 2.0f) + Margin;
		end.y = Camera.main.orthographicSize + (_c.bounds.size.y / 2.0f) + Margin;
		_c.enabled = false;
	}


	Vector3 tmpPos;
	void Update()
	{
		tmpPos = _t.position;

		tmpPos.x += FloatSpeed.x * Time.deltaTime;
		tmpPos.y += FloatSpeed.y * Time.deltaTime;

		if (tmpPos.x > end.x)
			tmpPos.x = start.x;

		if (tmpPos.y > end.y)
			tmpPos.y = start.y;

		_t.position = tmpPos;
	}

	Color tmpColor;
	public void SetAlpha(float alpha)
	{
		tmpColor = _r.material.color;
		tmpColor.a = alpha;
		_r.material.color = tmpColor;
	}
}
