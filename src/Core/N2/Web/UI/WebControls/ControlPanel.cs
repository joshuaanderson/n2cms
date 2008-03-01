#region License

/* Copyright (C) 2007 Cristian Libardo
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 */

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2.Resources;

namespace N2.Web.UI.WebControls
{
	/// <summary>
	/// A control panel for on page editing. The control displays buttons for 
	/// start editing and saving changes. This control is in the early stages 
	/// and changes made through this are not version managed.
	/// </summary>
	public class ControlPanel : Control, IItemContainer
	{
		#region Fields

		private readonly HyperLink hlQuickEdit = new HyperLink();
		private readonly HyperLink hlEditMode = new HyperLink();
		private readonly HyperLink hlNew = new HyperLink();
		private readonly HyperLink hlEdit = new HyperLink();
		private readonly HyperLink hlDelete = new HyperLink();
		private readonly LinkButton btnSave = new LinkButton();
		private readonly LinkButton btnCancel = new LinkButton();

		#endregion

		#region Constructor

		public ControlPanel()
		{
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets the url to a style sheet added to the page when editing.</summary>
		public string StyleSheetUrl
		{
			get { return (string)(ViewState["StyleSheetUrl"] ?? "~/edit/Css/edit.css"); }
			set { ViewState["StyleSheetUrl"] = value; }
		}

		/// <summary>Gets or sets the current versioning mode.</summary>
		public ItemEditorVersioningMode VersioningMode
		{
			get { return (ItemEditorVersioningMode) (ViewState["VersioningMode"] ?? ItemEditorVersioningMode.VersionAndSave); }
			set { ViewState["VersioningMode"] = value; }
		}

		/// <summary>Gets or sets the quick edit link text.</summary>
		public string QuickEditText
		{
			get { return (string) (ViewState["QuickEditText"] ?? "Edit here"); }
			set { ViewState["QuickEditText"] = value; }
		}

		/// <summary>Gets or sets the the text on the new button.</summary>
		public string NewText
		{
			get { return (string)(ViewState["NewText"] ?? "Create new"); }
			set { ViewState["NewText"] = value; }
		}

		/// <summary>Gets or sets the the text on the edit page button.</summary>
		public string EditText
		{
			get { return (string)(ViewState["EditText"] ?? "Edit page"); }
			set { ViewState["EditText"] = value; }
		}

		/// <summary>Gets or sets the text on the delete button.</summary>
		public string DeleteText
		{
			get { return (string)(ViewState["DeleteText"] ?? "Delete page"); }
			set { ViewState["DeleteText"] = value; }
		}

		/// <summary>Gets or sets the edit mode link text.</summary>
		public string EditModeText
		{
			get { return (string) (ViewState["EditModeText"] ?? "To edit mode"); }
			set { ViewState["EditModeText"] = value; }
		}

		/// <summary>Gets or sets the save button text.</summary>
		public string SaveText
		{
			get { return (string) (ViewState["SaveText"] ?? "Save"); }
			set { ViewState["SaveText"] = value; }
		}

		/// <summary>Gets or sets the cancel button text.</summary>
		public string CancelText
		{
			get { return (string) (ViewState["CancelText"] ?? "Cancel"); }
			set { ViewState["CancelText"] = value; }
		}

		[NotifyParentProperty(true)]
		public HyperLink QuickEditLink
		{
			get
			{
				EnsureChildControls();
				return hlQuickEdit;
			}
		}

		[NotifyParentProperty(true)]
		public HyperLink EditModeLink
		{
			get
			{
				EnsureChildControls();
				return hlEditMode;
			}
		}

		[NotifyParentProperty(true)]
		public HyperLink NewLink
		{
			get
			{
				EnsureChildControls();
				return hlNew;
			}
		}

		[NotifyParentProperty(true)]
		public HyperLink EditLink
		{
			get
			{
				EnsureChildControls();
				return hlEdit;
			}
		}

		[NotifyParentProperty(true)]
		public HyperLink DeleteLink
		{
			get
			{
				EnsureChildControls();
				return hlDelete;
			}
		}

		[NotifyParentProperty(true)]
		public LinkButton SaveButton
		{
			get{return btnSave;}
		}

		[NotifyParentProperty(true)]
		public LinkButton CancelButton
		{
			get{return btnCancel;}
		}

		public virtual ContentItem CurrentItem
		{
			get { return Find.CurrentPage; }
		}

		#endregion

		#region Page Methods

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (GetState() != ControlPanelState.Hidden)
				AddControlPanelControls();
		}

		protected virtual void AddControlPanelControls()
		{
			if (GetState() == ControlPanelState.Visible)
			{
				AddEditButtons();
			}
			else if (GetState() == ControlPanelState.Editing)
			{
				AddSaveCancelButtons();
			}
		}

		protected virtual void AddSaveCancelButtons()
		{
			AddSaveButton();
			AddCancelButton();

			Register.JQuery(Page);
			Register.StyleSheet(Page, Utility.ToAbsolute(StyleSheetUrl), Media.All);
		}

		protected virtual void AddEditButtons()
		{
			AddQuickEditButton();
			AddEditModeButton();
			AddCreateNewButton();
			AddEditButton();
			AddDeleteButton();
		}

		protected virtual void AddDeleteButton()
		{
			hlDelete.NavigateUrl = N2.Context.Current.EditManager.GetDeleteUrl(CurrentItem);

			hlDelete.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/delete.gif"), DeleteText);
			hlDelete.CssClass = "delete";
			Controls.Add(hlDelete);
		}

		protected virtual void AddCreateNewButton()
		{
			hlNew.NavigateUrl = N2.Context.Current.EditManager.GetSelectNewItemUrl(CurrentItem);
			hlNew.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/add.gif"), NewText);
			hlNew.CssClass = "new";
			Controls.Add(hlNew);
		}

