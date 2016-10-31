namespace SmartMirror
{
    //This class is a State Controller for the display of the mirror.
    //It has variables, and getters and setters for each possible
    //display element.
    public class MirrorState
    {
        private static bool _mirrorOn;
        private static bool _weatherOn;
        private static bool _weeksWeatherOn;
        private static string _mainWeatherInfo;
        private static bool _connectedToInternet;

        public const string MAIN_WTHR_TODAY = "TODAY";
        public const string MAIN_WTHR_TMRW = "TOMORROW";

        //This is a static singleton class used to
        //contain the state of the mirror display elements,
        //We will not 
        protected MirrorState()
        { }

        public static void InitialMirrorState()
        {
            //Set up the default config of the mirror at startup
            //Mirror on
            MirrorState.SetMirrorOn(true);
            //Main weather display on
            MirrorState.SetWeatherOn(true);
            //Main weather set to today
            MirrorState.SetMainWeatherInfo(MirrorState.MAIN_WTHR_TODAY);
            //Weeks weather forecast off
            MirrorState.SetWeeksWeatherOn(false);
        }

        public static void SetMirrorOn(bool mirrorOn)
        {
            _mirrorOn = mirrorOn;
        }

        public static bool IsMirrorOn()
        {
            return _mirrorOn;
        }

        public static void SetWeatherOn(bool weatherOn)
        {
            _weatherOn = weatherOn;
        }

        public static bool IsWeatherOn()
        {
            return _weatherOn;
        }

        public static void SetWeeksWeatherOn(bool weeksWeatherOn)
        {
            _weeksWeatherOn = weeksWeatherOn;
        }

        public static bool IsWeeksWeatherOn()
        {
            return _weeksWeatherOn;
        }

        public static void SetMainWeatherInfo(string mainWeatherInfo)
        {
            _mainWeatherInfo = mainWeatherInfo;
        }

        public static string GetMainWeatherInfo()
        {
            return _mainWeatherInfo;
        }

        public static void SetConnectedToInternet(bool connected)
        {
            _connectedToInternet = connected;
        }

        public static bool IsConnectedToInternet()
        {
            return _connectedToInternet;
        }
    }
}