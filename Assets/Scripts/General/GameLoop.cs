using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLoop : BaseObject {
	
	protected GameState _gameState = GameState.StartMenu;
	public GameState gameState
	{
		get { return _gameState; }
	}
	
	protected int _currentFrame = 0;
	public int currentFrame
	{
		get { return _currentFrame; }
	}
	
	protected int[] frameScore = new int[12];
	
	public int maxThrows = 2;
	public bool gameOver
	{
		get { return currentFrame > maxFrames; }
	}
	protected int maxFrames = 10;
	
	protected int pinsHit = 0;
	protected int throws = 0;
	
	[HideInInspector]
	public Queue<Destructible> destructibles = new Queue<Destructible>();
	public int maxDestructibles = 25;
	public int maxDestructiblesMobile = 50;
	
	public Score score = new Score();
	[HideInInspector]
	public List<Pin> activePins = new List<Pin>();
	
	public float curTime = 0f;
	
	public List<SyncedPinPositions> pinPositions = new List<SyncedPinPositions>();
	protected PosAndTime nextWaypoint = null;
	
	public List<FrameDisplay> frameDisplays;
	public ParticleSystem cameraSplatter;
	public ParticleSystem cameraSplatterHeavy;
	
	public GameObject player;
	public GameObject startMenu;
	public tk2dTextMesh roundPhrase;
	public tk2dTextMesh roundInfo;
	public List<string> roundPhrases;
	public List<string> names;
	
	public tk2dTextMesh nameTextPrefab;
	public tk2dTextMesh headerTextPrefab;
	public tk2dTextMesh rollScoreText;
	public Transform obitStartMarker;
	public Camera uiCamera;
	private Transform uiCameraTransform;
	private float creditsTimer;
	private bool creditsOver = false;
	private float creditsEndY;
	private float roundPhraseTimer = Mathf.Infinity;
	
	private bool splatteredThisFrame = false;
	
	public AudioClip[] clipsFrameStarted;

	override protected void Awake()
	{
		Persistent.LoadIfNeeded();
		
		base.Awake();
		
		gameLoop = this;
		score = new Score();
		
		if (uiCamera)
			uiCameraTransform = uiCamera.transform;
		
		if(Application.platform == RuntimePlatform.IPhonePlayer)
			maxDestructibles = maxDestructiblesMobile;
	}
	
	protected void Start()
	{
		InitFrame();
	}
	
	protected void OnEnable()
	{
		EventManager.PinHit += PinHit;
		EventManager.BallReturned += BallReturned;
		EventManager.FrameCompleted += FrameCompleted;
	}
	
	protected void OnDisable()
	{
		EventManager.PinHit -= PinHit;
		EventManager.BallReturned -= BallReturned;
		EventManager.FrameCompleted -= FrameCompleted;
	}
	
	protected void Update()
	{
		if (_gameState == BaseObject.GameState.StartMenu)
		{
			if (Input.GetButtonUp("Fire1"))
			{
				startMenu.SetActive(false);
				player.SetActive(true);
				ShowRoundPhrase(1);
				ShowRoundInfo(true);
				_gameState = BaseObject.GameState.FrameActive;
			}
		}
		
		if(gameOver)
		{
			if (_gameState == GameState.EndCredits)
			{
				UpdateGameOver();
			}
			else
			{
				InitiateGameOver();
				_gameState = GameState.EndCredits;
			}
			return;
		}
		
		if (Time.time > roundPhraseTimer)
			UpdateFadeOutPhrase();
		
		if(nextWaypoint != null)
		{
			curTime += Time.deltaTime;
			if(curTime > nextWaypoint.time)
				GoToNextWaypoint();
		}
		
//		DebugInput();
	}
	
	protected void PinHit(Pin pin)
	{
		pinsHit++;
	}
	
	protected void BallReturned()
	{
		throws++;
		
		if(throws == 1)
		{
			score.AddScore1(currentFrame, pinsHit);
			ShowRoundInfo(false);
			if(score.frameScores[currentFrame-1].isStrike)
				StartCoroutine(DisplayRollScore("X"));
			else
				StartCoroutine(DisplayRollScore(pinsHit.ToString()));
		}
		else
		{
			score.AddScore2(currentFrame, pinsHit);
			if(score.frameScores[currentFrame-1].isSpare)
				StartCoroutine(DisplayRollScore("/"));
			else
				StartCoroutine(DisplayRollScore(pinsHit.ToString()));
		}
		
		pinsHit = 0;
		
		UpdateScoreDisplay();
		
		splatteredThisFrame = false;
		
		if(throws >= maxThrows || activePins.Count == 0)
			EventManager.frameCompleted(currentFrame);
	}
	
	protected void FrameCompleted(int frame)
	{
		StartCoroutine(ClearFrame());
	}
	
	protected void InitFrame()
	{
		if(clipsFrameStarted.Length > 0 && player && player.GetComponent<AudioSource>().enabled)
			player.GetComponent<AudioSource>().PlayOneShot(clipsFrameStarted[Random.Range(0, clipsFrameStarted.Length)]);
		_currentFrame++;
		curTime = 0;
		throws = pinsHit = 0;
		EventManager.frameStarted(currentFrame);
		score.frameScores[currentFrame-1].maxScore = activePins.Count;
		if (_currentFrame > 1)
		{
			ShowRoundPhrase(_currentFrame);
			ShowRoundInfo(true);
		}
		GoToNextWaypoint();
	}
			
	protected void GoToNextWaypoint()
	{	
		if(gameOver)
			return;
		
		curTime = 0f;
		nextWaypoint = pinPositions[currentFrame-1].GetNextPosAndTime();
		EventManager.goToWaypoint(nextWaypoint.node, nextWaypoint.time);
	}
	
	protected IEnumerator ClearFrame()
	{
		yield return null;

		InitFrame();
	}
	
	public void AddDestructible(Destructible destructible)
	{
		if(destructibles.Count >= maxDestructibles)
			RemoveDestructible();
		
		destructibles.Enqueue(destructible);
	}
	
	public void RemoveDestructible()
	{
		Destructible destructible = destructibles.Dequeue();
		if(destructible)
			ObjectPoolManager.DestroyPooled(destructible.gameObject);
	}
	
	protected void DebugInput()
	{
		if(Input.GetKeyDown(KeyCode.X) && activePins.Count > 0)
			for(int i=activePins.Count-1; i>=0; i--)
				activePins[i].DebugPinHit();
	}
	
	void OnGUI()
	{
//		int height = 25;
//		int width = 200;
//		GUI.Label(new Rect(0, 0, width, height), "Pins Hit: " + pinsHit + " Pins Remaining: " + activePins.Count);
//		GUI.Label(new Rect(0, height, width, height), "Throws: " + throws);
//		GUI.Label(new Rect(0, 2*height, width, height), "Frame: " + currentFrame);
//		
//		string scoreString = "Score: " + score.GetFrameScore(currentFrame) + " - ";
//		for(int i=1; i<=currentFrame; i++)
//			scoreString += score.GetFrameDisplay(i) + " | ";
//		
//		GUI.Label(new Rect(0, 4*height, width * 3, 3*height), scoreString);
		if(gameOver && GUI.Button(new Rect((Screen.width*4) / 5, (Screen.height*4) / 5, Screen.width /5, Screen.height / 5), "Play Again"))
			Application.LoadLevel(Application.loadedLevel);
	}
	
	private void UpdateScoreDisplay()
	{		
		string t, f, s;
		for(int i=1; i<=currentFrame; i++)
		{
			if (score.CanGetFrameScore(i))
				t = score.GetFrameScore(i).ToString();
			else
				t = "";
			
			f = score.frameScores[i-1].score1String;
			s = score.frameScores[i-1].score2String;
			
			frameDisplays[i-1].SetScore(t,f,s);
		}
	}
	
	public void PlaySplatter()
	{
		if (!splatteredThisFrame)
		{
			splatteredThisFrame = true;
			if (_currentFrame >= 7)
			{
				if (cameraSplatterHeavy)
					cameraSplatterHeavy.Play();
			}
			else
			{
				if (cameraSplatter)
					cameraSplatter.Play();
			}
		}
	}
	
	private void ShowRoundPhrase(int round)
	{
		if (roundPhrases.Count < 1 || !roundPhrase)
			return;
		
		int i = round-1;
		i = Mathf.Clamp(i, 0, roundPhrases.Count-1);
		roundPhrase.text = roundPhrases[i];
		roundPhrase.color = Color.white;
		roundPhrase.Commit();
		roundPhrase.GetComponent<Renderer>().enabled = true;
		roundPhraseTimer = Mathf.Infinity;
	}
	
	public void HideRoundPhrase()
	{
		if (roundPhrase)
		{
			roundPhraseTimer = Time.time + 1;
		}
	}
	
	private void CreateObituary()
	{
		if (!obitStartMarker)
			return;
		
		GameObject headerTextPrefabObj = headerTextPrefab.gameObject;
		Transform headerTextPrefabTransform = headerTextPrefab.transform;
		GameObject nameTextPrefabObj = nameTextPrefab.gameObject;
		Transform nameTextPrefabTransform = nameTextPrefab.transform;
		
		for (int i = 0;i < names.Count; i++)
		{
			string name = names[i];
			int r = Random.Range(0, names.Count);
			names[i] = names[r];
			names[r] = name;
		}
		
		tk2dTextMesh text = ObjectPoolManager.CreatePooled(headerTextPrefabObj, obitStartMarker.position, headerTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "TOTAL SCORE " + score.GetFrameScore(10);
		text.Commit();
		
		Vector3 pos = obitStartMarker.position;
		pos.y -= 0.6f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(headerTextPrefabObj, obitStartMarker.position, headerTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "FALLEN PINS";
		text.Commit();
		
		pos = obitStartMarker.position;
		pos.y -= 0.3f;
		obitStartMarker.position = pos;
		
		int nameIdx = 0;
		
		for (int i = 0; i < 10; i++)
		{
			text = ObjectPoolManager.CreatePooled(headerTextPrefabObj, obitStartMarker.position, headerTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
			text.text = "FRAME " + (i+1);
			text.Commit();
			
			pos.y -= 0.2f;
			obitStartMarker.position = pos;
			
			for (int j = 0; j < score.frameScores[i].score; j++)
			{
				text = ObjectPoolManager.CreatePooled(nameTextPrefabObj, obitStartMarker.position, nameTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
				text.text = names[nameIdx];
				text.Commit();
				nameIdx++;
				if (nameIdx >= names.Count)
					nameIdx = 0;
				
				pos.y -= 0.15f;
				obitStartMarker.position = pos;
			}
			
			pos.y -= 0.2f;
			obitStartMarker.position = pos;
		}
		
		pos.y -= 0.1f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(headerTextPrefabObj, obitStartMarker.position, headerTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "ADDRESS ALL COMPLAINTS TO";
		text.Commit();
		
		pos.y -= 0.2f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(headerTextPrefabObj, obitStartMarker.position, headerTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "TEAM MANticore";
		text.Commit();
		
		pos.y -= 0.2f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(nameTextPrefabObj, obitStartMarker.position, nameTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "Lee Petty";
		text.Commit();
		
		pos.y -= 0.15f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(nameTextPrefabObj, obitStartMarker.position, nameTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "Drew Skillman";
		text.Commit();
		
		pos.y -= 0.15f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(nameTextPrefabObj, obitStartMarker.position, nameTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "Patrick Connor";
		text.Commit();
		
		pos.y -= 0.15f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(nameTextPrefabObj, obitStartMarker.position, nameTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "William Gahr";
		text.Commit();
		
		pos.y -= 0.15f;
		obitStartMarker.position = pos;
		
		text = ObjectPoolManager.CreatePooled(nameTextPrefabObj, obitStartMarker.position, nameTextPrefabTransform.rotation).GetComponent<tk2dTextMesh>();
		text.text = "Alex Vaughan";
		text.Commit();
		
		creditsEndY = pos.y - 1.3f;
	}
	
	private void InitiateGameOver()
	{	
		if (!uiCameraTransform)
			return;
		
		CreateObituary();
		
		Vector3 pos = uiCameraTransform.position;
		pos.x = obitStartMarker.position.x;
		uiCameraTransform.position = pos;
		creditsTimer = Time.time + 2f;
		
		player.SetActive(false);
	}
	
	private void UpdateGameOver()
	{
		if (creditsOver || Time.time < creditsTimer)
			return;
		
		Vector3 pos = uiCameraTransform.position;
		pos.y -= 0.3f * Time.deltaTime;
		if (pos.y <= creditsEndY)
		{
			pos.y = creditsEndY;
			creditsOver = true;
		}
		
		uiCameraTransform.position = pos;
	}
	
	private void UpdateFadeOutPhrase()
	{
		if (!roundPhrase || !roundInfo)
			return;
		
		float f = Time.time - roundPhraseTimer;
		
		if (f > 1)
		{
			roundPhrase.GetComponent<Renderer>().enabled = false;
			roundInfo.GetComponent<Renderer>().enabled = false;
			return;
		}
		
		Color fadedOut = new Color(1,1,1,0);
		Color newColor = Color.Lerp(Color.white, fadedOut, f);
		roundPhrase.color = newColor;
		roundPhrase.Commit();
		roundInfo.color = newColor;
		roundInfo.Commit();
	}
	
	private IEnumerator DisplayRollScore(string score)
	{
		float timeTillFade = 3f;
		float fadeTime = 1f;
		float curTime = -Time.deltaTime;
		Color defaultColor = new Color(149f/255, 39f/255, 22f/255, 1);
		Color fadedOut = new Color(149f/255,39f/255,22f/255,0);
		
		rollScoreText.GetComponent<Renderer>().enabled = true;
		rollScoreText.text = score;
		rollScoreText.color = defaultColor;
		rollScoreText.Commit();
		
		
		
		while(curTime < timeTillFade + fadeTime)
		{
			curTime += Time.deltaTime;
			if(curTime > timeTillFade)
			{
				Color newColor = Color.Lerp(defaultColor, fadedOut, (curTime - timeTillFade) / fadeTime);
				rollScoreText.color = newColor;
				rollScoreText.Commit();
			}
			yield return null;
		}
		
		rollScoreText.GetComponent<Renderer>().enabled = false;
	}
	
	public void ShowRoundInfo(bool firstThrow)
	{
		if (!roundInfo)
			return;
		
		roundInfo.text = "Frame " + _currentFrame + "\n" + (firstThrow ? "1st Roll" : "2nd Roll");
		roundInfo.color = Color.white;
		roundInfo.Commit();
		roundInfo.GetComponent<Renderer>().enabled = true;
		roundPhraseTimer = Mathf.Infinity;
	}
}
