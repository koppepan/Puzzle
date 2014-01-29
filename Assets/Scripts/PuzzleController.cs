using UnityEngine;
using System.Collections;

//===================================================
/*!
 * @brief	パズル部分の管理
 * 
 * @date	2014/01/09
 * @author	Daichi Horio
*/
//===================================================
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

public enum PieceDirction
{
	HEIGHT = 0,
	WIDTH
};

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

public struct PieceDeathData
{
	public PiecePos Pos;
	public PieceDirction Dir;
	public int Count;

	public PieceDeathData(PiecePos pos, PieceDirction dir, int count)
	{
		Pos = pos;
		Dir = dir;
		Count = count;
	}
};

public class PuzzleController : MonoBehaviour
{

	private enum PuzzleState
	{
		NONE,
		SELECT,		// ピースの選択
		MOVE,		// ピースの移動
		JUDGE,		// ピース消すかの判定
		DEATH,		// ピースを消す
		DOWN,		// 隙間を埋める
		END
	};

	public GameObject		PiecePrefab;		//!< ピースプレハブ

	static public int		Width		=	6;		//!< ピースの横数
	static public int		Height		=	5;		//!< ピースの縦数

	public float			PieceLength =	1.05f;	//!< ピースの大きさ

	public int				DeleteCount	=	3;		//!< 揃っていたら消す数
	public float			DeleteTime	=	0.5f;	//!< 消すタイムラグ

	public float			InitMoveSpeed	=	1;	//!< 初期化時のピースの移動スピード
	public float			NormalMoveSpeed	=	5;	//!< ノーマル時のピースの移動スピード


	private PuzzleState		mState;		//!< 遷移


	private PieceObject		mNowSelectPiece;					//!< 現在選択されているピースオブジェ

	private PieceObject[,]	mPieces		= new PieceObject[Width, Height];	//!< ピース管理用
	private ArrayList		mActivList	= new ArrayList();		//!< 現在動いているピースリスト

	private Queue			mDeathList	= new Queue();		//!< 消すリスト

	private float			mTime;	//!< タイムのワーク

	// Use this for initialization
	void Start()
	{
		mState = PuzzleState.SELECT;

		Init();

		while (!JudgeProcess())
		{
			AllDestroy();
			Init();
		}
	}


	// Update is called once per frame
	void Update()
	{
		switch (mState)
		{
			case PuzzleState.NONE:
				break;

			// ピース選択
			case PuzzleState.SELECT:
				if (SelectUpdate())
					mState++;
				break;

			// ピース移動
			case PuzzleState.MOVE:
				if (MoveUpdate())
					mState++;
				break;

			// 消すかどうか判定
			case PuzzleState.JUDGE:
				// 何もそろってなければセレクトへ
				if (JudgeProcess())
				{
					mState = PuzzleState.SELECT;
				}
				else
				{
					mState = PuzzleState.DEATH;
				}
				break;

			case PuzzleState.DEATH:
				if (DeathUpdate())
				{
					DownProcess();
					mState = PuzzleState.DOWN;
				}
				break;
			case PuzzleState.DOWN:
				if(mActivList.Count == 0)
				{
					mState = PuzzleState.JUDGE;
				}
				break;
		}

		// 各ピースの更新
		PieceUpdate();
	}

