/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Baris Cicek <baris@teamforce.name.tr>
 *
 *  Copyright 2006 Baris Cicek
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
using System.Runtime.InteropServices;

using Gtk;

using Notifications;

using Mono.Unix;

namespace LastExit
{
	public class TrayIcon : Gtk.EventBox {
		private NotificationArea trayicon;
		private EventBox event_box;
		private Gtk.Window player_window;
		private Image trayicon_image;
		private TrackInfoPopup popup;
		private bool can_show_popup = false;
		public  bool CanShowPopup {
			set { can_show_popup = value; }
			get { return can_show_popup; }
		}		
		private bool cursor_over_trayicon = false;
		private Song current_song = null;
				private Menu menu;

				private int menu_x;
				private int menu_y;

				private static bool show_notifications = Driver.config.ShowNotifications;

		public static bool ShowNotifications {
			set { show_notifications = value; }
		}
		
				private void PositionMenu (Menu menu, out int x, out int y, out bool push_in) {
			x = menu_x;
			y = menu_y;
			
						int		   monitor = ((Widget) menu).Screen.GetMonitorAtPoint  (x, y   );
			Gdk.Rectangle rect	= ((Widget) menu).Screen.GetMonitorGeometry (monitor);
			
						int space_above = y - rect.Y;
						int space_below = rect.Y + rect.Height - y;
			
						Requisition requisition = menu.SizeRequest ();
			
						if (requisition.Height <= space_above ||
				requisition.Height <= space_below) {
				
				if (requisition.Height <= space_below)
					y += event_box.Allocation.Height;
				else
					y -= requisition.Height;
				
						} else if (requisition.Height > space_below &&
				   requisition.Height > space_above) {
				
				if (space_below >= space_above)
					y = rect.Y + rect.Height - requisition.Height;
				else
					y = rect.Y;
				
						} else {
								y = rect.Y;
						}
			
						push_in = true;
				}
		
		private void OnNotificationAreaIconClick (object o, 
							  ButtonPressEventArgs args) {
			switch (args.Event.Button) {
			case 1:
				Driver.PlayerWindow.SetWindowVisible (!Driver.PlayerWindow.WindowVisible, args.Event.Time);
				break;
			case 3:
				//icon.State = StateType.Active;
				menu_x = (int) args.Event.XRoot - (int) args.Event.X;
				menu_y = (int) args.Event.YRoot - (int) args.Event.Y;
				menu.Popup (null, null, new MenuPositionFunc (PositionMenu),
						args.Event.Button, args.Event.Time);
				break;
			}
		}
		
				private void OnMenuDeactivated (object o, EventArgs args) {
			//icon.State = StateType.Normal
		}

				public void OnGConfShowNotificationsChanged (object o, GConf.NotifyEventArgs args) {
						show_notifications = Driver.config.ShowNotifications;
				}
		
		private void PositionWidget (Widget widget, 
						 out int x, 
						 out int y, 
						 int yPadding) {
			int button_y, panel_width, panel_height;
			
			Gtk.Requisition requisition = widget.SizeRequest ();
			
			event_box.GdkWindow.GetOrigin (out x, out button_y);
			(event_box.Toplevel as Gtk.Window).GetSize(out panel_width, out panel_height);
			
			y = (button_y + panel_height + requisition.Height >= event_box.Screen.Height) 
				? button_y - requisition.Height - yPadding
				: button_y + panel_height + yPadding;
		}
		
		private void PositionPopup () {
			int x, y;
			
			Gtk.Requisition event_box_req;
			
			event_box_req = event_box.SizeRequest();
			Gtk.Requisition popup_req = popup.SizeRequest();
			
			PositionWidget(popup, out x, out y, 5);
			
			x = x - (popup_req.Width / 2) + (event_box_req.Width / 2);	 
			if (x + popup_req.Width >= event_box.Screen.Width) { 
				x = event_box.Screen.Width - popup_req.Width - 5;
			}
			
			popup.Move (x, y);
		}
		
		private void OnLeaveNotifyEvent (object o, 
						 LeaveNotifyEventArgs args) {
			cursor_over_trayicon = false;
			HidePopup ();
		}
		
		private void OnEnterNotifyEvent(object o, EnterNotifyEventArgs args) {
			cursor_over_trayicon = true;
			if (can_show_popup) {
				// only show the popup when the cursor is still over the
				// tray icon after 500ms
				GLib.Timeout.Add (500, delegate {
					if ((cursor_over_trayicon) 
						&& (can_show_popup)) {
						ShowPopup();
					}
					return false;
				});
			}
		}
		
		private void HidePopup () {
			popup.Hide ();
		}
		
		public void ShowPopup () {
			PositionPopup ();
			popup.Show ();
		}
		
		public void FillPopup (Song song) {
			current_song = song;

			if (song.Track != null) {
				popup.TrackTitle = String.Format(Catalog.GetString("<span weight=\"bold\">{0}</span>"), StringUtils.EscapeForPango (song.Track));
			} else {
				popup.TrackTitle = "";
			}
			
			if (song.Album != null && song.Artist != null) {
				popup.Artist = String.Format(Catalog.GetString("<span size=\"smaller\">By <span weight=\"bold\">{0}</span></span>"), StringUtils.EscapeForPango (song.Artist));
			} else if (song.Album == null) {
				popup.Artist = String.Format(Catalog.GetString("<span size=\"smaller\">By <span weight=\"bold\">{0}</span></span>"), StringUtils.EscapeForPango (song.Artist));
			} else if (song.Artist == null) {
				popup.Artist = "";
			} else {
				popup.Artist = "";
			}
			
			CanShowPopup = true;
		}
		
