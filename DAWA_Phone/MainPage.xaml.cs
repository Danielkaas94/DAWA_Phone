using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Windows.Web.Http; // Den har nogle mangler i forhold til System.net.http 
using System.Net.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace DAWA_Phone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private static HttpClient client;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 20, 0);
            client.BaseAddress = new Uri("http://dawa.aws.dk/");
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string text = textBox.Text;
            if (radioButton_IdSearch.IsChecked == true)
            {
                //HentAdresse(client, "0a3f50a0-73bf-32b8-e044-0003ba298018");
                HentAdresse(client, text);
            }
            else if (radioButton_StreetSearch.IsChecked == true)
            {
                //HentAdresser(client, "Rødkildevej&postnr=2400");
                HentAdresser(client, text);
            }
            else if (radioButton_DataWash.IsChecked == true)
            {
                //DataWash(client, "Rante mester vej 8, 2400 København NV");
                DataWash(client, text);
            }
        }


        private void HentAdresse(HttpClient client, string id)
        {
            try
            {
                string url = "adresser/" + id;

                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                dynamic adresse = JValue.Parse(responseBody);
       
                // ListBoxItem
                ListBoxItem listBox_item = new ListBoxItem();
                listBox_item.Content = FormatAdresse(adresse);
                MainListBox.Items.Add(listBox_item);
            }
            catch (Exception e)
            {
                FailMessage(e.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="query"></param>
        private void HentAdresser(HttpClient client, string query)
        {
            try
            {
                //string url = "adresser/" + (query.Length == 0 ? "" : "?") + query;
                string url = "adresser/" + (query.Length == 0 ? "" : "?") + "vejnavn=" + query;
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                dynamic adresser = JArray.Parse(responseBody);
                foreach (dynamic adresse in adresser)
                {
                    ListBoxItem listBox_item = new ListBoxItem();
                    listBox_item.Content = FormatAdresse(adresse);
                    MainListBox.Items.Add(listBox_item);
                }
            }
            catch (Exception e)
            {
                FailMessage(e.Message);
            }

        }

        private void DataWash(HttpClient client, string query)
        {
            try
            {
                string url = "datavask/adgangsadresser/" + (query.Length == 0 ? "" : "?") + "betegnelse=" + query;
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;

                responseBody = responseBody.Substring(38);
                responseBody = responseBody.TrimEnd('}');
                responseBody = responseBody.TrimEnd(']', '\n');

                dynamic adresser = JValue.Parse(responseBody);

                    ListBoxItem listBox_item = new ListBoxItem();
                    listBox_item.Content = FormatAdresseDataWash(adresser);
                    MainListBox.Items.Add(listBox_item);


            }
            catch (Exception e)
            {
                FailMessage(e.Message);
            }
        }

        private string FormatAdresseDataWash(dynamic DWadresse)
        {
            return string.Format($"Mente du {DWadresse.adresse.vejnavn} {DWadresse.adresse.husnr} {DWadresse.adresse.postnr} {DWadresse.adresse.postnrnavn} ? - Du skrev: {DWadresse.vaskeresultat.parsetadresse.vejnavn}, {DWadresse.vaskeresultat.parsetadresse.husnr} {DWadresse.vaskeresultat.parsetadresse.postnr} {DWadresse.vaskeresultat.parsetadresse.postnrnavn} ");
        }

        private string FormatAdresse(dynamic adresse)
        {
            return string.Format($"{adresse.adgangsadresse.vejstykke.navn} {adresse.adgangsadresse.husnr} , {adresse.etage} {adresse.dør} {adresse.adgangsadresse.postnummer.nr} {adresse.adgangsadresse.postnummer.navn} , {adresse.adgangsadresse.politikreds.navn} , {adresse.adgangsadresse.region.navn}");
        }



        private void GoToHelpPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BasicPage_Help));
        }


        private void FailMessage(string failMassage)
        {
            ListBoxItem failed_Massage_Item = new ListBoxItem();
            failed_Massage_Item.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            failed_Massage_Item.Content = "failed -" + failMassage;
            MainListBox.Items.Add(failed_Massage_Item);
        }

        #region Clicks

        private void radioButton_IdSearch_Click(object sender, RoutedEventArgs e)
        {
            textBox.PlaceholderText = "0a3f50a0-73bf-32b8-e044-0003ba298018";
        }

        private void radioButton_StreetSearch_Click(object sender, RoutedEventArgs e)
        {
            textBox.PlaceholderText = "Rødkildevej&postnr=2400";
        }

        private void radioButton_DataWash_Click(object sender, RoutedEventArgs e)
        {
            textBox.PlaceholderText = "Rante mester vej 8, 2400 København NV";
        }

        private void ClearListBox_Click(object sender, RoutedEventArgs e)
        {
            MainListBox.Items.Clear();
        }

        #endregion

    }
}
