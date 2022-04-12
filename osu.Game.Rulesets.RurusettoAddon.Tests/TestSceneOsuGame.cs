using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

public class TestSceneOsuGame : OsuTestScene {
	protected override void LoadComplete () {
		base.LoadComplete();

        Children = new Drawable[] {
            new Box {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
            }
        };
        AddGame( new OsuGame() );
    }
}