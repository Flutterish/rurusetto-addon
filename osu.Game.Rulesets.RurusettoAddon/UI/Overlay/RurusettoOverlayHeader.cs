using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

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
	protected RurusettoAPI API { get; private set; } = null!;

	private bool userNaviaged = true;
	private int navigationDirection = -1;
	public RurusettoOverlayHeader () {
		TabControl.AddItem( ListingTab );
		TabControl.Current.Value = ListingTab;

		Current.ValueChanged += v => {
			CategoryControl.Current.Value = v.NewValue.Category;
			if ( userNaviaged && v.NewValue.Tab is null ) {
				if ( navigationDirection == 1 )
					NavigateForward();
				else
					NavigateBack();
			}

			switch ( v.NewValue ) {
				case { Tab: APIRuleset ruleset }:
					ruleset.RequestDarkCover( texture => {
						background.SetCover( texture, expanded: false );
					} );
					break;

				case { Tab: APIUser user }:
					user.RequestDarkCover( cover => {
						background.SetCover( cover, expanded: true );
					} );
					break;

				default:
					background.SetCover( null, expanded: true );
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
			Tab = tab.Tab,
			Title = tab.Title,
			Category = tab.Category
		};

		if ( !perserveCategories && Current.Value.Category != tab.Category ) {
			TabControl.Clear();
		}

		clearHistoryAfterCurrent();
		TabControl.AddItem( item );
		userNaviaged = false;
		TabControl.Current.Value = item;
		userNaviaged = true;
	}

	public void NavigateTo ( object tab, LocalisableString title, bool perserveCategories = false ) {
		if ( tab == Current.Value.Tab )
			return;

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
		userNaviaged = false;
		TabControl.Current.Value = item;
		userNaviaged = true;
	}

	private void clearHistoryAfterCurrent () {
		while ( TabControl.Items.Any() && TabControl.Items[^1] != Current.Value ) {
			TabControl.RemoveItem( TabControl.Items[^1] );
		}
	}

	public bool NavigateBack () {
		if ( TabControl.Items[0] == Current.Value )
			return false;

		var prevDir = navigationDirection;

		navigationDirection = -1;
		TabControl.SwitchTab( -1, wrap: false );
		navigationDirection = prevDir;
		return true;
	}

	public bool NavigateForward () {
		if ( TabControl.Items[^1] == Current.Value )
			return false;

		var prevDir = navigationDirection;

		navigationDirection = 1;
		TabControl.SwitchTab( 1, wrap: false );
		navigationDirection = prevDir; 
		return true;
	}

	protected override OverlayTitle CreateTitle ()
		=> new HeaderTitle();

	private RurusettoOverlayBackground background = null!;
	protected override RurusettoOverlayBackground CreateBackground ()
		=> background = new RurusettoOverlayBackground();

	private class HeaderTitle : OverlayTitle {
		public HeaderTitle () {
			Title = "rūrusetto";
			Description = Localisation.Strings.RurusettoDescription;
			Icon = OsuIcon.Rulesets;
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
				var prev = (ControlTabItem)TabMap[Items[0]];
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

		protected override Dropdown<RurusettoTabItem> CreateDropdown () {
			return new ControlDropdown();
		}

		[BackgroundDependencyLoader]
		private void load ( OverlayColourProvider colourProvider ) {
			AccentColour = colourProvider.Light2;
		}

		protected override TabItem<RurusettoTabItem> CreateTabItem ( RurusettoTabItem value ) => new ControlTabItem( value ) {
			AccentColour = AccentColour,
		};

		private class ControlDropdown : OsuTabDropdown<RurusettoTabItem> {
			protected override LocalisableString GenerateItemText ( RurusettoTabItem item )
				=> item.Title;
		}

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
				AlwaysPresent = true;
			}

			protected override void LoadComplete () {
				base.LoadComplete();

				Text.Text = Value.Title;
			}

			protected override void Update () {
				base.Update();
				if ( Alpha == 0 ) {
					AutoSizeAxes = Axes.None;
					Width = 0;
				}
				else {
					AutoSizeAxes = Axes.X;
				}
			}

			// base OsuTabItem makes font bold on activation, we don't want that here
			protected override void OnActivated () => FadeHovered();

			protected override void OnDeactivated () => FadeUnhovered();
		}
	}
}