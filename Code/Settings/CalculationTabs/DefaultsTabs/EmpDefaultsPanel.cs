using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class EmpDefaultsPanel : RICODefaultsPanel
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal EmpDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons</param>
        protected override void FooterButtons(float yPos)
        {
            base.FooterButtons(yPos);

            // Save button.
            UIButton saveButton = AddSaveButton(panel, yPos);
            saveButton.eventClicked += Apply;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            base.Apply(control, mouseEvent);

            // Clear population caches.
            PopData.instance.workplaceCache.Clear();
            PopData.instance.visitplaceCache.Clear();

            // Clear RICO cache.
            if (ModUtils.ricoClearAllWorkplaces != null)
            {
                ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
            }
        }
    }
}