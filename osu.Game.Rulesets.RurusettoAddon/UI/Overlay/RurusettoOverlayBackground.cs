using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public class RurusettoOverlayBackground : CompositeDrawable {
		public RurusettoOverlayBackground () {
			Height = 80;
			RelativeSizeAxes = Axes.X;

			Masking = true;
		}

		private Dictionary<Texture, Sprite> covers = new();
		Sprite currentCover;
		Texture defaultCover;

		[BackgroundDependencyLoader]
		private void load ( GameHost host, TextureStore textures, RurusettoAddonRuleset ruleset ) {
			if ( !textures.GetAvailableResources().Contains( "Textures/cover.jpg" ) )
				textures.AddStore( host.CreateTextureLoaderStore( ruleset.CreateResourceStore() ) );

			SetCover( defaultCover = textures.Get( "Textures/cover.jpg" ) );
		}

		public void SetCover ( Texture cover ) {
			cover ??= defaultCover;

			if ( !covers.TryGetValue( cover, out var sprite ) ) {
				AddInternal( sprite = new Sprite {
					RelativeSizeAxes = Axes.Both,
					Texture = cover,
					FillMode = FillMode.Fill,
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre
				} );

				covers.Add( cover, sprite );
			}

			currentCover?.FadeOut( 400 );
			ChangeInternalChildDepth( sprite, (float)Clock.CurrentTime );
			sprite.FadeIn();

			currentCover = sprite;

			if ( currentCover.Texture == defaultCover ) {
				this.ResizeHeightTo( 80, 400, Easing.Out );
			}
			else {
				this.ResizeHeightTo( 160, 400, Easing.Out );
			}
		}
	}
}
