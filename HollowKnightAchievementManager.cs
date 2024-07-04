using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using SFCore.Utils;
using UObject = UnityEngine.Object;
using UScenes = UnityEngine.SceneManagement;
using Satchel.BetterMenus;
using UnityEngine;

namespace HollowKnightAchievementManager;

[UsedImplicitly]
public class HollowKnightAchievementManager : Mod, ICustomMenuMod
{
    bool ICustomMenuMod.ToggleButtonInsideMenu => true;
    public Menu MenuRef;

    public override string GetVersion() => Util.GetVersion(Assembly.GetExecutingAssembly());

    public HollowKnightAchievementManager() : base("Hollow Knight Achievement Manager")
    {
    }

    public override void Initialize()
    {
        Log("Initializing");

        Log("Initialized");
    }

    private void SetAchievementUnlocked(string achievementKey, bool val)
    {
        DesktopOnlineSubsystem onlineSubsystem = (Platform.Current as DesktopPlatform).GetAttr<DesktopPlatform, DesktopOnlineSubsystem>("onlineSubsystem");
        if (onlineSubsystem != null)
        {
            if (onlineSubsystem is GameCoreOnlineSubsystem gameCore)
            {
                gameCore.SetAchievementStatus(achievementKey, val);
            }
            else if (onlineSubsystem is GOGGalaxyOnlineSubsystem gogGalaxy)
            {
                gogGalaxy.SetAchievementStatus(achievementKey, val);
            }
            else if (onlineSubsystem is SteamOnlineSubsystem steam)
            {
                steam.SetAchievementStatus(achievementKey, val);
            }
        }

        Platform.Current.EncryptedSharedData.SetBool(achievementKey, val);
    }

    private bool GetAchievementUnlocked(string achievementKey)
    {
        return GameManager.instance.IsAchievementAwarded(achievementKey);
    }

    public Menu PrepareMenu()
    {
        List<Element> elements = new List<Element>();
        elements.Add(new TextPanel("Achievements"));

        foreach (Achievement achievement in GameManager.instance.achievementHandler.achievementsList.achievements)
        {
            elements.Add(new HorizontalOption(Language.Language.Get(achievement.localizedTitle, "Achievements"),
                Language.Language.Get(achievement.localizedText, "Achievements"),
                new[] { "Locked", "Unlocked" },
                (option) => SetAchievementUnlocked(achievement.key, option == 1),
                () => GetAchievementUnlocked(achievement.key) ? 1 : 0));
        }

        return new Menu("Achievements Manager", elements.ToArray());
    }

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        MenuRef ??= PrepareMenu();

        return MenuRef.GetMenuScreen(modListMenu);
    }
}

public static class GameCoreExtension
{
    // reimplementation of GameCoreOnlineSubsystem.PushAchievementUnlock
    public static void SetAchievementStatus(this GameCoreOnlineSubsystem gameCore, string achievementKey, bool val)
    {
        AchievementIDMap achievementIdMap = gameCore.GetAttr<GameCoreOnlineSubsystem, AchievementIDMap>("achievementIdMap");
        HashSet<int> awardedAchievements = gameCore.GetAttr<GameCoreOnlineSubsystem, HashSet<int>>("awardedAchievements");
        XGamingRuntime.XUserHandle _userHandle = gameCore.GetAttr<GameCoreOnlineSubsystem, XGamingRuntime.XUserHandle>("_userHandle");
        XGamingRuntime.XblContextHandle _xblContextHandle = gameCore.GetAttr<GameCoreOnlineSubsystem, XGamingRuntime.XblContextHandle>("_xblContextHandle");

        MethodInfo succeededMethod = typeof(GameCoreOnlineSubsystem).GetMethod("Succeeded", BindingFlags.NonPublic | BindingFlags.Static);
        bool returnValue;

        int? serviceId = ((achievementIdMap != null) ? achievementIdMap.GetServiceIdForInternalId(achievementKey) : null);
        if (serviceId == null || achievementIdMap == null)
        {
            return;
        }

        ulong num;
        returnValue = (bool)succeededMethod.Invoke(null, new object[] { XGamingRuntime.SDK.XUserGetId(_userHandle, out num), "Get Xbox user ID" });
        if (!returnValue)
        {
            return;
        }

        HashSet<int> currentAwardedAchievements = awardedAchievements;
        if (currentAwardedAchievements.Contains(serviceId.Value))
        {
            return;
        }

        XGamingRuntime.SDK.XBL.XblAchievementsUpdateAchievementAsync(_xblContextHandle, num, serviceId.Value.ToString(), val ? 100U : 0U, delegate(int hresult)
        {
            returnValue = (bool)succeededMethod.Invoke(null, new object[] { hresult, "Unlock achievement" });
            if (returnValue)
            {
                currentAwardedAchievements.Add(serviceId.Value);
            }
        });
    }
}

public static class GogGalaxyExtension
{
    // reimplementation of GOGGalaxyOnlineSubsystem.PushAchievementUnlock
    public static void SetAchievementStatus(this GOGGalaxyOnlineSubsystem gogGalaxy, string achievementKey, bool val)
    {
        object authorization = typeof(GOGGalaxyOnlineSubsystem).GetField("authorization", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gogGalaxy);
        bool isAuthorized = (bool)authorization.GetType().GetField("isAuthorized", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(authorization);
        if (isAuthorized)
        {
            try
            {
                if (val)
                {
                    Galaxy.Api.GalaxyInstance.Stats().SetAchievement(achievementKey);
                }
                else
                {
                    Galaxy.Api.GalaxyInstance.Stats().ClearAchievement(achievementKey);
                }

                Galaxy.Api.GalaxyInstance.Stats().StoreStatsAndAchievements();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}

public static class SteamExtension
{
    // reimplementation of SteamOnlineSubsystem.PushAchievementUnlock
    public static void SetAchievementStatus(this SteamOnlineSubsystem steam, string achievementKey, bool val)
    {
        if (steam.GetAttr<SteamOnlineSubsystem, bool>("didInitialize"))
        {
            try
            {
                if (val)
                {
                    Steamworks.SteamUserStats.SetAchievement(achievementKey);
                }
                else
                {
                    Steamworks.SteamUserStats.ClearAchievement(achievementKey);
                }

                Steamworks.SteamUserStats.StoreStats();
                Debug.LogFormat("Pushing achievement {0} with value {1}", new object[] { achievementKey, val });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}