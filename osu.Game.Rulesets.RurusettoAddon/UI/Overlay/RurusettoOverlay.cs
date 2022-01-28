using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.Configuration;
using osu.Game.Rulesets.RurusettoAddon.UI.Info;
using osu.Game.Rulesets.RurusettoAddon.UI.Listing;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	[Cached]
	public class RurusettoOverlay : FullscreenOverlay<RurusettoOverlayHeader> {
		FillFlowContainer content;
		OverlayScrollContainer scroll;
		Container tabContainer;
		RurusettoAddonRuleset ruleset;

		LoadingLayer loading;
		OverlayTab currentTab;
		ListingTab listing;
		Dictionary<APIRuleset, InfoTab> infoTabs = new();
		Dictionary<APIUser, UserTab> userTabs = new();

		[Cached]
		new RurusettoAPI API = new();

		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
			var dep = new DependencyContainer( base.CreateChildDependencies( parent ) );

			dep.CacheAs( ruleset );
			dep.CacheAs( new RulesetIdentityManager( dep.Get<Storage>(), dep.Get<IRulesetStore>(), API ) );
			dep.CacheAs( new UserIdentityManager( API ) );
			dep.CacheAs( new RulesetDownloadManager( API, dep.Get<Storage>() ) );

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

			Header.SelectedInfo.ValueChanged += _ => onSelectedInfoChanged();
		}

		private void onSelectedInfoChanged () {
			if ( Header.SelectedInfo.Value is APIRuleset ruleset ) {
				if ( !infoTabs.TryGetValue( ruleset, out var tab ) ) {
					infoTabs.Add( ruleset, tab = new( ruleset ) );
					tabContainer.Add( tab );
				}

				presentTab( tab );
			}
			else if ( Header.SelectedInfo.Value is APIUser user ) {
				if ( !userTabs.TryGetValue( user, out var tab ) ) {
					userTabs.Add( user, tab = new( user ) );
					tabContainer.Add( tab );
				}

				presentTab( tab );
			}
			else {
				presentTab( listing );
			}
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
			Header.SelectedInfo.Value = null;
			foreach ( var i in infoTabs ) {
				tabContainer.Remove( i.Value );
				i.Value.Dispose();
			}
			userTabs.Clear();
			infoTabs.Clear();
			isFullHidden = true;
		}

		Dictionary<OverlayTab, int> loadingTabs = new();
		public void StartLoading ( OverlayTab tab ) {
			if ( loadingTabs.ContainsKey( tab ) ) {
				loadingTabs[ tab ]++;
			}
			else {
				loadingTabs.Add( tab, 1 );
			}
			updateLoading();
		}

		public void FinishLoadiong ( OverlayTab tab ) {
			if ( loadingTabs.ContainsKey( tab ) ) {
				loadingTabs[ tab ]--;

				if ( loadingTabs[ tab ] <= 0 ) {
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
			if ( e.Action == GlobalAction.Back && Header.SelectedInfo.Value != null ) {
				Header.SelectedInfo.Value = null;
				return true;
			}

			return base.OnPressed( e );
		}
	}
}
