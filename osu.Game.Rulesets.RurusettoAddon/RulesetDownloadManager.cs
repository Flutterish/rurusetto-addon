using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RulesetDownloadManager {
		RurusettoAPI API;
		Storage storage;

		public RulesetDownloadManager ( RurusettoAPI API, Storage storage ) {
			this.API = API;
			this.storage = storage;

			PerformPreCleanup();
		}

		//private Dictionary<string, Bindable<DownloadState>> downloadStates = new();
		public Bindable<DownloadState> GetStateBindable ( RulesetIdentity ruleset ) {
			return new();
			//if ( !downloadStates.TryGetValue( shortName, out var state ) ) {
			//	downloadStates.Add( shortName, state = new Bindable<DownloadState>( DownloadState.Unknown ) );
			//	CheckAvailability( ruleset );
			//}

			//return state;
		}

		public void BindWith ( RulesetIdentity ruleset, IBindable<DownloadState> bindable ) {
			//bindable.BindTo( GetStateBindable( ruleset ) );
		}

		public void CheckAvailability ( RulesetIdentity ruleset ) {
			//var status = GetStateBindable( ruleset );

			//status.Value = DownloadState.Unknown;

			//API.RequestRulesetDetail( shortName ).ContinueWith( t => {
			//	if ( tasks.TryGetValue( shortName, out var task ) ) {
			//		if ( task.Type is TaskType.Install or TaskType.Update ) {
			//			status.Value = DownloadState.ToBeImported;
			//			return;
			//		}
			//		else if ( task.Type == TaskType.Remove ) {
			//			status.Value = DownloadState.ToBeRemoved;
			//			return;
			//		}
			//	}
				
			//	if ( UnimportedRulesets.Any( x => x.shortname == shortName ) || GetLocalRuleset( t.Result.Slug, t.Result.Name, t.Result.GithubFilename ) != null ) {
			//		status.Value = DownloadState.AvailableLocally;
			//		return;
			//	}

			//	if ( t.Result.CanDownload && status.Value is DownloadState.NotAvailableOnline or DownloadState.Unknown ) {
			//		status.Value = DownloadState.AvailableOnline;
			//	}
			//	else if ( !t.Result.CanDownload && status.Value is DownloadState.AvailableOnline or DownloadState.Unknown ) {
			//		status.Value = DownloadState.NotAvailableOnline;
			//	}
			//} );
		}

		private bool wasTaskCancelled ( RulesetIdentity ruleset, RulesetManagerTask task ) {
			return false;
			//return !tasks.TryGetValue( shortName, out var currentTask ) || !ReferenceEquals( task, currentTask );
		}

		//Dictionary<string, RulesetManagerTask> tasks = new();
		private void createDownloadTask ( RulesetIdentity ruleset, TaskType type, DownloadState duringState, DownloadState finishedState ) {
			//var task = new RulesetManagerTask( type, null );
			//tasks[ shortName ] = task;

			//GetStateBindable( ruleset ).Value = duringState;

			//API.RequestRulesetDetail( shortName ).ContinueWith( async t => {
			//	if ( wasTaskCancelled( shortName, task ) ) return;

			//	if ( !t.Result.CanDownload ) {
			//		tasks.Remove( shortName );
			//		return;
			//	}
				
			//	var filename = $"./rurusetto-addon-temp/{t.Result.GithubFilename}";
			//	if ( !storage.Exists( filename ) ) {
			//		var data = await new HttpClient().GetStreamAsync( t.Result.Download );
			//		if ( wasTaskCancelled( shortName, task ) ) return;

			//		var file = storage.GetStream( filename, FileAccess.Write, FileMode.OpenOrCreate );
			//		await data.CopyToAsync( file );
			//		file.Dispose();
			//		data.Dispose();

			//		if ( wasTaskCancelled( shortName, task ) ) return;
			//	}

			//	tasks[ shortName ] = task with { Source = filename };
			//	GetStateBindable( ruleset ).Value = finishedState;
			//} );
		}

		public void DownloadRuleset ( RulesetIdentity ruleset ) {
			//if ( tasks.TryGetValue( shortName, out var task ) && task.Type == TaskType.Install )
			//	return;

			//createDownloadTask( shortName, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}
		public void CancelRulesetDownload ( RulesetIdentity ruleset ) {
			//if ( tasks.TryGetValue( shortName, out var task ) && task.Type is TaskType.Install or TaskType.Update ) {
			//	tasks.Remove( shortName );
			//	CheckAvailability( shortName );
			//}
		}

		public void UpdateRuleset ( RulesetIdentity ruleset ) {
			//if ( tasks.TryGetValue( shortName, out var task ) && task.Type == TaskType.Update )
			//	return;

			//createDownloadTask( shortName, TaskType.Install, DownloadState.Downloading, DownloadState.ToBeImported );
		}

		public void RemoveRuleset ( RulesetIdentity ruleset ) {
			//if ( tasks.TryGetValue( shortName, out var task ) ) {
			//	if ( task.Type == TaskType.Remove ) return;

			//	if ( task.Type is TaskType.Install ) {
			//		tasks.Remove( shortName );
			//		CheckAvailability( shortName );
			//		return;
			//	}
			//}

			//var unimported = UnimportedRulesets.Where( x => x.shortname.ToLower() == shortName.ToLower() );
			//var installed = InstalledRulesetFilenames.Where( x => x.Key.ShortName.ToLower() == shortName.ToLower() );
			//string location;
			//if ( unimported.Any() ) {
			//	location = unimported.First().filename;
			//}
			//else if ( installed.Any() ) {
			//	location = installed.First().Value;
			//}
			//else {
			//	return;
			//}

			//tasks[ shortName ] = new RulesetManagerTask( TaskType.Remove, storage.GetFullPath( $"./rulesets/{location}" ) );
			//GetStateBindable( shortName ).Value = DownloadState.ToBeRemoved;
		}
		public void CancelRulesetRemoval ( RulesetIdentity ruleset ) {
			//if ( tasks.TryGetValue( shortName, out var task ) && task.Type == TaskType.Remove ) {
			//	tasks.Remove( shortName );
			//	CheckAvailability( shortName );
			//}
		}

		public void PerformPreCleanup () {
			//foreach ( var i in storage.GetFiles( "./rulesets", "*.dll-removed" ) ) {
			//	storage.Delete( i );
			//}
		}

		public void PerformTasks () {
			//foreach ( var i in tasks.Values ) {
			//	if ( i.Type == TaskType.Install || i.Type == TaskType.Update ) {
			//		var filename = Path.GetFileName( i.Source );
			//		if ( File.Exists( $"./rulesets/{filename}" ) ) {
			//			File.Move( $"./rulesets/{filename}", $"./rulesets/{filename}-removed" );
			//		}

			//		var to = storage.GetStream( $"./rulesets/{filename}", FileAccess.Write, FileMode.CreateNew );
			//		var from = storage.GetStream( i.Source, FileAccess.Read, FileMode.Open );

			//		from.CopyTo( to );

			//		from.Dispose();
			//		to.Dispose();
			//	}
			//	else if ( i.Type == TaskType.Remove ) {
			//		File.Move( i.Source, i.Source + "-removed" );
			//	}
			//}

			//if ( storage.ExistsDirectory( "./rurusetto-addon-temp/" ) ) {
			//	storage.DeleteDirectory( "./rurusetto-addon-temp/" );
			//}

			//tasks.Clear();
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
