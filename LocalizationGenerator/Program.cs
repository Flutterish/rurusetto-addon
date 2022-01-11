using Humanizer;
using ICSharpCode.Decompiler.Util;
using Newtonsoft.Json;
using osu.Game.Rulesets.RurusettoAddon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocalisationGenerator {
	class Program {
		static void Main () {
			// TODO split this into several files (currently its 'Strings', but we might want 'Tags', 'Defaults' etc.)

			Console.WriteLine( $"Loading {yellow("'meta.jsonc'")}..." );
			var errors = new List<string>();

			Meta meta;
			try {
				meta = JsonConvert.DeserializeObject<Meta>( File.ReadAllText( "./Source/meta.jsonc" ) );
				Console.WriteLine( $"Default locale: {meta.Default}" );
			}
			catch ( Exception e ) {
				fail( $"Could not load {yellow("'meta.jsonc'")} - {e.Message}" );
				return;
			}

			Dictionary<string, string> localeFileNames = new();
			Dictionary<string, List<string>> localesWithKeys = new();
			Dictionary<string, Dictionary<string, string>> locales = new();
			foreach ( var file in Directory.EnumerateFiles( "./Source/" ) ) {
				var filenameWithoutExtension = Path.GetFileNameWithoutExtension( file );
				if ( filenameWithoutExtension == "meta" ) {
					continue;
				}

				var filename = Path.GetFileName( file );
				Console.WriteLine( "---------------" );
				Console.WriteLine( $"Loading {yellow($"'{filename}'")}..." );
				Dictionary<string, string> contents;
				try {
					contents = JsonConvert.DeserializeObject<Dictionary<string, string>>( File.ReadAllText( file ) );
				}
				catch ( Exception e ) {
					logError( $"Could not load {yellow($"'{filename}'")} - {e.Message}" );
					continue;
				}

				if ( !contents.TryGetValue( "locale", out var locale ) ) {
					logError( $"'{filename}' does not contain a {yellow("'locale'")} key" );
					continue;
				}
				contents.Remove( "locale" );

				Console.WriteLine( $"Locale: {locale}" );
				if ( locale != filenameWithoutExtension ) {
					logError( $"File '{filename}' does not match its defined locale {yellow($"'{locale}'")}" );
				}

				if ( locales.ContainsKey( locale ) ) {
					logError( $"File {yellow($"'{filename}'")} defines its locale as {yellow($"'{locale}'")}, but file {yellow($"'{localeFileNames[locale]}'")} already defined it" );
					continue;
				}
				localeFileNames.Add( locale, filename );
				locales.Add( locale, contents );

				foreach ( var (key, value) in contents ) {
					if ( !localesWithKeys.TryGetValue( key, out var list ) ) {
						localesWithKeys.Add( key, list = new() );
					}

					list.Add( locale );
				}

				// TODO check argcount
			}

			Console.WriteLine( "---------------" );
			Console.WriteLine( "Checking keys..." );

			foreach ( var (key, localesWithKey) in localesWithKeys ) {
				if ( localesWithKey.Count != locales.Count ) {
					var defined = localesWithKey.Select( x => $"{localeFileNames[x]} ({x})" );
					var undefined = locales.Keys.Except( localesWithKey ).Select( x => $"{localeFileNames[x]} ({x})" );
					logError( $"The key {yellow($"'{key}'")} is defined in [{string.Join(", ", defined)}] but not in [{string.Join(", ", undefined)}]" );
				}
			}

			if ( !locales.ContainsKey( meta.Default ) ) {
				fail( $"The default locale ({meta.Default}) was not found" );
				return;
			}

			Console.WriteLine( "---------------" );

			string generateLocalisationClass () {
				var ns = $"{typeof( RurusettoAddonRuleset ).Namespace}.Localisation";
				StringBuilder sb = new();
				sb.AppendLine( $"using osu.Framework.Localisation;" );
				sb.AppendLine();
				sb.AppendLine( $"namespace {ns} {{" );
				sb.AppendLine( $"	public static class Strings {{" );
				sb.AppendLine( $"		private const string prefix = \"{ns}.Strings\";" );
				foreach ( var (key, value) in locales[meta.Default] ) {
					sb.AppendLine( $"		" );
					if ( meta.Args.TryGetValue( key, out var args ) ) {
						sb.AppendLine( $"		/// <summary>" );
						sb.AppendLine( $"		/// {value.Replace("\n", "\n		/// ")}" );
						sb.AppendLine( $"		/// </summary>" );
						sb.AppendLine( $"		public static LocalisableString {key.Replace( '-', '_' ).Pascalize()} ( {string.Join(", ", args.Select( x => $"LocalisableString {x}" ))} )" );
						sb.AppendLine( $"			=> new TranslatableString( getKey( {JsonConvert.SerializeObject(key)} ), {JsonConvert.SerializeObject(value)}, {string.Join(", ", args)} );" );
					}
					else {
						sb.AppendLine( $"		/// <summary>" );
						sb.AppendLine( $"		/// {value.Replace( "\n", "\n		/// " )}" );
						sb.AppendLine( $"		/// </summary>" );
						sb.AppendLine( $"		public static LocalisableString {key.Replace( '-', '_' ).Pascalize()} => new TranslatableString( getKey( {JsonConvert.SerializeObject(key)} ), {JsonConvert.SerializeObject(value)} );" );
					}
				}
				sb.AppendLine( $"		" );
				sb.AppendLine( $"		private static string getKey ( string key ) => $\"{{prefix}}:{{key}}\";" );
				sb.AppendLine( $"	}}" );
				sb.AppendLine( $"}}" );

				return sb.ToString();
			}

			var l12nDir = "./../../../../osu.Game.Rulesets.RurusettoAddon/Localisation/";
			Console.WriteLine( $"Generating files in {Path.GetFullPath(l12nDir)}\nContinue? [Y]/N" );
			if ( Console.ReadLine() is "n" or "N" ) {
				fail( "Cancelled." );
				return;
			}

			foreach ( var i in Directory.GetFiles( l12nDir ) ) {
				File.Delete( i );
			}

			makeResxFile( $"Strings.resx", meta.Default );
			foreach ( var (name, values) in locales ) {
				if ( name == meta.Default )
					continue;

				makeResxFile( $"Strings.{name}.resx", name );
			}

			void makeResxFile ( string filename, string locale ) {
				Console.WriteLine( $"Generating {yellow( $"'{filename}'" )} ({locale})..." );
				using var writer = new ResXResourceWriter( Path.Combine( l12nDir, filename ) );

				foreach ( var (key, value) in locales[locale] ) {
					writer.AddResource( key, value );
				}

				writer.Generate();
			}

			File.WriteAllText( Path.Combine( l12nDir, "Strings.cs" ), generateLocalisationClass() );

			logErrors();
			Console.WriteLine( "---------------" );
			Console.WriteLine( green("[Done]") );

			void logError ( string msg ) {
				Console.WriteLine( red($"[! Error]: {msg}") );
				errors.Add( msg );
			}

			void logErrors () {
				if ( errors.Any() ) {
					Console.WriteLine( "---------------" );
					foreach ( var i in errors ) {
						Console.WriteLine( red($"[! Error]: {i}") );
					}
				}
			}

			void fail ( string msg ) {
				logErrors();
				Console.WriteLine( "---------------" );
				Console.WriteLine( red($"[! Failure]: Could not finish - {msg}") );
			}

			string red ( string msg ) {
				return $"\u001B[31m{msg}\u001B[0m";
			}

			string green ( string msg ) {
				return $"\u001B[32m{msg}\u001B[0m";
			}

			string yellow ( string msg ) {
				return $"\u001B[33m{msg}\u001B[0m";
			}
		}
	}

	class Meta {
		[JsonProperty( "default" )]
		public string Default;
		[JsonProperty( "comments" )]
		public Dictionary<string, string> Comments;
		[JsonProperty( "args" )]
		public Dictionary<string, string[]> Args;
	}
}