		protected virtual void AddEditButton()
		{
			hlEdit.NavigateUrl = N2.Context.Current.EditManager.GetEditExistingItemUrl(CurrentItem);
			hlEdit.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/page_edit.gif"), EditText);
			hlEdit.CssClass = "edit";
			Controls.Add(hlEdit);
		}

		protected virtual void AddCancelButton()
		{
			btnCancel.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/cancel.gif"), CancelText);
			btnCancel.CssClass = "cancel";
			Controls.Add(btnCancel);
			btnCancel.CausesValidation = false;
			btnCancel.Command += btnCancel_Command;
		}

		protected virtual void AddSaveButton()
		{
			btnSave.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/disk.gif"), SaveText);
			btnSave.CssClass = "save";
			Controls.Add(btnSave);
			btnSave.Command += btnSave_Command;
		}

		protected virtual void AddEditModeButton()
		{
			hlEditMode.NavigateUrl = N2.Context.Current.EditManager.GetEditModeUrl(CurrentItem);
			hlEditMode.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/sitemap_color.gif"), EditModeText);
			hlEditMode.Target = "_top";
			hlEditMode.CssClass = "editMode";
			Controls.Add(hlEditMode);
		}

		protected virtual void AddQuickEditButton()
		{
			hlQuickEdit.NavigateUrl = GetQuickEditUrl("true");
			hlQuickEdit.Text = FormatImageAndText(Utility.ToAbsolute("~/edit/img/ico/png/script_edit.png"), QuickEditText);
			hlQuickEdit.CssClass = "quickEdit";
			Controls.Add(hlQuickEdit);
		}

		private void btnSave_Command(object sender, CommandEventArgs e)
		{
			Save();
		}

		private void btnCancel_Command(object sender, CommandEventArgs e)
		{
			Page.Response.Redirect(Find.CurrentPage.Url);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write("<div class='controlPanel'>");
			base.Render(writer);
			writer.Write("</div>");
		}

		#endregion

		#region Methods

		/// <summary>Gets the url for editing the page directly.</summary>
		public virtual string GetQuickEditUrl(string editParameter)
		{
			string url = Request.RawUrl;
			url += (url.IndexOf('?') >= 0) ? "&" : "?";
			url += "edit=" + editParameter;
			return url;
		}

		/// <summary>Saves edited fields.</summary>
		public virtual void Save()
		{
			IList<IItemEditor> itemEditors = GetEditedItems();

			foreach (IItemEditor itemEditor in itemEditors)
			{
				N2.Context.Current.EditManager.Save(itemEditor, Page.User);
			}

			Page.Response.Redirect(Find.CurrentPage.Url);
		}

		protected virtual IList<IItemEditor> GetEditedItems()
		{
			Dictionary<ContentItem, IDictionary<string, Control>> itemsEditors =
				new Dictionary<ContentItem, IDictionary<string, Control>>();

			IEnumerable<IEditableEditor> editors = ItemUtility.FindInChildren<IEditableEditor>(Page);
			foreach (EditableDisplay ed in editors)
			{
				if (!itemsEditors.ContainsKey(ed.CurrentItem))
				{
					itemsEditors[ed.CurrentItem] = new Dictionary<string, Control>();
				}
				itemsEditors[ed.CurrentItem][ed.PropertyName] = ed.Editor;
			}

			IList<IItemEditor> items = new List<IItemEditor>();
			foreach (ContentItem item in itemsEditors.Keys)
			{
				items.Add(new OnPageItemEditor(VersioningMode, item.ZoneName, itemsEditors[item], item));
			}
			return items;
		}

		#endregion

		#region class OnPageItemEditor

		private class OnPageItemEditor : IItemEditor
		{
			public OnPageItemEditor(ItemEditorVersioningMode versioningMode, string zoneName,
			                        IDictionary<string, Control> addedEditors, ContentItem currentItem)
			{
				this.versioningMode = versioningMode;
				this.zoneName = zoneName;
				this.addedEditors = addedEditors;
				this.currentItem = currentItem;
			}

			#region IItemEditor Members

			private ItemEditorVersioningMode versioningMode = ItemEditorVersioningMode.VersionAndSave;
			private string zoneName = string.Empty;
			private readonly IDictionary<string, Control> addedEditors = new Dictionary<string, Control>();
			private ContentItem currentItem;

			public ItemEditorVersioningMode VersioningMode
			{
				get { return versioningMode; }
				set { versioningMode = value; }
			}

			public string ZoneName
			{
				get { return zoneName; }
				set { zoneName = value; }
			}

			public IDictionary<string, Control> AddedEditors
			{
				get { return addedEditors; }
			}

			#endregion

			#region IItemContainer Members

			public ContentItem CurrentItem
			{
				get { return currentItem; }
				set { currentItem = value; }
			}

			#endregion


			#region IItemEditor Members


			public event EventHandler<N2.Persistence.ItemEventArgs> Saved;

			#endregion
		}

		#endregion

		#region Static Methods

		protected static HttpRequest Request
		{
			get { return HttpContext.Current.Request; }
		}
		protected static IPrincipal User
		{
			get { return HttpContext.Current.User; }
		}

		public static ControlPanelState GetState()
		{
			if (N2.Context.SecurityManager.IsEditor(User))
			{
				if (Request["edit"] == "true")
					return ControlPanelState.Editing;
				else if (Request["edit"] == "drag")
					return ControlPanelState.DragDrop;
				else
					return ControlPanelState.Visible;
			}
			return ControlPanelState.Hidden;
		}

		protected string FormatImageAndText(string iconUrl, string text)
		{
			return string.Format("<img src='{0}' alt='icon'/>{1}", iconUrl, text);
		}

		#endregion
	}
}