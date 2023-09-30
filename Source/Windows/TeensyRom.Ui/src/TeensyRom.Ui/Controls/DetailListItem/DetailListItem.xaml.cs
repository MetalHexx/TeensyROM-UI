using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TeensyRom.Ui.Controls
{
    /// <summary>
    /// Interaction logic for DetailListItem.xaml
    /// </summary>
    public partial class DetailListItem : UserControl
    {
        public DetailListItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DetailListItem), new PropertyMetadata("No value set"));

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(DetailListItem), new PropertyMetadata("No value set"));

        public string EditToolTip
        {
            get { return (string)GetValue(EditTooltipProperty); }
            set { SetValue(EditTooltipProperty, value); }
        }
        public static readonly DependencyProperty EditTooltipProperty =
            DependencyProperty.Register("EditToolTip", typeof(string), typeof(DetailListItem), new PropertyMetadata(""));



        public string RemoveToolTip
        {
            get { return (string)GetValue(RemoveToolTipProperty); }
            set { SetValue(RemoveToolTipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RemoveToolTip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveToolTipProperty =
            DependencyProperty.Register("RemoveToolTip", typeof(string), typeof(DetailListItem), new PropertyMetadata(""));





        public string DateTime
        {
            get { return (string)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }
        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register("DateTime", typeof(string), typeof(DetailListItem), new PropertyMetadata("No value set"));


        public bool EnableEditButton
        {
            get { return (bool)GetValue(EnableEditButtonProperty); }
            set { SetValue(EnableEditButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableEditButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableEditButtonProperty =
            DependencyProperty.Register("EnableEditButton", typeof(bool), typeof(DetailListItem), new PropertyMetadata(false));




        public bool EnableRemoveButton
        {
            get { return (bool)GetValue(EnableRemoveButtonProperty); }
            set { SetValue(EnableRemoveButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableRemoveButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableRemoveButtonProperty =
            DependencyProperty.Register("EnableRemoveButton", typeof(bool), typeof(DetailListItem), new PropertyMetadata(false));


        public static readonly DependencyProperty RemoveCommandProperty =
        DependencyProperty.Register(
            "RemoveCommand",
            typeof(ICommand),
            typeof(DetailListItem),
            new UIPropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get
            {
                return (ICommand)GetValue(RemoveCommandProperty);
            }
            set
            {
                SetValue(RemoveCommandProperty, value);
            }
        }

        public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(
            "EditCommand",
            typeof(ICommand),
            typeof(DetailListItem),
            new UIPropertyMetadata(null));

        public ICommand EditCommand
        {
            get
            {
                return (ICommand)GetValue(EditCommandProperty);
            }
            set
            {
                SetValue(EditCommandProperty, value);
            }
        }
    }
}
