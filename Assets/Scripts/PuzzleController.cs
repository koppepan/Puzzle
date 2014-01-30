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
	private struct PieceDeathData
	{
		public int ID;
		public PiecePos Pos;
		public PieceDirction Dir;
		public int Count;

		public PieceDeathData(int id, PiecePos pos, PieceDirction dir, int count)
		{
			ID = id;
			Pos = pos;
			Dir = dir;
			Count = count;
		}
	};

	public GameObject		PiecePrefab;		//!< ピースプレハブ

	static public int		Width		=	6;		//!< ピースの横数
	static public int		Height		=	5;		//!< ピースの縦数

	public float			PieceLength =	1.05f;	//!< ピースの大きさ

	public int				DeleteCount	=	3;		//!< 揃っていたら消す数
	public float			DeleteTime	=	0.5f;	//!< 消すタイムラグ

	public float			InitMoveSpeed	=	1;	//!< 初期化時のピースの移動スピード
	public float			NormalMoveSpeed	=	5;	//!< ノーマル時のピースの移動スピード

	private PieceObject		mNowSelectPiece;					//!< 現在選択されているピースオブジェ

	private PieceObject[,]	mPieces		= new PieceObject[Width, Height];	//!< ピース管理用
	
	private List<PieceObject>		mActivList	= new List<PieceObject>();		//!< 現在動いているピースリスト
	private Queue<PieceDeathData>	mDeathList	= new Queue<PieceDeathData>();	//!< 消すリスト
	

	private float			mTime;	//!< タイムのワーク

	// Use this for initialization
	void Awake()
	{
		Init();

		while (JudgeProcess())
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
				if (SelectUpdate(mouseData))
					nowState++;
				break;

			// ピース移動
			case PuzzleState.MOVE:
				if (MoveUpdate(mouseData))
					nowState++;
				break;

			// 消すかどうか判定
			case PuzzleState.JUDGE:
				// 何もそろってなければセレクトへ
				if (JudgeProcess())
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

		Vector2 pos = new Vector2(initPos.x * PieceLength, PieceLength * initPos.y);
		GameObject obj = Instantiate(PiecePrefab, pos, Quaternion.identity) as GameObject;
		obj.transform.parent = transform;

		p = obj.GetComponent<PieceObject>();
		p.Color = (PieceColor)Random.Range(1, 7);

		return p;
	}

	//===================================================
	/*!
　　 	@brief		タッチ処理
	 
　　		@param		タッチされた座標
		@return		クリックされたオブジェクト
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private GameObject MouseUpdate(Vector3 mousePos)
	{
		int mask = 1 << LayerMask.NameToLayer("Piece");

		Vector2 worldPoint2d = Camera.main.ScreenToWorldPoint(mousePos);
		Collider2D collider2D = Physics2D.OverlapPoint(worldPoint2d, mask);

		if (collider2D)
			return collider2D.transform.gameObject;

		return null;
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
			if (obj.Dead)
			{
				mPieces[obj.PicecPosition.x, obj.PicecPosition.y] = null;
				Destroy(obj.gameObject);
			}
		}
	}

	//===================================================
	/*!
　　 	@brief		移動させるピースの選択
	 
		@param		MouseData	マウスの座標データ
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool SelectUpdate(MouseData mouseData)
	{
		GameObject obj = null;

		if (mouseData.down)
		{
			obj = MouseUpdate(mouseData.pos);
		}

		if (obj == null)
			return false;

		mNowSelectPiece = obj.GetComponent<PieceObject>();
		mNowSelectPiece.Catch();
		mActivList.Add(mNowSelectPiece);

		return true;
	}

	//===================================================
	/*!
　　 	@brief		ピースの移動＆入れ替え
			
		@param		MouseData	マウスのデータ
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool MoveUpdate(MouseData mouseData)
	{
		GameObject obj = MouseUpdate(mouseData.pos);

		// 他のピースに触れたら選択しているピースと位置入れ替え
		if (obj != null)
		{
			PieceObject p = obj.GetComponent<PieceObject>();
			if (p.GetState() == 0)
			{
				ReplacePiece(mNowSelectPiece, p);
			}
		}

		if (mouseData.up || mNowSelectPiece.transform.position.y > (PieceLength * Height))
		{
			mNowSelectPiece.Relese();
			return true;
		}

		return false;
	}

	//===================================================
	/*!
　　 	@brief		ピースの入れ替え
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

	//===================================================
	/*!
　　 	@brief		ピースがそろっているか判定
	 
　　 	@date		2013/01/29
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool JudgeProcess()
	{
		PiecePos nowPos;
		int id = 0;
		List<PieceDeathData> list = new List<PieceDeathData>();

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
						PieceDeathData data = new PieceDeathData(id, nowPos, PieceDirction.WIDTH, val);
						list.Add(data);
						
						id++;
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
						PieceDeathData data = new PieceDeathData(id, nowPos, PieceDirction.HEIGHT, val);
						list.Add(data);

						id++;
						j += (val - 1);
					}
				}
			}
		}

		return SortDeathPieceData(list);
	}

	//===================================================
	/*!
　　 	@brief		消すピースのデータを並び替え
	 
　　 	@date		2013/01/30
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool SortDeathPieceData(List<PieceDeathData> list)
	{
		if (list.Count == 0)
			return false;

		/*for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (i != j)
				{
					PieceDeathData data1 = list[i];
					PieceDeathData data2 = list[j];

					if (JudgeOverLap(data1, data2))
					{
						data2.ID = data1.ID;
						list[i] = data1;
						list[j] = data2;
					}
				}
			}
		}*/

		

		foreach (PieceDeathData obj in list)
		{
			mDeathList.Enqueue(obj);
		}

		return true;
	}

	//===================================================
	/*!
　　 	@brief		ピースの重なり等を判定
	 
　　 	@date		2013/01/30
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private bool JudgeOverLap(PieceDeathData data1, PieceDeathData data2)
	{
		PieceObject p1 = mPieces[data1.Pos.x, data1.Pos.y];
		PieceObject p2 = mPieces[data2.Pos.x, data1.Pos.y];

		if (p1.Color != p2.Color)
			return false;

		return true;
	}

	//===================================================
	/*!
　　 	@brief		X軸の探索
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

	//===================================================
	/*!
　　 	@brief		Y軸の探索
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

	//===================================================
	/*!
　　 	@brief		ピースの削除
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private void DeathPiece(PieceDeathData data)
	{
		if (data.Dir == PieceDirction.WIDTH)
		{
			for (int i = 0; i < data.Count; i++)
			{
				if (mPieces[data.Pos.x + i, data.Pos.y] != null)
				{
					mPieces[data.Pos.x + i, data.Pos.y].Death();
					mActivList.Add(mPieces[data.Pos.x + i, data.Pos.y]);
				}
			}
		}
		else
		{
			for (int i = 0; i < data.Count; i++)
			{
				if (mPieces[data.Pos.x, data.Pos.y + i] != null)
				{
					mPieces[data.Pos.x, data.Pos.y + i].Death();
					mActivList.Add(mPieces[data.Pos.x, data.Pos.y + i]);
				}
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

			if (mDeathList.Count != 0)
			{
				PieceDeathData p = mDeathList.Dequeue();
				DeathPiece(p);
			}
			else
			{
				return true;
			}
		}
		return false;
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
