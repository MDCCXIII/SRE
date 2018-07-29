using SpeechRecognition.SpeechRecognitionAI.RecognitionLibraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition.SpeechRecognitionAI
{
    public class AI
    {
        public AI(string Name = "Jarvis", int synthesizationRate = -1, string disableSpeechMonitoringCommand = "ignore")
        {
            this.Name = Name;
            _Recognition = new Recognition(this, disableSpeechMonitoringCommand);
            _Synthesizer = new Synthesizer(synthesizationRate);
        }

        public AI(string Name = "Jarvis", int synthesizationRate = -1, params string[] disableSpeechMonitoringCommand)
        {
            this.Name = Name;
            _Recognition = new Recognition(this, disableSpeechMonitoringCommand);
            _Synthesizer = new Synthesizer(synthesizationRate);
        }

        public string Name { get; set; }
        public Recognition _Recognition { get; set; }
        public Synthesizer _Synthesizer { get; set; }
    }
}
