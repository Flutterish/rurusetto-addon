using osu.Framework;
using osu.Framework.Platform;
using osu.Game.Tests;

using DesktopGameHost host = Host.GetSuitableDesktopHost( @"osu", new() { BindIPC = true } );
host.Run( new OsuTestBrowser() );