using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class DrawableTag : CompositeDrawable, IHasTooltip {
		public DrawableTag ( LocalisableString tag, Colour4 colour, bool solid, float height = 18 ) {
			AutoSizeAxes = Axes.X;
			Height = height;

			if ( solid ) {
				AddInternal( new Box {
					RelativeSizeAxes = Axes.Both,
					Colour = colour
				} );

				AddInternal( new Container {
					Padding = new MarginPadding { Horizontal = 4 },
					AutoSizeAxes = Axes.Both,
					Child = new OsuSpriteText {
						Colour = Colour4.FromHex( "#191C17" ),
						UseFullGlyphHeight = false,
						Font = OsuFont.GetFont( Typeface.Torus, size: height - 2, weight: FontWeight.Bold ),
						Text = tag,
						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft
					}
				} );

				Masking = true;
				CornerRadius = 4;
			}
			else {
				Masking = true;
				CornerRadius = 4;
				BorderColour = colour;
				BorderThickness = 3;

				AddInternal( new Box {
					RelativeSizeAxes = Axes.Both,
					Colour = Colour4.Transparent
				} );

				AddInternal( new Container {
					Padding = new MarginPadding { Horizontal = 4 },
					AutoSizeAxes = Axes.Both,
					Child = new OsuSpriteText {
						Colour = colour,
						UseFullGlyphHeight = false,
						Font = OsuFont.GetFont( Typeface.Torus, size: height - 2, weight: FontWeight.Bold ),
						Text = tag,
						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft
					}
				} );
			}
		}

		public static DrawableTag CreateArchived ( bool large = false ) => new( Localisation.Strings.TagArchived, Colour4.FromHex( "#FFE766" ), solid: false, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagArchivedTooltip
		};
		public static DrawableTag CreateLocal ( bool large = false ) => new( Localisation.Strings.TagLocal, Colour4.FromHex( "#FFE766" ), solid: true, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagLocalTooltip
		};
		public static DrawableTag CreateHardCoded ( bool large = false ) => new( Localisation.Strings.TagHardcoded, Colour4.FromHex( "#FF6060" ), solid: true, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagHardcodedTooltip
		};
		public static DrawableTag CreateFailledImport ( bool large = false ) => new( Localisation.Strings.TagFailedImport, Colour4.FromHex( "#FF6060" ), solid: true, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagFailedImportTooltip
		};

		public static DrawableTag CreateBorked ( bool large = false ) => new( Localisation.Strings.TagBorked, Colour4.FromHex( "#FF6060" ), solid: false, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagBorkedTooltip
		};
		public static DrawableTag CreatePlayable ( bool large = false ) => new( Localisation.Strings.TagPlayable, Colour4.FromHex( "#6CB946" ), solid: false, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagPlayableTooltip
		};
		public static DrawableTag CreatePrerelease ( bool large = false ) => new( Localisation.Strings.TagPrerelease, Colour4.FromHex( "#FFE766" ), solid: false, height: large ? 26 : 18 ) {
			TooltipText = Localisation.Strings.TagPrereleaseTooltip
		};


		public LocalisableString TooltipText { get; set; }
	}
}
