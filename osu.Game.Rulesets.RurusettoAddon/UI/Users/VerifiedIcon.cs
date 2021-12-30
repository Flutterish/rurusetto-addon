using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Graphics;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Users {
	public class VerifiedIcon : CompositeDrawable, IHasTooltip {
		SpriteIcon icon;
		public VerifiedIcon () {
			AddInternal( icon = new SpriteIcon {
				Icon = FontAwesome.Solid.Certificate,
				Colour = Colour4.HotPink,
				RelativeSizeAxes = Axes.Both
			} );
		}

		protected override bool OnHover ( HoverEvent e ) {
			icon.FlashColour( Color4.White, 600 );

			return true;
		}

		public LocalisableString TooltipText => "Verified Ruleset Creator";
	}
}
