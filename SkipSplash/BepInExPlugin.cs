using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkipSplash
{
    [BepInPlugin("aedenthorn.SkipSplash", "Skip Splash Screen", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }
        private void Awake()
        {

            context = this;
            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");

            //nexusID = Config.Bind<int>("General", "NexusID", 1, "Nexus mod ID for updates");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }

        //[HarmonyPatch(typeof(GameFlowController), nameof(GameFlowController.SkipIntro))]
        //[HarmonyPatch(MethodType.Getter)]
        static class GameFlowController_SkipIntro_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!modEnabled.Value)
                    return true;
                __result = true;
                return false;
            }
        }
     
        [HarmonyPatch(typeof(GameFlowController), nameof(GameFlowController.SwitchState))]
        static class GameFlowController_SwitchState_Patch
        {
            static void Prefix(ref string name)
            {
                if (!modEnabled.Value || name != "LoadEpilepsyWarning")
                    return;
                name = "LoadAutosaveWarning";
            }
        }

    }
}
