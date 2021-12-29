using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.Configuration;
using osu.Game.Rulesets.RurusettoAddon.UI.Info;
using osu.Game.Rulesets.RurusettoAddon.UI.Listing;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	[Cached]
	public class RurusettoOverlay : FullscreenOverlay<RurusettoOverlayHeader> {
		FillFlowContainer content;
		OsuScrollContainer scroll;
		Container tabContainer;
		RurusettoAddonRuleset ruleset;

		LoadingLayer loading;
		OverlayTab currentTab;
		ListingTab listing;
		Dictionary<string, InfoTab> infoTabs = new();

		[Cached]
		new RurusettoAPI API = new();

		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
			var dep = new DependencyContainer( base.CreateChildDependencies( parent ) );

			dep.CacheAs( ruleset );
			if ( dep.TryGet<IRulesetStore>( out var store ) ) {
				dep.CacheAs( new RulesetDownloadManager( API, dep.Get<Storage>(), store ) );
			}
			else {
				dep.CacheAs( new RulesetDownloadManager( API, dep.Get<Storage>() ) );
			}

			try {
				var rulesetconfig = dep.Get<IRulesetConfigCache>();
				var config = rulesetconfig?.GetConfigFor( ruleset ) as RurusettoConfigManager;

				config?.BindWith( RurusettoSetting.APIAddress, API.Address );
			}
			catch ( Exception ) { }

			_ = API.RequestImage( StaticAPIResource.DefaultCover );

			return dep;
		}

		public RurusettoOverlay ( RurusettoAddonRuleset ruleset ) : base( OverlayColourScheme.Pink ) {
			this.ruleset = ruleset;

			Add( new OsuContextMenuContainer {
				RelativeSizeAxes = Axes.Both,
				Child = scroll = new OsuScrollContainer( Direction.Vertical ) {
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

			Header.SelectedRuleset.ValueChanged += v => {
				if ( v.NewValue?.Slug == v.OldValue?.Slug )
					return;

				scroll.ScrollToStart();

				currentTab.Hide();

				if ( v.NewValue == null ) {
					currentTab = listing;
				}
				else {
					if ( !infoTabs.TryGetValue( v.NewValue.Slug, out var tab ) ) {
						tab = new( v.NewValue );
						tabContainer.Add( tab );
						infoTabs.Add( v.NewValue.Slug, tab );
					}

					currentTab = tab;
				}

				currentTab.Show();
				tabContainer.ChangeChildDepth( currentTab, (float)-Clock.CurrentTime );
				updateLoading();
			};
		}

		protected override void PopIn () {
			base.PopIn();

			scroll.ScrollToStart();
		}

		protected override void PopOutComplete () {
			base.PopOutComplete();

			loadingTabs.Clear();
			updateLoading();
			Header.SelectedRuleset.Value = null;
			foreach ( var i in infoTabs ) {
				tabContainer.Remove( i.Value );
				i.Value.Dispose();
			}
			infoTabs.Clear();
			listing.ReloadListing();
		}

		HashSet<OverlayTab> loadingTabs = new();
		public void StartLoading ( OverlayTab tab ) {
			loadingTabs.Add( tab );
			updateLoading();
		}

		public void FinishLoadiong ( OverlayTab tab ) {
			loadingTabs.Remove( tab );
			updateLoading();
		}

		private void updateLoading () {
			if ( loadingTabs.Contains( currentTab ) ) {
				loading.Show();
			}
			else {
				loading.Hide();
			}
		}

		protected override RurusettoOverlayHeader CreateHeader ()
			=> new();
	}
}
