using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System;

namespace TeensyRom.Ui.Features.Connect
{
    public class RichTextBindingBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty TextCollectionProperty =
            DependencyProperty.Register(
                "TextCollection",
                typeof(ObservableCollection<string>),
                typeof(RichTextBindingBehavior),
                new PropertyMetadata(null, OnTextCollectionChanged));

        public ObservableCollection<string> TextCollection
        {
            get { return (ObservableCollection<string>)GetValue(TextCollectionProperty); }
            set { SetValue(TextCollectionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateRichTextBox();
        }

        private static void OnTextCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RichTextBindingBehavior behavior)
            {
                throw new Exception("Was not able to cast the dependency object to a RichTextBindingBehavior");
            }
            if (e.OldValue is ObservableCollection<string> oldCollection)
            {
                oldCollection.CollectionChanged -= behavior.OnCollectionChanged;
            }
            if (e.NewValue is ObservableCollection<string> newCollection)
            {
                newCollection.CollectionChanged += behavior.OnCollectionChanged;
            }
            behavior.UpdateRichTextBox();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateRichTextBox();
        }

        private void UpdateRichTextBox()
        {
            if (AssociatedObject != null && TextCollection != null)
            {
                AssociatedObject.Document.Blocks.Clear();
                foreach (var text in TextCollection)
                {
                    Paragraph formattedParagraph = text.ToRichText();

                    AssociatedObject.Document.Blocks.Add(formattedParagraph);
                }
            }
            ScrollToBottom(AssociatedObject!);
        }

        private void ScrollToBottom(RichTextBox richTextBox)
        {
            if (richTextBox.Document != null)
            {
                richTextBox.ScrollToEnd();
            }
        }
    }
}