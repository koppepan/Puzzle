using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//===================================================
/*!
 * @brief	ピースの選択を管理
 * 
 * @date	2014/03/19
 * @author	Daichi Horio
*/
//===================================================
public class PieceTouch {

	private PuzzleController mRoot;		// 親

	private int Height;			// 縦軸
	private int Width;			// 横軸

	private float PieceSize;	//!< ピースの大きさ
	private float NormalMoveSpeed = 5;	//!< ノーマル時のピースの移動スピード

	private PieceObject		mNowSelectPiece;		//!< 現在選択されているピースオブジェ

	private Queue<PiecePos> mRouteQueue = new Queue<PiecePos>();

	public PieceTouch(PuzzleController parent, int height, int width, float pieceSize)
	{
		mRoot = parent;
		Height = height;
		Width = width;

		PieceSize = pieceSize;
	}

	public Queue<PiecePos> RouteQueue
	{
		get { return mRouteQueue; }
	}

	//===================================================
	/*!
　　 	@brief		移動させるピースの選択
	 
		@param		MouseData	マウスの座標データ
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	public bool SelectUpdate(MouseData mouseData)
	{
		GameObject obj = null;

		if (mouseData.down)
		{
			obj = TouchUpdate(mouseData.pos);
		}

		if (obj == null)
			return false;

		mNowSelectPiece = obj.GetComponent<PieceObject>();
		mNowSelectPiece.Catch();

		mRoot.ActivList = mNowSelectPiece;

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
	public bool MoveUpdate(MouseData mouseData)
	{
		GameObject obj = TouchUpdate(mouseData.pos);

		// 他のピースに触れたら選択しているピースと位置入れ替え
		if (obj != null)
		{
			PieceObject p = obj.GetComponent<PieceObject>();
			if (p.GetState() == 0)
			{
				ReplacePiece(mNowSelectPiece, p);
				mRouteQueue.Enqueue(p.PicecPosition);
			}
		}

		if (mouseData.up || mNowSelectPiece.transform.position.y > (PieceSize * Height))
		{
			mRouteQueue.Enqueue(mNowSelectPiece.PicecPosition);
			mNowSelectPiece.Relese();
			mNowSelectPiece = null;
			return true;
		}

		return false;
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
	private GameObject TouchUpdate(Vector3 mousePos)
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
　　 	@brief		ピースの入れ替え
	 
　　 	@date		2013/01/20
　　 	@author		Daichi Horio
　　*/
	//===================================================
	private void ReplacePiece(PieceObject now, PieceObject any)
	{
		PiecePos pos1 = now.PicecPosition;
		PiecePos pos2 = any.PicecPosition;

		PieceObject obj = mRoot.PiecesList[pos2.x, pos2.y];
		mRoot.PiecesList[pos2.x, pos2.y] = now;
		mRoot.PiecesList[pos1.x, pos1.y] = obj;

		any.SetPosition(pos1, NormalMoveSpeed);
		now.PicecPosition = pos2;

		mRoot.ActivList = any;
	}
}
