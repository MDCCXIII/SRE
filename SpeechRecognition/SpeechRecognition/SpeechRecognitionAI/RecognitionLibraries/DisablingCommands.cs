using System.Collections.Generic;
using System.Linq;

namespace SpeechRecognition.SpeechRecognitionAI.RecognitionLibraries
{
    public class DisablingCommands : List<string>, IList<string>
    {
        /// <summary>
        /// Adds a command for disabling speech recognition to the end of the <see cref="List{T}"/>
        /// </summary>
        public new void Add(string command)
        {
            if (!Contains(command))
                base.Add(command);
        }

        /// <summary>
        /// Adds a collection of commands for disabling speech recognition to the end of the <see cref="List{T}"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="commands"></param>
        public new void AddRange(IEnumerable<string> commands)
        {
            base.AddRange(commands.Where(x => !Contains(x)));
        }
    }
}
