using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartMirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Timers for updating all mirror elements
        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _todaysWeatherTimer;
        private readonly DispatcherTimer _tomorrowsWeatherTimer;
        private readonly DispatcherTimer _weeksWeatherTimer;
        private readonly DispatcherTimer _refreshRateTimer;

        //Rates at which mirror elements should update
        private const long ClockRefreshRate = 1L; // 1 Second
        private const long TodaysWeatherRefreshRate = (60L * 5L); //5 Minutes
        private const long TomorrowsWeatherRefreshRate = 6L; //6 Hours
        private const long WeeksWeatherRefreshRate = 8L; //8 Hours
        private const long DisplayRefreshRate = 500L; // 500 Milliseconds

        //Flags to indicate when elements have been updated
        private bool _clockTicked;
        private bool _todaysWeatherTicked;
        private bool _tomorrowsWeatherTicked;
        private bool _weeksWeatherTicked;

        public MainPage()
        {
            //Initialize all the things
            this.InitializeComponent();
            VoiceController.InitializeSpeechRecognizer();
            MirrorState.InitialMirrorState();
            _clockTimer = new DispatcherTimer();
            _todaysWeatherTimer = new DispatcherTimer();
            _tomorrowsWeatherTimer = new DispatcherTimer();
            _weeksWeatherTimer = new DispatcherTimer();;
            _refreshRateTimer = new DispatcherTimer();

            //Set the refresh rates on the timers
            _clockTimer.Interval = TimeSpan.FromSeconds(ClockRefreshRate);
            _todaysWeatherTimer.Interval = TimeSpan.FromSeconds(TodaysWeatherRefreshRate);
            _tomorrowsWeatherTimer.Interval = TimeSpan.FromHours(TomorrowsWeatherRefreshRate);
            _weeksWeatherTimer.Interval = TimeSpan.FromHours(WeeksWeatherRefreshRate);
            _refreshRateTimer.Interval = TimeSpan.FromMilliseconds(DisplayRefreshRate);

            //Set all the flags for updates to true at start
            _clockTicked = true;
            _todaysWeatherTicked = true;
            _tomorrowsWeatherTicked = true;
            _weeksWeatherTicked = true;

            //Populate all weather information at the beginning
            WeatherLogic.UpdateTodaysWeather();
            WeatherLogic.UpdateTomorrowsWeather();
            WeatherLogic.UpdateThisWeeksWeather();

            //Start the program
            StartTimers();
            RefreshDisplay();
        }

        //When the program ends, kill the speech recognizer
        public static async void MainPage_Unloaded(object sender, object args)
        {
            //Stop voice recognition
            VoiceController.UnloadSpeechRecognizer();
        }

        //Refresh all the display elements
        private void RefreshDisplay()
        {
            //Determine if the mirror is on or off and
            //adjust display elements accordingly
            if (MirrorState.IsMirrorOn())
            {
                ClockTxtBlock.Visibility = Visibility.Visible;
                if (MirrorState.IsWeatherOn() && MirrorState.IsConnectedToInternet())
                {
                    TurnOnWeather();
                }
            }
            else
            {
                ClockTxtBlock.Visibility = Visibility.Collapsed;
                TurnOffWeather();
            }

            if (!MirrorState.IsConnectedToInternet())
            {
                TurnOffWeather();
            }

            //Refresh each element if it ticked since last refresh
            //Refresh clock
            if (_clockTicked)
            {
                ClockTxtBlock.Text = DateTime.Now.ToString(@"h\:mm");
                _clockTicked = false;
            }
            //Refresh today's weather
            if (_todaysWeatherTicked)
            {
                UpdateTodaysWeatherDisplay();
                _todaysWeatherTicked = false;
            }
            //Refresh tomorrow's weather
            if (_tomorrowsWeatherTicked)
            {
                UpdateTomorrowsWeatherDisplay();
                _tomorrowsWeatherTicked = false;
            }
            //Refresh the weeks weather forecast
            if (_weeksWeatherTicked)
            {
                UpdateWeeksWeatherForecastDisplay();
                _weeksWeatherTicked = false;
            }
        }

        //Turn on the weather elements per MirrorState
        private void TurnOnWeather()
        {
            //If Main Weather display set to todays weather
            if (MirrorState.GetMainWeatherInfo().Equals(MirrorState.MAIN_WTHR_TODAY))
            {
                //Set todays weather visible and tomorrows weather collapsed
                TodaysWeather.Visibility = Visibility.Visible;
                TomorrowsWeather.Visibility = Visibility.Collapsed;
            }
            //If Main Weather display set to tomorrows weather
            else
            {
                //Set todays weather collapsed and tomorrows weather visible
                TodaysWeather.Visibility = Visibility.Collapsed;
                TomorrowsWeather.Visibility = Visibility.Visible;
            }
            //If the weeks weather is on
            if (MirrorState.IsWeeksWeatherOn())
            {
                //Set the weeks weather visible
                WeeksWeatherForecast.Visibility = Visibility.Visible;
            }
            //If the weeks weather is off
            else
            {
                //Set the weeks weather collapsed
                WeeksWeatherForecast.Visibility = Visibility.Collapsed;
            }
        }

        //Turn off all weather display elements.
        private void TurnOffWeather()
        {
            TodaysWeather.Visibility = Visibility.Collapsed;
            TomorrowsWeather.Visibility = Visibility.Collapsed;
            WeeksWeatherForecast.Visibility = Visibility.Collapsed;
        }

        //Start all timers and set their tick methods
        private void StartTimers()
        {
            _clockTimer.Tick += ClockTick;
            _clockTimer.Start();
            _todaysWeatherTimer.Tick += TodaysWeatherTick;
            _todaysWeatherTimer.Start();
            _tomorrowsWeatherTimer.Tick += TomorrowsWeatherTick;
            _tomorrowsWeatherTimer.Start();
            _weeksWeatherTimer.Tick += WeeksWeatherTick;
            _weeksWeatherTimer.Start();
            _refreshRateTimer.Tick += RefreshTick;
            _refreshRateTimer.Start();
        }

        //------------------------------------------------------------------------------------------------
        //-----------------------------------------Timer Ticks--------------------------------------------
        //------------------------------------------------------------------------------------------------
        private void ClockTick(object sender, object e)
        {
            _clockTicked = true;
        }

        private void TodaysWeatherTick(object sender, object e)
        {
            _todaysWeatherTicked = true;
        }

        private void TomorrowsWeatherTick(object sender, object e)
        {
            _tomorrowsWeatherTicked = true;
        }

        private void WeeksWeatherTick(object sender, object e)
        {
            _weeksWeatherTicked = true;
        }

        private void RefreshTick(object sender, object e)
        {
            RefreshDisplay();
        }

        //------------------------------------------------------------------------------------------------
        //----------------------Modify XAML Elements For Each Weather Component---------------------------
        //------------------------------------------------------------------------------------------------
        private void UpdateTodaysWeatherDisplay()
        {
            WeatherLogic.UpdateTodaysWeather();
            if (MirrorState.IsConnectedToInternet())
            {
                TodaysWeatherTypeImage.Source = new BitmapImage(new Uri(WeatherLogic.GetTodaysWeatherIcon()));
                TodaysWeatherLocationTextBlock.Text = WeatherLogic.GetCurrentLocation();
                TodaysWeatherTempTextBlock.Text = WeatherLogic.GetTodaysTemperature();
                TodaysWeatherHiTempTextBlock.Text = WeatherLogic.GetTodaysHiTemp();
                TodaysWeatherLowTempTextBlock.Text = WeatherLogic.GetTodaysLowTemp();
                TodaysWeatherDate.Text = WeatherLogic.GetTodaysLastUpdateDay();
            }
        }

        private void UpdateTomorrowsWeatherDisplay()
        {
            WeatherLogic.UpdateTomorrowsWeather();
            if (MirrorState.IsConnectedToInternet())
            {
                TomorrowsWeatherLocationTextBlock.Text = WeatherLogic.GetCurrentLocation();
                TomorrowsWeatherLastUpdateDate.Text = WeatherLogic.GetTomorrowsLastUpdateDay();
                Time6AM.Text = WeatherLogic.Get6AMTime();
                Temp6AM.Text = WeatherLogic.Get6AMTemp();
                Weather6AM.Source = new BitmapImage(new Uri(WeatherLogic.Get6AMWeatherIcon()));
                Time9AM.Text = WeatherLogic.Get9AMTime();
                Temp9AM.Text = WeatherLogic.Get9AMTemp();
                Weather9AM.Source = new BitmapImage(new Uri(WeatherLogic.Get9AMWeatherIcon()));
                Time12PM.Text = WeatherLogic.Get12PMTime();
                Temp12PM.Text = WeatherLogic.Get12PMTemp();
                Weather12PM.Source = new BitmapImage(new Uri(WeatherLogic.Get12PMWeatherIcon()));
                Time3PM.Text = WeatherLogic.Get3PMTime();
                Temp3PM.Text = WeatherLogic.Get3PMTemp();
                Weather3PM.Source = new BitmapImage(new Uri(WeatherLogic.Get3PMWeatherIcon()));
                Time6PM.Text = WeatherLogic.Get6PMTime();
                Temp6PM.Text = WeatherLogic.Get6PMTemp();
                Weather6PM.Source = new BitmapImage(new Uri(WeatherLogic.Get6PMWeatherIcon()));
                Time9PM.Text = WeatherLogic.Get9PMTime();
                Temp9PM.Text = WeatherLogic.Get9PMTemp();
                Weather9PM.Source = new BitmapImage(new Uri(WeatherLogic.Get9PMWeatherIcon()));
                TomorrowsHighTextBlock.Text = WeatherLogic.GetTomorrowsHighTemp();
                TomorrowsLowTextBlock.Text = WeatherLogic.GetTomorrowsLowTemp();
            }
        }

        private void UpdateWeeksWeatherForecastDisplay()
        {
            WeatherLogic.UpdateThisWeeksWeather();
            if (MirrorState.IsConnectedToInternet())
            {
                //WeeksWeatherLocationTextBlock.Text = WeatherLogic.GetCurrentLocation();
                ForecastDay1Date.Text = WeatherLogic.GetDay1Date();
                ForecastDay1High.Text = WeatherLogic.GetDay1HighTemp();
                ForecastDay1Low.Text = WeatherLogic.GetDay1LowTemp();
                ForecastDay1Weather.Source = new BitmapImage(new Uri(WeatherLogic.GetDay1WeatherIcon()));
                ForecastDay2Date.Text = WeatherLogic.GetDay2Date();
                ForecastDay2High.Text = WeatherLogic.GetDay2HighTemp();
                ForecastDay2Low.Text = WeatherLogic.GetDay2LowTemp();
                ForecastDay2Weather.Source = new BitmapImage(new Uri(WeatherLogic.GetDay2WeatherIcon()));
                ForecastDay3Date.Text = WeatherLogic.GetDay3Date();
                ForecastDay3High.Text = WeatherLogic.GetDay3HighTemp();
                ForecastDay3Low.Text = WeatherLogic.GetDay3LowTemp();
                ForecastDay3Weather.Source = new BitmapImage(new Uri(WeatherLogic.GetDay3WeatherIcon()));
                ForecastDay4Date.Text = WeatherLogic.GetDay4Date();
                ForecastDay4High.Text = WeatherLogic.GetDay4HighTemp();
                ForecastDay4Low.Text = WeatherLogic.GetDay4LowTemp();
                ForecastDay4Weather.Source = new BitmapImage(new Uri(WeatherLogic.GetDay4WeatherIcon()));
                ForecastDay5Date.Text = WeatherLogic.GetDay5Date();
                ForecastDay5High.Text = WeatherLogic.GetDay5HighTemp();
                ForecastDay5Low.Text = WeatherLogic.GetDay5LowTemp();
                ForecastDay5Weather.Source = new BitmapImage(new Uri(WeatherLogic.GetDay5WeatherIcon()));
            }
        }
    }
}
