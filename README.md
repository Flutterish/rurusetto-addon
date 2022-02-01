# Rūruestto Addon
An addon for osu!lazer which allows you to browse the [rūrusetto](https://rulesets.info) wiki in-game and manage your rulesets.

## Special thanks
To [Nao](https://github.com/naoei) for graphical design for the addon and [Yulianna](https://github.com/HelloYeew) for making Rurūsetto.
To [Loreos](https://github.com/Loreos7) for the russian locale.

## Installing
* Open osu!lazer
* Go to settings
* Click `Open osu! foler` at the bottom of `General` settings
* Open the `rulesets` folder
* Copy the path
* Go to [Releases](/releases) of this repository
* Click the topmost one
* Download the `osu.Game.Rulesets.RurusettoAddon.dll` file. Save to the copied path.
* Restart osu!
* Done! Click the ![overlay button](./overlayButton.png) button to browse the wiki

You can also install and update locale (translation) files by following the same steps for the `.zip` files included with each release.

https://user-images.githubusercontent.com/40297338/149041138-003f4a3e-3144-4139-8558-34d13da8d40f.mp4

## Contributing
If you can code, you can contribute the standard [github way](https://github.com/firstcontributions/first-contributions):
* Fork this repository
* Apply code changes
* Open a PR (Pull Request) to submit your changes for review

If you can't code, you can still contribute localisation (translation files). You still need to follow the steps outlined in [here](https://github.com/firstcontributions/first-contributions):
* Fork this repository
* Open the [Localisation Generator Source folder](./LocalizationGenerator/Source)
  * The files in this folder are named after the locale it uses. For example english uses `en.jsonc`. For a list of ISO language codes, see [this website](http://www.lingoes.net/en/translator/langcode.htm) or if you can't find your locale there, wikipedia has a list (although it it a bit harder to read than this site)
  * If your locale is not there, create a file following the naming convention outlined above. Initialize the file with `{ "locale": "ISO language code" }`
  * Open the file. It can be opened in a regular text editor, although we recommend [VS Code](https://code.visualstudio.com)
  * You probably also want to open `en.jsonc` or any other translation file as a reference
* If you can build, you can run `LocalisationGenerator.csproj`
  * The program will perform some checks to see what resources are missing across all localisation files
  * You can exit the program. You don't need to confirm to generate files just yet
* Add translations or typecheck your file
  * Since the format is `.jsonc` and not `.json`, you can add comments to the file if you want by typing `// this is my comment!` anywhere. Even though github credits commit authors, feel free to credit yourself at the top of the file with a triple slash comment
  * Some strings contain a `{0}` or `{1}` etc. These are going to be replaced by something such as a number, a link or an error code. All entries which contain them must be commented by explaining what they are
* If you can build, run `LocalisationGenerator.csproj`. This time confirm and generate resource files. If you do not know how to build, a collaborator will do this step for you when you submit your PR
* Open a PR (Pull Request) to submit your changes for review

You can also contribute to the [rūrusetto wiki](https://rulesets.info) or any of the rulesets you discover! Make sure to show some ❤️ to the awesome people who develop and maintain them.
