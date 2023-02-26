using System;
using DDEngine.Action;

namespace DDEngine.GUI
{
    public static class GUIConditionHelper
    {
        public static string[] GetParticipateVariablesInCondition(string varListName)
        {
            return varListName.Split('^');
        }

        public static string[] GetRequiredValues(string values)
        {
            return values.Split(',');
        }

        public static bool GUIConditionResult(this ActionInterpreter interpreter, string[] variableList, string[] valueList)
        {
            if ((variableList == null) || (valueList == null))
            {
                return true;
            }

            if (variableList.Length != valueList.Length)
            {
                throw new InvalidOperationException("The number of required arguments are not equal to the number of required values!");
            }

            for (int i = 0; i < variableList.Length; i++)
            {
                string value = interpreter.GetValue(variableList[i], out _);
                string valueToCompare = valueList[i];

                bool shouldNegate = false;
                if (valueToCompare[0] == '!')
                {
                    shouldNegate = true;
                    valueToCompare = valueToCompare.Substring(1);
                }

                bool result = false;
                if (valueToCompare == "*")
                {
                    result = true;
                }
                else
                {
                    result = (value == valueToCompare);
                }

                if (shouldNegate)
                {
                    result = !result;
                }

                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool GUIConditionResult(this ActionInterpreter interpreter, string condition, string expectedValues)
        {
            string[] participators = GetParticipateVariablesInCondition(condition);
            string[] expectedValueList = GetRequiredValues(expectedValues);

            return GUIConditionResult(interpreter, participators, expectedValueList);
        }
    };
}