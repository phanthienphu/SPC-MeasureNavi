using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InspectionMeasure
{
    public class clsPublic
    {
        public static string Left(string param, int length)
        {
            try
            {
                //we start at 0 since we want to get the characters starting from the
                //left and with the specified lenght and assign it to a variable
                string result = param.Substring(0, length);
                //return the result of the operation
                return result;
            }
            catch
            {
                return param;
            }
        }
        public static string Right(string param, int length)
        {
            try
            {
                //start at the index based on the lenght of the sting minus
                //the specified lenght and assign it a variable
                string result = param.Substring(param.Length - length, length);
                //return the result of the operation
                return result;
            }
            catch
            {
                return param;
            }
        }

        public static string Mid(string param, int startIndex, int length)
        {
            try
            {
                //start at the specified index in the string ang get N number of
                //characters depending on the lenght and assign it to a variable
                string result = param.Substring(startIndex, length);
                //return the result of the operation
                return result;
            }
            catch
            {
                return param;
            }
        }

        public static string Mid(string param, int startIndex)
        {
            try
            {
                //start at the specified index and return all characters after it
                //and assign it to a variable
                string result = param.Substring(startIndex);
                //return the result of the operation
                return result;
            }
            catch
            {
                return param;
            }
        }

        public static int IsNullTo0(string st)
        {
            try
            {
                if (st == null || st == "")
                    return 0;
                return Int32.Parse(st = st.Replace(",", ""));
            }
            catch (Exception) { return 0; }
        }

        public static System.Boolean IsNumeric (System.Object Expression)
        {
            if(Expression == null || Expression is DateTime)
                return false;

            if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try 
            {
                if(Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                    return true;
                } 
            catch {} // just dismiss errors but return false
                return false;
        }
    }
}
