/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/* vim:tabstop=8:noexpandtab:shiftwidth=8:
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005, 2006 Iain Holmes
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of version 2 of the GNU General Public License as 
 *  published by the Free Software Foundation.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Street #330, Boston, MA 02111-1307, USA.
 *
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Gtk;

namespace LastExit
{
	public sealed class Driver
	{
		// The size of the cover images.
		public static int CoverSize = 66;

		private static Actions actions;
		private static PlayerWindow player_window;

		public static FMConnection connection;
		public static Player player;

		private static string config_directory;
		public static string ConfigDirectory {
			get { return config_directory; }
		}

		public static void DBusMessage (string message, string content)
		{
			switch (message) {
				case "change_station":
					connection.ChangeStation(content);
					player_window.Present();
					break;
				case "focus_instance":
					// if player window is hidden don't show it
					if (player_window.Visible == true) { 
						player_window.Present();
					}
					break;
			}
		}

		public static Config config;

		public static void Main (string[] args) {
			DBus.dbus_g_thread_init ();
			string username;
			string password;

			Driver.SetProcessName ("last-exit");
			Application.Init("last-exit", ref args);


					
			switch (DBus.CheckInstance ()) {
				case DBus.DBusState.Error:
					Console.WriteLine ("Error contacting other instance.");
					break;

				case DBus.DBusState.AlreadyRunning:
					if (args.Length > 0) {
						DBus.ChangeStation (args[0]);
					} else {
						DBus.FocusInstance ();
					}
					return;
				
				case DBus.DBusState.NotRunning:
					DBus.Init (DBusMessage);
					break;
			}
					

			StockIcons.Initialize ();

			SetDefaultWindowIcon ();

			config = new Config ();

			if (config.FirstRun) {
				FirstRunDialog frd = new FirstRunDialog ();
				
				int response = frd.Run ();

				frd.Visible = false;

				switch (response) {
					case (int) ResponseType.Reject:
						Gtk.Application.Quit();
						Environment.Exit (0);
						break;

					case (int) ResponseType.Ok:
						config.Username = frd.Username;
						config.Password = frd.Password;
						break;
					
					default:
						break;
				}

				frd.Destroy ();
				config.FirstRun = false;
			}

			Driver.SetUpConfigDirectory ();

			//TrayIcon.ShowNotifications = config.ShowNotifications;
		
			username = config.Username;
			password = config.Password;

			connection = new FMConnection (username, password);
			connection.Handshake ();

			player = new Player ();

			actions = new Actions ();
			player_window = new PlayerWindow ();

			if (args.Length > 0) {
				player_window.InitialStation = args[0];
			}

			player_window.Run ();

			Application.Run ();
		}

		public static void Exit () {
			Environment.Exit (0);
		}

		// FIXME: Needs to handle no gnome-open.
		public static void OpenUrl (string url)
		{
			string [] argv = new string [] { "gnome-open", url };
			InternalProcess.Spawn (argv);
		}

		private static void SetDefaultWindowIcon ()
		{
			Gdk.Pixbuf [] default_icon_list = { new Gdk.Pixbuf (null, "last-exit-16.png") };
			Gtk.Window.DefaultIconList = default_icon_list;
		}

		[DllImport ("libc")]
		private static extern int prctl (int option,  
				 byte[] arg2,
				 ulong arg3,
				 ulong arg4,
				 ulong arg5);
	
		private static void SetProcessName (string name)
		{
			if (prctl (15, Encoding.ASCII.GetBytes (name + "\0"), 0, 0, 0) != 0) {
				throw new ApplicationException ("Error setting process name: " + Mono.Unix.Native.Stdlib.GetLastError ());
			}
		}

		private static void SetUpConfigDirectory ()
		{
			config_directory = Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "last-exit");
			DirectoryInfo dinfo = new DirectoryInfo (config_directory);
			if (dinfo.Exists) {
				return;
			}
			
			dinfo.Create ();
		}

		public static Actions Actions {
			get { return actions; }
		}

		public static PlayerWindow PlayerWindow {
			get { return player_window; }
		}
	}
}
