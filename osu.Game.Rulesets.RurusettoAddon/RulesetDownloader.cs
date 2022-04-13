using osu.Framework.Platform;
using System.IO;
using System.Net.Http;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RulesetDownloader {
		RurusettoAPI API;
		Storage storage;

		public RulesetDownloader ( RurusettoAPI API, Storage storage ) {
			this.API = API;
			this.storage = storage;
		}

		Dictionary<APIRuleset, Bindable<DownloadState>> downloadStates = new();
		Dictionary<APIRuleset, Bindable<Availability>> availabilities = new();
		public Bindable<DownloadState> GetStateBindable ( APIRuleset ruleset )
			=> downloadStates.GetOrAdd( ruleset, () => new Bindable<DownloadState>( DownloadState.NotDownloading ) );

		Bindable<Availability> getAvailabilityBindable ( APIRuleset ruleset, bool checkOnCreate = true ) {
			return availabilities.GetOrAdd( ruleset, () => new Bindable<Availability>( Availability.Unknown ), _ => {
				if ( checkOnCreate ) CheckAvailability( ruleset );
			} );
		}
		public Bindable<Availability> GetAvailabilityBindable ( APIRuleset ruleset ) {
			return getAvailabilityBindable( ruleset, true );
		}

		public void BindWith ( APIRuleset ruleset, IBindable<DownloadState> bindable ) {
			bindable.BindTo( GetStateBindable( ruleset ) );
		}
		public void BindWith ( APIRuleset ruleset, IBindable<Availability> bindable ) {
			bindable.BindTo( GetAvailabilityBindable( ruleset ) );
		}

		public void CheckAvailability ( APIRuleset ruleset ) {
			var availability = getAvailabilityBindable( ruleset, false );

			availability.Value = Availability.Unknown;

			if ( ruleset.Source == Source.Local || ruleset.IsPresentLocally )
				availability.Value |= Availability.AvailableLocally;
			else
				availability.Value |= Availability.NotAvailableLocally;

			if ( ruleset.Source == Source.Web ) {
				ruleset.RequestDetail( detail => {
					if ( detail.CanDownload )
						availability.Value |= Availability.AvailableOnline;
					else
						availability.Value |= Availability.NotAvailableOnline;
				}, failure: _ => {
					availability.Value |= Availability.NotAvailableOnline;
				} );

				if ( ruleset.ListingEntry?.Status is Status s ) {
					if ( File.Exists( ruleset.LocalPath ) ) {
						var info = new FileInfo( ruleset.LocalPath );
						info.Refresh();

						if ( s.LatestUpdate.HasValue && info.LastWriteTimeUtc < s.LatestUpdate.Value )
							availability.Value |= Availability.Outdated;
						else if ( s.FileSize != 0 && info.Length != s.FileSize )
							availability.Value |= Availability.Outdated;
					}
				}
			}
			else {
				availability.Value |= Availability.NotAvailableOnline;
			}
		}

		bool wasTaskCancelled ( APIRuleset ruleset, RulesetManagerTask task ) {
			return !tasks.TryGetValue( ruleset, out var currentTask ) || !ReferenceEquals( task, currentTask );
		}

		Dictionary<APIRuleset, RulesetManagerTask> tasks = new();
		void createDownloadTask ( APIRuleset ruleset, TaskType type, DownloadState duringState, DownloadState finishedState ) {
			var task = new RulesetManagerTask( type, null );
			tasks[ ruleset ] = task;

			GetStateBindable( ruleset ).Value = duringState;

			ruleset.RequestDetail( async detail => {
				if ( wasTaskCancelled( ruleset, task ) ) return;

				if ( !detail.CanDownload ) {
					tasks.Remove( ruleset );
					return;
				}

				var filename = $"./rurusetto-addon-temp/{detail.GithubFilename}";
				if ( !storage.Exists( filename ) ) {
					using var data = await new HttpClient().GetStreamAsync( detail.Download );
					if ( wasTaskCancelled( ruleset, task ) ) return;

					using var file = storage.GetStream( filename, FileAccess.Write, FileMode.OpenOrCreate );
					await data.CopyToAsync( file );

					if ( wasTaskCancelled( ruleset, task ) ) return;
				}

				tasks[ ruleset ] = task with { Source = filename };
				GetStateBindable( ruleset ).Value = finishedState;

			}, failure: e => {
				if ( wasTaskCancelled( ruleset, task ) ) return;
				tasks.Remove( ruleset );
				API.LogFailure( $"Downloader could not retrieve detail for {ruleset}", e );
			} );
		}

		public void DownloadRuleset ( APIRuleset ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type == TaskType.Install )
				return;

			createDownloadTask( ruleset, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}
		public void CancelRulesetDownload ( APIRuleset ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type is TaskType.Install or TaskType.Update ) {
				tasks.Remove( ruleset );
				GetStateBindable( ruleset ).Value = DownloadState.NotDownloading;
			}
		}

		public void UpdateRuleset ( APIRuleset ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type == TaskType.Update )
				return;

			createDownloadTask( ruleset, TaskType.Update, DownloadState.Downloading, DownloadState.ToBeImported );
		}

		public void RemoveRuleset ( APIRuleset ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) ) {
				if ( task.Type == TaskType.Remove ) return;

				if ( task.Type is TaskType.Install or TaskType.Update ) {
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
		public void CancelRulesetRemoval ( APIRuleset ruleset ) {
			if ( tasks.TryGetValue( ruleset, out var task ) && task.Type == TaskType.Remove ) {
				tasks.Remove( ruleset );
				GetStateBindable( ruleset ).Value = DownloadState.NotDownloading;
			}
		}

		/// <summary>
		/// Cleans up all files used by previous instances
		/// </summary>
		/// <returns>Whether the previous instance possibly finished its work. A value of <see langword="false"/> means it definitely did not finish the work.</returns>
		public bool PerformPreCleanup () {
			foreach ( var i in storage.GetFiles( "./rulesets", "*.dll-removed" ) ) {
				storage.Delete( i );
			}
			// we use this format, but the above was used previously so we should keep it
			foreach ( var i in storage.GetFiles( "./rulesets", "*.dll~" ) ) {
				storage.Delete( i );
			}

			if ( storage.ExistsDirectory( "./rurusetto-addon-temp/" ) ) {
				storage.DeleteDirectory( "./rurusetto-addon-temp/" );
				return false;
			}

			return true;
		}

		public void PerformTasks () { // TODO we can do this at runtime, with user consent as its probably not the greatest idea
			foreach ( var i in tasks.Values ) {
				if ( i.Source is null )
					continue;

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

	public record RulesetManagerTask ( TaskType Type, string? Source );
	public enum TaskType {
		Install,
		Update,
		Remove
	}
}
