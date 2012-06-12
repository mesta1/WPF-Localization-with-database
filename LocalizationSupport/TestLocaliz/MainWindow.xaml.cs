using System.Windows;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Threading;
using TestLocaliz.Properties;
using LocalizationSupport.Providers;
using LocalizationSupport;
using System;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Generic;

namespace TestLocaliz
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region Public Properties

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set { _currentDate = value; this.OnPropertyChanged("CurrentDate"); }
        }

        ResourceFileProvider _provider = new ResourceFileProvider(Settings.Default.translationsConnectionString);
        public ResourceFileProvider Provider
        {
            get { return _provider; }
        }


        #endregion

        #region Private Fields

        private Settings settings = Settings.Default;
        
        DispatcherTimer timer = new DispatcherTimer();
        
        #endregion      
        

        public MainWindow()
        {
            // load cultures in Localization Manager
            foreach(CultureInfo culture in _provider.CultureList)   
                LocalizationManager.SupportedCultures.Add(culture, _provider);
            // after loading set the current culture
            LocalizationManager.CurrentCulture = settings.currentCulture;
            //after setting the localization manager set the UI and current thread with the current culture
            Thread.CurrentThread.CurrentCulture = settings.currentCulture;
            Thread.CurrentThread.CurrentUICulture = settings.currentCulture;
            
            // your code...
            InitializeComponent();
            this.DataContext = this;
            //listLanguages.ItemsSource = _provider.CultureList;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            CurrentDate = DateTime.Now;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            settings.currentCulture = (CultureInfo)listLanguages.SelectedItem;
            LocalizationManager.CurrentCulture = settings.currentCulture; //questo deve essere PRIMA DEL thread.currentCulture!!!
            Thread.CurrentThread.CurrentCulture = settings.currentCulture;  
            Thread.CurrentThread.CurrentUICulture = settings.currentCulture;
            settings.Save();        
        }

        private void btnShowMessageBox_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Provider.GetString("msgThisIsAMessageBox"),"MessageBox", MessageBoxButton.OK,MessageBoxImage.Exclamation);
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
