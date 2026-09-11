using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace ClientApp;

public partial class MainWindow : Window
{
    private HttpClient client = new HttpClient();
    private DispatcherTimer timer = new DispatcherTimer();

    public MainWindow()
    {
        InitializeComponent();

        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) => await GetServerStateAsync();
        timer.Start();

        _ = GetServerStateAsync();
    }

    private async Task GetServerStateAsync()
    {
        try
        {
            string res = await client.GetStringAsync("http://localhost:5050/state");
            var data = JObject.Parse(res);

            bool isLampOpen = (bool)data["lamp"]!;
            string serverText = (string)data["text"]!;

            btnLamp.Background = isLampOpen ? Brushes.Gold : Brushes.DimGray;
            btnLamp.Foreground = isLampOpen ? Brushes.Black : Brushes.White;

            btnText.Content = string.IsNullOrEmpty(serverText) ? "Text Butonu" : serverText;
        }
        catch { }
    }

    private async void btnLamp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await client.GetStringAsync("http://localhost:5050/toggle-lamp");
            await GetServerStateAsync();
        }
        catch { }
    }

    private async void btnText_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await client.GetStringAsync("http://localhost:5050/random-text");
            await GetServerStateAsync();
        }
        catch { }
    }
}


