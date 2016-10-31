using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.SpeechRecognition;
using Windows.Storage;

namespace SmartMirror
{
    //Static Singleton class to handle all voice controle
    public class VoiceController
    {
        //------------------------------------------------------------------------------------------------------------
        //---------------------------------------Voice Recognition Constants------------------------------------------
        //------------------------------------------------------------------------------------------------------------
        //Grammar File
        private const string SRGS_FILE = "Grammar\\grammar.xml";

        // Tag TARGET
        private const string TAG_TARGET = "target";
        // Tag CMD
        private const string TAG_CMD = "cmd";
        // Tag Device
        private const string TAG_TIME_FRAME = "timeFrame";
        // On Command
        private const string COMMAND_ON = "ON";
        // Off Command
        private const string COMMAND_OFF = "OFF";
        // Show Command
        private const string COMMAND_SHOW = "SHOW";
        // Show Command
        private const string COMMAND_HIDE = "HIDE";
        // Weather Target
        private const string TARGET_WEATHER = "WEATHER";
        // Todays Forecast
        public const string WEATHER_TODAY = "TODAY";
        // Weeks Forecast
        public const string WEATHER_WEEKS = "WEEK";
        // Tomorrows Forecast
        public const string WEATHER_TOMORROW = "TOMORROW";


        //Speech Recognizer
        private static SpeechRecognizer _recognizer;
        
        //Called when the program ends, stops the speech rocognizer
        public static async void UnloadSpeechRecognizer()
        {
            //Stop voice recognition
            await _recognizer.ContinuousRecognitionSession.StopAsync();
        }

        public static async void InitializeSpeechRecognizer()
        {
            // Initialize recognizer
            _recognizer = new SpeechRecognizer();

            // Set event handlers
            _recognizer.StateChanged += RecognizerStateChanged;
            _recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;

            // Load Grammar file
            string fileName = String.Format(SRGS_FILE);
            StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);

            //Create grammar constraint
            SpeechRecognitionGrammarFileConstraint grammarConstraint =
                new SpeechRecognitionGrammarFileConstraint(grammarContentFile);

            // Add to grammar constraint
            _recognizer.Constraints.Add(grammarConstraint);

            // Compile grammar
            SpeechRecognitionCompilationResult compilationResult = await _recognizer.CompileConstraintsAsync();

            Debug.WriteLine("Status: " + compilationResult.Status.ToString());

            // If successful, display the recognition result.
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("Result: " + compilationResult.ToString());

                await _recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                Debug.WriteLine("Status: " + compilationResult.Status);
            }
        }

        //Handle appropriate voice commands
        private static void RecognizerResultGenerated(SpeechContinuousRecognitionSession session,
                                                      SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // Output debug strings
            Debug.WriteLine(args.Result.Status);
            Debug.WriteLine(args.Result.Text);

            Debug.WriteLine(args.Result.Constraint.Tag);            

            int count = args.Result.SemanticInterpretation.Properties.Count;
            Debug.WriteLine("Count: " + count);
            Debug.WriteLine("Tag: " + args.Result.Constraint.Tag);

            // Check for different tags and set variables for logic based on commands
            String target = args.Result.SemanticInterpretation.Properties.ContainsKey(TAG_TARGET)
                ? args.Result.SemanticInterpretation.Properties[TAG_TARGET][0].ToString()
                : "";

            String cmd = args.Result.SemanticInterpretation.Properties.ContainsKey(TAG_CMD)
                ? args.Result.SemanticInterpretation.Properties[TAG_CMD][0].ToString()
                : "";

            String timeFrame = args.Result.SemanticInterpretation.Properties.ContainsKey(TAG_TIME_FRAME)
                ? args.Result.SemanticInterpretation.Properties[TAG_TIME_FRAME][0].ToString()
                : "";

            //Logic based on what commands were said.
            //Turn Mirror On
            if (cmd.Equals(COMMAND_ON))
            {
                MirrorState.SetMirrorOn(true);
            }
            //Turn Mirror Off
            else if (cmd.Equals(COMMAND_OFF))
            {
                MirrorState.SetMirrorOn(false);
            }
            //Show Weather Element specified
            else if(cmd.Equals(COMMAND_SHOW))
            {
                if (target.Equals(TARGET_WEATHER))
                {
                    ShowWeatherDisplay(timeFrame);
                }
                else
                {
                    Debug.WriteLine("Invalid Target for 'Show' Command");
                }
            }
            //Hide weather element specified
            else if (cmd.Equals(COMMAND_HIDE))
            {
                if (target.Equals(TARGET_WEATHER))
                {
                    HideWeatherDisplay(timeFrame);
                }
                else
                {
                    Debug.WriteLine("Invalid Target for 'Show' Command");
                }
            }
            else
            {
                Debug.WriteLine("Unknown Voice Command");
            }
        }

        // Debug changes to state of the recognizer for test purposes
        private static void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("Speech recognizer state: " + args.State.ToString());
        }

        //Logic to show the appropriate weather element when a voice command is given
        private static void ShowWeatherDisplay(string timeFrame)
        {
            if (timeFrame.Equals(WEATHER_WEEKS))
                MirrorState.SetWeeksWeatherOn(true);
            else if (timeFrame.Equals(WEATHER_TODAY))
            {
                MirrorState.SetWeatherOn(true);
                MirrorState.SetMainWeatherInfo(MirrorState.MAIN_WTHR_TODAY);
            }
            else if (timeFrame.Equals(WEATHER_TOMORROW))
            {
                MirrorState.SetWeatherOn(true);
                MirrorState.SetMainWeatherInfo(MirrorState.MAIN_WTHR_TMRW);
            }
            else
            {
                Debug.WriteLine("Unknown Time Frame for Show Weather command.");
            }
        }

        //Logic to hide the appropriate weather element when a voice command is given
        private static void HideWeatherDisplay(string timeFrame)
        {
            if (timeFrame.Equals(WEATHER_WEEKS))
                MirrorState.SetWeeksWeatherOn(false);
            else if (timeFrame.Equals(WEATHER_TODAY))
            {
                MirrorState.SetWeatherOn(true);
            }
            else if (timeFrame.Equals(WEATHER_TOMORROW))
            {
                MirrorState.SetMainWeatherInfo(MirrorState.MAIN_WTHR_TODAY);
            }
            else
            {
                Debug.WriteLine("Unknown Time Frame for Show Weather command.");
            }
        }
    }
}