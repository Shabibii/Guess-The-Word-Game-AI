using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordFamilies
{
    public class Dictionary
    {
        public static Dictionary DictionaryInstance;
        public string[] words; // Array of words in dictionary
              

        public Random random; // Random generator

        // Get word of specified length.
        public List<string> GetWordsOfLength(int length)
        {
            return words.Where(word => word.Length == length).ToList();
        }

        // Load words into dictionary.
        public static void LoadWords()
        {
            DictionaryInstance = new Dictionary();
        }

        // Dictionary object.
        public Dictionary()
        {
            // Read 'dictionary.txt' (file) line by line and store each line in an array 'words'. 
            words = System.IO.File.ReadAllLines(@"dictionary.txt");
            random = new Random(); // Create random generator
        }
    }
}
