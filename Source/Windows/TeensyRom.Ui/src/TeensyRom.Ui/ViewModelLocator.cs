using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MaterialDesignThemes.Wpf;
using System;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Midi;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Registers items for dependency injection.  
        /// <remarks>You can use this to inject classes into the constructors of your classes like view models</remarks>
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<DialogHost, DialogHost>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();
            SimpleIoc.Default.Register<ITeensyObservableSerialPort, TeensyObservableSerialPort>();
            SimpleIoc.Default.Register<NavigationHostViewModel>();
            SimpleIoc.Default.Register<ConnectViewModel>();
            SimpleIoc.Default.Register<FileTransferViewModel>();
            SimpleIoc.Default.Register<HelpViewModel>();
            SimpleIoc.Default.Register<MidiViewModel>();
        }

        /// <summary>
        /// Defaults the initial view to the Navigation Host View
        /// </summary>
        public NavigationHostViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NavigationHostViewModel>();
            }
        }

        public static void Cleanup()
        {
            //TODO: Clear the ViewModels
        }
    }
}
