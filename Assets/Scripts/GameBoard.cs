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
	}

	[System.NonSerialized]
	public int Score;

	public Canvas UICanvas;

	public int ScorePerBlock = 100;

	public int StartingBlocks = 10;
	public float BlockSize = 0.64f;
	public GameBlock BlockPrefab;
	public GameTile TilePrefab;
	public PendingBlock PendingPrefab;

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

		spawnTimer = 0;
		StopAllCoroutines();
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

		CreateTiles();
		CreateArrows();
		InitBlocks();
		StartCoroutine(SPAWN_TIMER_COROUTINE);
	}

	public const string SPAWN_TIMER_COROUTINE = "SpawnTimerCoroutine";
	IEnumerator SpawnTimerCoroutine()
	{
		spawnTimer = 0;
		while (spawnTimer < SpawnTimer)
		{
			spawnTimer += Time.deltaTime;
			yield return null;
		}

		SpawnBlock();
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

		if (!selectedX.HasValue || !selectedY.HasValue || Board[selectedY.Value, selectedX.Value] == null || SelectedBlock == null)
			return;

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
		if (x.HasValue && y.HasValue)
		{
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
				continue;
			}

			// TODO: Spawn pending with alternate sprite
			int blockId = Mathf.Clamp(Random.Range(0, BlockSprites.Length), 0, BlockSprites.Length - 1);
			PendingBlock.Spawn(BlockSprites[blockId],
				GameBlock.Spawn(BlockSprites[blockId], GetLocalPosition(x, y), blockId, _t, x, y, true),
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

		//for (int r = 0; r < Rows; r++)
		//{
		//	for (int c = 0; c < Rows; c++)
		//	{
				TestBlock(x,y);

				// Destroy blocks if we matched 3 of a kind
				if (collector.Count >= 3)
				{
					// TODO: Do destruction effect for each cell

					// Add score
					// TODO: Give bonuses for multiple blocks
					Score += collector.Count * ScorePerBlock;

					// Destroy all the blocks
					while (collector.Count > 0)
					{
						if (collector[0] == SelectedBlock)
							SelectBlock(null, null);
						if (collector[0].IsAnimating)
							CurrentState = GameState.Selecting;
						GameBlock.Despawn(collector[0]);
						collector.RemoveAt(0);
					}
				}

				// Finish testing this block
				currentlyTesting = null;
				collector.Clear();
			//}
		//}
	}

	public void TestForFailure()
	{

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

			int blockId = Mathf.Clamp(Random.Range(0, BlockSprites.Length), 0, BlockSprites.Length - 1);
			GameBlock.Spawn(BlockSprites[blockId], GetLocalPosition(x, y), blockId, _t, x, y);
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
}
