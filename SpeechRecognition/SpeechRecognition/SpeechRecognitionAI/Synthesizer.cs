using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition.SpeechRecognitionAI
{
    public class Synthesizer
    {
        public SpeechSynthesizer SpeechSynthesizer { get; internal set; }

        public int SynthesizationRate {
            get {
                return SpeechSynthesizer.Rate;
            }
            set {
                SpeechSynthesizer.Rate = value;
            }
        }

        public Synthesizer(int SynthesizationRate)
        {
            SpeechSynthesizer = new SpeechSynthesizer();
            this.SynthesizationRate = SynthesizationRate;
        }
    }
}
