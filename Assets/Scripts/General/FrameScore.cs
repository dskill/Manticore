using UnityEngine;
using System.Collections;

[System.Serializable]
public class FrameScore {
	
	protected int _score1;
	protected bool _score1Set;
	public int score1
	{
		get { return _score1; }
		set { _score1Set = true; _score1 = value; }
	}
	
	protected int _score2;
	protected bool _score2Set;
	public int score2
	{
		get { return _score2; }
		set { _score2Set = true; _score2 = value; }
	}
	public int score
	{
		get { return score1 + score2; }
	}
	public int maxScore = 10;
	
	public int bowls
	{
		get { int i=0; if(_score1Set) i++; if(_score2Set) i++; return i; }
	}
	
	public bool isStrike
	{
		get { return score1 == maxScore; }
	}
	
	public bool isSpare
	{
		get { return !isStrike && score == maxScore; }
	}
	
	public bool frameCompleted
	{
		get { return isStrike || isSpare || (_score1Set && _score2Set); }
	}
	
	public FrameScore()
	{
		_score1 = 0;
		_score1Set = false;
		_score2 = 0;
		_score2Set = false;
	}
	
	public string score1String
	{
		get { if (!_score1Set) return ""; if (isStrike) return "X"; return score1.ToString(); }
	}
	
	public string score2String
	{
		get { if (!_score2Set) return ""; if (isStrike) return "-"; if (isSpare) return "/"; return score2.ToString(); }
	}
}
