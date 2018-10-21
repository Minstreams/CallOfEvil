using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCostomize : MonoBehaviour {

	public string[] additions;


	private SkinnedMeshRenderer skin;

	void Start(){
		skin = GetComponentInChildren<SkinnedMeshRenderer>();
		foreach (string i in additions) {
			AddNewSkin (i);
		}
	}


	private void AddNewSkin(string name)
	{
		Object addition = Resources.Load ("Additions/" + name);

		var newObj = Instantiate(addition) as GameObject;
		var newSkin = newObj.GetComponentInChildren<SkinnedMeshRenderer> ();
		newSkin.transform.parent = transform;
		newSkin.rootBone = skin.rootBone;
		newSkin.bones = skin.bones;
//		foreach(var r in newObj.GetComponentsInChildren<SkinnedMeshRenderer>())
//		{
//			
//			r.transform.parent = transform;
//			r.rootBone = skin.rootBone;
//			r.bones = skin.bones;
//		}
		Destroy(newObj);

	}


}
