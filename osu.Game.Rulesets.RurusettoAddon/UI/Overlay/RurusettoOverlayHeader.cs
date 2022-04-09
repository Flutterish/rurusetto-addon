using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public record RurusettoTabItem {
		public object Target { get; init; }
		public LocalisableString Text { get; init; }
	}

	public class RurusettoOverlayHeader : TabControlOverlayHeader<RurusettoTabItem> {
		static readonly RurusettoTabItem listingTab = new() {
			Text = Localisation.Strings.ListingTab
		};

		[Resolved]
		protected RurusettoAPI API { get; private set; }

		public RurusettoOverlayHeader () {
			TabControl.AddItem( listingTab );

			SelectedTab.ValueChanged += v => {
				if ( v.OldValue != null )
					TabControl.RemoveItem( v.OldValue );

				switch ( v.NewValue ) {
					case { Target: APIRuleset ruleset }:
						TabControl.AddItem( v.NewValue );
						Current.Value = v.NewValue;

						ruleset.RequestDarkCover( texture => {
							background.SetCover( texture );
						} );
						break;

					case { Target: APIUser user }:

						break;

					default:
						Current.Value = listingTab;
						background.SetCover( null );
						break;
				}
			};

			Current.ValueChanged += v => {
				if ( v.NewValue == listingTab ) {
					SelectedTab.Value = null;
				}
			};
		}

		public readonly Bindable<RurusettoTabItem> SelectedTab = new();

		protected override OverlayTitle CreateTitle ()
			=> new HeaderTitle();

		private RurusettoOverlayBackground background;
		protected override RurusettoOverlayBackground CreateBackground ()
			=> background = new RurusettoOverlayBackground ();

		private class HeaderTitle : OverlayTitle {
			public HeaderTitle () {
				Title = "rūrusetto";
				Description = Localisation.Strings.RurusettoDescription;
				IconTexture = "Icons/Hexacons/chart";
			}
		}

		protected override OsuTabControl<RurusettoTabItem> CreateTabControl () => new OverlayHeaderBreadcrumbControl();

		public class OverlayHeaderBreadcrumbControl : BreadcrumbControl<RurusettoTabItem> {
			public OverlayHeaderBreadcrumbControl () {
				RelativeSizeAxes = Axes.X;
				Height = 47;
			}

			[BackgroundDependencyLoader]
			private void load ( OverlayColourProvider colourProvider ) {
				AccentColour = colourProvider.Light2;
			}

			protected override TabItem<RurusettoTabItem> CreateTabItem ( RurusettoTabItem value ) => new ControlTabItem( value ) {
				AccentColour = AccentColour,
			};

			private class ControlTabItem : BreadcrumbTabItem {
				protected override float ChevronSize => 8;

				public ControlTabItem ( RurusettoTabItem value )
					: base( value ) {
					RelativeSizeAxes = Axes.Y;
					Text.Font = Text.Font.With( size: 14 );
					Text.Anchor = Anchor.CentreLeft;
					Text.Origin = Anchor.CentreLeft;
					Chevron.Y = 1;
					Bar.Height = 0;
				}

				protected override void LoadComplete () {
					base.LoadComplete();

					Text.Text = Value.Text;
				}

				// base OsuTabItem makes font bold on activation, we don't want that here
				protected override void OnActivated () => FadeHovered();

				protected override void OnDeactivated () => FadeUnhovered();
			}
		}
	}
}
