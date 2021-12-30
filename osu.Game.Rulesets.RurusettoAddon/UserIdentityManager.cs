using osu.Game.Rulesets.RurusettoAddon.API;
using System.Collections.Generic;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class UserIdentityManager {
		public readonly RurusettoAPI API;

		public UserIdentityManager ( RurusettoAPI API ) {
			this.API = API;
			unknownUser = UserIdentity.Local( API );
		}

		UserIdentity unknownUser;
		private Dictionary<int, UserIdentity> users = new();
		public UserIdentity GetUserIdentity ( int id ) {
			if ( !users.TryGetValue( id, out var user ) ) {
				users.Add( id, user = UserIdentity.FromID( API, id ) );
			}

			return user;
		}
		public UserIdentity GetUserIdentity ( UserDetail detail ) {
			if ( detail?.ID is not int id )
				return unknownUser;
			else
				return GetUserIdentity( id );
		}
	}
}
