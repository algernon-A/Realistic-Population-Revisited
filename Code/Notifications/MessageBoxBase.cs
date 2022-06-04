// Based on from macsergey's work (NodeMarkup/Intersection Marking Tool)

using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Linq;
using UnityEngine;

namespace RealPop2.MessageBox
{
    /// <summary>
    /// Base class for displaying modal message boxes.
    /// </summary>
    public abstract class MessageBoxBase : UIPanel
    {
        // Layout constants.
        protected const float Width = 600f;
        protected const float Height = 200f;
        protected const float TitleBarHeight = 40f;
        protected const float ButtonHeight = 45f;
        protected const float Padding = 16f;
        protected const float ButtonSpacing = 15f;
        protected const float MaxContentHeight = 400f;

        // Reference constants.
        private const int DefaultButton = 1;


        // Panel components.
        private UILabel title;
        private UIDragHandle titleBar;
        private UIScrollablePanel mainPanel;
        private UIPanel buttonPanel;


        // Accessors.
        public string Title { get => title.text; set => title.text = value; }
        public UIScrollablePanel ScrollableContent => mainPanel;
        protected float ContentWidth => ScrollableContent.width - ScrollableContent.autoLayoutPadding.left - ScrollableContent.autoLayoutPadding.right;
        public void Close() => CloseModal(this);


        /// <summary>
        /// Show modal message box.
        /// </summary>
        /// <typeparam name="T">MessageBox type to show</typeparam>
        /// <returns>New MessageBox</returns>
        public static T ShowModal<T>()
        where T : MessageBoxBase
        {
            // Get global view.
            UIView view = UIView.GetAView();

            // Create new gameobject in global view and attach new MessageBox.
            GameObject gameObject = new GameObject();
            gameObject.transform.parent = view.transform;
            MessageBoxBase messageBox = gameObject.AddComponent<T>();

            // Display box as modal.
            UIView.PushModal(messageBox);
            messageBox.Show(true);
            messageBox.Focus();

            // Apply modal view effects.
            if (view.panelsLibraryModalEffect != null)
            {
                view.panelsLibraryModalEffect.FitTo(null);
                if (!view.panelsLibraryModalEffect.isVisible || view.panelsLibraryModalEffect.opacity != 1f)
                {
                    view.panelsLibraryModalEffect.Show(false);
                    ValueAnimator.Animate("ModalEffect67419", (value) => view.panelsLibraryModalEffect.opacity = value, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
                }
            }

            return messageBox as T;
        }


        /// <summary>
        ///  Closes the message box.
        /// </summary>
        public static void CloseModal(MessageBoxBase messageBox)
        {
            // Stop modality.
            UIView.PopModal();

            // Clear modal view effects.
            UIView view = UIView.GetAView();
            if (view.panelsLibraryModalEffect != null)
            {
                if (!UIView.HasModalInput())
                {
                    ValueAnimator.Animate("ModalEffect67419", (value) => view.panelsLibraryModalEffect.opacity = value, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), () => view.panelsLibraryModalEffect.Hide());
                }
                else
                {
                    view.panelsLibraryModalEffect.zOrder = UIView.GetModalComponent().zOrder - 1;
                }
            }

            // Hide messagebox, clear child event handlers, and destroy game object.
            messageBox.Hide();
            messageBox.RemoveEventHandlers();
            Destroy(messageBox.gameObject);
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBoxBase()
        {
            // Basic setup.
            autoSize = false;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = Width;
            height = Height;
            color = new Color32(58, 88, 104, 255);
            backgroundSprite = "MenuPanel";

            // Add components.
            AddTitleBar();
            AddMainPanel();
            AddButtonPanel();
            Resize();

            // Event handler for size change.
            mainPanel.eventSizeChanged += (component, size) => Resize();
        }


        /// <summary>
        /// Adds a button to the message box's button panel.
        /// </summary>
        /// <param name="buttonNumber">Number of this button (one-based)</param>
        /// <param name="totalButtons">Total number of buttons to add</param>
        /// <param name="action">Action to perform when button pressed</param>
        /// <returns>New UIButton</returns>
        protected UIButton AddButton(int buttonNumber, int totalButtons, Action action)
        {
            // Get zero-based button number.
            int zeroedNumber = buttonNumber - 1;

            // Basic setup.
            UIButton newButton = buttonPanel.AddUIComponent<UIButton>();
            newButton.normalBgSprite = "ButtonMenu";
            newButton.hoveredTextColor = new Color32(7, 132, 255, 255);
            newButton.pressedTextColor = new Color32(30, 30, 44, 255);
            newButton.disabledTextColor = new Color32(7, 7, 7, 255);
            newButton.horizontalAlignment = UIHorizontalAlignment.Center;
            newButton.verticalAlignment = UIVerticalAlignment.Middle;
            newButton.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => action?.Invoke();

            // Calculate size and position based on this button number and total number of buttons.
            // Width of button is avalable space divided by number of buttons, less spacing at both sides of button.
            float buttonPlaceWidth = this.width / totalButtons;
            float buttonWidth = buttonPlaceWidth - (ButtonSpacing * 2f);
            newButton.size = new Vector2(buttonWidth, ButtonHeight);
            newButton.relativePosition = new Vector2(((buttonNumber - 1) * buttonPlaceWidth) + ButtonSpacing, 0f);

            return newButton;
        }


        /// <summary>
        /// Centres the message box on the screen.  Called by the game when the panel size changes.
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
        }


