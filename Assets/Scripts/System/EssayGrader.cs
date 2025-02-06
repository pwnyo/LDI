using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EssayGrader
{
    public static bool isUp;
    public static string essay;

    //In order of priority
    public static bool essayPasses;

    public static EssayLength essayLength;

    public static bool hasNonos;
    public static bool hasRiskies;

    public static bool isOnTopic;

    public static bool hasSpaces;
    public static bool hasPeriods;
    public static bool hasCapitals;

    public static bool hasName;
    public static bool hasFullName;

    public enum EssayLength
    {
        VSHORT,
        SHORT,
        MEDIUM,
        LONG
    }

    static string[] nonos =
    {
        "dyke",
        "fag",
        "nigg",
        "tranny",
    };
    static string[] riskies = {
        "anal",
        "asshole",
        "bitch",
        "blowjob",
        "boner",
        "butthole",
        "clitoris",
        "cock",
        "cunt",
        "dick",
        "douche",
        "fuck",
        "penis",
        "pussy",
        "slut",
        "shit",
        "titty",
        "vagina",
        "whore",
    };
    static string[] topics =
    {
        "culture",
        "tv",
        "movie",
        "book",
        "novel",
        "blog",
        "video",
        "internet",
        "date",
        "dating",
        "relation",
        "sex",
        "friend",
        "partner",
        "marr",
        "husband",
        "wife",
        "love",
        "friend",
        "community",
    };

    const int vShortEssay = 100; //~min 10 words
    const int shortEssay = 250; //~min 30 words
    const int mediumEssay = 400; //~min 50 words
    const int minSpaces = 15;
    const int minPeriods = 5;
    const int minCapitals = 3;

    static EssayGrader()
    {
        essay = "";
        Reset();
    }

    public static string GetEssayFeedback(string input)
    {
        return "";
    }
    public static bool GradeEssay(string input)
    {
        essay = input;
        string essayLowercase = essay.ToLower();

        CheckLength();
        CheckWords(essayLowercase);
        CheckChars(input);
        Debug.Log("Length is " + essayLength.ToString() + " Has nonos? " + hasNonos + "Has riskies? " + hasRiskies +
            " On topic? " + isOnTopic + " Has spaces, periods, capitals, and name? " + hasSpaces + hasPeriods + hasCapitals + hasName);
        return ((essayLength == EssayLength.MEDIUM || essayLength == EssayLength.LONG) && !hasNonos && isOnTopic && hasSpaces && hasPeriods && hasCapitals && hasName);
    }
    public static void SaveEssay(string s)
    {
        essay = s;
    }
    static void CheckWords(string s)
    {
        hasName = s.Contains("eddy") || s.Contains("edward");
        hasFullName = s.Contains("eddy nguyen") || s.Contains("edward nguyen");
        hasNonos = false;
        hasRiskies = false;
        isOnTopic = false;

        foreach (string no in nonos)
        {
            if (s.Contains(no))
            {
                hasNonos = true;
                break;
            }
        }
        foreach (string risk in riskies)
        {
            if (s.Contains(risk))
            {
                hasRiskies = true;
                break;
            }
        }
        foreach (string topic in topics)
        {
            if (s.Contains(topic))
            {
                isOnTopic = true;
                break;
            }
        }
    }
    static void CheckChars(string s)
    {
        int countSpaces = 0, countPeriods = 0, countCapitals = 0;
        foreach (char c in s)
        {
            if (c.Equals(' '))
            {
                countSpaces++;
            }
            else if (c.Equals('.') || c.Equals('?') || c.Equals(','))
            {
                countPeriods++;
            }
            if (char.IsUpper(c))
            {
                countCapitals++;
            }
        }
        hasSpaces = countSpaces >= minSpaces;
        hasPeriods = countPeriods >= minPeriods;
        hasCapitals = countCapitals >= minCapitals;
    }
    static void CheckLength()
    {
        int length = essay.Length;
        if (length < vShortEssay)
        {
            essayLength = EssayLength.VSHORT;
        }
        else if (length < shortEssay)
        {
            essayLength = EssayLength.SHORT;
        }
        else if (length < mediumEssay)
        {
            essayLength = EssayLength.MEDIUM;
        }
        else
        {
            essayLength = EssayLength.LONG;
        }
    }

    public static void Reset()
    {
        isUp = false;
        essayLength = 0;
        essayPasses = false;
        isOnTopic = false;
        hasNonos = false;
        hasRiskies = false;
        hasName = false;
        hasFullName = false;
        hasSpaces = false;
        hasPeriods = false;
        hasCapitals = false;
    }
}
