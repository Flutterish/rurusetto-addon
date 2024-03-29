﻿namespace osu.Game.Rulesets.RurusettoAddon.UI.Users;

public class UserTab : OverlayTab {
	public readonly APIUser User;
	public UserTab ( APIUser user ) {
		User = user;
	}

	protected override void LoadContent () {
		Add( new DrawableRurusettoUser( User, false ) { Height = 80 } );

		User.RequestDetail( profile => {
			OnContentLoaded();
		}, failure: e => {
			API.LogFailure( $"Could not retrieve profile for {User}", e );
			OnContentLoaded();
		} );
	}
}