        /// <summary>
        /// Handles key down events - escape and return.
        /// </summary>
        /// <param name="keyEvent">UI key event</param>
        protected override void OnKeyDown(UIKeyEventParameter keyEvent)
        {
            // Ensure key hasn't already been used.
            if (!keyEvent.used)
            {
                // Checking for key - escape and enter.
                if (keyEvent.keycode == KeyCode.Escape)
                {
                    // Escape key pressed - use up the event and close the messagebox.
                    keyEvent.Use();
                    Close();
                }
                else if (keyEvent.keycode == KeyCode.Return)
                {
                    // Enter key pressed - simulate click of first button.
                    keyEvent.Use();
                    if (buttonPanel.components.OfType<UIButton>().Skip(DefaultButton - 1).FirstOrDefault() is UIButton button)
                    {
                        button.SimulateClick();
                    }
                }
            }
        }


        /// <summary>
        /// Resizes the message box to fit all contents.
        /// </summary>
        private void Resize()
        {
            // Set height.
            height = titleBar.height + mainPanel.height + buttonPanel.height + (Padding * 2f);

            // Position main panel under title bar and set width.
            mainPanel.relativePosition = new Vector2(0, titleBar.height);
            mainPanel.width = width - (mainPanel?.verticalScrollbar?.width ?? 0) - 3f;

            // Position button panel under main panel.
            buttonPanel.relativePosition = new Vector2(0, titleBar.height + mainPanel.height + Padding);

            // Set width of each component in main panel to panel width (less padding on either side).
            foreach (UIComponent component in mainPanel.components)
            {
                component.width = mainPanel.width - 2 * Padding;
            }
        }


        /// <summary>
        /// Adds the titlebar to the top of the message box.
        /// </summary>
        private void AddTitleBar()
        {
            // Drag handle.
            titleBar = AddUIComponent<UIDragHandle>();
            titleBar.relativePosition = Vector2.zero;
            titleBar.size = new Vector2(Width, TitleBarHeight);

            // Title
            title = titleBar.AddUIComponent<UILabel>();
            title.textAlignment = UIHorizontalAlignment.Center;
            title.textScale = 1.3f;
            title.anchor = UIAnchorStyle.Top;

            // Close button.
            UIButton closeButton = titleBar.AddUIComponent<UIButton>();
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.size = new Vector2(32f, 32f);
            closeButton.relativePosition = new Vector2(Width - 36f, 4f);

            // Event handler - resize.
            titleBar.eventSizeChanged += (component, newSize) =>
            {
                title.size = newSize;
                title.CenterToParent();
            };

            // Event handler - centre title on drag handle when text changes.
            title.eventTextChanged += (component, text) => title.CenterToParent();

            // Event handler - close button.
            closeButton.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => Close();
        }


        /// <summary>
        /// Adds the main panel to the message box.
        /// </summary>
        private void AddMainPanel()
        {

            // Basic setup.
            mainPanel = AddUIComponent<UIScrollablePanel>();
            mainPanel.width = Width;
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.autoLayoutPadding = new RectOffset((int)Padding, (int)Padding, 0, 0);
            mainPanel.clipChildren = true;
            mainPanel.builtinKeyNavigation = true;
            mainPanel.scrollWheelDirection = UIOrientation.Vertical;
            mainPanel.maximumSize = new Vector2(Width, MaxContentHeight);

            // Add scrollbar.
            UIControls.AddScrollbar(this, mainPanel);

            // Event handlers to add/remove event handlers for resizing when child components re resized.
            ScrollableContent.eventComponentAdded += (container, child) => AddChildEvents(child);
            ScrollableContent.eventComponentRemoved += (container, child) => RemoveChildEvents(child);
        }


        /// <summary>
        ///  Adds the button panel to the message box.
        /// </summary>
        private void AddButtonPanel()
        {
            buttonPanel = AddUIComponent<UIPanel>();
            buttonPanel.size = new Vector2(Width, ButtonHeight);
        }


        /// <summary>
        /// Attaches event handlers to child components to ensure that the main panel is resized to fit children when their size changes.
        /// </summary>
        /// <param name="child">Child component</param>
        private void AddChildEvents(UIComponent child)
        {
            child.eventVisibilityChanged += OnChildVisibilityChanged;
            child.eventSizeChanged += OnChildSizeChanged;
            child.eventPositionChanged += OnChildPositionChanged;
        }


        /// <summary>
        /// Removes event handlers from child components.  Should be called when child components are removed.
        /// </summary>
        /// <param name="child">Child component</param>
        private void RemoveChildEvents(UIComponent child)
        {
            child.eventVisibilityChanged -= OnChildVisibilityChanged;
            child.eventSizeChanged -= OnChildSizeChanged;
            child.eventPositionChanged -= OnChildPositionChanged;
        }

        /// <summary>
        /// Removes event handlers from all child components of the main panel.
        /// </summary>
        private void RemoveEventHandlers()
        {
            if (mainPanel != null)
            {
                // Iterate through each child component and remove event handler.
                foreach (UIComponent child in mainPanel.components)
                {
                    RemoveChildEvents(child);
                }
            }
        }

        /// <summary>
        /// Event handler delegate for child visibility changes.
        /// </summary>
        private void OnChildVisibilityChanged(UIComponent component, bool isVisible) => Resize();


        /// <summary>
        /// Event handler delegate for child size changes.
        /// </summary>
        private void OnChildSizeChanged(UIComponent component, Vector2 newSize) => Resize();


        /// <summary>
        /// Event handler delegate for child position changes.
        /// </summary>
        private void OnChildPositionChanged(UIComponent component, Vector2 position) => Resize();
    }
}