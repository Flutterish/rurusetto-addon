using osu.Game.Rulesets.RurusettoAddon.API;
using System.Collections.Generic;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class UserIdentityManager {
		public readonly RurusettoAPI API;

		public UserIdentityManager ( RurusettoAPI API ) {
			this.API = API;
			unknownUser = APIUser.Local( API );
		}

		APIUser unknownUser;
		private Dictionary<int, APIUser> users = new();
		public APIUser GetUserIdentity ( int id ) {
			if ( !users.TryGetValue( id, out var user ) ) {
				users.Add( id, user = APIUser.FromID( API, id ) );
			}

			return user;
		}
		public APIUser GetUserIdentity ( UserDetail detail ) {
			if ( detail?.ID is not int id )
				return unknownUser;
			else
				return GetUserIdentity( id );
		}
	}
}
