using System;
using System.Collections.Generic;
using System.Reflection;
using InControl;
using JetBrains.Annotations;
using Modding;
using Modding.Converters;
using Modding.Menu.Config;
using Newtonsoft.Json;
using SFCore.Generics;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using UObject = UnityEngine.Object;
using UScenes = UnityEngine.SceneManagement;
using Satchel.BetterMenus;

namespace HollowKnightAchievementManager;

/*
 * CHARMED
 * ENCHANTED
 * BLESSED
 * PROTECTED
 * MASKED
 * SOULFUL
 * WORLDSOUL
 * FK_DEFEAT
 * DREAM_FK
 * HORNET_1
 * HORNET_2
 * SOUL_MASTER_DEFEAT
 * BROKEN_VESSEL
 * DREAM_BROKEN_VESSEL
 * DUNG_DEFENDER
 * MANTIS_LORDS
 * COLLECTOR
 * ZOTE
 * ATTUNEMENT
 * AWAKENING
 * ASCENSION
 * GRUBFRIEND
 * METAMORPHOSIS
 * NEGLECT
 * NAILSMITH_KILL
 * NAILSMITH_SPARE
 * QUIRREL_EPILOGUE
 * MOURNER
 * TRAITOR_LORD
 * STAG_STATION_HALF
 * STAG_STATION_ALL
 * TEACHER
 * WATCHER
 * BEAST
 * MAP
 * COLOSSEUM_1
 * COLOSSEUM_2
 * COLOSSEUM_3
 * ENDING_A
 * ENDING_B
 * ENDING_C
 * VOID
 * SPEEDRUN_1
 * SPEEDRUN_2
 * COMPLETION
 * SPEED_COMPLETION
 * STEELSOUL
 * STEELSOUL_COMPLETION
 * HUNTER_1
 * HUNTER_2
 * MR_MUSHROOM
 * DREAM_SOUL_MASTER_DEFEAT
 * WHITE_DEFENDER
 * GREY_PRINCE
 * GRIMM
 * NIGHTMARE_GRIMM
 * BANISHMENT
 * PANTHEON1
 * PANTHEON2
 * PANTHEON3
 * PANTHEON4
 * ENDINGD
 * COMPLETIONGG
 *
 * or get a list from GameManager.instance.achievementHandler.achievementsList
 *
 * title: {key}_TITLE
 * text: {key}_TEXT
 *
 * get value with Platform.Current.EncryptedSharedData.GetBool("{key}", false);
 * set value with Platform.Current.EncryptedSharedData.SetBool("{key}", value);
 */

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
        // todo: implement this
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
            elements.Add(new HorizontalOption(achievement.localizedTitle,
                achievement.localizedText,
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