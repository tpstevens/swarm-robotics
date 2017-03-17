using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Utilities
{
    public class Words
    {
        public static readonly int MAX_PIXEL_LENGTH = 60;
        public static readonly int MAX_CHAR_LENGTH = 10;
        public static readonly int MAX_LINE_LENGTH = 5;

        private static Letter[] alphabet = new Letter[] {
            new LetterA(),
            new LetterB(),
            new LetterC(),
            new LetterD(),
            new LetterE(),
            new LetterF(),
            new LetterG(),
            new LetterH(),
            new LetterI(),
            new LetterJ(),
            new LetterK(),
            new LetterL(),
            new LetterM(),
            new LetterN(),
            new LetterO(),
            new LetterP(),
            new LetterQ(),
            new LetterR(),
            new LetterS(),
            new LetterT(),
            new LetterU(),
            new LetterV(),
            new LetterW(),
            new LetterX(),
            new LetterY(),
            new LetterZ()
        };

        public static List<Vector2> getBuildOrder(string str)
        {
            int letterIndex;
            List<Vector2> buildOrder = new List<Vector2>();
            str = str.Trim().ToUpper();

            string[] splitWords = str.Split(' ');
            List<string> lines = new List<string>();

            int currentLine = 0;

            if (splitWords.Length > 0 && splitWords[0].Length <= MAX_CHAR_LENGTH)
            {
                lines.Add(splitWords[0]);

                for (int i = 1; i < splitWords.Length; ++i)
                {
                    if (splitWords[i].Length > MAX_CHAR_LENGTH)
                    {
                        Log.a("WORDS", "Word \"" + splitWords[i] + "\" is too long");
                        continue;
                    }
                    else
                    {
                        if (splitWords[i].Length >= (MAX_CHAR_LENGTH - lines[currentLine].Length - 1) || splitWords[i][0] == '\n')
                        {
                            ++currentLine;
                            lines.Add(splitWords[i]);
                        }
                        else
                        {
                            lines[currentLine] += ' ' + splitWords[i];
                        }
                    }
                }
            }

            for (int i = 0; i < lines.Count; ++i)
            {
                Debug.Log(lines[i]);
            }

            for (int i = 0; i < lines.Count && i < MAX_LINE_LENGTH; ++i)
            {
                float pixelWidth = lines[i].Length * 5 + (lines[i].Length - 1);
                if (pixelWidth > MAX_PIXEL_LENGTH)
                {
                    return new List<Vector2>();
                }

                Vector2 startPosition = new Vector2(pixelWidth / -2.0f + 0.5f, -8 * (i) + 6);
                for (int layer = 0; layer < 7; ++layer)
                {
                    for (int c = 0; c < lines[i].Length; ++c)
                    {
                        letterIndex = getLetterIndex(lines[i][c]);
                        if (letterIndex != -1)
                        {
                            Vector2[] placements = alphabet[letterIndex].getRow(layer);
                            for (int p = 0; p < placements.Length; ++p)
                            {
                                buildOrder.Add(startPosition + new Vector2(c * 6, 0) + placements[p]);
                            }
                        }
                    }
                }
            }

            // Vector2 startPosition = new Vector2(pixelWidth / -2.0f + 0.5f, -7);
            // 
            // for (int layer = 0; layer < 7; ++layer)
            // {
            //     for (int c = 0; c < str.Length; ++c)
            //     {
            //         letterIndex = getLetterIndex(str[c]);
            //         if (letterIndex != -1)
            //         {
            //             Vector2[] placements = alphabet[letterIndex].getRow(layer);
            //             for (int i = 0; i < placements.Length; ++i)
            //             {
            //                 buildOrder.Add(startPosition + new Vector2(c * 6, 0) + placements[i]);
            //             }
            //         }
            //     }
            // }

            return buildOrder;
        }

        private static int getLetterIndex(char c)
        {
            int index = c - 65;

            if (index < 0 || index >= alphabet.Length)
                index = -1;

            return index;
        }
    }
}
