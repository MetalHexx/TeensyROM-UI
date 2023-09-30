using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Reactive.Subjects;

namespace TeensyRom.Ui.Controls
{
    public class DetailListItemViewModel : ViewModelBase
    {
        //Events
        public ISubject<DetailListItemViewModel> RemoveClicked { get; set; } = new Subject<DetailListItemViewModel>();
        public ISubject<DetailListItemViewModel> EditClicked { get; set; } = new Subject<DetailListItemViewModel>();

        //View Commands
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        //View Properties
        private string _title = string.Empty;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title == value)
                    return;
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        private string _subTitle = string.Empty;
        public string SubTitle
        {
            get
            {
                return _subTitle;
            }
            set
            {
                if (_subTitle == value)
                    return;
                _subTitle = value;
                RaisePropertyChanged("SubTitle");
            }
        }

        private string _editToolTip = string.Empty;
        public string EditToolTip
        {
            get
            {
                return _editToolTip;
            }
            set
            {
                if (_editToolTip == value)
                    return;
                _editToolTip = value;
                RaisePropertyChanged("EditToolTip");
            }
        }

        private string _removeToolTip = string.Empty;
        public string RemoveToolTip
        {
            get
            {
                return _removeToolTip;
            }
            set
            {
                if (_removeToolTip == value)
                    return;
                _removeToolTip = value;
                RaisePropertyChanged("RemoveToolTip");
            }
        }

        private string _dateTime = string.Empty;
        public string DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                if (_dateTime == value)
                    return;
                _dateTime = value;
                RaisePropertyChanged("DateTime");
            }
        }

        private bool _enableEditButton;
        public bool EnableEditButton
        {
            get
            {
                return _enableEditButton;
            }
            set
            {
                if (_enableEditButton == value)
                    return;
                _enableEditButton = value;
                RaisePropertyChanged("EnableEditButton");
            }
        }

        private bool _enableRemoveButton;
        public bool EnableRemoveButton
        {
            get
            {
                return _enableRemoveButton;
            }
            set
            {
                if (_enableRemoveButton == value)
                    return;
                _enableRemoveButton = value;
                RaisePropertyChanged("EnableRemoveButton");
            }
        }

        public DetailListItemViewModel()
        {

            EditCommand = new RelayCommand(HandleEditCommand);
            RemoveCommand = new RelayCommand(HandleRemoveCommand);
        }

        //Command Handlers
        private void HandleRemoveCommand()
        {
            RemoveClicked.OnNext(this);
        }

        private void HandleEditCommand()
        {
            EditClicked.OnNext(this);
        }
    }
}
