using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using ChangeLanguage.Properties;
using ChangeLanguage.ViewModel;

namespace ChangeLanguage
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        private Settings settings = Settings.Default;

        private DateTime _date;  
        public DateTime Date
        {
            get { return _date; }
            set 
            {
                _date = value;
                this.OnPropertyChanged("Date");
            }
        }
        
        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = settings.currentCulture;
            Thread.CurrentThread.CurrentUICulture = settings.currentCulture;
            this.DataContext = this;          

            InitializeComponent();

            SetTextsInCurrentLanguage();            
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            settings.currentCulture = (CultureInfo)listLanguages.SelectedItem;
            Thread.CurrentThread.CurrentCulture = settings.currentCulture;  //not so good to see... just to explain how it works
            Thread.CurrentThread.CurrentUICulture = settings.currentCulture;//you should force the user to re-initialize the program
            SetTextsInCurrentLanguage();
        }

        private void SetTextsInCurrentLanguage()
        {
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            LoadCmbItems();
            lblLanguages.Content = Translations.GetString("lblLanguages");
            btnApply.Content = Translations.GetString("btnApply");
            btnButton.Content = Translations.GetString("btnButton");
            lblLabel.Content = Translations.GetString("lblLabel");
            lblDateTime.Content = Translations.GetString("lblDateTime");
        }

        /// <summary> Detects languages in database and fill the listbox </summary>
        private void LoadCmbItems()
        {
            try
            {
                List<CultureInfo> cultureList = Translations.GetCultureList();
                listLanguages.ItemsSource = cultureList;
                listLanguages.SelectedItem = settings.currentCulture;
                Date = DateTime.Now;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Language Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
