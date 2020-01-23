using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;

namespace CloneDroneModdedMultiplayer.Patches
{
    public class GetLevelDescriptionsPatches
    {
        public static void Prefix(ref Tuple<GameMode, bool> __state)
        {
            GameMode currentGameMode = Accessor.GetPrivateField<GameFlowManager, GameMode>("_gameMode", GameFlowManager.Instance);
            bool cond = currentGameMode == Main.MODDED_MULTIPLAYER_TEXT_GAMEMODE;
            if(cond)
            {
                __state = new Tuple<GameMode, bool>(currentGameMode, cond);

                Accessor.SetPrivateField("_gameMode", GameFlowManager.Instance, GameMode.Story);
            }
        }
        public static List<LevelDescription> Postfix(List<LevelDescription> __result, Tuple<GameMode, bool> __state)
        {
            if(__state == null)
            {
                //debug.Log("__state is null");
                return __result;
            }

            if (__state.Item2)
            {
                //debug.Log("returning newReturnValue");
                Accessor.SetPrivateField("_gameMode", GameFlowManager.Instance, __state.Item1);
                return NewReturnValue;
            }
            //debug.Log("returning defualt");
            return __result;
        }

        static List<LevelDescription> NewReturnValue
        {
            get
            {
                string[] paths = PathUtils.GetModdedLevels(PathUtils.RootPath.Root);
                List<LevelDescription> levels = new List<LevelDescription>();
                for(int i = 0; i < paths.Length; i++)
                {
                    string[] splitPath = paths[i].Split("/\\".ToCharArray());

                    levels.Add(new LevelDescription
                    {
                        LevelJSONPath = paths[i],
                        LevelID = splitPath[splitPath.Length-1],
                        LevelTags = new List<LevelTags>()
                    });
                    
                }

                return levels;
            }
        }

    }

    public class GetCurrentGameDataPatches
    {
        public static GameData Postpatch(GameData __result)
        {
            if (GameModeManager.Is(Main.MODDED_MULTIPLAYER_TEXT_GAMEMODE))
            {
                return Internal.ServerRunner.CurrentGameData;
            }


            return __result;
        }
    }

    public class GameModeManagerAllowsLevelsWithNoEnemiesPatch
    {
        public static bool PostPatch(bool __result)
        {
            if(GameModeManager.Is(Main.MODDED_MULTIPLAYER_TEXT_GAMEMODE))
                return true;
            
            return __result;
        }
    }
}
