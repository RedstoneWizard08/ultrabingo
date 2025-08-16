using System.Collections.Generic;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoCardPauseMenu {
    public static readonly Dictionary<string, Color> TeamColors = new() {
        { "NONE", new Color(1, 1, 1, 1) },
        { "Red", new Color(1, 0, 0, 1) },
        { "Green", new Color(0, 1, 0, 1) },
        { "Blue", new Color(0, 0, 1, 1) },
        { "Yellow", new Color(1, 1, 0, 1) }
    };

    public static GameObject? Root;
    public static GameObject? Grid;
    public static GameObject? inGamePanel;
    public static GameObject? DescriptorText;
    public static Outline? pingedMap = null;

    private static void OnMouseEnterLevelSquare(PointerEventData data) {
        var angryLevelName = data.pointerEnter.gameObject.GetComponent<BingoLevelData>().LevelName.ToLower();
        var campaignLevelName = GameManager.CurrentGame.Grid.LevelTable[data.pointerEnter.gameObject.name].LevelId
            .ToLower();

        var path =
            $"assets/bingo/lvlimg/{(data.pointerEnter.gameObject.GetComponent<BingoLevelData>().IsAngryLevel ? "angry" : "campaign")}/{(data.pointerEnter.gameObject.GetComponent<BingoLevelData>().IsAngryLevel
                ? angryLevelName
                : campaignLevelName)}.png";

        if (!AssetLoader.Assets.Contains(path)) path = "assets/bingo/lvlimg/unknown.png";

        var levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);
        var levelSprite = Sprite.Create(
            levelImg,
            new Rect(0.0f, 0.0f, levelImg.width, levelImg.height),
            new Vector2(0.5f, 0.5f),
            100.0f
        );

        var img = GetGameObjectChild(Root, "SelectedLevelImage");

        if (img != null) img.GetComponent<Image>().overrideSprite = levelSprite;

        var canReroll = !data.pointerEnter.gameObject.GetComponent<BingoLevelData>().IsClaimed
                        && !MonoSingleton<BingoVoteManager>.Instance.voteOngoing;

        var level = GameManager.CurrentGame.Grid.LevelTable[data.pointerEnter.gameObject.name];

        FindObject(Root, "SelectedLevel", "Text (TMP)")?.GetComponent<TextMeshProUGUI>()
            .SetText(
                level.LevelName
                + (level.ClaimedBy != "NONE"
                    ? $"\n<color=orange>{GetFormattedTime(level.TimeToBeat)}</color>"
                    : "")
                + (canReroll ? "\n<color=orange>R: Start a reroll vote</color>" : "")
            );

        GetGameObjectChild(Root, "SelectedLevel")?.SetActive(true);
        GetGameObjectChild(Root, "SelectedLevelImage")?.SetActive(true);
    }

    public static void ShowBingoCardInPauseMenu() {
        var currentGame = GameManager.CurrentGame;
        var templateSquare = GetGameObjectChild(GetGameObjectChild(Root, "Card"), "Image");

        templateSquare?.SetActive(false);

        for (var x = 0; x < currentGame.Grid.Size; x++)
        for (var y = 0; y < currentGame.Grid.Size; y++) {
            var levelSquare = Object.Instantiate(templateSquare, templateSquare?.transform.parent.transform);

            if (levelSquare == null) continue;

            levelSquare.name = $"{x}-{y}";
            var levelObject = GameManager.CurrentGame.Grid.LevelTable[levelSquare.name];

            //Set up BingoLevelData
            var bld = levelSquare.AddComponent<BingoLevelData>();
            
            bld.LevelName = levelObject.LevelName;
            bld.IsAngryLevel = levelObject.IsAngryLevel;
            bld.AngryParentBundle = levelObject.AngryParentBundle;
            bld.AngryLevelId = levelObject.AngryLevelId;
            bld.Column = x;
            bld.Row = y;
            bld.IsClaimed = levelObject.ClaimedBy != "NONE";
            bld.ClaimedTeam = levelObject.ClaimedBy;

            levelSquare.AddComponent<BingoLevelSquare>();

            var claimedBy = currentGame.Grid.LevelTable[$"{x}-{y}"].ClaimedBy;

            if (claimedBy != null)
                levelSquare.GetComponent<Image>().color = TeamColors[claimedBy];

            if (GameManager.CurrentRow == x && GameManager.CurrentColumn == y) {
                levelSquare.AddComponent<Outline>();
                levelSquare.GetComponent<Outline>().effectColor = Color.magenta;
                levelSquare.GetComponent<Outline>().effectDistance = new Vector2(2f, -2f);
            }

            levelSquare.AddComponent<EventTrigger>();

            var mouseEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };

            mouseEnter.callback.AddListener(data => { OnMouseEnterLevelSquare((PointerEventData)data); });

            levelSquare.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
            levelSquare.SetActive(true);
        }

        //Center the grid based on grid size.
        var card = GetGameObjectChild(Root, "Card");

        switch (GameManager.CurrentGame.Grid.Size) {
            case 3: {
                if (card != null) {
                    card.transform.localPosition = new Vector3(-65f, 170f, 0f);
                    card.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }

                break;
            }
            case 4: {
                if (card != null) {
                    card.transform.localPosition = new Vector3(-82.5f, 185f, 0f);
                    card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }

                break;
            }
        }
    }

    public static void Init(ref OptionsManager instance) {
        if (Root == null)
            Root = Object.Instantiate(AssetLoader.BingoPauseCard, instance.pauseMenu.gameObject.transform);

        Root.name = "BingoPauseCard";
        Grid = GetGameObjectChild(Root, "Card");
        if (Grid != null) Grid.GetComponent<GridLayoutGroup>().constraintCount = GameManager.CurrentGame.Grid.Size;

        DescriptorText = GetGameObjectChild(GetGameObjectChild(Root, "Descriptor"), "Text (TMP)");

        Root.SetActive(true);
    }
}