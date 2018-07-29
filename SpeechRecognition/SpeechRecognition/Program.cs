using SpeechRecognition.SpeechRecognitionAI;

namespace SpeechRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            AI ai = new AI();
            ai._Recognition.Start();
        }
    }
}
