using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmojiText.Taurus
{
	public static class SpriteAssetUtils
	{
		private static Dictionary<SpriteAsset, Dictionary<string, SpriteInforGroup>> s_dictSpriteAssset= new Dictionary<SpriteAsset, Dictionary<string, SpriteInforGroup>>();

		private static void Cache(SpriteAsset spriteAsset)
		{
            if (s_dictSpriteAssset.ContainsKey(spriteAsset))
			{
				return;
			}

			Dictionary<string, SpriteInforGroup> tmp = null;
            tmp = new Dictionary<string, SpriteInforGroup>();
            s_dictSpriteAssset.Add(spriteAsset, tmp);

			foreach (var item in spriteAsset.ListSpriteGroup)
            {
                if (string.IsNullOrEmpty(item.Tag) == false && item.ListSpriteInfor != null && item.ListSpriteInfor.Count > 0)
                {
					tmp.Add(item.Tag, item);
                }
            }
		}

		public static bool ContainTag(this SpriteAsset spriteAsset, string tag)
		{
			if (spriteAsset == null)
			{
				Debug.LogWarning("spriteAsset is null");
				return false;
			}

			Cache(spriteAsset);

			var dict = s_dictSpriteAssset[spriteAsset];
			return dict.ContainsKey(tag);
        }
       
		public static string[] GetTags(this SpriteAsset spriteAsset)
		{
			if (spriteAsset == null)
			{
				Debug.LogWarning("spriteAsset is null");
				return null;
			}
			
			int len = spriteAsset.ListSpriteGroup.Count;
			if ( len <= 0)
			{
				return null;
			}

			string[] list = new string[len];
			for (int i = 0; i < len; ++i)
			{
				list[i] = spriteAsset.ListSpriteGroup[i].Tag;
			}
			
			return list;
		}

		public static SpriteInforGroup GetSpriteInfoForGroup(this SpriteAsset spriteAsset, string tag)
        {
            if (spriteAsset == null)
            {
                Debug.LogWarning("spriteAsset is null");
                return null;
            }

            Cache(spriteAsset);

			var dict = s_dictSpriteAssset[spriteAsset];
			if(dict.ContainsKey(tag) == false)
			{
				return null;
			}

			return dict[tag];
		}
	}
}
