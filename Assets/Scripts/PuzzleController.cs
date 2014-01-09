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
public enum PuzzleColor
{
	NONE = 0,
	Red,
	Blue,
	Green,
	Yellow,
	Purple,
	Heart
};

public class PuzzleController : MonoBehaviour {

	public GameObject PiecePrefab;

	private PieceObject[,]	mPieces = new PieceObject[6,5];

	// Use this for initialization
	void Start () {
		for (int i = 0; i < mPieces.GetLength(0); i++)
		{
			for (int j = 0; j < mPieces.GetLength(1); j++)
			{
				Vector2 pos = new Vector2(i * 1.05f, 5 + (1.05f * j));
				GameObject obj = Instantiate(PiecePrefab, pos, Quaternion.identity) as GameObject;
				obj.transform.parent = transform;

				mPieces[i, j] = obj.GetComponent<PieceObject>();
				mPieces[i, j].SetPosition(i, j, 1);
				mPieces[i, j].SetColor((PuzzleColor)Random.Range(1,6));
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < mPieces.GetLength(0); i++)
			for (int j = 0; j < mPieces.GetLength(1); j++)
				mPieces[i, j].SelfUpdate();
	}
}
