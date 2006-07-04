/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005 Iain Holmes
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
using System.Collections;
using System.Text;
using System.Xml; 

using Gtk;

namespace LastExit
{
        public class InfoWindow : Dialog {
		[Glade.Widget] VBox info_contents;
		[Glade.Widget] VBox cover_image_container;

		[Glade.Widget] Label song_label;
		[Glade.Widget] Label album_label;
		[Glade.Widget] Label artist_label;

		[Glade.Widget] VBox user_name_container;
		[Glade.Widget] VBox real_name_container;

		[Glade.Widget] Label age_location_label;
		[Glade.Widget] Label track_count_label;
		[Glade.Widget] Label registered_label;

		private MagicCoverImage cover_image;

		private UrlLabel user_name_label;
		private UrlLabel real_name_label;

	        public InfoWindow (Song song, Window w) : base ("", w, DialogFlags.DestroyWithParent) {
		 	this.HasSeparator = false;

			Glade.XML glade_xml = new Glade.XML (null, "InfoWindow.glade", "info_contents", null);
			glade_xml.Autoconnect (this);
			this.VBox.Add (info_contents);

			cover_image = new MagicCoverImage ();
			cover_image_container.Add (cover_image);
			cover_image.Visible = true;

			//			Gdk.Pixbuf cover = new Gdk.Pixbuf (null, "unknown-cover.png", 66, 66);
			//cover_image.ChangePixbuf (cover);

			user_name_label = new UrlLabel ();
			user_name_label.UrlActivated += new UrlActivatedHandler (OnUrlActivated);
			user_name_container.Add (user_name_label);
			user_name_label.SetAlignment ((float) 0.0, (float) 0.5);
			user_name_label.Visible = true;

			real_name_label = new UrlLabel ();
			real_name_label.UrlActivated += new UrlActivatedHandler (OnUrlActivated);
			real_name_container.Add (real_name_label);
			real_name_label.SetAlignment ((float) 0.0, (float) 0.5);
			real_name_label.Visible = true;

			this.AddButton ("Close", ResponseType.Close);

			SetSong (song);
		}

		public void SetSong (Song song)
		{
			this.Title = StringUtils.EscapeForPango (song.Track);
			song_label.Markup = StringUtils.EscapeForPango (song.Track);
			album_label.Markup = StringUtils.EscapeForPango (song.Album);
			artist_label.Markup = StringUtils.EscapeForPango (song.Artist);

			song.ImageLoaded += new Song.ImageLoadedHandler (OnCoverLoaded);
			song.RequestImage (Driver.CoverSize, Driver.CoverSize);
			if (song.StationFeed != null) {
				GetUserInfo (song.StationFeed);
			}
		}

		private void OnCoverLoaded (Gdk.Pixbuf pixbuf)
		{
			cover_image.ChangePixbuf (pixbuf);
		}

		private void GetUserInfo (string username)
		{
			FMRequest fmr = new FMRequest ();
			string base_url = Driver.connection.BaseUrl;
			string url = "http://" + base_url + "/1.0/user/" + username + "/profile.xml";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (GetUserCompleted);
			fmr.DoRequest (url);

			Driver.connection.DoOperationStarted ();
		}

		static void OnUrlActivated (object o, UrlActivatedArgs args)
		{
			Driver.OpenUrl (args.Url);
		}
			

		static void OnUserLoaded (User user)
		{
			user_name_label.Markup = "<b>From the profile of <a href=\"" + user.Url + "\">" + StringUtils.EscapeForPango (user.Username) + "</a></b>";

			if (User.RealName == "") {
				real_name_label.Visible = false;
			} else {
				if (User.Homepage != "") {
					real_name_label.Markup = "<a href=\"" + User.Homepage + "\">" + StringUtils.EscapeForPango (User.RealName) + "</a>";
				} else {
					real_name_label.Markup = StringUtils.EscapeForPango (User.RealName);
				}
				real_name_label.Visible = true;
			}

			if (User.age != 0 || 
			    User.Gender != "" || 
			    User.Country != "") {
				StringBuilder asl = new StringBuilder ("");
				if (age != 0) {
					asl.Append (age + " years");
				}
				
				if (User.Gender != "") {
					asl.Append (" / " + User.Gender);
				}
				
				if (User.Country != "") {
					if (User.Age != "" || 
					    User.Gender != "") {
						asl.Append ("\n");
					}
					
					asl.Append (User.Country);
				}

				age_location_label.Text = asl.ToString ();
				age_location_label.Visible = true;
			} else {
				age_location_label.Visible = false;
			}

			registered_label.Text = "Member since: " + User.Registered;
			track_count_label.Text = "Tracks played: " + User.PlayCount;
		}
	}
}
