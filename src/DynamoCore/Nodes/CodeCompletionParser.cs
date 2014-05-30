using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace DynamoUtilities
{
    class CodeCompletionParser
    {
        Stack<string> expressionStack = new Stack<string>();
        string strPrefix = String.Empty;
        string type = string.Empty;
        int argCount = 0;

        //enum ParseState
        //{
        //    Global,
        //    Member,
        //    MethodArgument
        //}
        
        static Regex identifierDelimiter = new Regex(@"([.\[\]])");

        /// <summary>
        /// Given a line of code and the character that's currently being typed in a CBN,
        /// this function extracts the expression that needs to be code-completed
        /// </summary>
        /// <param name="strPrefix"></param>
        public string ParseCodeToComplete(char currentChar)
        {
            string prefix = string.Empty;
            switch (currentChar)
            {
                case ']':                    
                    strPrefix = PopFromExpressionStack(']');
                    break;
                case '[':
                    // 
                    if (!string.IsNullOrEmpty(strPrefix))
                    {
                        strPrefix += '[';
                        expressionStack.Push(strPrefix);
                        strPrefix = string.Empty;
                    }
                    break;
                case '}':                    
                    strPrefix = PopFromExpressionStack('}');
                    break;
                case '{':
                    // 
                    {
                        strPrefix = string.Empty;
                        strPrefix += '{';
                        expressionStack.Push(strPrefix);
                        strPrefix = string.Empty;
                    }
                    break;
                case ')':
                    argCount = 0;                    
                    strPrefix = PopFromExpressionStack(')');
                    break;
                case '(':
                    argCount = 1;
                    if (!string.IsNullOrEmpty(strPrefix))
                    {
                        // function call
                        // Auto-complete function signature for runtime type 
                        // or static type if runtime type is not available
                        // Class/Type and function name must be known at this point
                        string functionName = GetMemberIdentifier(strPrefix);
                        expressionStack.Push(strPrefix + @"(");                        
                    }
                    else
                    {
                        // simple expression
                        expressionStack.Push(@"(");
                    }
                    strPrefix = string.Empty;
                    break;
                case '.':
                    // Take string prefix and reflect on prefix type
                    // popup list of options available on type
                    // runtime or static type must be known 
                    // type = GetType(strPrefix);
                    strPrefix += '.';
                    break;
                case ' ':
                    break;
                case '=':
                    strPrefix = string.Empty;
                    break;
                default:
                    if (char.IsLetterOrDigit(currentChar))
                    {
                        strPrefix += currentChar;
                        
                        if(strPrefix.IndexOf('.') != -1)
                        {
                            // If type exists, extract string after previous '.'                            
                            string identToComplete = GetMemberIdentifier(strPrefix);
                            // Auto-completion happens over type, search for identToComplete in type's auto-complete list
                        }
                        else
                        {
                            // Search for identifier for auto-completion
                            // search in class, global function and symbol table to try to auto-complete
                        }
                    }                    
                    else
                    {
                        strPrefix = PopFromExpressionStack(currentChar);
                        expressionStack.Push(strPrefix);

                        strPrefix = string.Empty;
                    }
                    break;
            }
            return strPrefix;
        }

#region private utility methods
        private string GetMemberIdentifier(string strPrefix)
        {
            string[] idents = strPrefix.Split('.');
            string identToComplete = idents[idents.Length - 1];
            return identToComplete;
        }

        private string PopFromExpressionStack(char currentChar)
        {
            string prefix = string.Empty;
            if (expressionStack.Count > 0)
            {
                prefix = expressionStack.Pop();
            }
            return prefix + strPrefix + currentChar;
        }
#endregion 
    }
}