		public void UpdateCover (Gdk.Pixbuf newcover) { 
			popup.Cover = newcover;
			
			string byline;

			if (current_song.Artist != null) {
				byline = StringUtils.EscapeForPango (current_song.Track) + Catalog.GetString(" by ") + StringUtils.EscapeForPango (current_song.Artist);
			} else {
				byline = StringUtils.EscapeForPango (current_song.Track);
			}

			// Wait til cover is set before we notify :)
			Notify (Catalog.GetString("Now playing"),
				byline, newcover, event_box);
		}
		

		public TrayIcon (Gtk.Window window) {
			player_window = window;
			
			popup = new TrackInfoPopup ();
			trayicon = new NotificationArea ("last-exit");
			
			event_box = new EventBox ();
			trayicon_image = new Image ();
			trayicon_image.FromPixbuf = new Gdk.Pixbuf (null, "last-exit-16.png");
			event_box.Add (trayicon_image);
			trayicon_image.Visible = true;
			trayicon.Add(event_box);

			Driver.Actions.UIManager.AddUiFromResource ("TrayIcon.xml");
			menu = (Menu) Driver.Actions.UIManager.GetWidget ("/Menu");
			menu.Deactivated += new EventHandler (OnMenuDeactivated);

						// Watch the GConf show_notifications key and fire an event
						string NotifyKey = "/apps/lastexit/show_notifications";
						Driver.config.GConfAddNotify (NotifyKey, OnGConfShowNotificationsChanged);
			
						trayicon.ShowAll();
			
			// Setting callback procs 
			event_box.ButtonPressEvent += OnNotificationAreaIconClick;
			event_box.EnterNotifyEvent += OnEnterNotifyEvent;
			event_box.LeaveNotifyEvent += OnLeaveNotifyEvent;
		}
		
		private static void Notify (string summary, string message,
						Gdk. Pixbuf image, Widget widget) {
			if (show_notifications == false) {
				return;
			}
			
			try {
				Notification nf = new Notification(summary, message);
				nf.AttachToWidget(widget);
				nf.Timeout = 4000;
				nf.Urgency = Urgency.Low;
				if (image != null) {
					image = image.ScaleSimple (42, 42, Gdk.InterpType.Bilinear);
					nf.Icon = image;
				}

				nf.Show();				
			} catch (Exception) {
				show_notifications = false;
			}
		}
		
	}
	
	public class TrackInfoPopup : Gtk.Window {
		private string artist;
		private string title;
		
		private Label artist_album_label;
		private Label title_label;
		private VBox box;
		private HBox containerbox;
		private MagicCoverImage cover_image;
		
		public TrackInfoPopup () : base (Gtk.WindowType.Popup) {
			BorderWidth = 8;
			AppPaintable = true;
			/* 			Resizable = true; */
			
			containerbox = new HBox ();
			containerbox.Spacing = 3;
			cover_image = new MagicCoverImage ();
			containerbox.PackStart (cover_image, false, false, 0);
			cover_image.Visible = true;
			
			Alignment al = new Alignment ((float) 0.0, 
							  (float) 0.5, 
							  (float) 1.0, 
							  (float) 0.0);
			containerbox.Add (al);
			
			box = new VBox ();
			box.Spacing = 2;
			
			artist_album_label = new Label (artist);
			artist_album_label.Show ();
 			artist_album_label.Ellipsize = Pango.EllipsizeMode.End; 
			artist_album_label.Xalign = 0.0f;
			artist_album_label.Yalign = 0.5f;
			artist_album_label.Selectable = false;
			
			title_label = new Label (title);
			title_label.Show ();
/* 			title_label.Ellipsize = Pango.EllipsizeMode.End; */
			title_label.Xalign = 0.0f;
			title_label.Yalign = 0.5f;
			title_label.Selectable = false;
			
			box.PackStart (title_label, false, false, 0);
			box.PackStart (artist_album_label, false, false, 0);
			al.Add (box);
			box.ShowAll ();
			Add (containerbox);
			containerbox.ShowAll ();
			//Name = "gtk-tooltips";
		}
		
		protected override bool OnExposeEvent(Gdk.EventExpose evnt) {
			GdkWindow.DrawRectangle (Style.ForegroundGC (StateType.Normal), 
						 true, Allocation);
			GdkWindow.DrawRectangle (Style.BackgroundGC (StateType.Normal), 
						 true, 
						 Allocation.X + 1, 
						 Allocation.Y + 1, 
						 Allocation.Width - 2, 
						 Allocation.Height - 2);
			base.OnExposeEvent (evnt);
			return true;
		}
		
		private void UpdateSize () {
			QueueResize ();
		}
		
		
		public string Artist {
			set {
				artist = value;
				artist_album_label.Markup = value;
				UpdateSize();
			}
			get { return artist; }
		}
		
		public string TrackTitle {
			set {
				title_label.Markup = value;
				UpdateSize();
			}
		}
		
		public Gdk.Pixbuf Cover {
			set { cover_image.ChangePixbuf(value); }
		}
	}
}
