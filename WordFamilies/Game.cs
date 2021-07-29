using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace WordFamilies
{
    public class Game
    {
        //  variable list

        private int userWordLength; // Hidden word length

        private int guessesMade; // Guessed already made (correct & wrong)
        private int numberOfGuesses; // Total number of guesses

        private ConsoleKey showRemainingWords; // Show remainder? returns: Y or N
        private ConsoleKey playAgain; // Play again? Y or N

        private char input; // Character guess input
        private string hiddenWord; // Hidden word

  
        private List<string> WordList; // array of words after obtaining word length  

        private readonly List<char> correctLetters = new List<char>(); // List of correct letters
        private readonly List<char> wrongLetters = new List<char>(); // List of wrong letters

        private Dictionary Dictionary => Dictionary.DictionaryInstance; // Link to the DicitionaryInstance
        private readonly Dictionary<char, int> correctCharFrequency = new Dictionary<char, int>(); // Character frequency

        /// <summary>
        /// Consider character position when looking for other possible words that could replace the current hidden word.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="wordMask"></param>
        /// <param name="previousGuess"></param>
        /// <returns></returns>
        public bool IsSuitableWord(string word, string wordMask, char previousGuess = ' ')
        {
            bool isMatch = true;

            // Make sure words are the same length
            if (word.Length != wordMask.Length)
                return false;

            // Check character position matches
            for (int i = 0; i < wordMask.Length; i++)
            {
                if (wordMask[i] == '-')
                {
                    if (word[i] == previousGuess)
                        isMatch &= true;
                }
                else
                {
                    isMatch &= wordMask[i] == word[i];
                }
            }

            // Check to see if the character frequencies are the same
            for (int i = 0; i < correctLetters.Count; i++)
            {
                isMatch &= correctCharFrequency[correctLetters[i]] 
                    == word.Count(chr => chr == correctLetters[i]);
            }

            // Make sure it does not contain wrong words
            foreach (var c in word)
            {
                isMatch &= !wrongLetters.Contains(c);
            }

            return isMatch;
        }

        /// <summary>
        /// Keep track of word and its guessed letters.
        /// For showing word progress to player 2.
        /// </summary>
        /// <returns></returns>
        string GetWordProgress()
        {
            // -----
            string wordProgress = "";

            for (int i = 0; i < hiddenWord.Length; i++) // Put same amount dashes as word length
            {
                if (correctLetters.Contains(hiddenWord[i])) // If letter is guessed correct
                {
                    wordProgress += hiddenWord[i].ToString(); // Add letter to its position in the word 
                }
                else // If no correct letter
                {
                    wordProgress += "-"; // Keep dashes
                }
            }
            return wordProgress;
        }


        /// <summary>
        /// Method which starts the game and decides when to finish.
        /// </summary>
        public void StartGame()
        {
            Console.Clear();

            // Method: print the game's welcome message & list of rules.
            DisplayWelcomeMessage();

            // Method: request user for hidden word's word length, returns integer.
            GetWordLength();

            // Method: request user's choice for the number of guesses, returns integer.
            GetNumberOfGuesses();

            // Method: request user if remainder of possible words should be shown, returns character ('Y' or 'N').
            ShowRemainderOfPossibleWords();

            // Clear console for guessing window.
            Console.Clear();

            // Stay interactive by showing loading screen.
            ShowLoading();

            // Get the initial word from the first list.
            GetInitialWord();

            // For as long as there are guesses & word is not guessed, keep playing game.
            for (guessesMade = 0; guessesMade < numberOfGuesses && GetWordProgress().Contains("-");)
            {
                PlayGame();
            }

            // Word still not guessed after all guesses are used.
            if (GetWordProgress().Contains("-"))
            {
                Console.WriteLine("\n\nToo bad.. You ran out of guesses.");
                Console.WriteLine("The word was: {0}", hiddenWord); // Present hidden word
            }
            else // User has guessed the word, congratulate appropriately.
            {
                Console.WriteLine("\nWOW, YOU GUESSED THE WORD: {0}! ", hiddenWord);
                Console.WriteLine("Congratulations, someone with your intellect will always be welcome in our world.");
            }
            
            PlayAgain();           
        }

        /// <summary>
        /// Ask if user wants to play again, if so, then play again.
        /// </summary>
        private void PlayAgain()
        {
            // Ask user if he wants to play again           
            do
            {
                // Prompt user for displaying remaining possible words.
                Console.WriteLine("\nPlay again? (Y) or (N):  ");
                playAgain = Console.ReadKey(false).Key; // true is intercept key (don't show), false is show.
                if (playAgain != ConsoleKey.Enter)
                {
                    Console.ReadLine();                    
                }
            } while (playAgain != ConsoleKey.Y && playAgain != ConsoleKey.N); // While no correct letter (y or n) is entered.

            if (playAgain == ConsoleKey.Y)
            {
                // Clear data.
                Console.Clear();
                wrongLetters.Clear();
                correctLetters.Clear();
                correctCharFrequency.Clear();
                guessesMade = 0;
                StartGame(); // Start game again
            }
        }

        /// <summary>
        /// Method containing the welcome message and game rules. 
        /// This is the introduction to the game.
        /// </summary>
        private void DisplayWelcomeMessage()
        {
            Console.WriteLine("WELCOME TO THE WORLD OF WORD FAMILIES");
            Console.WriteLine("\n\nGame Rules:");
            Console.WriteLine("1. Only valid inputs accepted, the game will feedback appropriately.");
            Console.WriteLine("2. The number of guesses can only be an integer in the range of 2 and 26.");
            Console.WriteLine("3. There are no words with 26 or 27 letters.");
            Console.WriteLine("4. Last, but not least: Play at your own risk!");
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();

            Console.Clear();
        }

        /// <summary>
        /// Method which gets the word length for the hidden word, specified by the user.
        /// </summary>
        private void GetWordLength()
        {
            // Get letter count for smallest and biggest word in 'words' array.
            var minWordCount = Dictionary.words.Min(w => w.Length);
            var maxWordCount = Dictionary.words.Max(w => w.Length);

            // Prompt user for his choice of the 'secret' word's length.
            Console.WriteLine("How many letters should the secret word contain?  ");
            string userWordLengthString = Console.ReadLine();

            // Check if input from user is numerical, if not numerical, keep asking user for numerical value.
            while (!int.TryParse(userWordLengthString, out userWordLength))
            {
                Console.WriteLine("\nERROR - No numerical input, try again: ");
                userWordLengthString = Console.ReadLine();
            }

            // Check if input from user is an integer matching the character count of at least one word in the dictionary.
            while (userWordLength < minWordCount || userWordLength > maxWordCount || userWordLength == 26 || userWordLength == 27)
            {
                Console.WriteLine("\nERROR - No words with this amount of letters, try again: ");
                userWordLength = Convert.ToInt32(Console.ReadLine());
            }
        }

        /// <summary>
        /// Method which gets the number of guesses, specified by the user.
        /// </summary>
        private void GetNumberOfGuesses()
        {
            // Prompt user for the number of guesses he/she would like to have.
            Console.WriteLine("\nEnter number of guesses (Range: 2 - Legendary & 14 - Beginner): ");
            numberOfGuesses = Convert.ToInt32(Console.ReadLine());

            // Check if user input is valid (value above zero).
            while (numberOfGuesses < 2 || numberOfGuesses > 14)
            {
                Console.WriteLine("\nERROR - Enter an integer between 2 and 14: ");
                numberOfGuesses = Convert.ToInt32(Console.ReadLine());
            }
        }

        /// <summary>
        /// Method holding the answer to showing the remainder of possible words, yes or no.
        /// </summary>
        private void ShowRemainderOfPossibleWords()
        {
            // Ask user if he wants to see the possible amount of words remaining at each turn.            
            do
            {
                // Prompt user for displaying remaining possible words.
                Console.WriteLine("\nDisplay a running total of the number of words remaining in the word list? (Y) or (N):  ");
                showRemainingWords = Console.ReadKey(false).Key; // true is intercept key (don't show), false is show.
                if (showRemainingWords != ConsoleKey.Enter)
                {
                    Console.ReadLine();
                }
            } while (showRemainingWords != ConsoleKey.Y && showRemainingWords != ConsoleKey.N); // While no correct letter (y or n) is entered.           
        }

        /// <summary>
        /// Method showing a loading message
        /// </summary>
        private void ShowLoading()
        {
            int loadingLength = 7;
            Console.WriteLine("Your opponent is getting ready");
            for (int i = 0; i < loadingLength; i++)
                Console.Write(".");
            Thread.Sleep(500);

            Console.WriteLine("\nReady! Press any key to start");
            Console.ReadKey();

            Console.Clear();
        }

        /// <summary>
        /// Method which gets a random word from the first word list, we need a word to start the guesses!
        /// </summary>
        private void GetInitialWord()
        {
            WordList = Dictionary.GetWordsOfLength(userWordLength);
            hiddenWord = WordList[Dictionary.random.Next(0, WordList.Count)];
            WordList = WordList.Where(word => CalculateSimilarity(word, hiddenWord) > 30).ToList(); // Get first hidden word using normalized Levenshtein value.
        }

        /// <summary>
        /// Method when game is in progress, being played.
        /// </summary>
        private void PlayGame()
        {
            Console.Clear();
            if (numberOfGuesses - guessesMade > 1) // If still multiple guesses left
            {
                Console.WriteLine("You have {0} guesses left.", numberOfGuesses - guessesMade); // Show guesses left
            }
            else
            {
                Console.WriteLine("You have {0} guess left.", numberOfGuesses - guessesMade); // If single guess left
            }

            Console.WriteLine("\nWrong letters: {0}", string.Join(",", wrongLetters)); // Add to wrong letters list
            Console.WriteLine("\nCorrect letters: {0}", string.Join(",", correctLetters)); // Add to correct letters list

            if (showRemainingWords == ConsoleKey.Y) // If user specified to show remainder of possible words, then show.
            {
                Console.WriteLine("\nCount current word list: {0}", WordList.Count + 1); // Show remainder
            }

            Console.WriteLine("\n_________________________________");
            Console.WriteLine("\n\nThe secret word: {0}", GetWordProgress()); // Show word progress in ongoing game
            Console.Write("\nEnter new guess: "); // Request new guess
            input = Console.ReadKey().KeyChar;
            Console.ReadLine();
          
            if (hiddenWord.Contains(input.ToString()) && !correctLetters.Contains(input))
            {
                // Call method so computer cheats, artificial intelligence implementation.
                PerformMiniMax();
            }

            // Inspect input (guess) from user.
            if (char.IsLetter(input)) // Accept when only one character is given as input.
            {
                if (!correctLetters.Contains(input) && hiddenWord.Contains(input.ToString())) // If correct letter is guessed
                {
                    // New correct guess, add to correct letters.
                    correctLetters.Add(input);
                    correctCharFrequency.Add(input, hiddenWord.ToCharArray().Count(x => x == input));
                    // Create a new word family with words where the guessed letter(s) are positioned at the same location.
                    //firstWordList = firstWordList.Where(x => Comp/areCharacterPosition(x) && CalculateSimilarity(x,hiddenWord) > 55).ToList();
                }
                else if (correctLetters.Contains(input) || wrongLetters.Contains(input))  // Else if, check if letter is already guessed
                {
                    // User already guessed this.
                    Console.WriteLine("\nERROR - You already guessed this..");
                    Console.WriteLine("\nYou still have {0} guesses left.", numberOfGuesses - guessesMade); // No guesses lost
                    Console.ReadKey();
                }
                else if (!wrongLetters.Contains(input)) // Else if, wrong guess is given
                {
                    // New Wrong guess, add to wrong letters.
                    wrongLetters.Add(input);
                    guessesMade++; // Lose a guess
                }
            }
            else // Else, no character as input, ask user again.
            {
                // Ask user again
                Console.WriteLine("\nERROR - No valid input, enter a letter from the alphabet.");
                Console.ReadLine();
                PlayGame();
            }
            // Console.Title = hiddenWord; // Show hidden word in title for testing
        }
       
        /// <summary>
        /// Perform modifiied minimax algorithm to get the optimal next step of player 1 (cheat).
        /// </summary>
        private void PerformMiniMax()
        {
            var wordFamilies = GetWordFamilies();
            string wordMask = GetWordProgress();

            // Get Largest word family.
            var largestWordFamily = wordFamilies[wordFamilies.Keys.Max()];

            // Get least probable input by finding word that will reduce the word list the most.
            SortedList<int, IEnumerable<string>> subWordFamilies = new SortedList<int, IEnumerable<string>>();
            foreach (var item in largestWordFamily)
            {
                var newWordFamily = WordList.Where(word => IsSuitableWord(word, wordMask));
                int c = newWordFamily.Count();

                // Ignore adding multiple families with the same word length.
                if(!subWordFamilies.Keys.Contains(c))
                    subWordFamilies.Add(newWordFamily.Count(), newWordFamily);
            }

            WordList = subWordFamilies[wordFamilies.Keys.Min()].ToList();
            hiddenWord = WordList[Dictionary.random.Next(0, WordList.Count)];
        }

        /// <summary>
        /// Gets letter possibilities for words that can replace hidden word.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<char> GetUnguessedChars()
        {
            string currentWordMask = GetWordProgress();
            for (int i = 0; i < currentWordMask.Length; i++)
            {
                if (currentWordMask[i] == '-')
                    yield return hiddenWord[i]; // Return one by one
            }
        }

        /// <summary>
        /// Create word families based on remaining possible words (consider character position) to replace hidden word.
        /// </summary>
        /// <returns></returns>
        private SortedList<int, IEnumerable<string>> GetWordFamilies()
        {
            // Create possible future word masks
            SortedList<int,IEnumerable<string>> wordFamilies = new SortedList<int, IEnumerable<string>>();

            string wordMask = GetWordProgress();
            foreach (var c in GetUnguessedChars())
            {
                // Get word families after every potential guess
                var newWordFamily = WordList.Where(word => IsSuitableWord(word, wordMask, c));

                int count = newWordFamily.Count();
                if(!wordFamilies.ContainsKey(count))
                    wordFamilies.Add(newWordFamily.Count(),newWordFamily);
            }
            return wordFamilies;
        }

        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        /// <summary>
        /// Calculate percentage similarity of two strings.
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length))) * 100;
        }
    }
}
