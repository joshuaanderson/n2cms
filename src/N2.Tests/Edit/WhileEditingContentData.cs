﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using N2.Tests.Edit.Items;
using System.Web.UI.WebControls;
using System.Web.UI;
using N2.Web.UI.WebControls;
using Rhino.Mocks;
using System.Security.Principal;
using NUnit.Framework.SyntaxHelpers;

namespace N2.Tests.Edit
{
    [TestFixture]
    public class WhileEditingContentData : EditManagerTests
    {
        [Test]
        public void CanAddEditors()
        {
            Type itemType = typeof(ComplexContainersItem);
            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(itemType, editorContainer, null);
            Assert.AreEqual(5, added.Count);
            TypeAssert.Equals<TextBox>(added["MyProperty0"]);
            TypeAssert.Equals<TextBox>(added["MyProperty1"]);
            TypeAssert.Equals<TextBox>(added["MyProperty2"]);
            TypeAssert.Equals<FreeTextArea>(added["MyProperty3"]);
            TypeAssert.Equals<CheckBox>(added["MyProperty4"]);

            WebControlAssert.Contains(typeof(FieldSet), editorContainer);
            WebControlAssert.Contains(typeof(TextBox), editorContainer);
            WebControlAssert.Contains(typeof(FreeTextArea), editorContainer);
            WebControlAssert.Contains(typeof(CheckBox), editorContainer);
        }

        [Test]
        public void CanUpdateEditors()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            IDictionary<string, Control> added = AddEditors(item);

            TextBox tbp0 = added["MyProperty0"] as TextBox;
            TextBox tbp1 = added["MyProperty1"] as TextBox;
            TextBox tbp2 = added["MyProperty2"] as TextBox;
            FreeTextArea ftap3 = added["MyProperty3"] as FreeTextArea;
            CheckBox cbp4 = added["MyProperty4"] as CheckBox;

            Assert.IsEmpty(tbp0.Text);
            Assert.IsEmpty(tbp1.Text);
            Assert.IsEmpty(tbp2.Text);
            Assert.IsEmpty(ftap3.Text);
            Assert.IsFalse(cbp4.Checked);

            item.MyProperty0 = "one";
            item.MyProperty1 = "two";
            item.MyProperty2 = "three";
            item.MyProperty3 = "rock";
            item.MyProperty4 = true;

            editManager.UpdateEditors(item, added, null);

            Assert.AreEqual("one", tbp0.Text);
            Assert.AreEqual("two", tbp1.Text);
            Assert.AreEqual("three", tbp2.Text);
            Assert.AreEqual("rock", ftap3.Text);
            Assert.IsTrue(cbp4.Checked);
        }

        [Test]
        public void CanUpdateItem()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            IDictionary<string, Control> added = AddEditors(item);

            TextBox tbp0 = added["MyProperty0"] as TextBox;
            TextBox tbp1 = added["MyProperty1"] as TextBox;
            TextBox tbp2 = added["MyProperty2"] as TextBox;
            FreeTextArea ftap3 = added["MyProperty3"] as FreeTextArea;
            CheckBox cbp4 = added["MyProperty4"] as CheckBox;

            Assert.IsEmpty(item.MyProperty0);
            Assert.IsEmpty(item.MyProperty1);
            Assert.IsEmpty(item.MyProperty2);
            Assert.IsEmpty(item.MyProperty3);
            Assert.IsFalse(item.MyProperty4);

            tbp0.Text = "one";
            tbp1.Text = "two";
            tbp2.Text = "three";
            ftap3.Text = "rock";
            cbp4.Checked = true;

            editManager.UpdateItem(item, added, null);

