using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using System;
using System.Linq;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public class RurusettoOverlayBackground : CompositeDrawable {
		public RurusettoOverlayBackground () {
			Height = 80;
			RelativeSizeAxes = Axes.X;

			Masking = true;
		}

		[BackgroundDependencyLoader]
		private void load ( GameHost host, TextureStore textures, RurusettoAddonRuleset ruleset ) {
			if ( !textures.GetAvailableResources().Contains( "Textures/cover.jpg" ) )
				textures.AddStore( host.CreateTextureLoaderStore( ruleset.CreateResourceStore() ) );

			AddInternal( new Sprite {
				RelativeSizeAxes = Axes.Both,
				Texture = textures.Get( "Textures/cover.jpg" ),
				FillMode = FillMode.Fill,
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre
			} );
		}
	}
}
