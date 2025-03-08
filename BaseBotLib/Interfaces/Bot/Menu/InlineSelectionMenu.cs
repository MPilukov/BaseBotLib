using System.Collections.Generic;

namespace BaseBotLib.Interfaces.Bot.Menu
{
    public class InlineSelectionMenu : BaseMenu
    {
        public List<InlineSelectionMenuButton> Buttons { get; set; }
    }
}