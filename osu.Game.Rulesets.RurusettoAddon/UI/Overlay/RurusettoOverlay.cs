using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.RurusettoAddon.Configuration;
using osu.Game.Rulesets.RurusettoAddon.UI.Listing;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;
using osu.Game.Rulesets.RurusettoAddon.UI.Wiki;
using osuTK.Input;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

[Cached]
public class RurusettoOverlay : FullscreenOverlay<RurusettoOverlayHeader> {
	FillFlowContainer content;
	OverlayScrollContainer scroll;
	Container tabContainer;
	RurusettoAddonRuleset ruleset;

	LoadingLayer loading;
	OverlayTab currentTab;
	ListingTab listing;
	Dictionary<APIRuleset, WikiTab> infoTabs = new();
	Dictionary<APIUser, UserTab> userTabs = new();

	[Cached]
	new RurusettoAPI API = new();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var dep = new DependencyContainer( base.CreateChildDependencies( parent ) );

		dep.CacheAs( ruleset );
		dep.CacheAs( new RulesetIdentityManager( dep.Get<Storage>(), dep.Get<IRulesetStore>(), API ) );
		dep.CacheAs( new UserIdentityManager( API ) );
		RulesetDownloadManager download;
		dep.CacheAs( download = new( API, dep.Get<Storage>() ) );
		if ( !download.PerformPreCleanup() ) {
			Schedule( () => dep.Get<NotificationOverlay>()?.Post( new SimpleErrorNotification { Text = Localisation.Strings.NotificationWorkIncomplete } ) );
		}

		Schedule( () => {
			AddInternal( API );
		} );

		try {
			var rulesetconfig = dep.Get<IRulesetConfigCache>();
			var config = rulesetconfig?.GetConfigFor( ruleset ) as RurusettoConfigManager;

			config?.BindWith( RurusettoSetting.APIAddress, API.Address );
		}
		catch ( Exception ) { }

		return dep;
	}

	public RurusettoOverlay ( RurusettoAddonRuleset ruleset ) : base( OverlayColourScheme.Pink ) {
		this.ruleset = ruleset;

		Add( new OsuContextMenuContainer {
			RelativeSizeAxes = Axes.Both,
			Child = scroll = new OverlayScrollContainer {
				RelativeSizeAxes = Axes.Both,
				ScrollbarVisible = false,

				Child = content = new FillFlowContainer {
					Direction = FillDirection.Vertical,
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				}
			}
		} );

		Header.Depth = -1;
		content.Add( Header );
		content.Add( tabContainer = new() {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y
		} );

		tabContainer.Add( currentTab = listing = new() );
		Schedule( () => {
			listing.Show();
			listing.ReloadListing();
		} );

		Add( loading = new LoadingLayer( dimBackground: true ) );

		Header.Current.ValueChanged += _ => onSelectedInfoChanged();
	}

	private void onSelectedInfoChanged () {
		OverlayTab tab = listing;

		switch ( Header.Current.Value ) {
			case { Tab: APIRuleset ruleset }:
				if ( !infoTabs.TryGetValue( ruleset, out var rulesetTab ) ) {
					infoTabs.Add( ruleset, rulesetTab = new( ruleset ) );
					tabContainer.Add( rulesetTab );
				}
				tab = rulesetTab;
				break;

			case { Tab: APIUser user }:
				if ( !userTabs.TryGetValue( user, out var userTab ) ) {
					userTabs.Add( user, userTab = new( user ) );
					tabContainer.Add( userTab );
				}
				tab = userTab;
				break;
		};

		presentTab( tab );
	}

	private void presentTab ( OverlayTab tab ) {
		scroll.ScrollToStart();
		currentTab.Hide();

		currentTab = tab;

		currentTab.Show();
		tabContainer.ChangeChildDepth( currentTab, (float)-Clock.CurrentTime );
		updateLoading();
	}

	bool isFullHidden;
	protected override void PopIn () {
		base.PopIn();

		if ( isFullHidden ) {
			listing.Refresh();
			isFullHidden = false;
		}

		scroll.ScrollToStart();
	}

	protected override void PopOutComplete () {
		base.PopOutComplete();

		loadingTabs.Clear();
		updateLoading();

		if ( Header.Current.Value.Tab is null && Header.CurrentCategory.Value == Header.ListingTab.Category ) {
			foreach ( var i in infoTabs ) {
				tabContainer.Remove( i.Value );
				i.Value.Dispose();
			}
			userTabs.Clear();
			infoTabs.Clear();
		}
		isFullHidden = true;
	}

	Dictionary<OverlayTab, int> loadingTabs = new();
	public void StartLoading ( OverlayTab tab ) {
		if ( loadingTabs.ContainsKey( tab ) ) {
			loadingTabs[tab]++;
		}
		else {
			loadingTabs.Add( tab, 1 );
		}
		updateLoading();
	}

	public void FinishLoadiong ( OverlayTab tab ) {
		if ( loadingTabs.ContainsKey( tab ) ) {
			loadingTabs[tab]--;

			if ( loadingTabs[tab] <= 0 ) {
				loadingTabs.Remove( tab );
			}
		}
		updateLoading();
	}

	private void updateLoading () {
		if ( loadingTabs.ContainsKey( currentTab ) ) {
			loading.Show();
		}
		else {
			loading.Hide();
		}
	}

	protected override RurusettoOverlayHeader CreateHeader ()
		=> new();

	public override bool OnPressed ( KeyBindingPressEvent<GlobalAction> e ) {
		if ( e.Action == GlobalAction.Back && Header.NavigateBack() ) {
			return true;
		}

		return base.OnPressed( e );
	}

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( ( e.AltPressed || e.ControlPressed ) && e.Key == Key.Left ) {
			Header.NavigateBack();
			return true;
		}
		if ( ( e.AltPressed || e.ControlPressed ) && e.Key == Key.Right ) {
			Header.NavigateForward();
			return true;
		}
		return false;
	}
}