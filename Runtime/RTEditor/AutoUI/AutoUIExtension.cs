namespace Battlehub.RTEditor.UI
{
    public partial class AutoUIExtension : EditorExtension
    {
        private AutoUI m_autoUI;

        protected override void OnInit()
        {
            m_autoUI = new AutoUI();
        }

        protected override void OnCleanup()
        {
            m_autoUI.Dispose();
        }
    }
}