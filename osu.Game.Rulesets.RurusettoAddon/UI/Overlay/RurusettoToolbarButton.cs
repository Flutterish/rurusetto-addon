using osu.Game.Overlays.Toolbar;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

public class RurusettoToolbarButton : ToolbarOverlayToggleButton {
	protected override Anchor TooltipAnchor => Anchor.TopRight;

	public RurusettoToolbarButton () {
		//Hotkey = GlobalAction.ToggleChat;
	}

	[BackgroundDependencyLoader( true )]
	private void load ( RurusettoOverlay overlay ) {
		StateContainer = overlay;
	}
}