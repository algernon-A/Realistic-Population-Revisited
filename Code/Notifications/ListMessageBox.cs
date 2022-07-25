using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2.MessageBox
{
    /// <summary>
    /// Simple message box with separate pargaraphs and/or lists of dot points.
    /// </summary>
    public class ListMessageBox : MessageBoxBase
    {
        // Components.
        private UIButton closeButton;


        // Number of buttons for this panel (for layout).
        protected virtual int NumButtons => 1;

        /// <summary>
        /// Constructor - performs required basic setup.
        /// </summary>
        public ListMessageBox()
        {
            // Set title.
            Title = Mod.ModName;

            // Add buttons.
            AddButtons();
        }


        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public virtual void AddButtons()
        {
            // Add close button.
            closeButton = AddButton(1, NumButtons, Close);
            closeButton.text = Translations.Translate("MES_CLS");
        }


        /// <summary>
        /// Add paragraphs to message box.
        /// </summary>
        /// <param name="messages">Text for each paragraph</param>
        public void AddParas(params string[] messages)
        {
            // Iterate through each provided paragraph and create a separate UI label for each item.
            for (int i = 0; i < messages.Length; ++i)
            {
                // Basic setup.
                UILabel paraMessage = ScrollableContent.AddUIComponent<UILabel>();
                paraMessage.wordWrap = true;
                paraMessage.autoSize = false;
                paraMessage.autoHeight = true;
                paraMessage.width = ContentWidth;

                // Set text while we're autosizing.
                paraMessage.text = messages[i];

                // Now that the height is set after autosizing with the final text, fix the height and add a buffer for the next paragraph.
                paraMessage.autoHeight = false;
                paraMessage.height += 10f;
            }
        }


        /// <summary>
        /// Creates a blank panel spacer.
        /// </summary>
        /// <param name="height">Spacer height (default 10)</param>
        public void AddSpacer(float height = 10f)
        {
            UILabel spacer = ScrollableContent.AddUIComponent<UILabel>();

            spacer.autoSize = false;
            spacer.height = height;
            spacer.width = 10f;
        }


        /// <summary>
        /// Add dot pointed list.
        /// </summary>
        /// <param name="listItems">Array of messages for display as separate dot points</param>
        public void AddList(params string[] listItems)
        {
            // Iterate through each provided message string and create separate dot point for each item.
            for (int i = 0; i < listItems.Length; ++i)
            {
                ListItem listItem = ScrollableContent.AddUIComponent<ListItem>();
                listItem.width = ContentWidth;
                listItem.Text = listItems[i];
            }

            // Add spacer at end of list.
            AddSpacer();
        }


        /// <summary>
        /// A dot point list item for display.
        /// </summary>
        public class ListItem : UIPanel
        {
            // Layout constants.
            private const float DotPointX = 20f;
            private const float MessageX = 35f;
            private const float Padding = 3f;

            // Panel components.
            private readonly UILabel dotPoint;
            private readonly UILabel textLabel;


            // Text to display.
            public string Text { get => textLabel.text; set => textLabel.text = value; }


            /// <summary>
            /// Constructor.
            /// </summary>
            public ListItem()
            {
                // Use manual sizing and layout to avoid issues.
                autoLayout = false;
                autoSize = false;

                // Dot point.
                dotPoint = AddUIComponent<UILabel>();
                dotPoint.autoSize = true;
                dotPoint.relativePosition = new Vector2(DotPointX, 0f);
                dotPoint.text = ("-");

                // Text label.
                textLabel = AddUIComponent<UILabel>();
                textLabel.relativePosition = new Vector2(MessageX, Padding);
                textLabel.textScale = 0.8f;
                textLabel.wordWrap = true;
                textLabel.autoSize = false;
                textLabel.autoHeight = true;

                // Set list panel height.
                textLabel.width = width - MessageX - Padding;
                height = textLabel.height + (Padding * 2);
            }


            /// <summary>
            /// Set width of textlabel and height of panel; called when size is changed, e.g. when text is set/changed.
            /// </summary>
            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();

                if (textLabel != null)
                {
                    textLabel.width = width - MessageX - Padding;
                    height = textLabel.height + (Padding * 2);
                }
            }
        }
    }
}