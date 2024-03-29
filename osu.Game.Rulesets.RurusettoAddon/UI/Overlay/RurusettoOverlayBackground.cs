﻿using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

public class RurusettoOverlayBackground : CompositeDrawable {
	public RurusettoOverlayBackground () {
		Height = 80;
		RelativeSizeAxes = Axes.X;

		Masking = true;
	}

	private Dictionary<Texture, Sprite> covers = new();
	Sprite? currentCover;
	Texture defaultCover = null!;

	[BackgroundDependencyLoader]
	private void load ( GameHost host, TextureStore textures, RurusettoAddonRuleset ruleset ) {
		SetCover( defaultCover = ruleset.GetTexture( host, textures, TextureNames.HeaderBackground ), expanded: true );
	}

	public void SetCover ( Texture? cover, bool expanded ) {
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

		if ( expanded ) {
			this.FadeIn().ResizeHeightTo( 80, 400, Easing.Out );
		}
		else {
			this.ResizeHeightTo( 0, 400, Easing.Out ).Then().FadeOut();
		}
	}
}