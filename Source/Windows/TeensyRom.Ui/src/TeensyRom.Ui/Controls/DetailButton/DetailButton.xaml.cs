using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TeensyRom.Ui.Controls
{
    /// <summary>
    /// Interaction logic for DetailButton.xaml
    /// </summary>
    public partial class DetailButton : UserControl
    {
        public DetailButton()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DetailButton), new PropertyMetadata("No value set"));

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(DetailButton), new PropertyMetadata("No value set"));

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(DetailButton), null);

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent
        (
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(DetailButton)
        );

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public static readonly DependencyProperty ClickCommandProperty =
        DependencyProperty.Register(
            "ClickCommand",
            typeof(ICommand),
            typeof(DetailButton),
            new UIPropertyMetadata(null));
        public ICommand ClickCommand
        {
            get
            {
                return (ICommand)GetValue(ClickCommandProperty);
            }
            set
            {
                SetValue(ClickCommandProperty, value);
            }
        }

        private void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ClickEvent);
            RaiseEvent(newEventArgs);
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (ClickCommand != null && ClickCommand.CanExecute(null))
            {
                ClickCommand.Execute(null);
            }
            RaiseClickEvent();
        }
    }
}
