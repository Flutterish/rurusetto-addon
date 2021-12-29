using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.RurusettoAddon.Tests {
	public class TestSceneOsuGame : OsuTestScene {
        [BackgroundDependencyLoader]
        private void load( GameHost host, OsuGameBase gameBase ) {
            Children = new Drawable[] {
                new Box {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                }
            };

            AddGame( new OsuGame() );
        }
    }
}
