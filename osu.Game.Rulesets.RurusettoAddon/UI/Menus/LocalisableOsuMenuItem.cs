using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Menus {
	public class LocalisableOsuMenuItem : OsuMenuItem {
		public LocalisableOsuMenuItem ( LocalisableString text, MenuItemType type, Action action ) : base( "???", type, action ) {
			Text.Value = text;
		}
	}
}
