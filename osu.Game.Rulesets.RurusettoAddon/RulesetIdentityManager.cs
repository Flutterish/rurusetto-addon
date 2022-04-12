using Humanizer;
using osu.Framework.Platform;
using System.IO;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon;

public class RulesetIdentityManager {
	private Storage? storage;
	private IRulesetStore? rulesetStore;
	private RurusettoAPI? API;

	public RulesetIdentityManager ( Storage? storage, IRulesetStore? rulesetStore, RurusettoAPI? API ) {
		this.storage = storage;
		this.rulesetStore = rulesetStore;
		this.API = API;
	}

	List<APIRuleset> cachedIdentities = new();
	AsyncLazy<RulesetIdentities>? identities;
	public async Task<RulesetIdentities> RequestIdentities () {
		if ( identities is null ) {
			identities = new( async () => await getIdentities() );
		}
		return await identities.Value;
	}

	public void Refresh () {
		identities = null;
	}

	private async Task<RulesetIdentities> getIdentities () {
		var (newIdentities, hasLocal, hasWeb) = await requestIdentities();

		// we need to merge them since we dont want anything to get out of sync, like the download manager which uses a given APIRuleset instance
		lock ( cachedIdentities ) {
			for ( int i = 0; i < newIdentities.Count; i++ ) {
				var ruleset = newIdentities[i];

				var match = cachedIdentities.FirstOrDefault( x =>
					( x.Source is Source.Web && ruleset.Source is Source.Web && x.Slug == ruleset.Slug ) ||
					( x.Source is Source.Local && ruleset.Source is Source.Local && x.LocalPath == ruleset.LocalPath ) ||
					( x.Source is Source.Local && ruleset.Source is Source.Web && Path.GetFileName( x.LocalPath ) == Path.GetFileName( ruleset.ListingEntry?.Download ) ) ||
					( x.Source is Source.Web && ruleset.Source is Source.Local && Path.GetFileName( ruleset.LocalPath ) == Path.GetFileName( x.ListingEntry?.Download ) )
				);

				if ( match != null ) {
					match.Merge( ruleset );
					newIdentities[i] = match;
				}
				else {
					cachedIdentities.Add( ruleset );
				}
			}
		}

		return new( newIdentities ) {
			ContainsWebListing = hasWeb,
			ContainsLocalListing = hasLocal
		};
	}

	private async Task<(List<APIRuleset> rulesets, bool hasLocal, bool hasWeb)> requestIdentities () {
		List<APIRuleset> identities = new();

		Dictionary<string, APIRuleset> webFilenames = new();
		Dictionary<string, APIRuleset> webNames = new();

		bool hasLocal = storage != null;
		bool hasWeb = false;

		if ( API != null ) {
			IEnumerable<ListingEntry> listing = Array.Empty<ListingEntry>();
			var task = new TaskCompletionSource();

			API.RequestRulesetListing( result => {
				listing = result;
				hasWeb = true;
				task.SetResult();

			}, failure: e => {
				API.LogFailure( $"Identity manager could not retrieve ruleset listing", e );
				task.SetResult();
			},
				cancelled: () => task.SetResult()
			);

			await task.Task;

			foreach ( var entry in listing ) {
				APIRuleset id;
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
			Dictionary<string, APIRuleset> localPaths = new();
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

		return (identities, hasLocal, hasWeb);
	}
}

public record RulesetIdentities ( IEnumerable<APIRuleset> Rulesets ) : IEnumerable<APIRuleset> {
	public bool ContainsWebListing { get; init; }
	public bool ContainsLocalListing { get; init; }

	public IEnumerator<APIRuleset> GetEnumerator () {
		return Rulesets.GetEnumerator();
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
		return ( (System.Collections.IEnumerable)Rulesets ).GetEnumerator();
	}
}