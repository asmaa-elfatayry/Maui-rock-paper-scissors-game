using CommunityToolkit.Maui.Views;

namespace game_rps
{
    public partial class RulesPopup : Popup
    {
        public RulesPopup()
        {
            InitializeComponent();
        }

        private void ClosePopup(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}