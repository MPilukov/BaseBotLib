using System.Collections.Generic;

namespace BaseBotLib.Interfaces.Bot.Menu
{
    public class SelectionMenu : BaseMenu
    {
        public List<SelectionMenuButton> Buttons { get; set; }
    }
}