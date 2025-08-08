﻿using System.Collections.Generic;
using TMPro;
using UltraBINGO.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoCardPauseMenu {
    public static Dictionary<string, Color> teamColors = new() {
        { "NONE", new Color(1, 1, 1, 1) },
        { "Red", new Color(1, 0, 0, 1) },
        { "Green", new Color(0, 1, 0, 1) },
        { "Blue", new Color(0, 0, 1, 1) },
        { "Yellow", new Color(1, 1, 0, 1) }
    };

    public static GameObject Root;
    public static GameObject Grid;

    public static GameObject inGamePanel;

    public static GameObject DescriptorText;

    public static Outline pingedMap = null;

    public static void onMouseEnterLevelSquare(PointerEventData data) {
        var angryLevelName = data.pointerEnter.gameObject.GetComponent<BingoLevelData>().levelName.ToLower();
        var campaignLevelName = GameManager.CurrentGame.grid.levelTable[data.pointerEnter.gameObject.name].levelId
            .ToLower();

        var path = "assets/bingo/lvlimg/"
                   + (data.pointerEnter.gameObject.GetComponent<BingoLevelData>().isAngryLevel ? "angry" : "campaign")
                   + "/"
                   + (data.pointerEnter.gameObject.GetComponent<BingoLevelData>().isAngryLevel
                       ? angryLevelName
                       : campaignLevelName)
                   + ".png";

        if (!AssetLoader.Assets.Contains(path)) path = "assets/bingo/lvlimg/unknown.png";

        var levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);
        var levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height),
            new Vector2(0.5f, 0.5f), 100.0f);

        GetGameObjectChild(Root, "SelectedLevelImage").GetComponent<Image>().overrideSprite = levelSprite;

        var canReroll = !data.pointerEnter.gameObject.GetComponent<BingoLevelData>().isClaimed
                        && !MonoSingleton<BingoVoteManager>.Instance.voteOngoing;

        var level = GameManager.CurrentGame.grid.levelTable[data.pointerEnter.gameObject.name];

        GetGameObjectChild(GetGameObjectChild(Root, "SelectedLevel"), "Text (TMP)").GetComponent<TextMeshProUGUI>()
                .text = level.levelName
                        + (level.claimedBy != "NONE"
                            ? "\n<color=orange>" + GetFormattedTime(level.timeToBeat) + "</color>"
                            : "")
                        + (canReroll ? "\n<color=orange>R: Start a reroll vote</color>" : "");

        GetGameObjectChild(Root, "SelectedLevel").SetActive(true);
        GetGameObjectChild(Root, "SelectedLevelImage").SetActive(true);
    }

    public static void ShowBingoCardInPauseMenu(ref OptionsManager __instance) {
        var currentGame = GameManager.CurrentGame;
        var templateSquare = GetGameObjectChild(GetGameObjectChild(Root, "Card"), "Image");
        templateSquare.SetActive(false);

        for (var x = 0; x < currentGame.grid.size; x++)
        for (var y = 0; y < currentGame.grid.size; y++) {
            var levelSquare = Object.Instantiate(templateSquare, templateSquare.transform.parent.transform);
            levelSquare.name = x + "-" + y;
            var levelObject = GameManager.CurrentGame.grid.levelTable[levelSquare.name];

            //Set up BingoLevelData
            var bld = levelSquare.AddComponent<BingoLevelData>();
            bld.levelName = levelObject.levelName;
            bld.isAngryLevel = levelObject.isAngryLevel;
            bld.angryParentBundle = levelObject.angryParentBundle;
            bld.angryLevelId = levelObject.angryLevelId;
            bld.column = x;
            bld.row = y;
            bld.isClaimed = levelObject.claimedBy != "NONE";
            bld.claimedTeam = levelObject.claimedBy;

            levelSquare.AddComponent<BingoLevelSquare>();
            levelSquare.GetComponent<Image>().color = teamColors[currentGame.grid.levelTable[x + "-" + y].claimedBy];

            if (GameManager.CurrentRow == x && GameManager.CurrentColumn == y) {
                levelSquare.AddComponent<Outline>();
                levelSquare.GetComponent<Outline>().effectColor = Color.magenta;
                levelSquare.GetComponent<Outline>().effectDistance = new Vector2(2f, -2f);
            }

            levelSquare.AddComponent<EventTrigger>();
            var mouseEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            mouseEnter.callback.AddListener(data => { onMouseEnterLevelSquare((PointerEventData)data); });
            levelSquare.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
            levelSquare.SetActive(true);
        }

        //Center the grid based on grid size.
        var card = GetGameObjectChild(Root, "Card");

        switch (GameManager.CurrentGame.grid.size) {
            case 3: {
                card.transform.localPosition = new Vector3(-65f, 170f, 0f);
                card.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                break;
            }
            case 4: {
                card.transform.localPosition = new Vector3(-82.5f, 185f, 0f);
                card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                break;
            }
            default: {
                break;
            }
        }
    }

    public static GameObject Init(ref OptionsManager __instance) {
        if (Root == null)
            Root = Object.Instantiate(AssetLoader.BingoPauseCard, __instance.pauseMenu.gameObject.transform);
        Root.name = "BingoPauseCard";
        Grid = GetGameObjectChild(Root, "Card");
        Grid.GetComponent<GridLayoutGroup>().constraintCount = GameManager.CurrentGame.grid.size;

        DescriptorText = GetGameObjectChild(GetGameObjectChild(Root, "Descriptor"), "Text (TMP)");

        Root.SetActive(true);

        return Root;
    }
}