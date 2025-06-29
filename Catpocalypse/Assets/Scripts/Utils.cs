using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public static class Utils
{
    /// <summary>
    /// This function goes through the passed in string one char at a time and inserts spaces in front of every
    /// capital letter except the first one. It is used to add spaces to the names of enum values before displaying
    /// the value on-screen.
    /// </summary>
    /// <param name="input">The string to add spaces into.</param>
    /// <returns>The string with spaces added before every capital letter except the first.</returns>
    public static string InsertSpacesBeforeCapitalLetters(string input)
    {
        string output = "";

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (i > 0 && char.IsUpper(c))
                output += " ";

            output += c;
        }


        return output;
    }

}
