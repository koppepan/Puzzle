using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//===================================================
/*!
 * @brief	パズルの判定
 * 
 * @date	2014/02/13
 * @author	Daichi Horio
*/
//===================================================
public class PuzzleJudgment {

	private PuzzleController	mRoot;		// 親のパズルコントローラー

	private int Height;			// 縦軸
	private int Width;			// 横軸

	private int DeleteCount;	// 何こそろてったら消すか


	/* コンストラクタ */
	public PuzzleJudgment(PuzzleController parent, int height, int width, int deleteCount)
	{
		mRoot = parent;

		Height = height;
		Width = width;

		DeleteCount = deleteCount;
	}

	//===================================================
	/*!
　　 	@brief		全体のピースがそろっているか判定
	 
　　 	@date		2013/02/13
　　 	@author		Daichi Horio
　　*/
	//===================================================
	public bool AllJudge()
	{
		List<PiecePos> list = new List<PiecePos>();
		PiecePos nowPos;

		// x軸の判定
		for (int j = 0; j < Height; j++)
		{
			for (int i = 0; i < Width; i++)
			{
				if (mRoot.PiecesList[i, j] != null)
				{
					nowPos = new PiecePos(i, j);
					// X軸の探索
					list = SeekProcessX(list, nowPos, true);
					if (list.Count >= DeleteCount)
					{
						return true;
					}
					list.Clear();
				}
			}
		}

		// y軸の判定
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (mRoot.PiecesList[i, j] != null)
				{
					nowPos = new PiecePos(i, j);
					// Y軸の探索
					list = SeekProcessY(list, nowPos, true);
					if (list.Count >= DeleteCount)
					{
						return true;
					}
					list.Clear();
				}
			}
		}

		return false;
	}

	//===================================================
	/*!
　　 	@brief		ピースがそろっているか判定
	 
　　 	@date		2013/02/13
　　 	@author		Daichi Horio
　　*/
	//===================================================
	public List<PiecePos> Judge(PiecePos pos)
	{
		List<PiecePos> list = new List<PiecePos>();
		List<PiecePos> tempList = new List<PiecePos>();

		if (mRoot.PiecesList[pos.x, pos.y] == null)
		{
			return list;
		}

		tempList = SeekX(tempList, pos);
		list = ListAdd(list, tempList);
		
		foreach (var obj in tempList)
		{
			List<PiecePos> temp = new List<PiecePos>();
			temp = SeekY(temp, obj);
			list = ListAdd(list, temp);
		}

		tempList.Clear();


		tempList = SeekY(tempList, pos);
		list = ListAdd(list, tempList);

		foreach (var obj in tempList)
		{
			List<PiecePos> temp = new List<PiecePos>();
			temp = SeekX(temp, obj);
			list = ListAdd(list, temp);
		}
		
		tempList.Clear();

		return list;
	}

	//===================================================
	/*!
　　 	@brief		x軸探索
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private List<PiecePos> SeekX(List<PiecePos> list, PiecePos pos)
	{
		list = SeekProcessX(list, pos, true);
		list = SeekProcessX(list, pos, false);

		if (list.Count < DeleteCount)
		{
			list.Clear();
		}
		return list;
	}

	//===================================================
	/*!
　　 	@brief		y軸探索
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private List<PiecePos> SeekY(List<PiecePos> list, PiecePos pos)
	{
		list = SeekProcessY(list, pos, true);
		list = SeekProcessY(list, pos, false);

		if (list.Count < DeleteCount)
		{
			list.Clear();
		}
		return list;
	}

	//===================================================
	/*!
　　 	@brief		リストに加える
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private List<PiecePos> ListAdd(List<PiecePos> list, PiecePos pos)
	{
		if(!list.Contains(pos))
		{
			list.Add(pos);
		}
		return list;
	}

	private List<PiecePos> ListAdd(List<PiecePos> origin, List<PiecePos> other)
	{
		foreach (var pos in other)
		{
			if (!origin.Contains(pos))
			{
				origin.Add(pos);
			}
		}

		return origin;
	}

	//===================================================
	/*!
　　 	@brief		x軸方向にそろっているか判定
	 
		@param		pos		判定する座標
		@param		dir		どっち方向に判定する
	 
		@return		揃っているピース
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private List<PiecePos> SeekProcessX(List<PiecePos> list, PiecePos pos, bool dir)
	{
		if (mRoot.PiecesList[pos.x, pos.y] == null)
			return list;

		if (list.Count == 0)
		{
			list = ListAdd(list, pos);
		}

		PieceColor color = mRoot.PiecesList[pos.x, pos.y].Color;

		if (dir && pos.x < Width - 1)
		{
			if (mRoot.PiecesList[pos.x + 1, pos.y] == null)
			{
				return list;
			}
			else if (mRoot.PiecesList[pos.x + 1, pos.y].Color == color)
			{
				list = ListAdd(list, new PiecePos(pos.x + 1, pos.y));
				list = SeekProcessX(list, new PiecePos(pos.x + 1, pos.y), true);
			}
		}

		if (!dir && pos.x > 0)
		{
			if (mRoot.PiecesList[pos.x - 1, pos.y] == null)
			{
				return list;
			}
			else if (mRoot.PiecesList[pos.x - 1, pos.y].Color == color)
			{
				list = ListAdd(list, new PiecePos(pos.x - 1, pos.y));
				list = SeekProcessX(list, new PiecePos(pos.x - 1, pos.y), false);
			}
		}
		return list;
	}

	//===================================================
	/*!
　　 	@brief		y軸方向にそろっているか判定
	 
		@param		pos		判定する座標
		@param		dir		どっち方向に判定する
	 
		@return		揃っているピース
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private List<PiecePos> SeekProcessY( List<PiecePos> list, PiecePos pos, bool dir)
	{
		if (mRoot.PiecesList[pos.x, pos.y] == null)
			return list;

		if (list.Count == 0)
		{
			list = ListAdd(list, pos);
		}

		PieceColor color = mRoot.PiecesList[pos.x, pos.y].Color;

		if (dir && pos.y < Height - 1)
		{
			if (mRoot.PiecesList[pos.x, pos.y + 1] == null)
			{
				return list;
			}
			else if (mRoot.PiecesList[pos.x, pos.y + 1].Color == color)
			{
				list = ListAdd(list, new PiecePos(pos.x, pos.y + 1));
				list = SeekProcessY(list, new PiecePos(pos.x, pos.y + 1), true);
			}
		}

		if (!dir && pos.y > 0)
		{
			if (mRoot.PiecesList[pos.x, pos.y - 1] == null)
			{
				return list;
			}
			else if (mRoot.PiecesList[pos.x, pos.y - 1].Color == color)
			{
				list = ListAdd(list, new PiecePos(pos.x, pos.y - 1));
				list = SeekProcessY(list, new PiecePos(pos.x, pos.y - 1), false);
			}
		}
		return list;
	}
}
