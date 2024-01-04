using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace TeensyRom.Ui.Features.Files.DirectoryContent
{
    public static class FileDragDropBehavior
    {
        public static readonly DependencyProperty FileDropCommandProperty =
            DependencyProperty.RegisterAttached(
                "FileDropCommand",
                typeof(ICommand),
                typeof(FileDragDropBehavior),
                new PropertyMetadata(null, OnFileDropCommandChanged));

        public static void SetFileDropCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(FileDropCommandProperty, value);
        }

        public static ICommand GetFileDropCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(FileDropCommandProperty);
        }

        private static void OnFileDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.Drop += (sender, args) =>
                {
                    var command = GetFileDropCommand(d);
                    if (command != null && command.CanExecute(args))
                    {
                        command.Execute(args);
                    }
                };
            }
        }

        public static readonly DependencyProperty DragOverCommandProperty =
            DependencyProperty.RegisterAttached(
            "DragOverCommand",
            typeof(ICommand),
            typeof(FileDragDropBehavior),
            new PropertyMetadata(null, OnDragOverCommandChanged));

        public static void SetDragOverCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(DragOverCommandProperty, value);
        }

        public static ICommand GetDragOverCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(DragOverCommandProperty);
        }

        private static void OnDragOverCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.DragOver += (sender, args) =>
                {
                    var command = GetDragOverCommand(d);
                    if (command != null && command.CanExecute(args))
                    {
                        command.Execute(args);
                    }
                };
            }
        }
    }
}