#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows;
using System.Diagnostics;
using System.Globalization;

#endregion


namespace SpeechRecognition
{
    class JarvisDriver
    {
        #region locals

        /// <summary>
        /// the engine
        /// </summary>
        SpeechRecognitionEngine speechRecognitionEngine = null;

        /// <summary>
        /// The speech synthesizer
        /// </summary>
        SpeechSynthesizer speechSynthesizer = null;

        /// <summary>
        /// list of predefined commands
        /// </summary>
        List<Word> words = new List<Word>();

        /// <summary>
        /// The last command
        /// </summary>
        string lastCommand = "";

        /// <summary>
        /// The name to call commands
        /// </summary>
        static string aiName = "Jarvis";

        /// <summary>
        /// Switch for mute mode
        /// </summary>
        bool muteMode = false;

        /// <summary>
        /// switch to signify mute mode
        /// </summary>
        static bool completed = false;

        string disableAudioCommand = aiName + " mute";

        string enableAudioCommand = "unmute";

        string culture = "en-US";

        #endregion

        #region ctor

        public void Start()
        {
            try {
                initializeSpeechRecognitionEngine(culture);

                //Create the speech synthesizer
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.Rate = -1;

            }
            catch (Exception ex) {
                Console.WriteLine("Voice recognition failed " + ex.Message);
            }

            try {
                //force load jarvis into keyboard command entry mode (for developement and testing)
                disableAudioInput();
                speechRecognitionEngine.EmulateRecognize(disableAudioCommand, CompareOptions.IgnoreCase);

                //Keeps the command prompt going until you say jarvis quit
                while (lastCommand.ToLower() != "quit") {

                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// forces input into and out of keyboard entry
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool keyboardEntryMode(SpeechRecognizedEventArgs e)
        {
            bool isUnmuted = false;
            if (e.Result.Text == disableAudioCommand) {
                if (!muteMode) {
                    disableAudioInput();
                }
                if (muteMode) {
                    string typedCommand = "";
                    while (!typedCommand.ToLower().Equals(enableAudioCommand)) {
                        Console.WriteLine("-");
                        typedCommand = Console.ReadLine();
                        if (typedCommand.ToLower().Equals("quit")) {
                            Environment.Exit(0);
                        } else if (!typedCommand.ToLower().Equals(enableAudioCommand)) {
                            speechRecognitionEngine.EmulateRecognize(typedCommand, CompareOptions.IgnoreCase);
                        }
                    }
                    enableAudioInput();
                }
                isUnmuted = true;
            }
            return isUnmuted;
        }

        #endregion

        #region internal functions and methods

        /// <summary>
        /// Initializes the speech engine
        /// </summary>
        /// <param name="preferredCulture"></param>
        private void initializeSpeechRecognitionEngine(string preferredCulture)
        {
            // create the engine
            speechRecognitionEngine = createSpeechEngine(preferredCulture);

            // hook to event
            speechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(engine_SpeechRecognized);
            speechRecognitionEngine.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(RecognizeCompletedHandler);

            // load dictionary
            loadGrammarAndCommands();

            // use the system's default microphone
            speechRecognitionEngine.SetInputToDefaultAudioDevice();

            // start listening
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        /// <summary>
        /// Creates the speech engine.
        /// </summary>
        /// <param name="preferredCulture">The preferred culture.</param>
        /// <returns></returns>
        private SpeechRecognitionEngine createSpeechEngine(string preferredCulture)
        {
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers()) {
                if (config.Culture.ToString() == preferredCulture) {
                    speechRecognitionEngine = new SpeechRecognitionEngine(config);
                    break;
                }
            }

            // if the desired culture is not found, then load default
            if (speechRecognitionEngine == null) {
                Console.WriteLine("The desired culture is not installed on this machine, the speech-engine will continue using "
                    + SpeechRecognitionEngine.InstalledRecognizers()[0].Culture.ToString() + " as the default culture.",
                    "Culture " + preferredCulture + " not found!");
                speechRecognitionEngine = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);
            }

            return speechRecognitionEngine;
        }

        /// <summary>
        /// Loads the grammar and commands.
        /// </summary>
        private void loadGrammarAndCommands()
        {
            try {
                Choices texts = new Choices();
                texts.Add(aiName);
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\example.txt");
                foreach (string line in lines) {
                    // skip commentblocks and empty lines..
                    if (line.StartsWith("--") || line == String.Empty) continue;

                    // split the line
                    var parts = line.Split(new char[] { '|' });

                    // add commandItem to the list for later lookup or execution
                    words.Add(new Word() { Text = parts[0], AttachedText = parts[1], IsShellCommand = (parts[2] == "true") });

                    // add the text to the known choices of speechengine
                    texts.Add(parts[0]);
                }
                Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                speechRecognitionEngine.LoadGrammar(wordsList);

                DictationGrammar dict = new DictationGrammar();
                speechRecognitionEngine.LoadGrammar(dict);

            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the known command.
        /// </summary>
        /// <param name="command">The order.</param>
        /// <returns></returns>
        private string getKnownTextOrExecute(string command)
        {
            if (!command.StartsWith(aiName + " ")) {
                return "";
            } else {
                command = command.Replace(aiName + " ", "");

                //Console.WriteLine(command);
                try {
                    var cmd = words.Where(c => c.Text == command).First();

                    if (cmd.IsShellCommand) {
                        Process proc = new Process();
                        proc.EnableRaisingEvents = false;
                        proc.StartInfo.FileName = cmd.AttachedText;
                        proc.Start();
                        lastCommand = command;

                        if (command.ToLower() == "i have a burn victim") {
                            return "Fetching list of burn centers for you sir";
                        } else {
                            return "I've started : " + command;
                        }
                    } else {
                        lastCommand = command;
                        return cmd.AttachedText;
                    }
                }
                catch (Exception) {
                    lastCommand = command;
                    return command;
                }
            }
        }

        /// <summary>
        /// Handle the RecognizeCompleted event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void RecognizeCompletedHandler(object sender, RecognizeCompletedEventArgs e)
        {
            Console.WriteLine(" In RecognizeCompletedHandler.");

            if (e.Error != null) {
                Console.WriteLine(" - Error occurred during recognition: {0}", e.Error);
                return;
            }
            if (e.Cancelled) {
                Console.WriteLine(" - asynchronous operation canceled.");
            }
            if (e.InitialSilenceTimeout || e.BabbleTimeout) {
                Console.WriteLine(" - BabbleTimeout = {0}; InitialSilenceTimeout = {1}", e.BabbleTimeout, e.InitialSilenceTimeout);
                return;
            }
            if (e.InputStreamEnded) {
                Console.WriteLine(" - AudioPosition = {0}; InputStreamEnded = {1}", e.AudioPosition, e.InputStreamEnded);
            }
            if (e.Result != null) {
                Console.WriteLine(" - Grammar = {0}; Text = {1}; Confidence = {2}", e.Result.Grammar.Name, e.Result.Text, e.Result.Confidence);
            } else {
                Console.WriteLine(" - No result.");
            }

            completed = true;
        }

        /// <summary>
        /// Enables audio input
        /// </summary>
        private void enableAudioInput()
        {
            speechRecognitionEngine.EmulateRecognize(aiName + " Enabling voice input");
            initializeSpeechRecognitionEngine(culture);
            muteMode = false;
            lastCommand = "";
            Console.WriteLine("Voice input enabled");
        }

        /// <summary>
        /// Disables audio input
        /// </summary>
        private void disableAudioInput()
        {
            speechRecognitionEngine.RecognizeAsyncCancel();
            // Wait for the operation to complete.
            while (!completed) {
                System.Threading.Thread.Sleep(333);
            }
            speechRecognitionEngine.SetInputToNull();
            muteMode = true;
            speechRecognitionEngine.EmulateRecognize(aiName + " Voice input disabled");
            Console.WriteLine("Voice input disabled");
        }
        
        #endregion

        #region speechEngine events

        /// <summary>
        /// Handles the SpeechRecognized event of the engine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the event data.</param>
        void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Recognized: " + e.Result.Text);
            string command = "";
            if (!keyboardEntryMode(e)) {
                command = getKnownTextOrExecute(e.Result.Text);
            }


            if (command != "") {
                Console.WriteLine("Command: " + command);
                speechSynthesizer.SpeakAsync(command);
            }
        }

        #endregion

    }
}
