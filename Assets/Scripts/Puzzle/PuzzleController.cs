using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//===================================================
/*!
 * @brief	パズル部分の管理
 * 
 * @date	2014/01/09
 * @author	Daichi Horio
*/
//===================================================
public class PuzzleController : MonoBehaviour
{
	public GameObject		PiecePrefab;

	static public float		PieceSize = 1.05f;	//!< ピースの大きさ
	static public int		Width		=	6;		//!< ピースの横数
	static public int		Height		=	5;		//!< ピースの縦数

	public int				DeleteCount	=	3;		//!< 揃っていたら消す数
	public float			DeleteTime	=	0.5f;	//!< 消すタイムラグ

	public float			InitMoveSpeed	=	1;	//!< 初期化時のピースの移動スピード

	private PieceObject[,]	mPieces		= new PieceObject[Width, Height];	//!< ピース管理用
	

	private List<PieceObject>		mActivList	= new List<PieceObject>();		//!< 現在動いているピースリスト
	private Stack<PieceDeathData>	mDeathList	= new Stack<PieceDeathData>();	//!< 消すリスト


	private float			mTime;		//!< タイムのワーク

	private PieceTouch		mTouchCon;	//!< ピース移動管理クラス
	private PuzzleJudgment	mJudgeCon;	//!< パズルの判定クラス



	public PieceObject[,] PiecesList
	{
		get { return mPieces; }
	}

	public PieceObject ActivList
	{
		set { mActivList.Add(value); }
	}

	// Use this for initialization
	void Awake()
	{
		mTouchCon = new PieceTouch(this, Height, Width, PieceSize);
		mJudgeCon = new PuzzleJudgment(this, Height, Width, DeleteCount);
		Init();

		while (mJudgeCon.AllJudge() != null)
		{
			AllDestroy();
			Init();
		}
	}

	/*! 初期化  */
	private void Init()
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				PiecePos pos = new PiecePos(i, j + Height);
				mPieces[i, j] = PieceGenerate(pos);
				mPieces[i, j].SetPosition(new PiecePos(i, j), InitMoveSpeed);
				mActivList.Add(mPieces[i, j]);
			}
		}
	}

	/*! 全消去  */
	private void AllDestroy()
	{
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

	/*! パズル部分の更新
		@param	nowState		現在の遷移
		@param	mouseData		マウスの入力データ
		@param	PuzzleState		遷移を返す
	*/
	public PuzzleState  SelfUpdate(PuzzleState nowState, MouseData mouseData)
	{
		switch (nowState)
		{
			case PuzzleState.NONE:
				break;

			// ピース選択
			case PuzzleState.SELECT:
				if (mTouchCon.SelectUpdate(mouseData))
					nowState++;
				break;

			// ピース移動
			case PuzzleState.MOVE:
				if (mTouchCon.MoveUpdate(mouseData))
					nowState = PuzzleState.JUDGE;
				break;

			// 消すかどうか判定
			case PuzzleState.JUDGE:
				// 何もそろってなければセレクトへ
				if(mJudgeCon.AllJudge() != null)
					nowState = PuzzleState.DEATH;
				else
					nowState = PuzzleState.SELECT;

				break;

			// 揃っているピースを消す
			case PuzzleState.DEATH:
				if (DeathUpdate())
				{
					DownProcess();
					nowState = PuzzleState.DOWN;
				}
				break;

			// 隙間を埋めるためにピースを落とす
			case PuzzleState.DOWN:
				if(mActivList.Count == 0)
					nowState = PuzzleState.JUDGE;

				break;
		}

		// 各ピースの更新
		PieceUpdate();

		return nowState;
	}

	//===================================================
	/*!
　　 	@brief		ピースの生成
　　 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private PieceObject PieceGenerate(PiecePos initPos)
	{
		PieceObject p;

		Vector2 pos = new Vector2(initPos.x * PieceSize, PieceSize * initPos.y);
		GameObject obj = Instantiate(PiecePrefab, pos, Quaternion.identity) as GameObject;
		obj.transform.parent = transform;

		p = obj.GetComponent<PieceObject>();
		p.Color = (PieceColor)Random.Range(1, 7);

		return p;
	}


	//===================================================
	/*!
　　 	@brief		各ピースの更新
					アクティブなピースだけ更新
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	public void PieceUpdate()
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
			if (obj.DeadFlg)
			{
				mPieces[obj.PicecPosition.x, obj.PicecPosition.y] = null;
				Destroy(obj.gameObject);
			}
		}
	}

	//===================================================
	/*!
　　 	@brief		揃ってるピース消す
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool DeathUpdate()
	{
		mTime += Time.deltaTime;
		if (mTime > DeleteTime)
		{
			mTime = 0;

			if (mTouchCon.RouteQueue.Count != 0)
			{
				// 動いた順に座標取得
				PiecePos pos = mTouchCon.RouteQueue.Dequeue();
				if (mPieces[pos.x, pos.y] != null)
				{
					List<PiecePos> p = mJudgeCon.Judge(pos);

					if (!DeathPiece(p))
					{
						mTime = DeleteTime;
					}
				}
				else
				{
					mTime = DeleteTime;
				}
			}
			else
			{
				List<PiecePos> p = mJudgeCon.AllJudge();
				if (p != null)
				{
					if (!DeathPiece(p))
					{
						mTime = DeleteTime;
					}
				}
				else
				{
					return true;
				}
			}
		}
		return false;
	}

	
	//===================================================
	/*!
　　 	@brief		ピースの削除
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool DeathPiece(List<PiecePos> data)
	{
		if (data.Count == 0)
			return false;

		foreach (var pos in data)
		{
			if (mPieces[pos.x, pos.y] != null)
			{
				mPieces[pos.x, pos.y].Death();
				mActivList.Add(mPieces[pos.x, pos.y]);
			}
		}

		return true;
	}


	//===================================================
	/*!
　　 	@brief		空いてるところを埋める
	 
　　 	@date		2013/01/29
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

	//===================================================
	/*!
　　 	@brief		ピースを落とす
	 
　　 	@date		2013/01/29
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

	//===================================================
	/*!
　　 	@brief		新しく作ったピースで埋める
	 
　　 	@date		2013/01/29
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

	//===================================================
	/*!
　　 	@brief		下が何個空いてるか探索
	 
　　 	@date		2013/01/29
　　 	@author		Daichi Horio
　　*/
	//===================================================
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
