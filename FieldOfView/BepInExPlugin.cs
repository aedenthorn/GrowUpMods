using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FieldOfView
{
    [BepInPlugin("aedenthorn.FieldOfView", "Field of View", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<bool> useScrollWheel;
        public static ConfigEntry<float> incrementFast;
        public static ConfigEntry<float> incrementNormal;
        public static ConfigEntry<string> modKeyNormal;
        public static ConfigEntry<string> modKeyFast;
        public static ConfigEntry<string> keyIncrease;
        public static ConfigEntry<string> keyDecrease;
        public static ConfigEntry<int> nexusID;

        public static Dictionary<string, float> cameraManagerFOVs = new Dictionary<string, float>();

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
            
            useScrollWheel = Config.Bind<bool>("Options", "UseScrollWheel", true, "Use scroll wheel to adjust FOV");
            incrementFast = Config.Bind<float>("Options", "IncrementFast", 5, "Fast increment speed.");
            incrementNormal = Config.Bind<float>("Options", "IncrementNormal", 1, "Normal increment speed.");
            modKeyNormal = Config.Bind<string>("Options", "ModKeyNormal", "left ctrl", "Modifier key to increment at normal speed.");
            modKeyFast = Config.Bind<string>("Options", "ModKeyFast", "left alt", "Modifier key to increment at fast speed.");
            keyIncrease = Config.Bind<string>("Options", "KeyIncrease", "", "Key to increase FOV.");
            keyDecrease = Config.Bind<string>("Options", "KeyDecrease", "", "Key to decrease FOV.");

            //nexusID = Config.Bind<int>("General", "NexusID", 1, "Nexus mod ID for updates");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }

        [HarmonyPatch(typeof(CameraManager), "Update")]
        static class CameraManager_Update_Patch
        {
            static void Postfix(CameraManager __instance, CameraStateData ___m_State)
            {
                if (!modEnabled.Value)
                    return;
                string name = __instance.name;
                if (!cameraManagerFOVs.ContainsKey(name))
                    cameraManagerFOVs[name] = ___m_State.m_FieldOfView;
                if (
                    (useScrollWheel.Value && Input.mouseScrollDelta.y != 0 && (CheckKeyHeld(modKeyNormal.Value) || CheckKeyHeld(modKeyFast.Value))) ||
                    ((CheckKeyDown(keyIncrease.Value) || CheckKeyDown(keyDecrease.Value)) && (CheckKeyHeld(modKeyNormal.Value, false) || CheckKeyHeld(modKeyFast.Value, false)))
                )
                {
                    float change = CheckKeyHeld(modKeyFast.Value) ? incrementFast.Value : incrementNormal.Value;

                    if (Input.mouseScrollDelta.y > 0)
                        cameraManagerFOVs[name] -= change;
                    else if (Input.mouseScrollDelta.y < 0)
                        cameraManagerFOVs[name] += change;
                    else if (CheckKeyDown(keyIncrease.Value))
                        cameraManagerFOVs[name] += change;
                    else if (CheckKeyDown(keyDecrease.Value))
                        cameraManagerFOVs[name] -= change;

                    cameraManagerFOVs[name] = Mathf.Clamp(cameraManagerFOVs[name], 1, 180);
                    
                    Dbgl($"camera {name} field of view {cameraManagerFOVs[name]}");
                }

                for (int i = 0; i < __instance.m_GameCameras.Length; i++)
                {
                    __instance.m_GameCameras[i].fieldOfView = cameraManagerFOVs[name];
                }
            }

        }

        public static bool CheckKeyDown(string value)
        {
            try
            {
                return Input.GetKeyDown(value.ToLower());
            }
            catch
            {
                return false;
            }
        }
        public static bool CheckKeyHeld(string value, bool req = true)
        {
            try
            {
                return Input.GetKey(value.ToLower());
            }
            catch
            {
                return !req;
            }
        }
    }
}
