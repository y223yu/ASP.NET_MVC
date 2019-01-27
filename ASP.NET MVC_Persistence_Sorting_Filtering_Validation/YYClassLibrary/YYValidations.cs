using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;


namespace YYClassLibrary
{
    public class YYValidations : ValidationAttribute
    {
        public static string YYCapitalize(string input)
        {
            string output = "";
            if(String.IsNullOrWhiteSpace(input))
            {
                return output;
            }
            else
            {
                input = input.Trim().ToLower();

                string[] inputArray = input.Split(' ');
                for (int i = 0; i < inputArray.Length; i++)
                {
                    inputArray[i] = inputArray[i].Substring(0, 1).ToUpper() + inputArray[i].Substring(1);
                    output += inputArray[i] + " ";
                }
                return output;
            }
        }
        
        public static string YYExtractDigits(string input)
        {
            string output = "";
            foreach(char c in input)
            {
                if (char.IsDigit(c))
                {
                    output += c;
                }
            }
            return output;
        }

        public static bool YYPostalCodeValidation(string input)
        {
            input = input.Trim();
            if(String.IsNullOrEmpty(input))
            {
                return true;
            }
            else
            {
                Regex pattern = new Regex(@"^[ABCEGHJKLMNPRSTVXY]\d[ABCEGHJKLMNPRSTVWXYZ] ?\d[ABCEGHJKLMNPRSTVWXYZ]\d$",
                                            RegexOptions.IgnoreCase);
                if(pattern.IsMatch(input))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static string YYPostalCodeFormat(string input)
        {
            string output = input.Trim();
            if(YYPostalCodeValidation(output))
            {
                if (String.IsNullOrEmpty(output))
                {
                    return null;
                }
                else
                {
                    output = output.ToUpper();
                    if(output.Length == 6)
                    {
                        return output.Insert(3, " ");
                    }
                    else if(output.Length == 7)
                    {
                        return output;
                    }
                    else
                    {
                        return "Please use Canadian postal code format: N2G 4M4";
                    }
                }
            }
            else
            {
                return "Please use Canadian postal code format: N2G 4M4";
            }  
        }

        public static bool YYZipCodeValidation(ref string input)
        {
            if(String.IsNullOrEmpty(input))
            {
                input = "";
                return true;
            }
            else
            {
                input = YYExtractDigits(input);
                if(input.Length == 5)
                {
                    return true;
                }
                else if (input.Length == 9)
                {
                    input = input.Insert(5, "-").Substring(0, 7);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
