using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using LethalLib.Extras;
using LethalLib.Modules;
using Unity.Netcode.Components;
using UnityEngine;
using UnwantedCompany.MonoBehaviors;
using System.Linq;
using System.Text;
using Unity.Netcode.Samples;
using UnwantedCompany.Extensions;

namespace UnwantedCompany
{
    public class Content
    {
        public static AssetBundle MainBundle;

        public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
		public static List<CustomItem> customItems;

		public static void TryLoadAssets()
        {
			if (MainBundle == null)
			{
				MainBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ucmainbundle"));
				UnwantedCompany.logger.LogInfo("UCMain Bundle Loaded");
			}
		}

		private static void LinkAudioMixersToPrefabs()
        {
			if (BaseGameCaches.masterDiageticMixer)
			{
				foreach (CustomItem ci in customItems)
				{
					GameObject prefab = Prefabs[ci.name];
					UCGrabbableObject ucgo;
					if ((ucgo = prefab.GetComponent<UCGrabbableObject>()) != null && ucgo.injectMasterMixer)
					{
						foreach (var source in prefab.GetComponentsInChildren<AudioSource>())
						{
							if (ucgo.sourcesToLinkToMaster.Contains(source))
                            {
								source.outputAudioMixerGroup = BaseGameCaches.masterDiageticMixer;
								UnwantedCompany.logger.LogDebug(source.outputAudioMixerGroup.ToString());
							}
						}
					}
				}
			}
			else
				UnwantedCompany.logger.LogWarning("MasterDiageticMixer is not cached. Recheck load order.");
        }

		public static void Load()
        {
			TryLoadAssets();

			Patches.GameNetworkManagerStartEvent += LinkAudioMixersToPrefabs;

			customItems = new List<CustomItem>()
			{
				CustomScrap.Add("Greed", "Assets/UCAssets/Items/Greed/GreedBomb.asset",
				Levels.LevelTypes.OffenseLevel |
				Levels.LevelTypes.MarchLevel |
				Levels.LevelTypes.RendLevel |
				Levels.LevelTypes.DineLevel |
				Levels.LevelTypes.TitanLevel,
				50)
			};

			foreach (CustomItem customItem in customItems)
			{
				if (customItem.enabled)
				{
					Item val = MainBundle.LoadAsset<Item>(customItem.itemPath);
					if (val.spawnPrefab.GetComponent<NetworkTransform>() == null)
					{
						val.spawnPrefab.AddComponent<NetworkTransform>();
					}

					Prefabs.Add(customItem.name, val.spawnPrefab);
					NetworkPrefabs.RegisterNetworkPrefab(val.spawnPrefab);
					customItem.itemAction(val);

					if (customItem is CustomScrap)
					{
						Items.RegisterScrap(val, ((CustomScrap)customItem).rarity, ((CustomScrap)customItem).levelType);
					}
				}
			}

            try
            {
                var types = Assembly.GetExecutingAssembly().GetLoadableTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                        if (attributes.Length > 0)
                        {
                            method.Invoke(null, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnwantedCompany.logger.LogDebug(e);
            }


            UnwantedCompany.logger.LogInfo($"UC Content Loaded!");
		}

        public class CustomItem
        {
			public CustomItem(string name, string itemPath, string infoPath, Action<Item> action = null)
			{
				this.name = name;
				this.itemPath = itemPath;
				this.infoPath = infoPath;
				if (action != null)
				{
					this.itemAction = action;
				}
			}

			public static Content.CustomItem Add(string name, string itemPath, string infoPath = null, Action<Item> action = null)
			{
				return new Content.CustomItem(name, itemPath, infoPath, action);
			}

			public string name = "";

			public string itemPath = "";

			public string infoPath = "";

			public Action<Item> itemAction = delegate (Item item)
			{
			};

			public bool enabled = true;


		}

		public class CustomScrap : Content.CustomItem
		{
			public CustomScrap(string name, string itemPath, Levels.LevelTypes levelType, int rarity, Action<Item> action = null) : base(name, itemPath, null, action)
			{
				this.levelType = levelType;
				this.rarity = rarity;
			}

			public static Content.CustomScrap Add(string name, string itemPath, Levels.LevelTypes levelType, int rarity, Action<Item> action = null)
			{
				return new Content.CustomScrap(name, itemPath, levelType, rarity, action);
			}

			public Levels.LevelTypes levelType = Levels.LevelTypes.All;

			public int rarity;
		}
	}
}
