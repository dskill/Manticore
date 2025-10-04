using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Score {

	public List<FrameScore> frameScores = new List<FrameScore>(12);
	
	public Score()
	{
		for(int i=0; i<12; i++)
			frameScores.Add(new FrameScore());
	}
	
	public void AddScore1(int frame, int score)
	{
		frameScores[frame-1].score1 = score;
	}
	
	public void AddScore2(int frame, int score)
	{
		frameScores[frame-1].score2 = score;
	}
	
	public bool CanGetFrameScore(int frame)
	{		
		for(int i=0; i<frame; i++)
		{
			if(!frameScores[i].frameCompleted)
				return false;
			if(frameScores[i].isSpare && !frameScores[i+1].frameCompleted)
				return false;
			if(frameScores[i].isStrike && !frameScores[i+1].frameCompleted && !frameScores[i+2].frameCompleted)
				return false;
		}
		
		return true;
	}
	
	public int GetFrameScore(int frame)
	{
		int totalScore = 0;
		
		for(int i=0; i<frame; i++)
		{
			if(frameScores[i].isStrike)
			{
				if(frameScores[i+1].bowls == 1)
					totalScore += frameScores[i].score + frameScores[i+1].score1 + frameScores[i+2].score1;
				else
					totalScore += frameScores[i].score + frameScores[i+1].score;
			}
			else if(frameScores[i].isSpare)
				totalScore += frameScores[i].score + frameScores[i+1].score1;
			else
				totalScore += frameScores[i].score;
		}
		
		return totalScore;
	}
	
	public string GetFrameDisplay(int frame)
	{
		string ret_val = frame + ". " + GetFrameScore(frame) + " (";
		
		if(frameScores[frame-1].isStrike)
			ret_val += "X, -)";
		else if(frameScores[frame-1].isSpare)
			ret_val += frameScores[frame-1].score1 + ", /)";
		else
			ret_val += frameScores[frame-1].score+ ", " + frameScores[frame-1].score2 + ") ";
		
		return ret_val;
	}
}
