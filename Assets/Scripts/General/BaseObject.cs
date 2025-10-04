using UnityEngine;
using System.Collections;

public class BaseObject : MonoBehaviour {
	
	public enum GameState {StartMenu, FrameActive, FrameCompleted, EndCredits}
	
	static public GameLoop gameLoop;
	static public Persistent persistent;
	
	protected Transform _myTransform = null;
	public Transform myTransform
	{
		get { return _myTransform; }
	}
	
	protected GameObject _myGameObject;
	public GameObject myGameObject
	{
		get { return _myGameObject; }
	}
	
	virtual protected void Awake()
	{
		_myTransform = transform;
		_myGameObject = gameObject;
	}
}
