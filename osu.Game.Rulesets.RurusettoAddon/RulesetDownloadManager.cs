using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RulesetDownloadManager {
		RurusettoAPI API;
		Storage storage;
		IRulesetStore store;

		public RulesetDownloadManager ( RurusettoAPI API, Storage storage ) {
			this.API = API;
			this.storage = storage;
		}

		public RulesetDownloadManager ( RurusettoAPI API, Storage storage, IRulesetStore store ) : this( API, storage ) {
			this.store = store;
		}

		private Dictionary<string, Bindable<DownloadState>> downloadStates = new();
		private Dictionary<string, RulesetInfo> rulesets = new();

		public Bindable<DownloadState> GetStateBindable ( string shortName ) {
			if ( !downloadStates.TryGetValue( shortName, out var state ) ) {
				downloadStates.Add( shortName, state = new Bindable<DownloadState>( DownloadState.Unknown ) );
				CheckAvailability( shortName );
			}

			return state;
		}

		public void BindWith ( string shortName, IBindable<DownloadState> bindable ) {
			bindable.BindTo( GetStateBindable( shortName ) );
		}

		public RulesetInfo GetLocalRuleset ( string fullName, string shortName ) {
			if ( !rulesets.TryGetValue( shortName, out var ruleset ) ) {
				ruleset = store?.AvailableRulesets.FirstOrDefault( r =>
					r.Name.ToLower() == fullName.ToLower() ||
					r.ShortName.ToLower() == shortName.ToLower()
				) as RulesetInfo;

				if ( ruleset != null ) {
					rulesets.Add( shortName, ruleset );
				}
			}

			return ruleset;
		}

		public void CheckAvailability ( string shortName ) {
			var status = GetStateBindable( shortName );

			status.Value = DownloadState.Unknown;

			API.RequestRulesetDetail( shortName ).ContinueWith( t => {
				if ( tasks.TryGetValue( shortName, out var task ) ) {
					if ( task.Type is TaskType.Install or TaskType.Update ) {
						status.Value = DownloadState.ToBeImported;
						return;
					}
					else if ( task.Type == TaskType.Remove ) {
						status.Value = DownloadState.ToBeRemoved;
						return;
					}
				}
				
				if ( GetLocalRuleset( t.Result.Name, t.Result.ShortName ) != null ) {
					status.Value = DownloadState.AvailableLocally;
					return;
				}

				if ( t.Result.CanDownload && status.Value is DownloadState.NotAvailableOnline or DownloadState.Unknown ) {
					status.Value = DownloadState.AvailableOnline;
				}
				else if ( !t.Result.CanDownload && status.Value is DownloadState.AvailableOnline or DownloadState.Unknown ) {
					status.Value = DownloadState.NotAvailableOnline;
				}
			} );
		}

		private bool taskCancelled ( string shortName, RulesetManagerTask task ) {
			return !tasks.TryGetValue( shortName, out var currentTask ) || !ReferenceEquals( task, currentTask );
		}

		Dictionary<string, RulesetManagerTask> tasks = new();
		private void createDownloadTask ( string shortName, TaskType type, DownloadState duringState, DownloadState finishedState ) {
			var task = new RulesetManagerTask( type, null );
			tasks[ shortName ] = task;

			GetStateBindable( shortName ).Value = duringState;

			API.RequestRulesetDetail( shortName ).ContinueWith( async t => {
				if ( taskCancelled( shortName, task ) ) return;

				if ( !t.Result.CanDownload ) {
					tasks.Remove( shortName );
					return;
				}
				
				var filename = $"./rurusetto-addon-temp/{t.Result.GithubFilename}";
				if ( !storage.Exists( filename ) ) {
					var data = await new HttpClient().GetStreamAsync( t.Result.Download );
					if ( taskCancelled( shortName, task ) ) return;

					var file = storage.GetStream( filename, System.IO.FileAccess.Write, System.IO.FileMode.OpenOrCreate );
					await data.CopyToAsync( file );
					file.Dispose();
					data.Dispose();

					if ( taskCancelled( shortName, task ) ) return;
				}

				tasks[ shortName ] = task with { Source = filename };
				GetStateBindable( shortName ).Value = finishedState;
			} );
		}

		public void DownloadRuleset ( string shortName ) {
			if ( tasks.TryGetValue( shortName, out var task ) && task.Type == TaskType.Install )
				return;

			createDownloadTask( shortName, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}
		public void CancelRulesetDownload ( string shortName ) {
			if ( tasks.TryGetValue( shortName, out var task ) && task.Type is TaskType.Install or TaskType.Update ) {
				tasks.Remove( shortName );
				CheckAvailability( shortName );
			}
		}

		public void UpdateRuleset ( string shortName ) {
			if ( tasks.TryGetValue( shortName, out var task ) && task.Type == TaskType.Update )
				return;

			createDownloadTask( shortName, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}

		public void RemoveRuleset ( string shortName ) {
			if ( tasks.TryGetValue( shortName, out var task ) ) {
				if ( task.Type == TaskType.Remove ) return;

				if ( task.Type is TaskType.Install ) {
					tasks.Remove( shortName );
					CheckAvailability( shortName );
					return;
				}
			}

			if ( GetLocalRuleset( "", shortName )?.CreateInstance().GetType().Assembly.Location is not string location )
				return;

			if ( !storage.Exists( location ) )
				return;

			tasks[ shortName ] = new RulesetManagerTask( TaskType.Remove, location );
			GetStateBindable( shortName ).Value = DownloadState.ToBeRemoved;
		}

		public void CancelRulesetRemoval ( string shortName ) {
			if ( tasks.TryGetValue( shortName, out var task ) && task.Type == TaskType.Remove ) {
				tasks.Remove( shortName );
				CheckAvailability( shortName );
			}
		}
	}

	public enum DownloadState {
		Unknown,

		NotAvailableOnline,
		AvailableOnline,
		AvailableLocally,
		OutdatedAvailableLocally,

		Downloading,
		ToBeImported,

		ToBeRemoved
	}
}
