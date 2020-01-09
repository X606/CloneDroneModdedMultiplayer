using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLibrary;
using UnityEngine.UI;

namespace CloneDroneModdedMultiplayer.UI
{
    public static class ModdedMultiplayerUIManager
    {
        public static GameModeSelectScreen ModdedMultiplayerSelectScreen;

        public static void InitUI()
        {
            Transform titleScreenHolder = GameUIRoot.Instance.TitleScreenUI.RootButtonsContainer.GetChild(0);

            Transform singleplayerButtonCopy = GameObject.Instantiate(titleScreenHolder.GetChild(1), titleScreenHolder);
            singleplayerButtonCopy.SetSiblingIndex(2);
            singleplayerButtonCopy.position -= new Vector3(0, 1.5f);
            LocalizedTextField localizedTextField = singleplayerButtonCopy.GetChild(0).GetComponent<LocalizedTextField>();
            localizedTextField.LocalizationID = "moddedmultiplayermainmenumutton";
            Accessor.CallPrivateMethod("tryLocalizeTextField", localizedTextField);
            Button button = singleplayerButtonCopy.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent(); // remove old events
            button.onClick.AddListener(OnModdedMultiplayerClicked);

            titleScreenHolder.GetChild(1).position += new Vector3(0, 0.8f);
            titleScreenHolder.GetChild(3).position -= new Vector3(0, 1f);

            ModdedMultiplayerSelectScreen = GameObject.Instantiate(GameUIRoot.Instance.TitleScreenUI.SingleplayerModeSelectScreen.transform, GameUIRoot.Instance.transform).GetComponent<GameModeSelectScreen>();
            Button xButton = ModdedMultiplayerSelectScreen.transform.GetChild(1).GetChild(1).GetComponent<Button>();
            xButton.onClick = new Button.ButtonClickedEvent();
            xButton.onClick.AddListener(ModdedMultiplayerSelectScreen.Hide);
            xButton.onClick.AddListener(delegate
            {
                setLogoAndRootButtonsVisible(true);
            });

            ModdedMultiplayerSelectScreen.GameModeData = new GameModeCardData[1];
            ModdedMultiplayerSelectScreen.GameModeData[0] = new GameModeCardData()
            {
                Description = "test",
                NameOfMode = "test name",
                DisableOnConsoles = true
            };

            debug.PrintAllChildren(ModdedMultiplayerSelectScreen.transform);
        }
        public static void OnModdedMultiplayerClicked()
        {
            setLogoAndRootButtonsVisible(false);
            ModdedMultiplayerSelectScreen.Show();
        }
        public static void setLogoAndRootButtonsVisible(bool state)
        {
            Accessor.CallPrivateMethod("setLogoAndRootButtonsVisible", GameUIRoot.Instance.TitleScreenUI, new object[] { state });
        }
    }
}
