using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.RurusettoAddon.Tests {
	public class TestSceneOverlay : OsuTestScene {
		RurusettoOverlay overlay;
		public TestSceneOverlay () {
			Add( overlay = new RurusettoOverlay( new RurusettoAddonRuleset() ) );
			overlay.Show();
		}
	}
}
