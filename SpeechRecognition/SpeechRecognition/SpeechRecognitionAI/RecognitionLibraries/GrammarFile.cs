using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition.SpeechRecognitionAI.RecognitionLibraries
{
    public class GrammarFile
    {
        //todo clean and add more control for grammer/command loading  

        /// <summary>
        /// list of predefined commands
        /// </summary>
        public List<Word> words = new List<Word>();

        /// <summary>
        /// Loads the grammar and commands.
        /// </summary>
        public void loadGrammarAndCommands(AI AI)
        {
            try
            {
                Choices texts = new Choices();
                texts.Add(AI.Name);
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\AICommands.txt");
                foreach (string line in lines)
                {
                    // skip commentblocks and empty lines..
                    if (line.StartsWith("--") || line == String.Empty) continue;

                    // split the line
                    var parts = line.Split(new char[] { '|' });

                    // construct the word
                    Word word = new Word() { Text = parts[0], AttachedText = parts[1], IsShellCommand = (parts[2] == "true") };

                    if (parts.Length > 3)
                    {
                        word.AIResponse = parts[3];
                    }

                    // add commandItem to the list for later lookup or execution
                    words.Add(word);

                    // add the text to the known choices of speechengine
                    texts.Add(parts[0]);
                }
                Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                AI._Recognition._SpeechRecognitionEngine.LoadGrammar(wordsList);

                DictationGrammar dict = new DictationGrammar();
                AI._Recognition._SpeechRecognitionEngine.LoadGrammar(dict);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
