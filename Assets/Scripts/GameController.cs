using UnityEngine;
using System.Collections;


//===================================================
/*!
 * @brief	ゲームコントローラ
 * 
 * @date	2014/01/09
 * @author	Daichi Horio
*/
//===================================================
public class GameController : MonoBehaviour {

	private PuzzleState			mNowState;
	private PuzzleController	mPuzCon;
	private MouseData			mMouseData;

	// Use this for initialization
	void Start () {
		mNowState = PuzzleState.SELECT;
		mPuzCon = GameObject.Find("PuzzleController").GetComponent<PuzzleController>();
	}

	// マウスデータの取得
	private void MouseUpdate()
	{
		mMouseData.down = Input.GetMouseButtonDown(0);
		mMouseData.up = Input.GetMouseButtonUp(0);

		mMouseData.pos = Input.mousePosition;
	}
	
	// Update is called once per frame
	void Update () {

		MouseUpdate();

		mNowState = mPuzCon.SelfUpdate(mNowState, mMouseData);
	}
}
