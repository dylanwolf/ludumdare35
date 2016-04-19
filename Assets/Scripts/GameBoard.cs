using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour {

	#region Properties
	public enum GameState
	{
		Selecting,
		Animating,
		GameOver
	}

	public Sprite TimeStopPowerup;
	public Sprite BoardClearPowerup;

	public Transform ScoreTransform;
	public Transform BoardClearTransform;
	public Transform TimeStopTransform;

	[System.NonSerialized]
	public int Score;
	[System.NonSerialized]
	public int BoardClearItems = 0;
	[System.NonSerialized]
	public int TimeStopItems = 0;

	public Canvas UICanvas;

	public int ScorePerBlock = 100;
	public int ClearedBlocks = 0;
	public int Level = 1;
	public float DifficultyTimeScale = 1;

	public float SpeedIncreasePerLevel = 0.05f;

	public float TimeStopTimer = 3.0f;
	float timeStopTimer;

	public int StartingBlocks = 10;
	public float BlockSize = 0.64f;
	public GameBlock BlockPrefab;
	public GameTile TilePrefab;
	public PendingBlock PendingPrefab;
	public Flier FlierPrefab;
	public CloudPuff CloudPrefab;

	public SliderArrow ArrowPrefab;
	public Sprite LeftArrow;
	public Sprite RightArrow;
	public Sprite UpArrow;
	public Sprite DownArrow;

	public float SpawnTimer = 3.0f;
	float spawnTimer = 0;

	public int Rows = 9;
	public int Cols = 9;
	public Sprite[] BlockSprites;

	public GameState CurrentState = GameState.Selecting;

	[System.NonSerialized]
	public GameBlock[,] Board;

	[System.NonSerialized]
	public GameTile[,] Tiles;

	[System.NonSerialized]
	public SliderArrow[][] ColArrows;
	[System.NonSerialized]
	public SliderArrow[][] RowArrows;

	[System.NonSerialized]
	public GameBlock SelectedBlock;

	public static GameBoard Current;
	#endregion

	#region Unity Lifecycle
	void ClearPool<T>(List<T> pool) where T : MonoBehaviour
	{
		if (pool == null)
			return;

		for (int i = 0; i < pool.Count; i++)
		{
			pool[i].gameObject.SetActive(false);
		}
	}

	void ClearArray<T>(T[,] array) where T : class
	{
		if (array == null)
			return;

		for (int i = 0; i < array.GetLength(0); i++)
		{
			for (int j = 0; j< array.GetLength(1); j++)
			{
				array[i, j] = null;
			}
		}
	}

	void ClearArray<T>(T[][] array) where T : class
	{
		if (array == null)
			return;

		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
				continue;

			for (int j = 0; j < array[i].Length; j++)
			{
				array[i][j] = null;
			}
		}
	}

	public void ResetState()
	{
		ClearPool(GameBlock.Pool);
		ClearPool(GameTile.Pool);
		ClearPool(SliderArrow.Pool);
		ClearPool(PendingBlock.Pool);

		ClearArray(Board);
		ClearArray(Tiles);
		ClearArray(RowArrows);
		ClearArray(ColArrows);

		if (collector != null)
			collector.Clear();

		Score = 0;
		BoardClearItems = 0;
		TimeStopItems = 0;
		DifficultyTimeScale = 1;
		if (ClearedBlocksText.Current != null)
			ClearedBlocksText.Current.Reset();
		if (LevelText.Current != null)
			LevelText.Current.Reset();
        if (ScoreText.Current != null)
			ScoreText.Current.SetScore(0);
		if (BoardClearButton.Current != null)
			BoardClearButton.Current.UpdateCount(0);
		if (TimeStopButton.Current != null)
			TimeStopButton.Current.UpdateCount(0);

		StopAllCoroutines();

		CurrentState = GameState.Selecting;
	}

	public void SetupBoard()
	{
		CreateTiles();
		CreateArrows();
		InitBlocks();
		StartCoroutine(SPAWN_TIMER_COROUTINE);
	}

	Transform _t;
	void Awake()
	{
		Current = this;
		Board = new GameBlock[Rows, Cols];
		Tiles = new GameTile[Rows, Cols];
		RowArrows = new SliderArrow[Rows][];
		ColArrows = new SliderArrow[Cols][];
		_t = transform;

		ResetState();
	}

	void Start()
	{
		SliderArrow.prefab = ArrowPrefab;
		GameTile.prefab = TilePrefab;
		GameBlock.prefab = BlockPrefab;
		PendingBlock.prefab = PendingPrefab;
		Flier.prefab = FlierPrefab;
		CloudPuff.prefab = CloudPrefab;

		SetupBoard();
	}

	[System.NonSerialized]
	public bool isTimeStopped = false;
	public const string TIME_STOP_COROUTINE = "TimeStopCoroutine";
	IEnumerator TimeStopCoroutine()
	{
		isTimeStopped = true;
		timeStopTimer = 0;
		while (timeStopTimer < TimeStopTimer)
		{
			timeStopTimer += Time.deltaTime;
			yield return null;
		}

		isTimeStopped = false;
	}

	const string TICK_SOUND_COROUTINE = "TickSoundCoroutine";
	float tickTimer = 0;
	int tickTimerCount = 0;
	int tickTimerCountMax = 4;
	IEnumerator TickSoundCoroutine()
	{
		tickTimer = 0;
		tickTimerCount = 0;
		tickTimerCountMax = 4;

		while (isTimeStopped)
		{
			// First 6 seconds, every half second
			if (timeStopTimer <= 6.0f)
				tickTimerCountMax = 4;
			// Next 3 seconds, every quarter second
			else if (timeStopTimer <= 9.0f)
				tickTimerCountMax = 2;
			// Next 3 seconds, every quarter second
			else
				tickTimerCountMax = 1;

			tickTimer += Time.deltaTime;
			if (tickTimer >= 0.125f)
			{
				tickTimer = tickTimer % 0.125f;
				tickTimerCount++;
				if (tickTimerCount >= tickTimerCountMax)
				{
					tickTimerCount = tickTimerCount % tickTimerCountMax;
					SoundBoard.PlayTimeStopTick();
                }
			}
			
			yield return null;
		}

		SoundBoard.PlayTimeStopEnd();
	}

	public const string SPAWN_TIMER_COROUTINE = "SpawnTimerCoroutine";
	IEnumerator SpawnTimerCoroutine()
	{
		spawnTimer = 0;
		while (spawnTimer < SpawnTimer)
		{
			if (isTimeStopped)
			{
				yield return null;
				continue;
			}

			spawnTimer += Time.deltaTime * DifficultyTimeScale;
			yield return null;
		}

		SpawnBlock();
	}

	#region Input
	bool wasMouseDown = false;
	Vector2? fingerStart;
	Vector2 fingerDistance;
	Vector2 absFingerDistance;
	const float GESTURE_LENGTH = 0.05f;

	void SetTouchStart(Vector2 pos)
	{
		fingerStart = pos;
	}

	bool SetTouchDrag(Vector2 pos)
	{
		if (fingerStart.HasValue)
		{
			fingerDistance = pos - fingerStart.Value;
			fingerDistance.x = fingerDistance.x / Camera.main.pixelWidth;
			fingerDistance.y = fingerDistance.y / Camera.main.pixelHeight;

			absFingerDistance = fingerDistance;
			absFingerDistance.x = Mathf.Abs(fingerDistance.x);
			absFingerDistance.y = Mathf.Abs(fingerDistance.y);

			// Horizontal swipe
			if (absFingerDistance.x >= GESTURE_LENGTH && absFingerDistance.x > absFingerDistance.y)
			{
				SlideSelection((int)Mathf.Sign(fingerDistance.x), 0);
				fingerStart = null;
				return true;
			}
			// Vertical swipe
			else if (absFingerDistance.y > GESTURE_LENGTH)
			{
				SlideSelection(0, (int)Mathf.Sign(fingerDistance.y));
				fingerStart = null;
				return true;
			}
		}
		return false;
	}

	void SetTouchEnd()
	{
		fingerStart = null;
	}

	const string HORIZONTAL_AXIS = "Horizontal";
	const string VERITCAL_AXIS = "Vertical";
	#endregion

	void Update()
	{
		if (SelectedBlock != null && CurrentState == GameState.Selecting)
		{
			// Detect arrow keys
			if (Input.GetAxis(HORIZONTAL_AXIS) < 0)
			{
				SlideSelection(-1, 0);
				return;
			}
			else if (Input.GetAxis(HORIZONTAL_AXIS) > 0)
			{
				SlideSelection(1, 0);
				return;
			}

			if (Input.GetAxis(VERITCAL_AXIS) < 0)
			{
				SlideSelection(0, -1);
				return;
			}
			else if (Input.GetAxis(VERITCAL_AXIS) > 0)
			{
				SlideSelection(0, 1);
				return;
			}

			// Detect mouse gestures
			if (Input.GetMouseButtonDown(0))
			{
				wasMouseDown = true;
				SetTouchStart(Input.mousePosition);
				return;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				wasMouseDown = false;
				SetTouchEnd();
				return;
			}
			else if (wasMouseDown)
			{
				SetTouchDrag(Input.mousePosition);
				return;
			}

			// Detect touch gestures
			foreach (Touch t in Input.touches)
			{
				if (t.phase == TouchPhase.Began)
				{
					SetTouchStart(t.position);
					return;
				}
				else if (t.phase == TouchPhase.Moved)
				{
					SetTouchDrag(t.position);
					return;
				}
				else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
				{
					SetTouchEnd();
					return;
				}
			}
		}
	}
	#endregion

	#region Arrows
	void CreateArrows()
	{
		for (int c = 0; c < Cols; c++)
		{
			ColArrows[c] = new SliderArrow[]
			{
				SliderArrow.Spawn(DownArrow, GetLocalPosition(c, -1), _t, 0, -1),
				SliderArrow.Spawn(UpArrow, GetLocalPosition(c, Rows), _t, 0, 1),
			};
		}

		
		for (int r = 0; r < Rows; r++)
		{
			RowArrows[r] = new SliderArrow[]
			{
				SliderArrow.Spawn(LeftArrow, GetLocalPosition(-1, r), _t, -1, 0),
				SliderArrow.Spawn(RightArrow, GetLocalPosition(Cols, r), _t, 1, 0),

			};
		}
	}

	public void SlideSelection(int directionX, int directionY)
	{
		if (CurrentState != GameState.Selecting)
			return;

		if (!selectedX.HasValue || !selectedY.HasValue || Board[selectedY.Value, selectedX.Value] == null || SelectedBlock == null || 
			selectedX.Value + directionX < 0 || selectedY.Value + directionY < 0 || selectedX.Value + directionX >= Cols ||
			selectedY.Value + directionY >= Rows || Board[selectedY.Value+directionY, selectedX.Value+directionX] != null)
			return;

		SoundBoard.PlayBlockShift();
		Board[selectedY.Value, selectedX.Value].StartSliding(directionX, directionY);
    }
	#endregion

	#region Tiles
	void CreateTiles()
	{
		for (int y = 0; y < Rows; y++)
		{
			for (int x = 0; x< Cols; x++)
			{
				var tile = GameTile.Spawn(GetLocalPosition(x, y), _t, x, y);
				Tiles[y, x] = tile;
			}
		}
	}

	int? selectedX;
	int? selectedY;
	public void SelectBlock(int? x, int? y)
	{
		if (CurrentState == GameState.GameOver)
			return;

		if (x.HasValue && y.HasValue)
		{
			if (Board[y.Value, x.Value] != null && Board[y.Value, x.Value] != SelectedBlock)
				SoundBoard.PlayBlockSelect();

			SelectedBlock = Board[y.Value, x.Value];
            if ((Board[y.Value, x.Value] == null || Board[y.Value, x.Value].IsPending))
			{
				x = null;
				y = null;
				SelectedBlock = null;
			}
		}
		else
		{
			SelectedBlock = null;
		}

		selectedX = x;
		selectedY = y;

		for (int r = 0; r < Rows; r++)
		{
			for (int c = 0; c< Cols; c++)
			{
				if (r == y && c == x)
					Tiles[r, c].SetSelectedCell();
				else if (r == y || c == x)
					Tiles[r, c].SetSelectedRowCol();
				else
					Tiles[r, c].SetUnselected();
			}
		}
		ShowAndHideArrows();
	}

	void ShowAndHideArrows()
	{
		if (!selectedX.HasValue || !selectedY.HasValue)
		{
			for (int c = 0; c < Cols; c++)
				for (int i = 0; i < ColArrows[c].Length; i++)
					ColArrows[c][i].TileSelectionChanged(false);

			for (int r = 0; r < Rows; r++)
				for (int i = 0; i < RowArrows[r].Length; i++)
					RowArrows[r][i].TileSelectionChanged(false);

			return;
		}

			for (int c = 0; c < Cols; c++)
		{
			for (int i = 0; i < ColArrows[c].Length; i++)
			{
				ColArrows[c][i].TileSelectionChanged(
					selectedX.Value == c &&
					selectedY.Value + ColArrows[c][i].DirectionY >= 0 &&
					selectedY.Value + ColArrows[c][i].DirectionY < Rows &&
					Board[selectedY.Value + ColArrows[c][i].DirectionY, selectedX.Value] == null
				);
			}
		}

		for (int r = 0; r < Rows; r++)
		{
			for (int i = 0; i < RowArrows[r].Length; i++)
			{
				RowArrows[r][i].TileSelectionChanged(
					selectedY.Value == r &&
					selectedX.Value + RowArrows[r][i].DirectionX >= 0 &&
					selectedX.Value + RowArrows[r][i].DirectionX < Cols &&
					Board[selectedY.Value, selectedX.Value + RowArrows[r][i].DirectionX] == null);
			}
		}
	}
	#endregion

	#region Blocks
	int spawnX;
	int spawnY;
	int spawnBlockId;
	int blockCount = 0;
	GameBlock tmpBlock;
	public void SpawnBlock()
	{
		while (true)
		{
			int x = Mathf.Clamp(Random.Range(0, Cols), 0, Cols - 1);
			int y = Mathf.Clamp(Random.Range(0, Rows), 0, Rows - 1);

			if (Board[y, x] != null)
			{
				// Prevent the possibility of an infinite loop
				TestForFailure();
				if (CurrentState == GameState.GameOver)
					return;
				continue;
			}

			tmpBlock = GenerateBlock(x, y, true);
            PendingBlock.Spawn(BlockSprites[tmpBlock.BlockType],
				tmpBlock,
                GetLocalPosition(x, y), UICanvas, _t, x, y);

			break;
		}
		ShowAndHideArrows();
    }

	public void ResetSelection(GameBlock block, int x, int y)
	{
		CurrentState = GameState.Selecting;
		if (block == SelectedBlock)
			SelectBlock(x, y);
		else if (selectedX.HasValue && selectedY.HasValue)
			SelectBlock(selectedX, selectedY);
	}

	
	GameBlock[,] boardCopy;
	public void CopyBoard()
	{
		if (boardCopy == null)
			boardCopy = new GameBlock[Rows, Cols];

		for (int r = 0; r < Rows; r++)
		{
			for (int c = 0; c < Cols; c++)
			{
				boardCopy[r, c] = Board[r, c];
			}
		}
	}

	GameBlock currentlyTesting;
	public void TestBlock(int x, int y)
	{
		// Already tested
		if (boardCopy[y, x] == null)
		{
			return;
		}

		// Can't actually test this yet
		if (boardCopy[y, x].IsPending)
		{
			boardCopy[y, x] = null;
			return;
		}

		// Start testing
		else if (currentlyTesting == null)
		{
			currentlyTesting = boardCopy[y, x];
			boardCopy[y, x] = null;
			collector.Add(currentlyTesting);
		}

		// Not the same shape... leave and come back to this later
		else if (currentlyTesting.BlockType != boardCopy[y,x].BlockType)
		{
			return;
		}

		// Matching shape
		else
		{
			collector.Add(boardCopy[y, x]);
			boardCopy[y, x] = null;
		}

		// Test around the block
		if (x > 0)
			TestBlock(x - 1, y);
		if (y > 0)
			TestBlock(x, y - 1);
		if (x < Cols - 1)
			TestBlock(x + 1, y);
		if (y < Rows - 1)
			TestBlock(x, y + 1);
	}

	List<GameBlock> collector = new List<GameBlock>();
	public void TestForClear(int x, int y)
	{
		CopyBoard();
		collector.Clear();

		// Only the currently moved block can trigger a clear
		TestBlock(x,y);

		// Destroy blocks if we matched 3 of a kind
		if (collector.Count >= 3)
		{
			SoundBoard.PlayBlockMatch();

			// Destroy all the blocks
			while (collector.Count > 0)
			{
				ClearBlock(collector[0], true);
				collector.RemoveAt(0);
			}

		}

		// Finish testing this block
		currentlyTesting = null;
		collector.Clear();
	}



	void ClearBlock(GameBlock block, bool score)
	{
		if (block == SelectedBlock)
			SelectBlock(null, null);
		if (block.IsAnimating)
			CurrentState = GameState.Selecting;

		if (score)
		{
			// Collect Powerups
			if (block.PowerupId.HasValue)
			{
				if (block.PowerupId.Value == GameBlock.Powerup.BoardClear)
				{
					Flier.Spawn(block.PowerupSprite, block.transform.position, BoardClearTransform.position, BoardClearTransform.gameObject, 1, Flier.AudioToPlay.ItemCollect);
				}
				else if (block.PowerupId.Value == GameBlock.Powerup.TimeStop)
				{
					Flier.Spawn(block.PowerupSprite, block.transform.position, TimeStopTransform.position, TimeStopTransform.gameObject, 1, Flier.AudioToPlay.ItemCollect);
				}
			}

			Flier.Spawn(block.BlockSprite, block.transform.position, ScoreTransform.position, ScoreTransform.gameObject,
					(collector.Count > 3) ? ScorePerBlock * (collector.Count - 2) : ScorePerBlock, Flier.AudioToPlay.BlockCollect
				);
		}

		CloudPuff.Spawn(block.transform.position);
		GameBlock.Despawn(block);
	}

	public void TestForFailure()
	{
		// Test whether all the spaces are filled
		for (int r = 0; r < Rows; r++)
		{
			for (int c = 0; c < Cols; c++)
			{
				// If not, return
				if (Board[r, c] == null)
					return;
			}
		}

		// Go to Game Over state
		GameOver.Current.Show();
		SelectBlock(null, null);
		CurrentState = GameState.GameOver;
	}

	int blockId;
	Sprite powerupSprite;
	GameBlock.Powerup? blockPowerup;
	GameBlock GenerateBlock(int x, int y, bool isDeactivated = false)
	{
		blockId = Mathf.Clamp(Random.Range(0, BlockSprites.Length), 0, BlockSprites.Length - 1);
		float random = Random.value;
		//if (random >= 0.75f)
		if (random >= 0.99f)
		{
			blockPowerup = GameBlock.Powerup.BoardClear;
			powerupSprite = BoardClearPowerup;
		}
		//else if (random >= 0.5f)
		else if (random >= 0.95f)
		{
			blockPowerup = GameBlock.Powerup.TimeStop;
			powerupSprite = TimeStopPowerup;
		}
		else
		{
			blockPowerup = null;
			powerupSprite = null;
		}

		return GameBlock.Spawn(BlockSprites[blockId], powerupSprite, GetLocalPosition(x, y), blockId, blockPowerup, _t, x, y, isDeactivated);
	}

	void InitBlocks()
	{
		// TODO: Prevent automatic clears
		for (int i = 0; i < StartingBlocks; i++)
		{
			int x = Mathf.Clamp(Random.Range(0, Cols), 0, Cols - 1);
			int y = Mathf.Clamp(Random.Range(0, Rows), 0, Rows - 1);

			if (Board[y, x] != null)
			{
				i--;
				continue;
			}

			GenerateBlock(x, y);	
		}
	}
	#endregion

	#region Positioning / Translation
	public Vector3 GetLocalPosition(int x, int y)
	{
		return new Vector3(
				(x - ((Cols - 1) / 2.0f)) * BlockSize,
				(y - ((Rows - 1) / 2.0f)) * BlockSize,
				0
			);
	}
	#endregion

	#region Powers
	public void DoBoardClear()
	{
		if (BoardClearItems > 0 && CurrentState != GameState.GameOver)
		{
			SoundBoard.PlayBoardClear();
			BoardClearItems--;
			BoardClearButton.Current.UpdateCount(BoardClearItems);
			for (int r = 0; r < Rows; r++)
			{
				for (int c = 0; c < Cols; c++)
				{
					if (Board[r, c] != null && !Board[r, c].IsPending)
						GameBlock.Despawn(Board[r, c]);
				}
			}
			SelectBlock(null, null);
		}
	}

	public void DoTimeStop()
	{
		if (TimeStopItems > 0 && CurrentState != GameState.GameOver)
		{
			SoundBoard.PlayTimeStopStart();
			TimeStopItems--;
			TimeStopButton.Current.UpdateCount(TimeStopItems);

			StopCoroutine(TIME_STOP_COROUTINE);
			StopCoroutine(TICK_SOUND_COROUTINE);

			StartCoroutine(TIME_STOP_COROUTINE);
			StartCoroutine(TICK_SOUND_COROUTINE);
		}
	}
	#endregion
}
