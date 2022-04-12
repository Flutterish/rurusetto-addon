using osu.Game.Rulesets.RurusettoAddon;
using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;
using osu.Game.Tests.Visual;

public class TestSceneOverlay : OsuTestScene {
	RurusettoOverlay overlay;
	public TestSceneOverlay () {
		Add( overlay = new RurusettoOverlay( new RurusettoAddonRuleset() ) );
		overlay.Show();
	}
}