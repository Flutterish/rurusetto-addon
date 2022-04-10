using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;
using System.Linq;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public class RurusettoTabItem : CategorisedTabItem<LocalisableString, object> {
		public LocalisableString Title { get; init; }
	}

	public class RurusettoOverlayHeader : CategorisedTabControlOverlayHeader<RurusettoTabItem, LocalisableString, object> {
		public readonly RurusettoTabItem ListingTab = new() {
			Category = Localisation.Strings.ListingTab,
			Title = Localisation.Strings.ListingTab
		};
		public readonly RurusettoTabItem UsersTab = new() {
			Category = Localisation.Strings.UsersTab,
			Title = Localisation.Strings.UsersTab
		};
		public readonly RurusettoTabItem CollectionsTab = new() {
			Category = Localisation.Strings.CollectionsTab,
			Title = Localisation.Strings.CollectionsTab
		};

		[Resolved]
		protected RurusettoAPI API { get; private set; }

		public RurusettoOverlayHeader () {
			TabControl.AddItem( ListingTab );
			TabControl.Current.Value = ListingTab;

			Current.ValueChanged += v => {
				CategoryControl.Current.Value = v.NewValue.Category;

				switch ( v.NewValue ) {
					case { Tab: APIRuleset ruleset }:
						ruleset.RequestDarkCover( texture => {
							background.SetCover( texture );
						} );
						break;

					case { Tab: APIUser user }:

						break;

					default:
						background.SetCover( null );
						break;
				}
			};

			CategoryControl.AddItem( ListingTab.Category );
			CategoryControl.AddItem( UsersTab.Category );
			CategoryControl.AddItem( CollectionsTab.Category );

			CategoryControl.Current.ValueChanged += v => {
				NavigateTo( categoryFromName( v.NewValue ) );
			};
		}

		private RurusettoTabItem categoryFromName ( LocalisableString name ) {
			return name == Localisation.Strings.ListingTab
				? ListingTab
				: name == Localisation.Strings.UsersTab
				? UsersTab
				: CollectionsTab;
		}

		public void NavigateTo ( RurusettoTabItem tab, bool perserveCategories = false ) {
			if ( categoryFromName( Current.Value.Category ) == tab )
				return;

			RurusettoTabItem item = new() {
				Tab = tab,
				Title = tab.Title,
				Category = tab.Category
			};

			if ( !perserveCategories && Current.Value.Category != tab.Category ) {
				TabControl.Clear();
			}

			clearHistoryAfterCurrent();
			TabControl.AddItem( item );
			TabControl.Current.Value = item;
		}

		public void NavigateTo ( object tab, LocalisableString title, bool perserveCategories = false ) {
			var category = tab switch {
				APIRuleset => ListingTab,
				APIUser => UsersTab,
				_ => CollectionsTab
			};
			RurusettoTabItem item = new() {
				Tab = tab,
				Title = title,
				Category = category.Category
			};

			if ( item.Category != Current.Value.Category ) {
				NavigateTo( categoryFromName( item.Category ), perserveCategories );
			}

			clearHistoryAfterCurrent();
			TabControl.AddItem( item );
			TabControl.Current.Value = item;
		}

		private void clearHistoryAfterCurrent () {
			while ( TabControl.Items.Any() && TabControl.Items[^1] != Current.Value ) {
				TabControl.RemoveItem( TabControl.Items[^1] );
			}
		}

		public bool NavigateBack () {
			if ( TabControl.Items[0] == Current.Value )
				return false;

			TabControl.SwitchTab( -1, wrap: false );
			return true;
		}

		public bool NavigateForward () {
			if ( TabControl.Items[^1] == Current.Value )
				return false;

			TabControl.SwitchTab( 1, wrap: false );
			return true;
		}

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

				Current.ValueChanged += index => {
					if ( index.NewValue is null )
						return;

					var category = Items[0].Category;
					var prev = TabMap[Items[0]] as ControlTabItem;
					foreach ( var item in TabContainer.Children.OfType<ControlTabItem>().Skip( 1 ) ) {
						if ( item.Value.Category != category ) {
							prev.Chevron.Icon = FontAwesome.Solid.AngleDoubleRight;
							category = item.Value.Category;
						}
						else {
							prev.Chevron.Icon = FontAwesome.Solid.ChevronRight;
						}

						prev = item;
					}
				};
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

					Text.Text = Value.Title;
				}

				// base OsuTabItem makes font bold on activation, we don't want that here
				protected override void OnActivated () => FadeHovered();

				protected override void OnDeactivated () => FadeUnhovered();
			}
		}
	}
}
