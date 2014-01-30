using UnityEngine;
using System.Collections;

// パズル遷移
public enum PuzzleState
{
	NONE,
	SELECT,		// ピースの選択
	MOVE,		// ピースの移動
	JUDGE,		// ピース消すかの判定
	DEATH,		// ピースを消す
	DOWN,		// 隙間を埋める
	END
};

// マウスデータ
public struct MouseData
{
	public bool down;
	public bool up;
	public Vector3 pos;
};

// ピースの色
public enum PieceColor
{
	NONE = 0,
	Red,
	Blue,
	Green,
	Yellow,
	Purple,
	Heart
};

// ピースがそろっている方向
public enum PieceDirction
{
	HEIGHT = 0,
	WIDTH
};

// ピースの座標
public struct PiecePos
{
	public int x;
	public int y;

	public PiecePos(int i, int j)
	{
		x = i;
		y = j;
	}
};