	/*! 初期化  */
	private void Init()
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				PiecePos pos = new PiecePos(i, j + 5);
				mPieces[i, j] = PieceGenerate(pos);
				mPieces[i, j].SetPosition(new PiecePos(i, j), InitMoveSpeed);
				mActivList.Add(mPieces[i, j]);
			}
		}
	}

	/*! 全消去  */
	private void AllDestroy()
	{
		mNowSelectPiece = null;
		mActivList.Clear();
		mDeathList.Clear();
		
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Destroy(mPieces[i, j].gameObject);
				mPieces[i, j] = null;
			}
		}
	}

	/*! ピースの生成 */
	private PieceObject PieceGenerate(PiecePos initPos)
	{
		PieceObject p;

		Vector2 pos = new Vector2(initPos.x * PieceLength, PieceLength * initPos.y);
		GameObject obj = Instantiate(PiecePrefab, pos, Quaternion.identity) as GameObject;
		obj.transform.parent = transform;

		p = obj.GetComponent<PieceObject>();
		p.Color = (PieceColor)Random.Range(1, 7);

		return p;
	}

	/*! マウスクリックしてオブジェクトを返す 
		@return クリックされたオブジェクト
	 */
	private GameObject MouseUpdate()
	{
		int mask = 1 << LayerMask.NameToLayer("Piece");

		Vector2 worldPoint2d = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Collider2D collider2D = Physics2D.OverlapPoint(worldPoint2d, mask);

		if (collider2D)
			return collider2D.transform.gameObject;

		return null;
	}

	/*! 各ピースの更新 
		動いてるピースだけ更新する
	 */
	private void PieceUpdate()
	{
		ArrayList deadList = new ArrayList();
		foreach (PieceObject obj in mActivList)
		{
			obj.SelfUpdate();

			if (obj.GetState() == 0)
				deadList.Add(obj);
		}

		foreach (PieceObject obj in deadList)
		{
			mActivList.Remove(obj);
			if (obj.Dead)
			{
				mPieces[obj.PicecPosition.x, obj.PicecPosition.y] = null;
				Destroy(obj.gameObject);
			}
		}
	}

	/*! 移動させるピースの選択	 */
	private bool SelectUpdate()
	{
		GameObject obj = null;

		if (Input.GetMouseButtonDown(0))
		{
			obj = MouseUpdate();
		}

		if (obj == null)
			return false;

		mNowSelectPiece = obj.GetComponent<PieceObject>();
		mNowSelectPiece.Catch();
		mActivList.Add(mNowSelectPiece);

		return true;
	}

	/*! ピースの移動、入れ替え	 */
	private bool MoveUpdate()
	{
		GameObject obj = MouseUpdate();

		// 他のピースに触れたら選択しているピースと位置入れ替え
		if (obj != null)
		{
			PieceObject p = obj.GetComponent<PieceObject>();
			if (p.GetState() == 0)
			{
				ReplacePiece(mNowSelectPiece, p);
			}
		}

		if (Input.GetMouseButtonUp(0) || mNowSelectPiece.transform.position.y > (PieceLength * 5))
		{
			mNowSelectPiece.Relese();
			return true;
		}

		return false;
	}

	/*! ピースの入れ替え	 */
	private void ReplacePiece(PieceObject now, PieceObject any)
	{
		PiecePos pos1 = now.PicecPosition;
		PiecePos pos2 = any.PicecPosition;

		PieceObject obj = mPieces[pos2.x, pos2.y];
		mPieces[pos2.x, pos2.y] = now;
		mPieces[pos1.x, pos1.y] = obj;

		any.SetPosition(pos1, NormalMoveSpeed);
		now.PicecPosition = pos2;

		mActivList.Add(any);
	}

	/*! ピースがそろっているか判定	 */
	private bool JudgeProcess()
	{
		PiecePos nowPos;

		for (int j = 0; j < Height; j++)
		{
			for (int i = 0; i < Width; i++)
			{
				if (mPieces[i, j] != null)
				{
					nowPos = new PiecePos(i, j);
					// X軸の探索
					int val = SeekColorX(nowPos, mPieces[i, j].Color, 0);
					if (val >= DeleteCount)
					{
						PieceDeathData data = new PieceDeathData(nowPos, PieceDirction.WIDTH, val);
						mDeathList.Enqueue(data);

						i += (val - 1);
					}
				}
			}
		}
		

		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (mPieces[i, j] != null)
				{
					nowPos = new PiecePos(i, j);
					// Y軸の探索
					int val = SeekColorY(nowPos, mPieces[i, j].Color, 0);
					if (val >= DeleteCount)
					{
						PieceDeathData data = new PieceDeathData(nowPos, PieceDirction.HEIGHT, val);
						mDeathList.Enqueue(data);

						j += (val - 1);
					}
				}
			}
		}

		if (mDeathList.Count != 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/*! X軸の探索	 */
	private int SeekColorX(PiecePos pos, PieceColor color, int count)
	{
		if (mPieces[pos.x, pos.y] == null)
			return 0;

		if (mPieces[pos.x, pos.y].Color == color)
		{
			count++;
			if (pos.x < mPieces.GetLength(0) - 1)
			{
				count = SeekColorX(new PiecePos(pos.x + 1, pos.y), color, count);
			}
		}
		return count;
	}

	/*! Y軸の探索	 */
	private int SeekColorY(PiecePos pos, PieceColor color, int count)
	{
		if (mPieces[pos.x, pos.y] == null)
			return 0;

		if (mPieces[pos.x, pos.y].Color == color)
		{
			count++;
			if (pos.y < mPieces.GetLength(1) - 1)
			{
				count = SeekColorY(new PiecePos(pos.x, pos.y + 1), color, count);
			}
		}
		return count;
	}

	/*! ピースの削除 */
	private void DeathPiece(PiecePos pos, PieceDirction dir, int count)
	{
		if (dir == PieceDirction.WIDTH)
		{
			for (int i = 0; i < count; i++)
			{
				if (mPieces[pos.x + i, pos.y] != null)
				{
					mPieces[pos.x + i, pos.y].Death();
					mActivList.Add(mPieces[pos.x + i, pos.y]);
				}
			}
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				if (mPieces[pos.x, pos.y + i] != null)
				{
					mPieces[pos.x, pos.y + i].Death();
					mActivList.Add(mPieces[pos.x, pos.y + i]);
				}
			}
		}
	}

	// 揃ってるのが全部消えるまで
	private bool DeathUpdate()
	{
		mTime += Time.deltaTime;
		if (mTime > DeleteTime)
		{
			mTime = 0;

			if (mDeathList.Count != 0)
			{
				PieceDeathData p = (PieceDeathData)mDeathList.Dequeue();
				DeathPiece(p.Pos, p.Dir, p.Count);
			}
			else
			{
				return true;
			}
		}
		return false;
	}

	// 空いてるところを埋める
	private void DownProcess()
	{
		int count = 0;

		for (int i = 0; i < Width; i++)
		{
			// 落とす
			count = LineDown(i);

			// 埋める
			FillPiece(i, count);
		}
	}

	// 下が空いてるピースを落とす
	private int LineDown(int line)
	{
		int count = 0;

		for (int j = 0; j < Height; j++)
		{
			if (mPieces[line, j] != null)
			{
				count++;

				int val = SeekDown(new PiecePos(line, j), 0);
				if (val != 0)
				{
					mPieces[line, j - val] = mPieces[line, j];
					mPieces[line, j] = null;
					mPieces[line, j - val].SetPosition(new PiecePos(line, j - val), InitMoveSpeed);

					mActivList.Add(mPieces[line, j - val]);
				}
			}
		}

		return count;
	}

	// 隙間を新しく作ったピースで埋める
	private void FillPiece(int line, int count)
	{
		if (count == Height)
			return;

		PiecePos p;
		p.x = line;
		p.y = Height - (Height - count);

		for (int j = 0; j < Height - count; j++)
		{
			mPieces[p.x, p.y + j] = PieceGenerate(new PiecePos(p.x, Height + j));
			mPieces[p.x, p.y + j].SetPosition(new PiecePos(p.x, p.y + j), InitMoveSpeed);

			mActivList.Add(mPieces[p.x, p.y + j]);
		}
	}

	// 下が何個空いてるか探索
	private int SeekDown(PiecePos pos, int count)
	{
		if (pos.y == 0)
			return count;

		if (mPieces[pos.x, pos.y - 1] != null)
			return count;

		count++;
		count = SeekDown(new PiecePos(pos.x, pos.y - 1), count);
		
		return count;
	}
}
