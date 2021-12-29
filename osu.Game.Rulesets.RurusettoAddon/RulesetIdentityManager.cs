using osu.Framework.Platform;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;
using System.Collections.Generic;
using System.IO;
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

		private async Task<List<RulesetIdentity>> requestIdentities () {
			List<RulesetIdentity> identities = new();

			Dictionary<string, RulesetIdentity> webFilenames = new();
			if ( API != null ) {
				var listing = await API.RequestRulesetListing();

				foreach ( var entry in listing ) {
					RulesetIdentity id;
					identities.Add( id = new() {
						Source = Source.Web,
						API = API,
						Name = entry.Name,
						Slug = entry.Slug
					} );

					var filename = Path.GetFileName( entry.Download );
					if ( !string.IsNullOrWhiteSpace( filename ) ) {
						// there shouldnt be multiple, but if there are we dont want to crash
						// TODO we might want to report if this ever happens
						webFilenames.TryAdd( filename, id );
					}
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
							continue;
						}

						if ( webFilenames.TryGetValue( filename, out var id ) ) {
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
					foreach ( var path in storage.GetFiles( "./rulesets", "*.dll" ) ) {
						if ( !localPaths.ContainsKey( storage.GetFullPath( path ) ) ) {
							identities.Add( new() {
								Source = Source.Local,
								API = API,
								IsPresentLocally = true,
								HasImportFailed = true,
								LocalPath = storage.GetFullPath( path )
							} );
						}
					}
				}
			}
			else if ( storage != null ) {
				foreach ( var path in storage.GetFiles( "./rulesets", "*.dll" ) ) {
					if ( !webFilenames.ContainsKey( Path.GetFileName( path ) ) ) {
						identities.Add( new() {
							Source = Source.Local,
							API = API,
							IsPresentLocally = true,
							IsModifiable = true,
							LocalPath = storage.GetFullPath( path )
						} );
					}
				}
			}

			return identities;
		}
	}
}
