using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;

namespace SpeechRecognition.SpeechRecognitionAI.RecognitionLibraries
{
    public class Recognition
    {
        public SpeechRecognitionEngine _SpeechRecognitionEngine { get; internal set; }
        public Locals Locals { get; set; } = new Locals();
        public GrammarFile _Grammar { get; set; } = new GrammarFile();
        public bool IsMuted { get; private set; }
        public AI AI { get; set; }
        public string LastCommand { get; private set; } = "";

        public Recognition(AI AI, string DisableSpeechMonitoringCommand)
        {
            this.AI = AI;
            Locals.DisablingCommands.Add(DisableSpeechMonitoringCommand);
            _SpeechRecognitionEngine = new SpeechRecognitionEngine(CultureInfo.CurrentCulture);
            _SpeechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Engine_SpeechRecognized);
            _SpeechRecognitionEngine.SetInputToDefaultAudioDevice();
        }

        public Recognition(AI AI, string[] DisableSpeechMonitoringCommands)
        {
            this.AI = AI;
            Locals.DisablingCommands.AddRange(DisableSpeechMonitoringCommands);
            _SpeechRecognitionEngine = new SpeechRecognitionEngine(CultureInfo.CurrentCulture);
            _SpeechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Engine_SpeechRecognized);
            _SpeechRecognitionEngine.SetInputToDefaultAudioDevice();
        }

        public void Start()
        {
            //loadGrammer
            _Grammar.loadGrammarAndCommands(AI);
            
            // start listening
            _SpeechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            //
            DisableAudioInput();
            _SpeechRecognitionEngine.EmulateRecognize(Locals.DisablingCommands[0], CompareOptions.None);



            //KeepAlive
            while (LastCommand.ToLower() != "quit")
            {

            }
        }

        /// <summary>
        /// Disables audio input
        /// </summary>
        public void DisableAudioInput()
        {
            _SpeechRecognitionEngine.RecognizeAsyncStop();
            IsMuted = true;
            if (AI._Synthesizer.SpeechSynthesizer != null)
                AI._Synthesizer.SpeechSynthesizer.SpeakAsync("Voice input disabled");
            _SpeechRecognitionEngine.RecognizeAsyncCancel();
            Console.WriteLine("Keyboard entry mode - Awaiting input.");
            
            //"Voice input disabled";
        }

        /// <summary>
        /// Enables audio input
        /// </summary>
        public void EnableAudioInput()
        {
            _SpeechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            IsMuted = false;
            if (AI._Synthesizer.SpeechSynthesizer != null)
                AI._Synthesizer.SpeechSynthesizer.SpeakAsync("Voice input enabled");
            LastCommand = "";
            //"Voice input enabled";
        }



        /// <summary>
        /// Handles the SpeechRecognized event of the engine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the event data.</param>
        void Engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //TEST:
            LastCommand = e.Result.Text;
            Console.WriteLine(LastCommand);
            //TODO:
            string command = "";
            if (!keyboardEntryMode(e))
            {
                command = getKnownTextOrExecute(e.Result.Text);
            }
            //if (command != "")
            //{
            //    Console.WriteLine("Command: " + command);
            //}
        }

        /// <summary>
        /// forces input into and out of keyboard entry
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool keyboardEntryMode(SpeechRecognizedEventArgs e)
        {
            string enableAudioCommand = "?";
            if (Locals.DisablingCommands.Contains(e.Result.Text.ToLower()))
            {
                if (!IsMuted)
                {
                    DisableAudioInput();
                    _SpeechRecognitionEngine.EmulateRecognize("Keyboard input enabled.", CompareOptions.IgnoreCase);
                }
                if (IsMuted)
                {
                    string typedCommand = "";
                    while (!typedCommand.ToLower().Equals(enableAudioCommand))
                    {
                        Console.WriteLine("-");
                        typedCommand = Console.ReadLine();
                        if (typedCommand.ToLower().Equals("quit"))
                        {
                            Environment.Exit(0);
                        }
                        else if (!typedCommand.ToLower().Equals(enableAudioCommand))
                        {
                            _SpeechRecognitionEngine.EmulateRecognize(typedCommand, CompareOptions.IgnoreCase);
                        }
                    }
                    EnableAudioInput();
                }
            }
            return !IsMuted;
        }

        /// <summary>
        /// Gets the known command.
        /// </summary>
        /// <param name="command">The order.</param>
        /// <returns></returns>
        private string getKnownTextOrExecute(string command)
        {
            if (command.ToLower().Contains(AI.Name.ToLower()))
            {
                command = command.Replace(AI.Name, "").Trim();
            }
            return ProcessCommand(command);
        }

        private string ProcessCommand(string command)
        {
            string response;
            try
            {
                var cmd = _Grammar.words.Where(c => c.Text.ToLower() == command.ToLower()).First();

                if (cmd.IsShellCommand)
                {
                    Process proc = new Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.FileName = cmd.AttachedText;
                    proc.Start();
                    LastCommand = command;

                    if (cmd.AIResponse != null && !cmd.AIResponse.Equals(""))
                    {
                        response = cmd.AIResponse;
                    }
                    else
                    {
                        response = "I've started : " + command;
                    }
                }
                else
                {
                    LastCommand = command;
                    response = cmd.AttachedText;
                }
            }
            catch (Exception)
            {
                LastCommand = command;
                response = command;
            }

            return response;
        }

    }
}
