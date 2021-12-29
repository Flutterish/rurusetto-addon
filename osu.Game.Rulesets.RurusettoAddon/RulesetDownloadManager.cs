using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;
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

		private Dictionary<RulesetIdentity, Bindable<DownloadState>> downloadStates = new();
		private Dictionary<RulesetIdentity, Bindable<Availability>> availabilities = new();
		public Bindable<DownloadState> GetStateBindable ( RulesetIdentity ruleset ) {
			if ( !downloadStates.TryGetValue( ruleset, out var state ) ) {
				downloadStates.Add( ruleset, state = new Bindable<DownloadState>( DownloadState.NotDownloading ) );
			}

			return state;
		}

		private Bindable<Availability> getAvailabilityBindable ( RulesetIdentity ruleset, bool checkOnCreate = true ) {
			if ( !availabilities.TryGetValue( ruleset, out var state ) ) {
				availabilities.Add( ruleset, state = new Bindable<Availability>( Availability.Unknown ) );
				if ( checkOnCreate ) CheckAvailability( ruleset );
			}

			return state;
		}
		public Bindable<Availability> GetAvailabilityBindable ( RulesetIdentity ruleset ) {
			return getAvailabilityBindable( ruleset, true );
		}

		public void BindWith ( RulesetIdentity ruleset, IBindable<DownloadState> bindable ) {
			bindable.BindTo( GetStateBindable( ruleset ) );
		}
		public void BindWith ( RulesetIdentity ruleset, IBindable<Availability> bindable ) {
			bindable.BindTo( GetAvailabilityBindable( ruleset ) );
		}

		public void CheckAvailability ( RulesetIdentity ruleset ) {
			var availability = getAvailabilityBindable( ruleset, false );

			availability.Value = Availability.Unknown;

			if ( ruleset.Source == Source.Local || ruleset.IsPresentLocally ) {
				availability.Value |= Availability.AvailableLocally;
			}
			else {
				availability.Value |= Availability.NotAvailableLocally;
			}

			if ( ruleset.Source == Source.Web ) {
				ruleset.RequestDetail().ContinueWith( t => {
					if ( !t.IsFaulted && t.Result.CanDownload ) {
						availability.Value |= Availability.AvailableOnline;
					}
					else {
						availability.Value |= Availability.NotAvailableOnline;
					}
				} );
			}
			else {
				availability.Value |= Availability.NotAvailableOnline;
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
				GetStateBindable( ruleset ).Value = DownloadState.NotDownloading;
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
					GetStateBindable( ruleset ).Value = DownloadState.NotDownloading;
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
				GetStateBindable( ruleset ).Value = DownloadState.NotDownloading;
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

			if ( storage.ExistsDirectory( "./rurusetto-addon-temp/" ) ) {
				storage.DeleteDirectory( "./rurusetto-addon-temp/" );
			}
		}

		public void PerformTasks () {
			foreach ( var i in tasks.Values ) {
				if ( i.Type == TaskType.Install || i.Type == TaskType.Update ) {
					var filename = Path.GetFileName( i.Source );
					var path = storage.GetFullPath( $"./rulesets/{filename}" );
					if ( File.Exists( path ) ) {
						File.Move( path, path + "~" );
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
		NotDownloading,

		Downloading,
		ToBeImported,
		ToBeRemoved
	}

	[Flags]
	public enum Availability {
		Unknown = 0,

		NotAvailableLocally = 1,
		AvailableLocally = 2,
		NotAvailableOnline = 4,
		AvailableOnline = 8,
		Outdated = 16
	}
}
