using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;

namespace ServerApp;

public partial class MainWindow : Window
{
    private HttpListener listener = new HttpListener();
    private bool isLampOpen = false;

    public MainWindow()
    {
        InitializeComponent();
        UpdateLampUI();
        StartServer();
    }

    private void StartServer()
    {
        try
        {
            listener.Prefixes.Add("http://localhost:5050/");
            listener.Start();
            Task.Run(ListenLoop);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Server başlatılamadı: " + ex.Message);
        }
    }

    private async Task ListenLoop()
    {
        while (listener.IsListening)
        {
            try
            {
                var context = await listener.GetContextAsync();
                string path = context.Request.Url?.AbsolutePath.ToLower() ?? "";

                if (path == "/toggle-lamp" || path == "/lamp/toggle")
                {
                    Dispatcher.Invoke(() =>
                    {
                        isLampOpen = !isLampOpen;
                        UpdateLampUI();
                    });
                }
                else if (path == "/random-text")
                {
                    Dispatcher.Invoke(() =>
                    {
                        txtServerInput.Text = new Random().Next(1000, 9999).ToString();
                    });
                }

                string currentText = Dispatcher.Invoke(() => txtServerInput.Text);
                var data = new { lamp = isLampOpen, text = currentText };
                string json = JsonConvert.SerializeObject(data);

                byte[] buffer = Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch { }
        }
    }

    private void UpdateLampUI()
    {
        lampGraphic.Fill = isLampOpen ? Brushes.Yellow : Brushes.Gray;
    }

    private void btnToggleLamp_Click(object sender, RoutedEventArgs e)
    {
        isLampOpen = !isLampOpen;
        UpdateLampUI();
    }

    private void btnRandomNumber_Click(object sender, RoutedEventArgs e)
    {
        txtServerInput.Text = new Random().Next(1000, 9999).ToString();
    }

    private void txtServerInput_TextChanged(object sender, TextChangedEventArgs e)
    {
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        try { listener.Stop(); } catch { }
    }
}


