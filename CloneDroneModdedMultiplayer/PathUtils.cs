using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace CloneDroneModdedMultiplayer
{
    public static class PathUtils
    {
        public static string[] GetModdedLevels()
        {
            string[] paths = Directory.GetFiles(Application.persistentDataPath + "/ModdedLevels");

			for(int i = 0; i < paths.Length; i++)
			{
				string[] subStrings = paths[i].Split("/\\".ToCharArray());
				paths[i] = Application.persistentDataPath + "/ModdedLevels/" + subStrings[subStrings.Length-1];


			}

            return paths;
        }
        public static string GetRandomModdedLevel(string[] levels)
        {
            return levels[UnityEngine.Random.Range(0, levels.Length)];
        }

    }
}
