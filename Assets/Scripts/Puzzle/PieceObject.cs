using UnityEngine;
using System.Collections;

//===================================================
/*!
 * @brief	パズルピースクラス
 * 
 * @date	2014/01/09
 * @author	Daichi Horio
*/
//===================================================
public class PieceObject : MonoBehaviour
{

	private enum PieceState
	{
		STOP = 0,
		MOVE,
		SELECT,
		DEATH
	};

	private	PieceColor		mColor;		//!< パズルの種類

	private Animator		mAnime;		//!< 色変更のためのアニメーター
	private SpriteRenderer	mRender;	//!< 描画順などをいじるため

	private PiecePos		mPos;		//!< 現在位置

	private PieceState		mState;		//!< 遷移
	private float			mMoveSpeed;	//!< 移動スピード
	private Vector2			mTargetPos;	//!< 移動先

	private bool			mDead;		//!< 死んだフラグ

	// Use this for initialization
	void Awake () {
		mAnime = GetComponent<Animator>();
		mRender = GetComponent<SpriteRenderer>();
		mMoveSpeed = 0;
		mTargetPos = new Vector2(0,0);
		mState = PieceState.STOP;

		mDead = false;
	}

	/*! 現在の遷移取得
        @return		color	色
    */
	public int GetState()
	{
		return (int)mState;
	}

	public bool DeadFlg
	{
		get { return mDead; }
	}
	
	// Update is called once per frame
	public void SelfUpdate () {
		switch (mState)
		{
			case PieceState.STOP:
				break;
			case PieceState.MOVE:
				MoveUpdate();
				break;
			case PieceState.SELECT:
				SelectUpdate();
				break;
			case PieceState.DEATH:
				DeathUpdate();
				break;
		}
	}

	/*! ピースの移動遷移	*/
	private void MoveUpdate()
	{
		Vector2 nowPos = transform.position;

		// ターゲットにたどり着いたらストップ
		if (nowPos == mTargetPos)
		{
			mState = PieceState.STOP;
			return;
		}

		mMoveSpeed ++;

		// x軸の移動
		if (nowPos.x != mTargetPos.x)
			nowPos.x = PieceMove(nowPos.x, mTargetPos.x, mMoveSpeed);

		// y軸の移動
		if (nowPos.y != mTargetPos.y)
			nowPos.y = PieceMove(nowPos.y, mTargetPos.y, mMoveSpeed);

		transform.position = nowPos;
	}

	/*! セレクトされているピースをタッチされたポイントに追従	*/
	private void SelectUpdate()
	{
		Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		transform.position = pos;
	}

	/*! ピースを消す	*/
	private void DeathUpdate()
	{
		Vector3 s = transform.localScale;

		s = new Vector3(s.x - (0.1f * Time.deltaTime * 60), s.y - (0.1f * Time.deltaTime * 60), 1);
		transform.localScale = s;

		if (transform.localScale.x < 0.1f)
		{
			mDead = true;
			mState = PieceState.STOP;
		}
	}

	/*! ピースの移動
        @param	now			現在の座標
        @param	target		ターゲット
		@param	speed		移動速度
    */
	private float PieceMove(float now, float target, float speed)
	{
		if (now > target)
		{
			now -= speed * Time.deltaTime;
			if (now < target)	now = target;
		}
		else
		{
			now += speed * Time.deltaTime;
			if (now > target)	now = target;
		}
		return now;
	}


	/*! ピースに指定の座標をセット
        @param	x		x軸
        @param	y		y軸
		@param	speed	移動速度
    */
	public void SetPosition(PiecePos pos, float speed)
	{
		mState = PieceState.MOVE;
		mMoveSpeed = speed;
		mPos = pos;
		mTargetPos = new Vector2(mPos.x * 1.05f, mPos.y * 1.05f);
	}

	/*! ピースの座標	*/
	public PiecePos PicecPosition
	{
		get { return mPos; }
		set { mPos = value; }
	}

	/*! ピースの色指定
        @param		value	色
		@return		color	色
    */
	public PieceColor Color
	{
		get { return mColor;}
		set 
		{
			mColor = value;
			mAnime.Play(mColor.ToString());
			name = mColor.ToString();
		}
	}

	/*! ピースを選択された	*/
	public void Catch()
	{
		mState = PieceState.SELECT;

		// 描画順を変える
		mRender.sortingOrder = 1;
		
		// 当たり判定のレイヤーを変える
		gameObject.layer = LayerMask.NameToLayer("Select");
	}

	/*! タッチが離された時	*/
	public void Relese()
	{
		SetPosition(mPos, 10);

		// 描画順を変える
		mRender.sortingOrder = 0;

		// 当たり判定のレイヤーを変える
		gameObject.layer = LayerMask.NameToLayer("Piece");
	}

	/* ピースを殺す	*/
	public void Death()
	{
		mState = PieceState.DEATH;

		// 描画順を変える
		mRender.sortingOrder = -1;

		// 当たり判定のレイヤーを変える
		gameObject.layer = LayerMask.NameToLayer("Default");
	}
}
