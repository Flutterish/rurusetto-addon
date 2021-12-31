using Humanizer;
using osu.Framework.Platform;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RulesetIdentityManager {
		private Storage? storage;
		private IRulesetStore? rulesetStore;
		private RurusettoAPI? API;

		public RulesetIdentityManager ( Storage? storage, IRulesetStore? rulesetStore, RurusettoAPI? API ) {
			this.storage = storage;
			this.rulesetStore = rulesetStore;
			this.API = API;

			identities = new( async () => await requestIdentities() );
		}

		AsyncLazy<IEnumerable<RulesetIdentity>> identities;
		public async Task<IEnumerable<RulesetIdentity>> RequestIdentities () {
			return await identities.Value;
		}

		private async Task<IEnumerable<RulesetIdentity>> requestIdentities () {
			List<RulesetIdentity> identities = new();

			Dictionary<string, RulesetIdentity> webFilenames = new();
			Dictionary<string, RulesetIdentity> webNames = new();
			if ( API != null ) {
				IEnumerable<ListingEntry> listing = Array.Empty<ListingEntry>();
				var task = new TaskCompletionSource();
				API.RequestRulesetListing( result => {
					listing = result;
					task.SetResult();
				}, failure: () => task.SetResult() /* TODO report this */ );

				await task.Task;

				foreach ( var entry in listing ) {
					RulesetIdentity id;
					identities.Add( id = new() {
						Source = Source.Web,
						API = API,
						Name = entry.Name,
						Slug = entry.Slug,
						ListingEntry = entry,
						IsModifiable = true
					} );

					var filename = Path.GetFileName( entry.Download );
					if ( !string.IsNullOrWhiteSpace( filename ) ) {
						// there shouldnt be multiple, but if there are we dont want to crash
						// TODO we might want to report if this ever happens
						webFilenames.TryAdd( filename, id );
					}
					webNames.TryAdd( entry.Name.Humanize().ToLower(), id );
				}
			}

			if ( rulesetStore != null ) {
				Dictionary<string, RulesetIdentity> localPaths = new();
				var imported = rulesetStore.AvailableRulesets;

				foreach ( var ruleset in imported ) {
					try {
						var instance = ruleset.CreateInstance();
						if ( instance is null ) {
							// TODO report this
							continue;
						}

						var path = instance.GetType().Assembly.Location;
						var filename = Path.GetFileName( path );
						if ( string.IsNullOrWhiteSpace( filename ) ) {
							// TODO use type name as filename and if a file with that name isnt found report this
							filename = "";
						}

						if ( webFilenames.TryGetValue( filename, out var id ) || webNames.TryGetValue( ruleset.Name.Humanize().ToLower(), out id ) ) {
							id.IsPresentLocally = true;
							id.LocalPath = path;
							id.LocalRulesetInfo = ruleset;
							id.Name = ruleset.Name;
							id.ShortName = ruleset.ShortName;
							id.IsModifiable = storage != null && path.StartsWith( storage.GetFullPath( "./rulesets" ), StringComparison.Ordinal );
						}
						else {
							identities.Add( id = new() {
								Source = Source.Local,
								API = API,
								IsPresentLocally = true,
								LocalPath = path,
								LocalRulesetInfo = ruleset,
								Name = ruleset.Name,
								ShortName = ruleset.ShortName,
								IsModifiable = storage != null && path.StartsWith( storage.GetFullPath( "./rulesets" ), StringComparison.Ordinal )
							} );
						}

						localPaths.Add( path, id );
					}
					catch ( Exception ) {
						// TODO report this
					}
				}

				if ( storage != null ) {
					foreach ( var path in storage.GetFiles( "./rulesets", "osu.Game.Rulesets.*.dll" ) ) {
						if ( localPaths.TryGetValue( storage.GetFullPath( path ), out var id ) ) {
							// we already know its there then
						}
						else if ( webFilenames.TryGetValue( Path.GetFileName( path ), out id ) ) {
							id.IsPresentLocally = true;
							id.IsModifiable = true;
							id.LocalPath = storage.GetFullPath( path );
							id.HasImportFailed = true;
						}
						else {
							identities.Add( new() {
								Source = Source.Local,
								API = API,
								Name = Path.GetFileName( path ).Split( '.' ).SkipLast( 1 ).Last(),
								IsPresentLocally = true,
								HasImportFailed = true,
								LocalPath = storage.GetFullPath( path ),
								IsModifiable = true
							} );
						}
					}
				}
			}
			else if ( storage != null ) {
				foreach ( var path in storage.GetFiles( "./rulesets", "osu.Game.Rulesets.*.dll" ) ) {
					if ( webFilenames.TryGetValue( Path.GetFileName( path ), out var id ) ) {
						id.IsPresentLocally = true;
						id.LocalPath = storage.GetFullPath( path );
						id.IsModifiable = true;
					}
					else {
						identities.Add( new() {
							Source = Source.Local,
							API = API,
							Name = Path.GetFileName( path ).Split( '.' ).SkipLast( 1 ).Last(),
							IsPresentLocally = true,
							LocalPath = storage.GetFullPath( path ),
							IsModifiable = true
						} );
					}
				}
			}

			return identities;
		}
	}
}
