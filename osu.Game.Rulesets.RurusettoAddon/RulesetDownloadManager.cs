using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Rulesets.RurusettoAddon.API;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RulesetDownloadManager {
		RurusettoAPI API;
		Storage storage;

		public RulesetDownloadManager ( RurusettoAPI API, Storage storage ) {
			this.API = API;
			this.storage = storage;

			PerformPreCleanup();
		}

		// TODO probably split this into download state and online availiability state
		private Dictionary<RulesetIdentity, Bindable<DownloadState>> downloadStates = new();
		public Bindable<DownloadState> GetStateBindable ( RulesetIdentity ruleset ) {
			if ( !downloadStates.TryGetValue( ruleset, out var state ) ) {
				downloadStates.Add( ruleset, state = new Bindable<DownloadState>( DownloadState.Unknown ) );
				CheckAvailability( ruleset );
			}

			return state;
		}

		public void BindWith ( RulesetIdentity ruleset, IBindable<DownloadState> bindable ) {
			if ( !downloadStates.TryGetValue( ruleset, out var state ) ) {
				downloadStates.Add( ruleset, state = new Bindable<DownloadState>( DownloadState.Unknown ) );
				CheckAvailability( ruleset );
			}

			bindable.BindTo( state );
		}

		public void CheckAvailability ( RulesetIdentity ruleset ) {
			var status = GetStateBindable( ruleset );

			status.Value = DownloadState.Unknown;

			if ( ruleset.Source == Source.Local || ruleset.IsPresentLocally ) {
				status.Value = DownloadState.AvailableLocally;
			}
			else if ( ruleset.Source == Source.Web ) {
				if ( tasks.TryGetValue( ruleset, out var task ) ) {
					if ( task.Type is TaskType.Install or TaskType.Update ) {
						status.Value = DownloadState.ToBeImported;
						return;
					}
					else if ( task.Type == TaskType.Remove ) {
						status.Value = DownloadState.ToBeRemoved;
						return;
					}
				}

				ruleset.RequestDetail().ContinueWith( t => {
					if ( t.Result.CanDownload && status.Value is DownloadState.NotAvailableOnline or DownloadState.Unknown ) {
						status.Value = DownloadState.AvailableOnline;
					}
					else if ( !t.Result.CanDownload && status.Value is DownloadState.AvailableOnline or DownloadState.Unknown ) {
						status.Value = DownloadState.NotAvailableOnline;
					}
				} );
			}
		}

		private bool wasTaskCancelled ( RulesetIdentity ruleset, RulesetManagerTask task ) {
			return !tasks.TryGetValue( ruleset, out var currentTask ) || !ReferenceEquals( task, currentTask );
		}

		Dictionary<RulesetIdentity, RulesetManagerTask> tasks = new();
		private void createDownloadTask ( RulesetIdentity ruleset, TaskType type, DownloadState duringState, DownloadState finishedState ) {
			var task = new RulesetManagerTask( type, null );
			tasks[ ruleset ] = task;

			GetStateBindable( ruleset ).Value = duringState;


			ruleset.RequestDetail().ContinueWith( async t => {
				if ( wasTaskCancelled( ruleset, task ) ) return;

				if ( !t.Result.CanDownload ) {
					tasks.Remove( ruleset );
					return;
				}
				
				var filename = $"./rurusetto-addon-temp/{t.Result.GithubFilename}";
				if ( !storage.Exists( filename ) ) {
					var data = await new HttpClient().GetStreamAsync( t.Result.Download );
					if ( wasTaskCancelled( ruleset, task ) ) return;

					var file = storage.GetStream( filename, FileAccess.Write, FileMode.OpenOrCreate );
					await data.CopyToAsync( file );
					file.Dispose();
					data.Dispose();

					if ( wasTaskCancelled( ruleset, task ) ) return;
				}

				tasks[ ruleset ] = task with { Source = filename };
				GetStateBindable( ruleset ).Value = finishedState;
			} );
		}

		public void DownloadRuleset ( RulesetIdentity ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type == TaskType.Install )
				return;

			createDownloadTask( ruleset, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}
		public void CancelRulesetDownload ( RulesetIdentity ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type is TaskType.Install or TaskType.Update ) {
				tasks.Remove( ruleset );
				CheckAvailability( ruleset );
			}
		}

		public void UpdateRuleset ( RulesetIdentity ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type == TaskType.Update )
				return;

			createDownloadTask( ruleset, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}

		public void RemoveRuleset ( RulesetIdentity ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) ) {
				if ( task.Type == TaskType.Remove ) return;

				if ( task.Type is TaskType.Install ) {
					tasks.Remove( ruleset );
					CheckAvailability( ruleset );
					return;
				}
			}

			if ( string.IsNullOrWhiteSpace( ruleset.LocalPath ) ) {
				// TODO report this
				return;
			}

			tasks[ ruleset ] = new RulesetManagerTask( TaskType.Remove, ruleset.LocalPath );
			GetStateBindable( ruleset ).Value = DownloadState.ToBeRemoved;
		}
		public void CancelRulesetRemoval ( RulesetIdentity ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type == TaskType.Remove ) {
				tasks.Remove( ruleset );
				CheckAvailability( ruleset );
			}
		}

		public void PerformPreCleanup () {
			foreach ( var i in storage.GetFiles( "./rulesets", "*.dll-removed" ) ) {
				storage.Delete( i );
			}
			// we use this format, but the above was used previously so we should keep it
			foreach ( var i in storage.GetFiles( "./rulesets", "*.dll~" ) ) {
				storage.Delete( i );
			}
		}

		public void PerformTasks () {
			foreach ( var i in tasks.Values ) {
				if ( i.Type == TaskType.Install || i.Type == TaskType.Update ) {
					var filename = Path.GetFileName( i.Source );
					if ( File.Exists( $"./rulesets/{filename}" ) ) {
						File.Move( $"./rulesets/{filename}", $"./rulesets/{filename}-removed" );
					}

					var to = storage.GetStream( $"./rulesets/{filename}", FileAccess.Write, FileMode.CreateNew );
					var from = storage.GetStream( i.Source, FileAccess.Read, FileMode.Open );

					from.CopyTo( to );

					from.Dispose();
					to.Dispose();
				}
				else if ( i.Type == TaskType.Remove ) {
					File.Move( i.Source, i.Source + "~" );
				}
			}

			if ( storage.ExistsDirectory( "./rurusetto-addon-temp/" ) ) {
				storage.DeleteDirectory( "./rurusetto-addon-temp/" );
			}

			tasks.Clear();
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
