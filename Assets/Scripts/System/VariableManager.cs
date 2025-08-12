using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn;

public class VariableManager : Yarn.Unity.VariableStorageBehaviour
{
    /// A default value to apply when the object wakes up, or when
    /// ResetToDefaults is called.

    [System.Serializable]
    public class DefaultVariable
    {
        public string name;
        public string value;
        public Yarn.Value.Type type;
    }

    public DefaultVariable[] defaultVariables;

    [SerializeField] 
    internal TextMeshProUGUI debugTextView = null;
    [SerializeField]
    internal RectTransform debugTextBox = null;

    public Dictionary<string, bool> playedNodes;

    internal void Awake()
    {
        //ResetToDefaults();

    }
    public override void ResetToDefaults()
    {
        //Clear();

        foreach (var variable in defaultVariables) {
                
            object value;

            switch (variable.type) {
                case Yarn.Value.Type.Number:
                    float f = 0.0f;
                    float.TryParse(variable.value, out f);
                    value = f;
                    break;

                case Yarn.Value.Type.String:
                    value = variable.value;
                    break;

                case Yarn.Value.Type.Bool:
                    bool b = false;
                    bool.TryParse(variable.value, out b);
                    value = b;
                    break;

                case Yarn.Value.Type.Variable:
                    // Doesn't support assigning default variables from other variables yet
                    Debug.LogErrorFormat("Can't set variable {0} to {1}: You can't " +
                        "set a default variable to be another variable, because it " +
                        "may not have been initialised yet.", variable.name, variable.value);
                    continue;

                case Yarn.Value.Type.Null:
                    value = null;
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            var v = new Yarn.Value(value);

            SetValue("$" + variable.name, v);
            //SetValue(variable.name, v);
        }
    }

    public override void SetValue(string variableName, Yarn.Value value)
    {
        //variables[variableName] = new Yarn.Value(value);
        if (variableName == "day")
        {
            GameManager.Instance.SetDay(value.AsString);
        }
        else if (variableName == "timeofday")
        {
            GameManager.Instance.SetTimeOfDay(value.AsString);
        }
        /*
        string identifier = "";

        switch (value.type)
        {
            case Yarn.Value.Type.Number:
                identifier = "f";
                break;

            case Yarn.Value.Type.String:
                identifier = "s";
                break;

            case Yarn.Value.Type.Bool:
                identifier = "b";
                break;

            case Yarn.Value.Type.Null:
                identifier = "n";
                break;
        }
        PlayerPrefs.SetString(variableName, identifier + value.AsString);
        Debug.Log("Setting " + variableName + " " + value.AsString);*/
        GameManager.Instance.SetValue(variableName, value);
        RedrawDebug();
    }

    public override Yarn.Value GetValue (string variableName)
    {
        /*
        if (!variables.ContainsKey(variableName))
            return Yarn.Value.NULL;

        return variables[variableName];*/

        /*
        Debug.Log("trying to get value " + variableName);
        */
        if (variableName == "$day")
        {
            return new Yarn.Value(GameManager.Instance.CurrentDay);
        }
        else if (variableName == "$timeofday")
        {
            return new Yarn.Value(GameManager.Instance.GetTimeOfDayString());
        }
        else if (variableName == "$essayhasname")
        {
            return new Yarn.Value(EssayGrader.hasName);
        }
        else if (variableName == "$essayhasfullname")
        {
            return new Yarn.Value(EssayGrader.hasFullName);
        }
        else if (variableName == "$essayhasnonos")
        {
            return new Yarn.Value(EssayGrader.hasNonos);
        }
        else if (variableName == "$essayhasriskies")
        {
            return new Yarn.Value(EssayGrader.hasRiskies);
        }
        else if (variableName == "$essayisontopic")
        {
            return new Yarn.Value(EssayGrader.isOnTopic);
        }
        else if (variableName == "$essayhasspaces")
        {
            return new Yarn.Value(EssayGrader.hasSpaces);
        }
        else if (variableName == "$essayhasperiods")
        {
            return new Yarn.Value(EssayGrader.hasPeriods);
        }
        else if (variableName == "$essayhascapitals")
        {
            return new Yarn.Value(EssayGrader.hasCapitals);
        }
        else if (variableName == "$essaylength")
        {
            return new Yarn.Value(EssayGrader.essayLength.ToString().ToLower());
        }
        else if (variableName == "$spawnpointname")
        {
            if (LevelLoader.Instance)
                return new Yarn.Value(LevelLoader.Instance.currentSpawnPoint);
            else
                return new Yarn.Value("");
        }
        else if (variableName == "$currentscene")
        {
            return new Yarn.Value(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        else if (variableName == "$testingskip")
        {
            return new Yarn.Value(GameManager.Instance.skipDialogue);
        }
        /*
        if (PlayerPrefs.GetString(variableName) == null || PlayerPrefs.GetString(variableName).Length < 2) {
            return Yarn.Value.NULL;
        }

        char identifier = PlayerPrefs.GetString(variableName)[0];
        string content = PlayerPrefs.GetString(variableName).Substring(1);
        //Debug.Log("Substrings: " + identifier + " " + content);

        object value;

        switch (identifier)
        {
            case 'f':
                float f = 0.0f;
                float.TryParse(content, out f);
                value = f;
                break;

            case 's':
                value = content;
                break;

            case 'b':
                bool b = false;
                bool.TryParse(content, out b);
                value = b;
                break;

            case 'n':
                value = null;
                break;

            default:
                value = null;
                break;
        }
        //Debug.Log("Getting " + variableName + " " + PlayerPrefs.GetString(variableName));
        return new Yarn.Value(value);*/
        return GameManager.Instance.GetValue(variableName);

    }
    void RedrawDebug()
    {
        if (debugTextView != null && debugTextBox != null)
        {
            var stringBuilder = new System.Text.StringBuilder();
            foreach (KeyValuePair<string, Yarn.Value> item in GameManager.Instance.variables)
            {
                string debugDescription;
                switch (item.Value.type)
                {
                    case Yarn.Value.Type.Bool:
                        debugDescription = item.Value.AsBool.ToString();
                        break;
                    case Yarn.Value.Type.Null:
                        debugDescription = "null";
                        break;
                    case Yarn.Value.Type.Number:
                        debugDescription = item.Value.AsNumber.ToString();
                        break;
                    case Yarn.Value.Type.String:
                        debugDescription = $@"""{item.Value.AsString}""";
                        break;
                    default:
                        debugDescription = "<unknown>";
                        break;

                }
                stringBuilder.AppendLine(string.Format("{0} = {1}",
                                                        item.Key,
                                                        debugDescription));
            }
            debugTextView.text = stringBuilder.ToString();
            debugTextBox.sizeDelta = new Vector2(debugTextView.preferredWidth, debugTextView.preferredHeight);
        }
    }

    public override void Clear()
    {
        GameManager.Instance.variables.Clear();
        //PlayerPrefs.DeleteAll();
    }
}
