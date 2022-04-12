namespace osu.Game.Rulesets.RurusettoAddon;

public class UserIdentityManager {
	RurusettoAPI API;
	APIUser unknownUser;
	Dictionary<int, APIUser> users = new();
	public UserIdentityManager ( RurusettoAPI API ) {
		this.API = API;
		unknownUser = APIUser.Local( API );
	}

	public APIUser GetUser ( int id )
		=> users.GetOrAdd( id, () => APIUser.FromID( API, id ) );

	public APIUser GetUser ( UserDetail detail )
		=> detail?.ID is int id ? GetUser( id ) : unknownUser;
}