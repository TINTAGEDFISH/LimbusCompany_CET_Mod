using System;
using System.Collections.Generic;
using BattleUI.BattleUnit;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.IO;
using MainUI;
using UnityEngine;

namespace Lethe.Texture
{
    // Token: 0x0200002F RID: 47
    public class Texture : Il2CppSystem.Object
    {
        // Token: 0x060000FD RID: 253 RVA: 0x0000AE23 File Offset: 0x00009023
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<Texture>();
            harmony.PatchAll(typeof(Texture));
        }
        /*
        // Token: 0x060000FE RID: 254 RVA: 0x0000AE40 File Offset: 0x00009040
        [HarmonyPatch(typeof(LobbyUIPresenter), "Initialize")]
        [HarmonyPostfix]
        private static void PostMainUILoad()
        {
            Texture.spritePaths.Clear();
            foreach (string path in Directory.GetDirectories(LetheMain.modsPath.FullPath))
            {
                string path2 = Path.Combine(path, "custom_sprites");
                bool flag = !Directory.Exists(path2);
                if (!flag)
                {
                    foreach (string item in Directory.GetFiles(path2, "*.png", SearchOption.AllDirectories))
                    {
                        Texture.spritePaths.Add(item);
                    }
                }
            }
        }
        */
        // Token: 0x060000FF RID: 255 RVA: 0x0000AF0C File Offset: 0x0000910C
        public static Sprite LoadSpriteFromFile(string fileName)
        {
            bool flag2;
            bool flag = Texture.SpriteExist.TryGetValue(fileName, out flag2) && !flag2;
            Sprite result;
            if (flag)
            {
                result = null;
            }
            else
            {
                Sprite sprite;
                bool flag3 = Texture.Sprites.TryGetValue(fileName, out sprite) && sprite != null;
                if (flag3)
                {
                    result = sprite;
                }
                else
                {
                    try
                    {
                        string path = Texture.spritePaths.Find((string x) => x.Contains(fileName));
                        bool flag4 = !File.Exists(path);
                        if (flag4)
                        {
                            Texture.SpriteExist[fileName] = false;
                            result = null;
                        }
                        else
                        {
                            Texture.SpriteExist[fileName] = true;
                            Il2CppStructArray<byte> il2CppStructArray = File.ReadAllBytes(path);
                            Texture2D texture2D = new Texture2D(2, 2);
                            ImageConversion.LoadImage(texture2D, il2CppStructArray);
                            sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f);
                            Texture.Sprites[fileName] = sprite;
                            result = sprite;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        /*
                        ManualLogSource log = LetheHooks.LOG;
                        string str = "Error loading sprite ";
                        string fileName2 = fileName;
                        string str2 = ": ";
                        System.Exception ex2 = ex;
                        log.LogError(str + fileName2 + str2 + ((ex2 != null) ? ex2.ToString() : null));*/
                        result = null;
                    }
                }
            }
            return result;
        }

        // Token: 0x06000100 RID: 256 RVA: 0x0000B088 File Offset: 0x00009288
        [HarmonyPatch(typeof(PlayerUnitSpriteList), "GetNormalProfileSprite")]
        [HarmonyPrefix]
        private static bool GetNormalProfileSprite(int personalityId, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("profile_" + personalityId.ToString());
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }

        // Token: 0x06000101 RID: 257 RVA: 0x0000B0C4 File Offset: 0x000092C4
        [HarmonyPatch(typeof(PlayerUnitSpriteList), "GetInfoSprite")]
        [HarmonyPrefix]
        private static bool GetInfoSprite(int personalityId, ref Sprite __result)
        {
            return Texture.GetNormalProfileSprite(personalityId, ref __result);
        }

        // Token: 0x06000102 RID: 258 RVA: 0x0000B0E0 File Offset: 0x000092E0
        [HarmonyPatch(typeof(PlayerUnitSpriteList), "GetCGData")]
        [HarmonyPrefix]
        private static bool GetCGData(int personalityId, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("cg_" + personalityId.ToString());
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }

        // Token: 0x06000104 RID: 260 RVA: 0x0000B19C File Offset: 0x0000939C
        [HarmonyPatch(typeof(BuffStaticData), "GetBuffIconSprite")]
        [HarmonyPrefix]
        private static bool GetBuffIconSprite(BuffStaticData __instance, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("buffIcon_" + __instance.GetBuffIconID());
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }

        // Token: 0x06000105 RID: 261 RVA: 0x0000B1D8 File Offset: 0x000093D8
        [HarmonyPatch(typeof(BuffStaticData), "GetBuffTypoIconSprite")]
        [HarmonyPrefix]
        private static bool GetBuffTypoIconSprite(BuffStaticData __instance, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("buffIcon_" + __instance.GetBuffIconID());
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }

        // Token: 0x06000106 RID: 262 RVA: 0x0000B214 File Offset: 0x00009414
        [HarmonyPatch(typeof(SkillModel), "GetSkillSprite")]
        [HarmonyPrefix]
        private static bool GetSkillSprite(SkillModel __instance, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("skill_" + __instance.GetSkillIconID()) ?? Texture.LoadSpriteFromFile("skill_" + __instance.GetID().ToString());
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }



        // Token: 0x06000108 RID: 264 RVA: 0x0000B2FC File Offset: 0x000094FC
        [HarmonyPatch(typeof(SkillIconUI), "GetEgoProfileSpriteAsync")]
        [HarmonyPrefix]
        private static void GetEgoProfileSpriteAsync(Il2CppSystem.Action<Sprite> onLoadAsset, int id, bool isErosion, SkillIconUI __instance)
        {
            string str;
            if (isErosion)
            {
                str = "skill_corrosion_";
            }
            else
            {
                str = "skill_awakening_";
            }
            Sprite sprite = Texture.LoadSpriteFromFile(str + id.ToString()) ?? Texture.LoadSpriteFromFile(str + id.ToString());
            bool flag = sprite == null;
            if (!flag)
            {
                onLoadAsset.Invoke(sprite);
            }
        }

        // Token: 0x06000109 RID: 265 RVA: 0x0000B364 File Offset: 0x00009564
        [HarmonyPatch(typeof(PlayerUnitSpriteList), "GetEgoCGData")]
        [HarmonyPrefix]
        private static bool GetEgoCGData(string cgId, PlayerUnitSpriteList __instance, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("egocg_" + cgId) ?? Texture.LoadSpriteFromFile("ego_cg" + cgId);
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }

        // Token: 0x0600010A RID: 266 RVA: 0x0000B3B0 File Offset: 0x000095B0
        [HarmonyPatch(typeof(PlayerUnitSpriteList), "GetEgoBannerSprite")]
        [HarmonyPrefix]
        private static bool GetEgoProfileData(int egoId, PlayerUnitSpriteList __instance, ref Sprite __result)
        {
            Sprite sprite = Texture.LoadSpriteFromFile("egobanner_" + egoId.ToString()) ?? Texture.LoadSpriteFromFile("egobanner_" + egoId.ToString());
            bool flag = sprite == null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                __result = sprite;
                result = false;
            }
            return result;
        }

        // Token: 0x04000070 RID: 112
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        // Token: 0x04000071 RID: 113
        private static readonly Dictionary<string, bool> SpriteExist = new Dictionary<string, bool>();

        // Token: 0x04000072 RID: 114
        public static List<string> spritePaths = new List<string>();
    }
}
