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

public class PuzzleController : MonoBehaviour {

	private enum PuzzleState
	{
		NONE,
		SELECT,
		MOVE,
		END
	};

	public GameObject PiecePrefab;		//!< ピースプレハブ

	private PuzzleState		mState;		//!< 遷移

	private PieceObject mObj;

	private PieceObject[,]	mPieces = new PieceObject[6,5];		//!< ピース管理用
	private ArrayList		mActivList = new ArrayList();		//!< 現在動いているピースリスト

	// Use this for initialization
	void Start () {
		Init();
	}

	/*! 初期化  */
	private void Init()
	{
		mState = PuzzleState.SELECT;

		for (int i = 0; i < mPieces.GetLength(0); i++)
		{
			for (int j = 0; j < mPieces.GetLength(1); j++)
			{
				Vector2 pos = new Vector2(i * 1.05f, 5 + (1.05f * j));
				GameObject obj = Instantiate(PiecePrefab, pos, Quaternion.identity) as GameObject;
				obj.transform.parent = transform;

				mPieces[i, j] = obj.GetComponent<PieceObject>();
				mPieces[i, j].SetPosition(new PiecePos(i, j), 1);
				mPieces[i, j].Color = (PieceColor)Random.Range(1, 6);

				mActivList.Add(mPieces[i, j]);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		GameObject obj = null;

		switch (mState)
		{
			case PuzzleState.NONE:
				break;

			case PuzzleState.SELECT:
				if(Input.GetMouseButtonDown(0))
				{
					obj = MouseUpdate();
				}
				
				if (obj != null)
				{
					mObj = obj.GetComponent<PieceObject>();
					mObj.Catch();
					mActivList.Add(mObj);
					mState = PuzzleState.MOVE;
				}
				break;

			case PuzzleState.MOVE:
				if (Input.GetMouseButtonUp(0) || mObj.transform.position.y > 5.0f)
				{
					mObj.Relese();
					mState = PuzzleState.SELECT;
				}

				obj = MouseUpdate();
				if (obj != null)
				{
					PieceObject p = obj.GetComponent<PieceObject>();
					if (p.GetState() == 0)
					{
						ReplacePiece(mObj, p);
					}
				}
				break;
		}
		TouchUpdate();
		
		PieceUpdate();
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
		}
	}

	/*! ピースの入れ替え	 */
	private void ReplacePiece(PieceObject p1, PieceObject p2)
	{
		PiecePos pos1 = p1.PicecPosition;
		PiecePos pos2 = p2.PicecPosition;

		PieceObject obj = mPieces[pos2.x, pos2.y];
		mPieces[pos2.x, pos2.y] = p1;
		mPieces[pos1.x, pos1.y] = obj;

		p2.SetPosition(pos1, 5);
		p1.PicecPosition = pos2;

		mActivList.Add(p2);
	}

	/*! タッチしてオブジェクトを返す 
		@return タッチされたオブジェクト
	 */
	private GameObject TouchUpdate()
	{
		if(Input.touchCount <= 0)
			return null;

		Touch touch = Input.GetTouch(0);
		if(touch.phase == TouchPhase.Began)
		{
			Vector2 worldPoint2d = Camera.main.ScreenToWorldPoint(touch.position);
			Collider2D collider2D = Physics2D.OverlapPoint(worldPoint2d);

			if (collider2D)
				return collider2D.transform.gameObject;
		}

		return null;
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
}
