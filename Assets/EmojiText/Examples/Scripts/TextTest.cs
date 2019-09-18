using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmojiText.Taurus;

public class TextTest : MonoBehaviour
{
    public InlineText t1 = null;
    public InlineText t2 = null;

	private string GenAllTags(InlineText text)
	{
		string str = "";
		string[] ary = text._spriteAsset.GetTags();
		for (int i = 0, iMax = ary.Length; i < iMax; ++i)
		{
			str += string.Format("[#{0}]", ary[i]);
			if ((i + 1) % 5 == 0)
			{
				str += "\n";
			}
		}

        return str;
	}

	// Use this for initialization
	void Start () {
		t1.text = string.Format("{0}\n{1}", @"<color=black>静态表情</color>", GenAllTags(t1));
		t2.text = string.Format("{0}\n{1}", @"<color=black>动态态表情</color>",  GenAllTags(t2));
	}
}
