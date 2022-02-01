using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RequestFailedDrawable : CompositeDrawable {
        public RequestFailedDrawable () {
            header = new OsuSpriteText {
                Font = OsuFont.GetFont( size: 45, weight: FontWeight.Bold ),
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                Text = Localisation.Strings.ErrorHeader,
                Rotation = 5
            };
            content = new OsuSpriteText {
                Font = OsuFont.GetFont( size: 38, weight: FontWeight.SemiBold ),
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                Text = Localisation.Strings.ErrorMessageGeneric
            };
            footer = new OsuSpriteText {
                Font = OsuFont.GetFont( size: 14 ),
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                Text = Localisation.Strings.ErrorFooter
            };
            button = new RetryButton {
                Margin = new MarginPadding { Top = 10 },
                MinWidth = 110,
                Height = 38,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                Text = Localisation.Strings.Retry
            };
        }

	    SpriteText header;
        SpriteText content;
        SpriteText footer;
        OsuButton button;

        public LocalisableString HeaderText {
            get => header.Text;
            set => header.Text = value;
		}
        public LocalisableString ContentText {
            get => content.Text;
            set => content.Text = value;
		}
        public LocalisableString FooterText {
            get => footer.Text;
            set => footer.Text = value;
		}
        public LocalisableString ButtonText {
            get => button.Text;
            set => button.Text = value;
		}

        public Action ButtonClicked {
            get => button.Action;
            set => button.Action = value;
		}

        [BackgroundDependencyLoader]
		private void load ( OverlayColourProvider colours, GameHost host, TextureStore textures, RurusettoAddonRuleset ruleset ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

            Masking = true;

			AddInternal( new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = new Drawable[] {
					new Container {
						RelativeSizeAxes = Axes.X,
						Height = 400,
						Children = new Drawable[] {
                            new Sprite {
                                RelativeSizeAxes = Axes.Both,
                                Texture = ruleset.GetTexture( host, textures, TextureNames.ErrorCover ),
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                FillMode = FillMode.Fit,
                                Scale = new( 1.18f )
							},
                            new Box {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical( colours.Background5.Opacity( 0 ), colours.Background5 )
                            },
                            new SectionTriangles {
								Origin = Anchor.BottomCentre,
								Anchor = Anchor.BottomCentre
                            },
                            new FillFlowContainer {
                                Direction = FillDirection.Vertical,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Y = -20,
                                Spacing = new osuTK.Vector2 { Y = 5 },
                                Children = new Drawable[] {
                                    button,
                                    footer,
                                    content,
                                    header
								}
                            }
						}
					},
					new Box {
						RelativeSizeAxes = Axes.X,
						Height = 14,
						Colour = colours.Background6
					}
				}
			} );

			Margin = new MarginPadding { Bottom = 8 };
		}

        private class SectionTriangles : BufferedContainer {
            private readonly Triangles triangles;

            public SectionTriangles () {
                RelativeSizeAxes = Axes.X;
                Height = 80;
                Masking = true;
                MaskingSmoothness = 0;
                Children = new Drawable[]
                {
                    triangles = new Triangles
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        TriangleScale = 3,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load ( OverlayColourProvider colours ) {
                Colour = ColourInfo.GradientVertical( Colour4.Transparent, Colour4.White );
                triangles.ColourLight = colours.Background5;
                triangles.ColourDark = colours.Background5.Darken( 0.2f );
            }
        }

        private class RetryButton : OsuButton {
            SpriteText text;
            public float MinWidth;

			protected override void Update () {
				base.Update();
                Width = Math.Max( MinWidth, text.DrawWidth + 18 );
			}

			protected override SpriteText CreateText () => text = new OsuSpriteText {
                Depth = -1,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Font = OsuFont.GetFont( size: 26, weight: FontWeight.Bold )
            };
        }
    }
}
