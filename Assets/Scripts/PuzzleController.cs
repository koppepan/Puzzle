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

public class PuzzleController : MonoBehaviour
{

	private enum PuzzleState
	{
		NONE,
		SELECT,		// ピースの選択
		MOVE,		// ピースの移動
		JUDGE,		// ピース消すかの判定
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


	private float			mTime;	//!< タイムのワーク

	// Use this for initialization
	void Start()
	{
		Init();
	}

	/*! 初期化  */
	private void Init()
	{
		mState = PuzzleState.SELECT;

		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Vector2 pos = new Vector2(i * PieceLength, (PieceLength * 5) + (PieceLength * j));
				GameObject obj = Instantiate(PiecePrefab, pos, Quaternion.identity) as GameObject;
				obj.transform.parent = transform;

				mPieces[i, j] = obj.GetComponent<PieceObject>();
				mPieces[i, j].SetPosition(new PiecePos(i, j), InitMoveSpeed);
				mPieces[i, j].Color = (PieceColor)Random.Range(1, 7);

				mActivList.Add(mPieces[i, j]);
			}
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
				mTime += Time.deltaTime;
				if (mTime > DeleteTime)
				{
					if (JudgeUpdate())
						mState = PuzzleState.SELECT;
					else
						mTime = 0;
				}
				break;
		}

		PieceUpdate();
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
	private bool JudgeUpdate()
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (mPieces[i, j] != null)
				{
					// X軸の探索
					int val = SeekColorX(new PiecePos(i, j), mPieces[i, j].Color, 0);
					if (val >= DeleteCount)
					{
						DeathPieceX(new PiecePos(i, j), val);
						return false;
					}

					// Y軸の探索
					val = SeekColorY(new PiecePos(i, j), mPieces[i, j].Color, 0);
					if (val >= DeleteCount)
					{
						DeathPieceY(new PiecePos(i, j), val);
						return false;
					}
				}
			}
		}
		return true;
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

	/*! X軸の削除 */
	private void DeathPieceX(PiecePos pos, int count)
	{
		for (int i = 0; i < count; i++)
		{
			mPieces[pos.x + i, pos.y].Death();
			mActivList.Add(mPieces[pos.x + i, pos.y]);
			//Destroy(mPieces[pos.x + i, pos.y].gameObject);
			//mPieces[pos.x + i, pos.y] = null;
		}
	}

	/*! Y軸の削除 */
	private void DeathPieceY(PiecePos pos, int count)
	{
		for (int i = 0; i < count; i++)
		{
			//Destroy(mPieces[pos.x, pos.y + i].gameObject);
			mPieces[pos.x, pos.y + i].Death();
			mActivList.Add(mPieces[pos.x, pos.y + i]);
			//mPieces[pos.x, pos.y + i] = null;
		}
	}
}
