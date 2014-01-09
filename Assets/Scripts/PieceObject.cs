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

	private enum PuzzleState
	{
		STOP = 0,
		MOVE
	};

	private	PuzzleColor		mColor;		//!< パズルの種類

	private Animator		mAnime;

	private PuzzleState		mState;		//!< 遷移
	private float			mMoveSpeed;	//!< 移動スピード
	private Vector2			mTargetPos;	//!< 移動先

	// Use this for initialization
	void Awake () {
		mAnime = GetComponent<Animator>();
		mMoveSpeed = 0;
		mTargetPos = new Vector2(0,0);
		mState = PuzzleState.STOP;
	}

	/*! ピースの色指定
        @param	color	色
    */
	public void SetColor(PuzzleColor color)
	{
		if (mColor != color)
		{
			mColor = color;
			mAnime.Play(mColor.ToString());
		}
	}
	
	// Update is called once per frame
	public void SelfUpdate () {
		switch (mState)
		{
			case PuzzleState.STOP:
				break;
			case PuzzleState.MOVE:
				MoveUpdate();
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
			mState = PuzzleState.STOP;
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
	public void SetPosition(int x, int y, float speed)
	{
		mState = PuzzleState.MOVE;
		mMoveSpeed = speed;
		mTargetPos = new Vector2(x * 1.05f, y * 1.05f);
	}
}
