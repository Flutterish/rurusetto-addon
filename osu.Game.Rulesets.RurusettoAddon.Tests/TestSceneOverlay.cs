using osu.Framework.Testing;
using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

namespace osu.Game.Rulesets.RurusettoAddon.Tests {
	public class TestSceneOverlay : TestScene {
		RurusettoOverlay overlay;
		public TestSceneOverlay () {
			Add( overlay = new RurusettoOverlay( new RurusettoAddonRuleset() ) );
			overlay.Show();
		}
	}
}
