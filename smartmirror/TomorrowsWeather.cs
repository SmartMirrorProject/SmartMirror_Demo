namespace SmartMirror
{
    //Class to hold information for tomorrows forecast.
    //Each instance of the class holds the weather for a single 3-Hour forecast
    //Simple POCO with tomorrows weather info.
    public class TomorrowsWeather
    {
        private double _temperature;
        private string _weather;
        private string _weatherIcon;
        private readonly string _time;

        public TomorrowsWeather(string time)
        {
            _time = ModifyTimeForDisplay(time);
            SetDefaults();
        }

        public double GetTemperature()
        {
            return _temperature;
        }

        public void SetTemperature(double temp)
        {
            _temperature = temp;
        }

        public string GetWeather()
        {
            return _weather;
        }

        public void SetWeather(string weather)
        {
            _weather = weather;
        }

        public string GetWeatherIcon()
        {
            return _weatherIcon;
        }

        public void SetWeatherIcon(string weatherIcon)
        {
            _weatherIcon = weatherIcon;
        }

        public string GetTime()
        {
            return _time;
        }

        public void SetDefaults()
        {
            _temperature = 78.0;
            _weather = "Sunny";
        }

        private string ModifyTimeForDisplay(string time)
        {
            //Trim leading 0 if necessary
            if (time.StartsWith("0"))
            {
                return time.Substring(1);
            }
            else
            {
                return time;
            }
        }
    }
}