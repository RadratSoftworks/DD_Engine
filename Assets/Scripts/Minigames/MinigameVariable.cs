using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDEngine.Minigame
{
    public class MinigameVariable
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();
        private string name;

        public Dictionary<string, object> Properties => properties;
        public string Name => name;
        public object Value { get; set; }

        public MinigameVariable(string name, object value = null)
        {
            this.name = name;
            this.Value = value;
        }

        public bool ConvertToVector2(out Vector2 result)
        {
            result = Vector2.zero;

            if (!TryGetValue("x", out int xValue) || !TryGetValue("y", out int yValue))
            {
                return false;
            }

            result = new Vector2(xValue, yValue);
            return true;
        }

        public bool ConvertToRect(out Rect result)
        {
            result = Rect.zero;

            if (!TryGetValue("x", out int xValue) || !TryGetValue("y", out int yValue) ||
                !TryGetValue("w", out int wValue) || !TryGetValue("h", out int hValue))
            {
                return false;
            }

            result = new Rect(new Vector2(xValue, yValue), new Vector2(wValue, hValue));
            return true;
        }

        public bool TryGetValues<T>(string name, out List<T> resultGet)
        {
            resultGet = null;
            int count = 0;

            while (true)
            {
                string nameArrayed = string.Format("{0}[{1}]", name, count);
                if (!properties.ContainsKey(nameArrayed))
                {
                    break;
                }

                object valueRaw = properties[nameArrayed];
                if (!(valueRaw is T))
                {
                    break;
                }

                if (resultGet == null)
                {
                    resultGet = new List<T>();
                }

                resultGet.Add((T)valueRaw);
                count++;
            }

            return (count != 0);
        }

        public bool TryGetValue<T>(string name, out T resultGet)
        {
            resultGet = default(T);

            if (properties.TryGetValue(name, out object resultValue))
            {
                if (resultValue is MinigameVariable)
                {
                    Type genericType = typeof(T);
                    if (genericType == typeof(MinigameVariable))
                    {
                        resultGet = (T)resultValue;
                        return true;
                    }

                    MinigameVariable resultVariable = resultValue as MinigameVariable;
                    if (resultVariable == null)
                    {
                        return false;
                    }

                    if (genericType == typeof(Vector2))
                    {
                        if (!resultVariable.ConvertToVector2(out Vector2 resultVec2))
                        {
                            return false;
                        }

                        resultGet = (T)(object)resultVec2;
                        return true;
                    }

                    if (!(resultVariable.Value is T))
                    {
                        return false;
                    }

                    resultGet = (T)resultVariable.Value;
                    return true;
                }
                else
                {
                    if (!(resultValue is T))
                    {
                        return false;
                    }

                    resultGet = (T)resultValue;
                    return true;
                }
            }

            return false;
        }

        public void AddProperty(string name, object obj)
        {
            if (properties.ContainsKey(name))
            {
                properties[name] = obj;
            }
            else
            {
                properties.Add(name, obj);
            }
        }

        public MinigameVariable AddOrGetChildMember(string name)
        {
            if (properties.ContainsKey(name))
            {
                if (!(properties[name] is MinigameVariable))
                {
                    MinigameVariable var = new MinigameVariable(name, properties[name]);
                    properties[name] = var;
                }

                return properties[name] as MinigameVariable;
            }
            else
            {
                MinigameVariable variable = new MinigameVariable(name);
                properties.Add(name, variable);

                return variable;
            }
        }
    }
}