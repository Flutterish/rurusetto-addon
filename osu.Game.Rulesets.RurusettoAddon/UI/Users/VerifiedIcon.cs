using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Users;

public class VerifiedIcon : CompositeDrawable, IHasTooltip {
	SpriteIcon icon;

	[BackgroundDependencyLoader]
	private void load ( OverlayColourProvider colours ) {
		AddInternal( icon = new SpriteIcon {
			Icon = FontAwesome.Solid.Certificate,
			Colour = colours.Colour1,
			RelativeSizeAxes = Axes.Both
		} );
	}

	protected override bool OnHover ( HoverEvent e ) {
		icon.FlashColour( Color4.White, 600 );

		return true;
	}

	public LocalisableString TooltipText => Localisation.Strings.CreatorVerified;
}