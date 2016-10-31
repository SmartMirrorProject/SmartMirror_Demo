using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SmartMirror
{
    public class WeatherLogic
    {
        //Location hardcoded to Orlando because we have no way to set it at runtime currently.
        //There are two options we could implement.
        //1. An External Application on a phone or pc to set the location
        //2. Use a config file users would have to edit on the pi.  Unfortunately, to edit files on a pi
        //      running IoT core users would need to use PowerShell or put the micro sd card into a computer
        //      neither of which seem like user friendly, viable options.
        private const string LocationCode = "4167147";
        private const string City = "Orlando";
        private const string State = "FL";
        private const string Country = "US";

        //Arrays for Days of Week and Months for use with DateTime.DayOfWeek and DateTime.MonthOfYear which return ints.
        private static readonly string[] DaysOfWeek = { "Sun", "Mon", "Tues", "Wed", "Thurs", "Fri", "Sat" };

        private static readonly string[] MonthsOfYear =
        {
            "", "January", "February", "March", "April", "May", "June", "July",
            "August", "September", "October", "November", "December"
        };
             
        //Weather COJOs for each weather display type
        private static readonly TodaysWeather Today = new TodaysWeather();

        private static readonly List<TomorrowsWeather> Tomorrow = new List<TomorrowsWeather>();
        private static string TomorrowsHigh;
        private static string TomorrowsLow;

        private static readonly List<DaysWeatherForecast> WeeksWeather = new List<DaysWeatherForecast>();

        // This class is a Singleton.
        //Private constructor to avoid instantiation
        private WeatherLogic() { }

        //------------------------------------------------------------------------------------------------
        //------------------------------------Today's Weather---------------------------------------------
        //------------------------------------------------------------------------------------------------
        public static void UpdateTodaysWeather()
        {
            //Read the weather then assign all the fields of Today
            WeatherData today = ReadTodaysWeatherInformation();
            if (today != null)
            {
                Today.SetLastUpdated(DateTime.Now);
                Today.SetTemperature(today.main.temp);
                Today.SetWeather(today.weather[0].main);
                Today.SetWeatherIcon(today.weather[0].icon);
                Today.SetHighTemperature(today.main.temp_max);
                Today.SetLowTemperature(today.main.temp_min);
                Today.SetCity(today.name);
                Today.SetCountry(today.sys.country);
            }
        }

        private static WeatherData ReadTodaysWeatherInformation()
        {
            try
            {
                //Request the JSON file from OpenWeatherMaps
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                    ($"http://api.openweathermap.org/data/2.5/weather?id=" + LocationCode + "&units=imperial&appID=6eeb0ae7137df453623bb2cb436db19a"));
                HttpClient client = new HttpClient();
                var response = client.SendAsync(request).Result;
                //Parse the object from the JSON and return it
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var bytes = Encoding.Unicode.GetBytes(result);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(WeatherData));
                        var data = (WeatherData)serializer.ReadObject(stream);
                        MirrorState.SetConnectedToInternet(true);
                        return data;
                    }
                }
                else
                {
                    return DefaultWeatherData();
                }
            }
            catch (Exception)
            {
                MirrorState.SetConnectedToInternet(false);
                return null;
            }
        }

        //------------------------------------------------------------------------------------------------
        //------------------------------------Tomorrow's Weather------------------------------------------
        //------------------------------------------------------------------------------------------------

        public static void UpdateTomorrowsWeather()
        {
            //Read tomorrows weather data and process it into Tomorrow
            TomorrowsWeatherData tomorrowsData = ReadTomorrowsWeatherInformation();
            if (tomorrowsData != null)
                ProcessTomorrowsWeatherData(tomorrowsData);
        }

        private static void ProcessTomorrowsWeatherData(TomorrowsWeatherData tomorrowsData)
        {
            //Begin by clearing the list of tomorrows forecast
            Tomorrow.Clear();
            //Init a high and low to ranges beyond what will ever be hit
            double daysHigh = -200, daysLow = 200;
            //Iterate through all weather data retrieved
            for (int i = 1; i < tomorrowsData.cnt; i++)
            {
                //Convert the UnixTime value to strings representing the day and time
                string date = ConvertUnixTimeToDate(tomorrowsData.list[i].dt);
                string time = ConvertUnixTimeToTime(tomorrowsData.list[i].dt);
                //If the current result is for tomorrow, process it and store it in Tomorrow
                if (ValidTimeTomorrow(date))
                {
                    TomorrowsWeather weather = new TomorrowsWeather(time);
                    weather.SetWeather(tomorrowsData.list[i].weather[0].main);
                    weather.SetWeatherIcon(tomorrowsData.list[i].weather[0].icon);
                    weather.SetTemperature(tomorrowsData.list[i].main.temp);
                    double low = tomorrowsData.list[i].main.temp_min;
                    double high = tomorrowsData.list[i].main.temp_max;
                    if (low < daysLow) daysLow = low;
                    if (high > daysHigh) daysHigh = high;
                    Tomorrow.Add(weather);
                }
            }
            //After processing all data we should have the accurate high and low, store them as strings
            TomorrowsHigh = Math.Round(daysHigh).ToString();
            TomorrowsLow = Math.Round(daysLow).ToString();
        }

        private static TomorrowsWeatherData ReadTomorrowsWeatherInformation()
        {
            try
            {
                //Request the JSON file from OpenWeatherMaps
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                    ($"http://api.openweathermap.org/data/2.5/forecast?id=" + LocationCode + "&units=imperial&appID=6eeb0ae7137df453623bb2cb436db19a"));
                HttpClient client = new HttpClient();
                //Parse the object from the JSON and return it
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var bytes = Encoding.Unicode.GetBytes(result);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(TomorrowsWeatherData));
                        var data = (TomorrowsWeatherData)serializer.ReadObject(stream);
                        MirrorState.SetConnectedToInternet(true);
                        return data;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                MirrorState.SetConnectedToInternet(false);
                return null;
            }

        }

        //Check the date passed in and ensure it is tomorrow, if so return true else return false
        private static bool ValidTimeTomorrow(string date)
        {
            DateTime tomorrowDt = DateTime.Today.AddDays(1);
            string tomorrow = tomorrowDt.ToString("yyyy-MM-dd");
            if (date.Equals(tomorrow))
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------
        //------------------------------------Week's Weather----------------------------------------------
        //------------------------------------------------------------------------------------------------

        public static void UpdateThisWeeksWeather()
        {
            //Read the new weather data then process it into WeeksWeather
            WeeksWeatherData weeksData = ReadThisWeeksWeatherInformation();
            if (weeksData != null)
                ProcessWeeksWeatherData(weeksData);
        }

        private static WeeksWeatherData ReadThisWeeksWeatherInformation()
        {
            try
            {
                //Request the JSON file from OpenWeatherMaps
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                    ($"http://api.openweathermap.org/data/2.5/forecast/daily?id=" + LocationCode +
                     "&cnt=6&units=imperial&appID=6eeb0ae7137df453623bb2cb436db19a"));
                HttpClient client = new HttpClient();
                var response = client.SendAsync(request).Result;

                //Parse the object from the JSON and return it
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var bytes = Encoding.Unicode.GetBytes(result);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        var serializer = new DataContractJsonSerializer(typeof (WeeksWeatherData));
                        var data = (WeeksWeatherData) serializer.ReadObject(stream);
                        MirrorState.SetConnectedToInternet(true);
                        return data;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                MirrorState.SetConnectedToInternet(false);
                return null;
            }
        }

        private static void ProcessWeeksWeatherData(WeeksWeatherData weeksData)
        {
            //Clear the weeks weather for new entries
            WeeksWeather.Clear();
            //Iterate through the forecast items for the week and convert them to DaysWeatherForecasts
            //then store them into WeeksWeather for retrieval for displaying
            for (int i = 1; i < weeksData.cnt; i++)
            {
                DaysWeatherForecast weather = new DaysWeatherForecast(UnixTimeStampToDateTime(weeksData.list[i].dt));
                weather.SetWeather(weeksData.list[i].weather[0].main);
                weather.SetWeatherIcon(weeksData.list[i].weather[0].icon);
                weather.SetHighTemperature(weeksData.list[i].temp.max);
                weather.SetLowTemperature(weeksData.list[i].temp.min);
                WeeksWeather.Add(weather);
            }
        }

        //------------------------------------------------------------------------------------------------
        //------------------------------------General Weather Helpers-------------------------------------
        //------------------------------------------------------------------------------------------------
        //If something fails we will use default weather info
        private static WeatherData DefaultWeatherData()
        {
            WeatherData defaultData = new WeatherData();
            defaultData.main.temp = -99.99;
            defaultData.main.temp_min = -99.99;
            defaultData.main.temp_max = -99.99;
            defaultData.weather[0].icon = "02d";
            return defaultData;
        }

        public static string GetCurrentLocation()
        {
            return City + ", " + State;
        }

        //Convert a UnixTime value to a string representing that date
        private static string ConvertUnixTimeToDate(long unixTime)
        {
            DateTime date = UnixTimeStampToDateTime(unixTime);
            return date.ToString("yyyy-MM-dd");
        }

        //Convert a UnixTime value to a string representing that time
        private static string ConvertUnixTimeToTime(long unixTime)
        {
            DateTime date = UnixTimeStampToDateTime(unixTime);
            return date.ToString("h:mm");
        }

        //Convert a UnixTime value to a C# DateTime object
        private static DateTime UnixTimeStampToDateTime(double unixTime)
        {
            //Set the time to 01/01/1970 00:00:00:00
            System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dateTime.AddSeconds(unixTime).ToLocalTime();
        }

        //Determine the appropriate icon based on the icon code from OpenWeatherMaps
        private static string GetWeatherIconPath(string icon)
        {
            switch (icon)
            {
                case "01d":
                    return "ms-appx:/Assets/Weather/Sunny.png";
                case "01n":
                    return "ms-appx:/Assets/Weather/ClearNight.png";

                case "02d":
                    return "ms-appx:/Assets/Weather/PartlyCloudy.png";
                case "02n":
                    return "ms-appx:/Assets/Weather/CloudyNight.png";

                case "03d":
                    return "ms-appx:/Assets/Weather/ScatteredClouds.png";
                case "03n":
                    return "ms-appx:/Assets/Weather/CloudyNight.png";

                case "04d":
                    return "ms-appx:/Assets/Weather/Cloudy.png";
                case "04n":
                    return "ms-appx:/Assets/Weather/CloudyNight.png";

                case "09d":
                    return "ms-appx:/Assets/Weather/LightRain.png";
                case "09n":
                    return "ms-appx:/Assets/Weather/NightRain.png";

                case "10d":
                    return "ms-appx:/Assets/Weather/LightRain.png";
                case "10n":
                    return "ms-appx:/Assets/Weather/NightRain.png";

                case "11d":
                case "11n":
                    return "ms-appx:/Assets/Weather/Thunderstorm.png";

                case "13d":
                case "13n":
                    return "ms-appx:/Assets/Weather/LightSnow.png";

                case "50d":
                    return "ms-appx:/Assets/Weather/MistDay.png";
                case "50n":
                    return "ms-appx:/Assets/Weather/MistNight.png";

                default:
                    return "ms-appx:/Assets/Weather/PartlyCloudy.png";
            }
        }

        //------------------------------------------------------------------------------------------------
        //---------------------------------Getters For Todays Weather-------------------------------------
        //------------------------------------------------------------------------------------------------
        public static string GetTodaysTemperature()
        {
            return "" + (Math.Round(Today.GetTemerature()));
        }

        public static string GetTodaysWeather()
        {
            return Today.GetWeather();
        }

        public static string GetTodaysHiTemp()
        {
            return ("Hi: " + Math.Round(Today.GetHighTemperature()) + "°");
        }

        public static string GetTodaysLowTemp()
        {
            return ("Low:" + Math.Round(Today.GetLowTemperature()) + "°");
        }

        public static string GetTodaysLastUpdateDay()
        {
            DateTime date = Today.GetLastUpdated();
            string day = DaysOfWeek[(Int32) date.DayOfWeek];
            string month = MonthsOfYear[date.Month];
            string dayOfMonth = date.Day.ToString();
            return day + ", " + month + " " + dayOfMonth;
        }

        public static string GetTodaysLastUpdateTime()
        {
            return Today.GetLastUpdated().ToString("H:mm");
        }

        public static string GetTodaysWeatherIcon()
        {
            return GetWeatherIconPath(Today.GetWeatherIcon());
        }

        //------------------------------------------------------------------------------------------------
        //----------------------------Getters For Tomorrows Weather---------------------------------------
        //------------------------------------------------------------------------------------------------
        public static string GetTomorrowsHighTemp()
        {
            return "High: " + TomorrowsHigh + "°";
        }

        public static string GetTomorrowsLowTemp()
        {
            return "Low: " + TomorrowsLow + "°";
        }

        public static string GetTomorrowsLastUpdateDay()
        {
            DateTime date = DateTime.Today.AddDays(1);
            string day = DaysOfWeek[(Int32)date.DayOfWeek];
            string month = MonthsOfYear[date.Month];
            string dayOfMonth = date.Day.ToString();
            return day + ", " + month + " " + dayOfMonth;
        }

        public static string Get6AMTime()
        {
            return Tomorrow[1].GetTime();
        }

        public static string Get6AMTemp()
        {
            return Math.Round(Tomorrow[1].GetTemperature()) + "°";
        }

        public static string Get6AMWeatherIcon()
        {
            return GetWeatherIconPath(Tomorrow[1].GetWeatherIcon());
        }

        public static string Get9AMTime()
        {
            return Tomorrow[2].GetTime();
        }

        public static string Get9AMTemp()
        {
            return Math.Round(Tomorrow[2].GetTemperature()) + "°";
        }

        public static string Get9AMWeatherIcon()
        {
            return GetWeatherIconPath(Tomorrow[2].GetWeatherIcon());
        }

        public static string Get12PMTime()
        {
            return Tomorrow[3].GetTime();
        }

        public static string Get12PMTemp()
        {
            return Math.Round(Tomorrow[3].GetTemperature()) + "°";
        }

        public static string Get12PMWeatherIcon()
        {
            return GetWeatherIconPath(Tomorrow[3].GetWeatherIcon());
        }

        public static string Get3PMTime()
        {
            return Tomorrow[4].GetTime();
        }

        public static string Get3PMTemp()
        {
            return Math.Round(Tomorrow[4].GetTemperature()) + "°";
        }

        public static string Get3PMWeatherIcon()
        {
            return GetWeatherIconPath(Tomorrow[4].GetWeatherIcon());
        }

        public static string Get6PMTime()
        {
            return Tomorrow[5].GetTime();
        }

        public static string Get6PMTemp()
        {
            return Math.Round(Tomorrow[5].GetTemperature()) + "°";
        }

        public static string Get6PMWeatherIcon()
        {
            return GetWeatherIconPath(Tomorrow[5].GetWeatherIcon());
        }

        public static string Get9PMTime()
        {
            return Tomorrow[6].GetTime();
        }

        public static string Get9PMTemp()
        {
            return Math.Round(Tomorrow[6].GetTemperature()) + "°";
        }

        public static string Get9PMWeatherIcon()
        {
            return GetWeatherIconPath(Tomorrow[6].GetWeatherIcon());
        }

        //------------------------------------------------------------------------------------------------
        //--------------------------Getters For Weeks Weather Forecast------------------------------------
        //------------------------------------------------------------------------------------------------
        public static string GetDay1Date()
        {
            return WeeksWeather[0].GetDate();
        }

        public static string GetDay1WeatherIcon()
        {
            return GetWeatherIconPath(WeeksWeather[0].GetWeatherIcon());
        }

        public static string GetDay1HighTemp()
        {
            return "" + Math.Round(WeeksWeather[0].GetHighTemperature());
        }

        public static string GetDay1LowTemp()
        {
            return "" + Math.Round(WeeksWeather[0].GetLowTemperature());
        }

        public static string GetDay2Date()
        {
            return WeeksWeather[1].GetDate();
        }

        public static string GetDay2WeatherIcon()
        {
            return GetWeatherIconPath(WeeksWeather[1].GetWeatherIcon());
        }

        public static string GetDay2HighTemp()
        {
            return "" + Math.Round(WeeksWeather[1].GetHighTemperature());
        }

        public static string GetDay2LowTemp()
        {
            return "" + Math.Round(WeeksWeather[1].GetLowTemperature());
        }

        public static string GetDay3Date()
        {
            return WeeksWeather[2].GetDate();
        }

        public static string GetDay3WeatherIcon()
        {
            return GetWeatherIconPath(WeeksWeather[2].GetWeatherIcon());
        }

        public static string GetDay3HighTemp()
        {
            return "" + Math.Round(WeeksWeather[2].GetHighTemperature());
        }

        public static string GetDay3LowTemp()
        {
            return "" + Math.Round(WeeksWeather[2].GetLowTemperature());
        }

        public static string GetDay4Date()
        {
            return WeeksWeather[3].GetDate();
        }

        public static string GetDay4WeatherIcon()
        {
            return GetWeatherIconPath(WeeksWeather[3].GetWeatherIcon());
        }

        public static string GetDay4HighTemp()
        {
            return "" + Math.Round(WeeksWeather[3].GetHighTemperature());
        }

        public static string GetDay4LowTemp()
        {
            return "" + Math.Round(WeeksWeather[3].GetLowTemperature());
        }

        public static string GetDay5Date()
        {
            return WeeksWeather[4].GetDate();
        }

        public static string GetDay5WeatherIcon()
        {
            return GetWeatherIconPath(WeeksWeather[4].GetWeatherIcon());
        }

        public static string GetDay5HighTemp()
        {
            return "" + Math.Round(WeeksWeather[4].GetHighTemperature());
        }

        public static string GetDay5LowTemp()
        {
            return "" + Math.Round(WeeksWeather[4].GetLowTemperature());
        }
    }
}