            Assert.AreEqual("one", item.MyProperty0);
            Assert.AreEqual("two", item.MyProperty1);
            Assert.AreEqual("three", item.MyProperty2);
            Assert.AreEqual("rock", item.MyProperty3);
            Assert.IsTrue(item.MyProperty4);
        }

        [Test]
        public void UpdateItem_WithChanges_ReturnsTrue()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            IDictionary<string, Control> added = AddEditors(item);

            TextBox tbp0 = added["MyProperty0"] as TextBox;
            TextBox tbp1 = added["MyProperty1"] as TextBox;
            TextBox tbp2 = added["MyProperty2"] as TextBox;
            FreeTextArea ftap3 = added["MyProperty3"] as FreeTextArea;
            CheckBox cbp4 = added["MyProperty4"] as CheckBox;

            tbp0.Text = "one";
            tbp1.Text = "two";
            tbp2.Text = "three";
            ftap3.Text = "rock";
            cbp4.Checked = true;

            bool result = editManager.UpdateItem(item, added, null);

            Assert.IsTrue(result, "UpdateItem didn't return true even though the editors were changed.");
        }

        [Test]
        public void UpdateItem_WithNoChanges_ReturnsFalse()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            Type itemType = item.GetType();
            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(itemType, editorContainer, null);

            item.MyProperty0 = "one";
            item.MyProperty1 = "two";
            item.MyProperty2 = "three";
            item.MyProperty3 = "rock";
            item.MyProperty4 = true;
            editManager.UpdateEditors(item, added, null);

            bool result = editManager.UpdateItem(item, added, null);

            Assert.IsFalse(result, "UpdateItem didn't return false even though the editors were unchanged.");
        }

        [Test]
        public void CanSaveItem()
        {
            ComplexContainersItem item = new ComplexContainersItem();

            persister.Save(item);
            mocks.Replay(persister);

            IItemEditor editor = SimulateEditor(item, ItemEditorVersioningMode.SaveOnly);
            DoTheSaving(null, editor);

            AssertItemHasValuesFromEditors(item);
        }

        [Test]
        public void NewItem_WithNoChanges_IsSaved()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            item.ID = 0;
            item.MyProperty0 = "one";
            item.MyProperty1 = "two";
            item.MyProperty2 = "three";
            item.MyProperty3 = "rock";
            item.MyProperty4 = true;

            Expect.On(versioner).Call(versioner.SaveVersion(null)).Repeat.Never();
            mocks.Replay(versioner);
            persister.Save(item);
            LastCall.Repeat.Once();
            mocks.Replay(persister);

            IItemEditor editor = SimulateEditor(item, ItemEditorVersioningMode.VersionAndSave);

            DoTheSaving(null, editor);
        }

        [Test]
        public void SavingWithLimitedPrincipal_DoesntChange_SecuredProperties()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            item.ID = 0;
            item.MyProperty1 = "I'm available";
            item.MyProperty5 = true;
            item.MyProperty6 = "I'm secure!";

            persister.Save(item);
            mocks.Replay(persister);

            IPrincipal user = CreateUser("Joe");

            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(typeof(ComplexContainersItem), editorContainer, null);
            Assert.AreEqual(5, added.Count);

            IItemEditor editor = mocks.StrictMock<IItemEditor>();
            using (mocks.Record())
            {
                Expect.Call(editor.CurrentItem).Return(item).Repeat.Any();
                Expect.Call(editor.AddedEditors).Return(added);
                Expect.Call(editor.VersioningMode).Return(ItemEditorVersioningMode.VersionAndSave);
            }

            DoTheSaving(user, editor);

            Assert.AreEqual("", item.MyProperty0);
            Assert.AreEqual("I'm secure!", item.MyProperty6);
            Assert.IsTrue(item.MyProperty5);
        }

        [Test]
        public void WillNotAddOrUpdateEditor_InSecuredContainer_WhenUntrustedUser()
        {
            ItemWithSecuredContainer item = new ItemWithSecuredContainer();
            item.HiddenText = "No way";

            IPrincipal user = CreateUser("Joe", "Editor");

            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(typeof(ItemWithSecuredContainer), editorContainer, user);
            editManager.UpdateEditors(item, added, user);

            Assert.That(added.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanUpdateEditor_InSecuredContainer_WhenUserIsTrusted()
        {
            ItemWithSecuredContainer item = new ItemWithSecuredContainer();
            item.HiddenText = "Yes way";

            IPrincipal user = CreateUser("Joe", "Administrators");

            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(typeof(ItemWithSecuredContainer), editorContainer, user);
            editManager.UpdateEditors(item, added, user);

            Assert.That(((TextBox)added["HiddenText"]).Text, Is.EqualTo("Yes way"));
        }

        [Test]
        public void Using_PrivilegedUser_AddsAllEditors()
        {
            ComplexContainersItem item = new ComplexContainersItem();

            IPrincipal user = CreateUser("Joe", "ÜberEditor");

            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(typeof(ComplexContainersItem), editorContainer, user);
            Assert.AreEqual(7, added.Count);
        }

        [Test]
        public void Validators_AreAdded_ToPage()
        {
            Page p = new Page();
            Assert.AreEqual(0, p.Validators.Count);

            editManager.AddEditors(typeof(ItemWithRequiredProperty), p, null);

            Assert.AreEqual(2, p.Validators.Count);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateEditors_PukesOnNullItem()
        {
            Dictionary<string, Control> editors = CreateEditorsForComplexContainersItem();
            editManager.UpdateEditors(null, editors, null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateEditors_PukesOnNullAddedEditors()
        {
            ContentItem item = new ComplexContainersItem();
            editManager.UpdateEditors(item, null, null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateItem_PukesOnNullItem()
        {
            Dictionary<string, Control> editors = CreateEditorsForComplexContainersItem();
            editManager.UpdateItem(null, editors, null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateItem_PukesOnNullAddedEditors()
        {
            ContentItem item = new ComplexContainersItem();
            editManager.UpdateItem(item, null, null);
        }

        [Test]
        public void AppliesModifications()
        {
            Control editorContainer = new Control();
            IDictionary<string, Control> added = editManager.AddEditors(typeof(ItemWithModification), editorContainer, null);

            ItemWithModification item = new ItemWithModification();
            editManager.UpdateEditors(item, added, null);

            TextBox tb = added["Essay"] as TextBox;

            Assert.AreEqual(10, tb.Rows);
            Assert.AreEqual(TextBoxMode.MultiLine, tb.TextMode);
        }

        [Test]
        public void AddingEditor_InvokesEvent()
        {
            Control editorContainer = new Control();
            editManager.AddedEditor += new EventHandler<N2.Web.UI.ControlEventArgs>(editManager_AddedEditor);
            IDictionary<string, Control> added = editManager.AddEditors(typeof(ComplexContainersItem), editorContainer, null);

            Assert.AreEqual(5, noticedByEvent.Count);
            EnumerableAssert.Contains(noticedByEvent, added["MyProperty0"]);
            EnumerableAssert.Contains(noticedByEvent, added["MyProperty0"]);
            EnumerableAssert.Contains(noticedByEvent, added["MyProperty0"]);
            EnumerableAssert.Contains(noticedByEvent, added["MyProperty0"]);
            EnumerableAssert.Contains(noticedByEvent, added["MyProperty0"]);
        }

        [Test]
        public void UpdateItem_WithNoChanges_IsNotSaved()
        {
            ComplexContainersItem item = new ComplexContainersItem();
            item.ID = 22;
            item.MyProperty0 = "one";
            item.MyProperty1 = "two";
            item.MyProperty2 = "three";
            item.MyProperty3 = "rock";
            item.MyProperty4 = true;

            Expect.On(versioner).Call(versioner.SaveVersion(item)).Return(item.Clone(false));
            mocks.Replay(versioner);
            persister.Save(item);
            LastCall.Repeat.Never();
            mocks.Replay(persister);

            IItemEditor editor = SimulateEditor(item, ItemEditorVersioningMode.VersionAndSave);

            DoTheSaving(null, editor);
        }
    }
}