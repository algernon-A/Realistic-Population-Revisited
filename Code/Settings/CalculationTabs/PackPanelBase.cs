using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal abstract class PackPanelBase : OptionsPanelTab
    {
        // Constants.
        protected const float Margin = 5f;
        protected const float TextFieldWidth = 85f;
        protected const float ColumnWidth = TextFieldWidth + (Margin * 2);
        protected const float LeftItem = 75f;
        protected const float FirstItem = 110f;
        protected const float RowHeight = 27f;
        protected const float MeasurementLabelY = -20f;


        // Panel components.
        protected UIDropDown packDropDown;
        private UIButton saveButton, deleteButton;

        // List of packs.
        protected List<DataPack> packList;

        // Tab sprite name and tooltip key.
        protected abstract string TabSprite { get; }
        protected abstract string TabTooltipKey { get; }

        // Pack name field.
        protected UITextField PackNameField { get; private set; }


        /// <summary>
        /// 'Add new pack' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected abstract void AddPack(UIComponent control, UIMouseEventParameter mouseEvent);


        /// <summary>
        /// 'Delete pack' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected abstract void DeletePack(UIComponent control, UIMouseEventParameter mouseEvent);


        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update</param>
        protected abstract void UpdatePack(DataPack pack);


        /// <summary>
        /// Constructor - adds editing options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal PackPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Layout constants.
            const float TabWidth = 50f;

            // Add tab and helper.
            panel = PanelUtils.AddIconTab(tabStrip, Translations.Translate(TabTooltipKey), tabIndex, new string[] { TabSprite }, new string[] { "ingame" }, TabWidth);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Adds panel footer controls (pack name textfield and buttons).
        /// </summary>
        /// <param name="yPos">Reference Y position</param>
        protected void PanelFooter(float yPos)
        {
            // Additional space before name textfield.
            float currentY = yPos + RowHeight;

            // Pack name textfield.
            PackNameField = UIControls.BigTextField(panel, 200f, currentY);
            UILabel packNameLabel = UIControls.AddLabel(PackNameField, -100f, (PackNameField.height - 18f) / 2, Translations.Translate("RPR_OPT_EDT_NAM"));

            // Adjsut pack name textfield position to accomodate longer translation strings.
            float excessWidth = packNameLabel.width - 95f;
            if (excessWidth > 0f)
            {
                Vector3 adjustment = new Vector3(excessWidth, 0f);
                packNameLabel.relativePosition -= adjustment;
                PackNameField.relativePosition += adjustment;
            }

            // Space for buttons.
            currentY += 50f;

            // 'Add new' button.
            UIButton addNewButton = UIControls.AddButton(panel, 20f, currentY, Translations.Translate("RPR_OPT_NEW"));
            addNewButton.eventClicked += AddPack;

            // Save pack button.
            saveButton = UIControls.AddButton(panel, 250f, currentY, Translations.Translate("RPR_OPT_SAA"));
            saveButton.eventClicked += Save;

            // Delete pack button.
            deleteButton = UIControls.AddButton(panel, 480f, currentY, Translations.Translate("RPR_OPT_DEL"));
            deleteButton.eventClicked += DeletePack;
        }


        /// <summary>
        /// Sets button and textfield enabled/disabled states.
        /// </summary>
        /// <param name="index">Selected pack list index</param>
        protected void ButtonStates(int index)
        {
            // Enable save and delete buttons and name textfield if this is a custom pack, otherwise disable.
            if (packList[index].version == DataVersion.customOne)
            {
                saveButton.Enable();
                deleteButton.Enable();
            }
            else
            {
                saveButton.Disable();
                deleteButton.Disable();
            }
        }


        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="yPos">Reference Y position</param>
        /// <param name="text">Label text</param>
        protected UILabel RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (FirstItem - Margin) - lineLabel.width);
            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;
            }
            lineLabel.relativePosition = new Vector3(xPos, yPos + 2);

            return lineLabel;
        }


        /// <summary>
        /// Save button event handler.
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        /// </summary>
        protected virtual void Save(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Update currently selected pack with information from the panel.
            UpdatePack(packList[packDropDown.selectedIndex]);

            // Update selected menu item in case the name has changed.
            packDropDown.items[packDropDown.selectedIndex] = packList[packDropDown.selectedIndex].DisplayName;

            // Update defaults panel menus.
            CalculationsPanel.Instance.UpdateDefaultMenus();

            // Save configuration file.
            ConfigUtils.SaveSettings();

            // Apply update.
            FloorData.instance.CalcPackChanged(packList[packDropDown.selectedIndex]);
        }
    }
}