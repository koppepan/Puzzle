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
	public List<PiecePos> AllJudge()
	{
		List<PiecePos> list = new List<PiecePos>();
		PiecePos nowPos;

		for (int j = 0; j < Height; j++)
		{
			for (int i = 0; i < Width; i++)
			{
				if (mRoot.PiecesList[i, j] != null)
				{
					nowPos = new PiecePos(i, j);
					list = Judge(nowPos);
					if (list.Count != 0)
					{
						return list;
					}
					list.Clear();
				}
			}
		}

		return null;
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

		list = SeekX(list, pos);
		list = ListAdd(list, tempList);
		tempList.Clear();

		list = SeekY(list, pos);
		list = ListAdd(list, tempList);
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
	private List<PiecePos> SeekX(List<PiecePos> origin, PiecePos pos)
	{
		List<PiecePos> tempList = new List<PiecePos>();

		// 右方向に探索
		tempList = SeekProcessX(tempList, pos, true);
		// 左方向に探索
		tempList = SeekProcessX(tempList, pos, false);

		if (tempList.Count >= DeleteCount)
		{
			origin = ListAdd(origin, tempList);

			tempList.Remove(pos);

			foreach (var obj in tempList)
			{
				origin = SeekY(origin, obj);
			}
		}

		return origin;
	}

	//===================================================
	/*!
　　 	@brief		y軸探索
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private List<PiecePos> SeekY(List<PiecePos> origin, PiecePos pos)
	{
		List<PiecePos> tempList = new List<PiecePos>();

		// 上方向に探索
		tempList = SeekProcessY(tempList, pos, true);
		// 下方向に探索
		tempList = SeekProcessY(tempList, pos, false);

		if (tempList.Count >= DeleteCount)
		{
			origin = ListAdd(origin, tempList);

			tempList.Remove(pos);

			foreach (var obj in tempList)
			{
				origin = SeekX(origin, obj);
			}
		}
		
		return origin;
	}

	//===================================================
	/*!
　　 	@brief		まだリスト無いか確認してに加える
	 
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

	//===================================================
	/*!
　　 	@brief		リストの合成
	 
　　 	@date		2014/03/19
　　 	@author		Daichi Horio
　　*/
	//===================================================
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

		// 一番最初に自分自身の座標をリストに追加
		if (list.Count == 0)
		{
			list = ListAdd(list, pos);
		}

		PieceColor color = mRoot.PiecesList[pos.x, pos.y].Color;

		PiecePos temp;
		// 方向によってどっちを探索するか
		if (dir)
			temp = new PiecePos(pos.x + 1, pos.y);
		else
			temp = new PiecePos(pos.x - 1, pos.y);

		if (pos.x < Width - 1 && pos.x > 0)
		{
			if (mRoot.PiecesList[temp.x, temp.y] == null)
			{
				return list;
			}
			else if (mRoot.PiecesList[temp.x, temp.y].Color == color)
			{
				list = ListAdd(list, temp);
				list = SeekProcessX(list, temp, dir);
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

		// 一番最初に自分自身の座標をリストに追加
		if (list.Count == 0)
		{
			list = ListAdd(list, pos);
		}

		PieceColor color = mRoot.PiecesList[pos.x, pos.y].Color;
		
		PiecePos temp;
		// 方向によってどっちを探索するか
		if (dir)
			temp = new PiecePos(pos.x, pos.y + 1);
		else
			temp = new PiecePos(pos.x, pos.y - 1);

		if (pos.y < Height - 1 && pos.y > 0)
		{
			if (mRoot.PiecesList[temp.x, temp.y] == null)
			{
				return list;
			}
			else if (mRoot.PiecesList[temp.x, temp.y].Color == color)
			{
				list = ListAdd(list, temp);
				list = SeekProcessY(list, temp, dir);
			}
		}
		return list;
	}
}
