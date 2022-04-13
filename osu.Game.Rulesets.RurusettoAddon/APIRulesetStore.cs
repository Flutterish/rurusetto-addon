using Humanizer;
using osu.Framework.Platform;
using System.IO;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon;

public class APIRulesetStore {
	public Storage? Storage { get; init; }
	public IRulesetStore? RulesetStore { get; init; }
	public RurusettoAPI? API { get; init; }

	List<APIRuleset> cachedIdentities = new();
	Task<RulesetIdentities>? identities;
	public Task<RulesetIdentities> RequestIdentities () {
		if ( identities is null )
			identities = getIdentities();

		return identities;
	}

	public void Refresh () => identities = null;

	async Task<RulesetIdentities> getIdentities () {
		var (newIdentities, hasLocal, hasWeb) = await fetchIdentities();

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

	async Task<(List<APIRuleset> rulesets, bool hasLocal, bool hasWeb)> fetchIdentities () {
		List<APIRuleset> identities = new();

		Dictionary<string, APIRuleset> webFilenames = new();
		Dictionary<string, APIRuleset> webNames = new();

		bool hasLocal = Storage != null;
		bool hasWeb = false;

		if ( API != null ) {
			IEnumerable<ListingEntry> listing = Array.Empty<ListingEntry>();
			var task = new TaskCompletionSource();

			API.RequestRulesetListing( result => {
				listing = result;
				hasWeb = true;
				task.SetResult();

			}, failure: e => {
				API.LogFailure( $"API ruleset store could not retrieve ruleset listing", e );
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

		if ( RulesetStore != null ) {
			Dictionary<string, APIRuleset> localPaths = new();
			var imported = RulesetStore.AvailableRulesets;

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
						id.IsModifiable = Storage != null && path.StartsWith( Storage.GetFullPath( "./rulesets" ), StringComparison.Ordinal );
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
							IsModifiable = Storage != null && path.StartsWith( Storage.GetFullPath( "./rulesets" ), StringComparison.Ordinal )
						} );
					}

					localPaths.Add( path, id );
				}
				catch ( Exception ) {
					// TODO report this
				}
			}

			if ( Storage != null ) {
				foreach ( var path in Storage.GetFiles( "./rulesets", "osu.Game.Rulesets.*.dll" ) ) {
					if ( localPaths.TryGetValue( Storage.GetFullPath( path ), out var id ) ) {
						// we already know its there then
					}
					else if ( webFilenames.TryGetValue( Path.GetFileName( path ), out id ) ) {
						id.IsPresentLocally = true;
						id.IsModifiable = true;
						id.LocalPath = Storage.GetFullPath( path );
						id.HasImportFailed = true;
					}
					else {
						identities.Add( new() {
							Source = Source.Local,
							API = API,
							Name = Path.GetFileName( path ).Split( '.' ).SkipLast( 1 ).Last(),
							IsPresentLocally = true,
							HasImportFailed = true,
							LocalPath = Storage.GetFullPath( path ),
							IsModifiable = true
						} );
					}
				}
			}
		}
		else if ( Storage != null ) {
			foreach ( var path in Storage.GetFiles( "./rulesets", "osu.Game.Rulesets.*.dll" ) ) {
				if ( webFilenames.TryGetValue( Path.GetFileName( path ), out var id ) ) {
					id.IsPresentLocally = true;
					id.LocalPath = Storage.GetFullPath( path );
					id.IsModifiable = true;
				}
				else {
					identities.Add( new() {
						Source = Source.Local,
						API = API,
						Name = Path.GetFileName( path ).Split( '.' ).SkipLast( 1 ).Last(),
						IsPresentLocally = true,
						LocalPath = Storage.GetFullPath( path ),
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