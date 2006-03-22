// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace LastExit {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public  class IconEntry : Gtk.Entry {

		~IconEntry()
		{
			Dispose();
		}

		public IconEntry(IntPtr raw) : base(raw) {}

		[DllImport("liblastexit")]
		static extern IntPtr sexy_icon_entry_new();

		public IconEntry () : base (IntPtr.Zero)
		{
			if (GetType () != typeof (IconEntry)) {
				CreateNativeObject (new string [0], new GLib.Value[0]);
				return;
			}
			Raw = sexy_icon_entry_new();
		}

		[GLib.CDeclCallback]
		delegate void IconPressedSignalDelegate (IntPtr arg0, int arg1, int arg2, IntPtr gch);

		static void IconPressedSignalCallback (IntPtr arg0, int arg1, int arg2, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			IconPressedArgs args = new IconPressedArgs ();
			args.Args = new object[2];
			args.Args[0] = (IconEntryPosition) arg1;
			args.Args[1] = arg2;
			IconPressedHandler handler = (IconPressedHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void IconPressedVMDelegate (IntPtr entry, int IconPosition, int button);

		static IconPressedVMDelegate IconPressedVMCallback;

		static void iconpressed_cb (IntPtr entry, int IconPosition, int button)
		{
			IconEntry entry_managed = GLib.Object.GetObject (entry, false) as IconEntry;
			entry_managed.OnIconPressed ((IconEntryPosition) IconPosition, button);
		}

		private static void OverrideIconPressed (GLib.GType gtype)
		{
			if (IconPressedVMCallback == null)
				IconPressedVMCallback = new IconPressedVMDelegate (iconpressed_cb);
			OverrideVirtualMethod (gtype, "icon_pressed", IconPressedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(IconEntry), ConnectionMethod="OverrideIconPressed")]
		protected virtual void OnIconPressed (IconEntryPosition IconPosition, int button)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (3);
			GLib.Value[] vals = new GLib.Value [3];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (IconPosition);
			inst_and_params.Append (vals [1]);
			vals [2] = new GLib.Value (button);
			inst_and_params.Append (vals [2]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("icon_pressed")]
		public event IconPressedHandler IconPressed {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "icon_pressed", new IconPressedSignalDelegate(IconPressedSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "icon_pressed", new IconPressedSignalDelegate(IconPressedSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[GLib.CDeclCallback]
		delegate void IconReleasedSignalDelegate (IntPtr arg0, int arg1, int arg2, IntPtr gch);

		static void IconReleasedSignalCallback (IntPtr arg0, int arg1, int arg2, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			IconReleasedArgs args = new IconReleasedArgs ();
			args.Args = new object[2];
			args.Args[0] = (LastExit.IconEntryPosition) arg1;
			args.Args[1] = arg2;
			IconReleasedHandler handler = (IconReleasedHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void IconReleasedVMDelegate (IntPtr entry, int IconPosition, int button);

		static IconReleasedVMDelegate IconReleasedVMCallback;

		static void iconreleased_cb (IntPtr entry, int IconPosition, int button)
		{
			IconEntry entry_managed = GLib.Object.GetObject (entry, false) as IconEntry;
			entry_managed.OnIconReleased ((IconEntryPosition) IconPosition, button);
		}

		private static void OverrideIconReleased (GLib.GType gtype)
		{
			if (IconReleasedVMCallback == null)
				IconReleasedVMCallback = new IconReleasedVMDelegate (iconreleased_cb);
			OverrideVirtualMethod (gtype, "icon_released", IconReleasedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(IconEntry), ConnectionMethod="OverrideIconReleased")]
		protected virtual void OnIconReleased (IconEntryPosition IconPosition, int button)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (3);
			GLib.Value[] vals = new GLib.Value [3];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (IconPosition);
			inst_and_params.Append (vals [1]);
			vals [2] = new GLib.Value (button);
			inst_and_params.Append (vals [2]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("icon_released")]
		public event IconReleasedHandler IconReleased {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "icon_released", new IconReleasedSignalDelegate(IconReleasedSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "icon_released", new IconReleasedSignalDelegate(IconReleasedSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[DllImport("liblastexit")]
		static extern void sexy_icon_entry_add_clear_button(IntPtr raw);

		public void AddClearButton() {
			sexy_icon_entry_add_clear_button(Handle);
		}

		[DllImport("liblastexit")]
		static extern void sexy_icon_entry_set_icon(IntPtr raw, int position, IntPtr icon);

		public void SetIcon(IconEntryPosition position, Gtk.Image icon) {
			sexy_icon_entry_set_icon(Handle, (int) position, icon == null ? IntPtr.Zero : icon.Handle);
		}

		[DllImport("liblastexit")]
		static extern IntPtr sexy_icon_entry_get_icon(IntPtr raw, int position);

		public Gtk.Image GetIcon(IconEntryPosition position) {
			IntPtr raw_ret = sexy_icon_entry_get_icon(Handle, (int) position);
			Gtk.Image ret = GLib.Object.GetObject(raw_ret) as Gtk.Image;
			return ret;
		}

		[DllImport("liblastexit")]
		static extern void sexy_icon_entry_set_icon_highlight(IntPtr raw, int position, bool highlight);

		public void SetIconHighlight(IconEntryPosition position, bool highlight) {
			sexy_icon_entry_set_icon_highlight(Handle, (int) position, highlight);
		}

		[DllImport("liblastexit")]
		static extern IntPtr sexy_icon_entry_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = sexy_icon_entry_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("liblastexit")]
		static extern bool sexy_icon_entry_get_icon_highlight(IntPtr raw, int position);

		public bool GetIconHighlight(IconEntryPosition position) {
			bool raw_ret = sexy_icon_entry_get_icon_highlight(Handle, (int) position);
			bool ret = raw_ret;
			return ret;
		}
	}
}
