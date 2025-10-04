using UnityEngine;
using System.Collections;

public class Persistent : BaseObject {
	
	public tk2dSprite loadingImagePrefab;
	protected tk2dSprite loadingImage;
	
	override protected void Awake()
	{
		base.Awake();
		
		persistent = this;
		
		DontDestroyOnLoad(myGameObject);
		
		if(loadingImagePrefab)
		{
			loadingImage = Instantiate(loadingImagePrefab) as tk2dSprite;
			loadingImage.gameObject.SetActive(false);
		}
	}
	
	static public void LoadIfNeeded()
	{
		if(!persistent)
		{
			GameObject persistentObj = Instantiate(Resources.Load("Persistent")) as GameObject;
			persistentObj.name = "Persistent";
			persistent = persistentObj.GetComponent<Persistent>();
		}
	}
	
	protected void Update()
	{
		if(Application.isLoadingLevel && loadingImage)
		{
			loadingImage.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
			loadingImage.transform.rotation = Quaternion.Inverse(Camera.main.transform.rotation);
			loadingImage.gameObject.SetActive(true);
		}
	}
	
	protected void OnLevelWasLoaded(int level)
	{
		if(loadingImage)
			loadingImage.gameObject.SetActive(false);
	}
}