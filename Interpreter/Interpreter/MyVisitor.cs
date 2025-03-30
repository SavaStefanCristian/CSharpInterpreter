using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Interpreter
{
    public class MyVisitor : MiniLanguageBaseVisitor<object>
    {
        public static object ConvertToType(string type, object value)
        {
            switch (type)
            {
                case "int":
                    if (value == null)
                        return Convert.ToInt32(value);
                    if (value.GetType()==typeof(string))
                    {
                        throw new InvalidCastException($"Cannot convert {value} to '{type}'.");
                    }
                    return Convert.ToInt32(value);
                case "float":
                    if (value == null)
                        return Convert.ToSingle(value);
                    if (value.GetType() == typeof(string))
                    {
                        throw new InvalidCastException($"Cannot convert {value} to '{type}'.");
                    }
                    return Convert.ToSingle(value);
                case "double":
                    if (value == null)
                        return Convert.ToDouble(value);
                    if (value.GetType() == typeof(string))
                    {
                        throw new InvalidCastException($"Cannot convert {value} to '{type}'.");
                    }
                    return Convert.ToDouble(value);
                case "string":
                    if (value == null)
                        return Convert.ToString(value);
                    if (value.GetType() != typeof(string))
                    {
                        throw new InvalidCastException($"Cannot convert {value} to '{type}'.");
                    }
                    return Convert.ToString(value);
                default:
                    return null;
            }
        }

        Scope _globalScope;
        Stack<Scope> _stack;
        private Dictionary<string, (string, MiniLanguageParser.FunctionBodyContext, List<(string,string)>)> _functionDeclarations;
        Stack<object> _returnValueStack;
        Stack<bool> _isReturningStack;


        public MyVisitor() 
        { 
            _globalScope = new Scope();
            _stack = new Stack<Scope>();
            _stack.Push(_globalScope);
            _functionDeclarations = new Dictionary<string, (string, MiniLanguageParser.FunctionBodyContext, List<(string,string)>)>();
            _returnValueStack = new Stack<object>();
            _isReturningStack = new Stack<bool>();
        }

        public override object VisitProgram([NotNull] MiniLanguageParser.ProgramContext context)
        {
            foreach (var globalLine in context.globalLine())
            {
                Visit(globalLine);
            }
            return null;
        }

        public override object VisitDeclaration([NotNull] MiniLanguageParser.DeclarationContext context)
        {
            string type = context.variableType().GetText();
            string name = context.VARIABLE_NAME().GetText();
            object value = context.expression() != null ? Visit(context.expression()) : null;
            if (value != null)
            {
                if (type == "string" ^ value.GetType() == typeof(string))
                {
                    throw new InvalidCastException($"Cannot convert {value} to '{type}'.");
                }
            }
            return _stack.Peek().AddVariable(name, ConvertToType(type, value));
        }

        public override object VisitParenthesisExp([NotNull] MiniLanguageParser.ParenthesisExpContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitPowerExp([NotNull] MiniLanguageParser.PowerExpContext context)
        {
            object left = Visit(context.expression(0));
            object right = Visit(context.expression(1));
            if (left.GetType() == typeof(string) || right.GetType() == typeof(string))
            {
                throw new InvalidOperationException("Cannot exponentiate a string.");
            }
            else return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
        }

        public override object VisitMulDivModExp([NotNull] MiniLanguageParser.MulDivModExpContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));
            var op = context.GetChild(1).GetText();

            if (left.GetType() == typeof(string) || right.GetType() == typeof(string)) {
                throw new InvalidOperationException("Cannot multiply, divide or modulo a string.");
            }

            if (op == "%")
            {
                if(left.GetType()!=typeof(int) || right.GetType() !=typeof(int))
                {
                    throw new InvalidOperationException("Can only perform modulo on int.");
                }
                return Convert.ToInt32(left) % Convert.ToInt32(right);
            }

            if (left.GetType() == typeof(double) || right.GetType() == typeof(double))
            {
                return op == "*"?
                    Convert.ToDouble(left) * Convert.ToDouble(right) :
                    Convert.ToDouble(left) / Convert.ToDouble(right);
            }
            if (left.GetType() == typeof(float) || right.GetType() == typeof(float))
            {
                return op == "*" ?
                    Convert.ToSingle(left) * Convert.ToSingle(right) :
                    Convert.ToSingle(left) / Convert.ToSingle(right);
            }
            return op == "*" ?
                    Convert.ToInt32(left) * Convert.ToInt32(right) :
                    Convert.ToInt32(left) / Convert.ToInt32(right);
        }

        public override object VisitAddSubExp([NotNull] MiniLanguageParser.AddSubExpContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));
            var op = context.GetChild(1).GetText();

            if(left.GetType() == typeof(string) && right.GetType() == typeof(string))
            {
                if(op == "+") 
                    return Convert.ToString(left) + Convert.ToString(right);

                throw new InvalidOperationException("Cannot perform subtraction on a string.");
            }
            if (left.GetType() == typeof(string) || right.GetType() == typeof(string))
            {
                throw new InvalidCastException($"Cannot perform addition between string and other type.");
            }


            if (left.GetType() == typeof(double) || right.GetType() == typeof(double))
            {
                return op == "+" ?
                    Convert.ToDouble(left) + Convert.ToDouble(right) :
                    Convert.ToDouble(left) - Convert.ToDouble(right);
            }
            if (left.GetType() == typeof(float) || right.GetType() == typeof(float))
            {
                return op == "+" ?
                    Convert.ToSingle(left) + Convert.ToSingle(right) :
                    Convert.ToSingle(left) - Convert.ToSingle(right);
            }
            return op == "+" ?
                    Convert.ToInt32(left) + Convert.ToInt32(right) :
                    Convert.ToInt32(left) - Convert.ToInt32(right);
        }

        public override object VisitFunctionExp([NotNull] MiniLanguageParser.FunctionExpContext context)
        {
            return Visit(context.functionCall());
        }

        public override object VisitIncrementExp([NotNull] MiniLanguageParser.IncrementExpContext context)
        {
            return Visit(context.increment());
        }

        public override object VisitDecrementExp([NotNull] MiniLanguageParser.DecrementExpContext context)
        {
            return Visit(context.decrement());
        }

        public override object VisitPreIncrementation([NotNull] MiniLanguageParser.PreIncrementationContext context)
        {
            string name = context.VARIABLE_NAME().GetText();
            object value = _stack.Peek().GetVariable(name);
            if(value == null)
            {
                throw new KeyNotFoundException($"Variable '{name}' does not exist in the current scope.");
            }
            if (value.GetType() == typeof(string)) 
            {
                throw new InvalidOperationException($"Cannot increment string {name}");
            }
            if (value.GetType() == typeof(double))
            {
                return _stack.Peek().UpdateVariable(name, Convert.ToDouble(value)+1);
            }
            if (value.GetType() == typeof(float))
            {
                return _stack.Peek().UpdateVariable(name, Convert.ToSingle(value) + 1);
            }
            return _stack.Peek().UpdateVariable(name, Convert.ToInt32(value)+1);
        }

        public override object VisitPostIncrementation([NotNull] MiniLanguageParser.PostIncrementationContext context)
        {
            string name = context.VARIABLE_NAME().GetText();
            object value = _stack.Peek().GetVariable(name);
            if (value == null)
            {
                throw new KeyNotFoundException($"Variable '{name}' does not exist in the current scope.");
            }
            if (value.GetType() == typeof(string))
            {
                throw new InvalidOperationException($"Cannot increment string {name}");
            }
            if (value.GetType() == typeof(double))
            {
                _stack.Peek().UpdateVariable(name, Convert.ToDouble(value) + 1);
                return value;
            }
            if (value.GetType() == typeof(float))
            {
                _stack.Peek().UpdateVariable(name, Convert.ToSingle(value) + 1);
                return value;
            }
            _stack.Peek().UpdateVariable(name, Convert.ToInt32(value) + 1);
            return value;
        }

        public override object VisitPreDecrementation([NotNull] MiniLanguageParser.PreDecrementationContext context)
        {
            string name = context.VARIABLE_NAME().GetText();
            object value = _stack.Peek().GetVariable(name);
            if (value == null)
            {
                throw new KeyNotFoundException($"Variable '{name}' does not exist in the current scope.");
            }
            if (value.GetType() == typeof(string))
            {
                throw new InvalidOperationException($"Cannot decrement string {name}");
            }
            if (value.GetType() == typeof(double))
            {
                return _stack.Peek().UpdateVariable(name, Convert.ToDouble(value) - 1);
            }
            if (value.GetType() == typeof(float))
            {
                return _stack.Peek().UpdateVariable(name, Convert.ToSingle(value) - 1);
            }
            return _stack.Peek().UpdateVariable(name, Convert.ToInt32(value) - 1);
        }

        public override object VisitPostDecrementation([NotNull] MiniLanguageParser.PostDecrementationContext context)
        {
            string name = context.VARIABLE_NAME().GetText();
            object value = _stack.Peek().GetVariable(name);
            if (value == null)
            {
                throw new KeyNotFoundException($"Variable '{name}' does not exist in the current scope.");
            }
            if (value.GetType() == typeof(string))
            {
                throw new InvalidOperationException($"Cannot decrement string {name}");
            }
            if (value.GetType() == typeof(double))
            {
                _stack.Peek().UpdateVariable(name, Convert.ToDouble(value) - 1);
                return value;
            }
            if (value.GetType() == typeof(float))
            {
                _stack.Peek().UpdateVariable(name, Convert.ToSingle(value) - 1);
                return value;
            }
            _stack.Peek().UpdateVariable(name, Convert.ToInt32(value) - 1);
            return value;
        }

        public override object VisitMinusExpression([NotNull] MiniLanguageParser.MinusExpressionContext context)
        {
            object value = Visit(context.expression());
            if (value == null)
            {
                throw new InvalidOperationException($"Cannot negate null.");
            }
            if (value.GetType() == typeof(string))
            {
                throw new InvalidOperationException($"Cannot negate string.");
            }
            if(value.GetType() == typeof(double))
            {
                return -Convert.ToDouble(value);
            }
            if (value.GetType() == typeof(float))
            {
                return -Convert.ToSingle(value);
            }
            return -Convert.ToInt32(value);
        }

        public override object VisitNumericAtomExp([NotNull] MiniLanguageParser.NumericAtomExpContext context)
        {
            return _stack.Peek().GetVariable(context.VARIABLE_NAME().GetText());
        }

        public override object VisitConstantAtomExp([NotNull] MiniLanguageParser.ConstantAtomExpContext context)
        {
            return Visit(context.constantValue());
        }

        public override object VisitIntConstant([NotNull] MiniLanguageParser.IntConstantContext context)
        {
            return int.Parse(context.INTEGER_VALUE().GetText());
        }
        public override object VisitFloatConstant([NotNull] MiniLanguageParser.FloatConstantContext context)
        {
            return float.Parse(context.FLOAT_VALUE().GetText());
        }
        public override object VisitDoubleConstant([NotNull] MiniLanguageParser.DoubleConstantContext context)
        {
            return double.Parse(context.DOUBLE_VALUE().GetText());
        }
        public override object VisitStringConstant([NotNull] MiniLanguageParser.StringConstantContext context)
        {
            string value = context.STRING_VALUE()?.GetText()??"";
            return value.Substring(1,value.Length-2);
        }


        public override object VisitBlock([NotNull] MiniLanguageParser.BlockContext context)
        {
            _stack.Push(new Scope(_stack.Peek()));
            VisitChildren(context);
            _stack.Pop();
            return null;
        }

        public override object VisitFunctionDeclaration([NotNull] MiniLanguageParser.FunctionDeclarationContext context)
        {
            string functionName = context.VARIABLE_NAME().GetText();
            string returnType = context.returnType().GetText();
            List<(string,string)> parameterList = context.parameterList()?.parameter()
                .Select(p => (p.variableType().GetText() , p.VARIABLE_NAME().GetText())).ToList() ?? new List<(string, string)>();
            if (_functionDeclarations.ContainsKey(functionName))
            {
                var existingFunction = _functionDeclarations[functionName];
                var existingParameters = existingFunction.Item3;
                if (existingParameters.Count == parameterList.Count)
                {
                    bool parametersMatch = true;
                    for (int i = 0; i < parameterList.Count; i++)
                    {
                        if (existingParameters[i].Item1 != parameterList[i].Item1)
                        {
                            parametersMatch = false;
                            break;
                        }
                    }
                    if (parametersMatch)
                    {
                        throw new Exception($"Error: Duplicate function definition for '{functionName}' with identical parameter types.");
                    }
                }
                else throw new InvalidOperationException($"Function '{functionName}' already exists.");
            }
            var seenParameters = new HashSet<string>();
            foreach (var parameter in parameterList)
            {
                if (!seenParameters.Add(parameter.Item2))
                {
                    throw new Exception($"Two parameters have the same name:'{parameter.Item2}' in function '{functionName}'");
                }
            }

            _functionDeclarations[functionName] = (returnType,context.functionBody(),parameterList);
            return null;
        }

        public override object VisitFunctionCall([NotNull] MiniLanguageParser.FunctionCallContext context)
        {
            string functionName = context.VARIABLE_NAME().GetText();
            List<object> arguments = context.argumentList()?.argument()
                .Select(arg => Visit(arg)).ToList() ?? new List<object>();
            if (!_functionDeclarations.ContainsKey(functionName))
            {
                throw new KeyNotFoundException($"Function {functionName} was not declared.");
            }
            var (returnType, functionBodyContext, parameters) = _functionDeclarations[functionName];
            if (arguments.Count() != parameters.Count())
            {
                throw new Exception($"Argument list does not match parameter list for the call of function {functionName}");
            }
            _stack.Push(new Scope(_globalScope));
            foreach (var pair in arguments.Zip(parameters, (arg, param) => (arg, param)))
            {
                var (arg, param) = pair;
                string paramType = param.Item1;
                string paramName = param.Item2;
                _stack.Peek().AddVariable(paramName, ConvertToType(paramType,arg));
            }


            _isReturningStack.Push(false);
            _returnValueStack.Push(null);
            Visit(functionBodyContext);
            bool isReturning = _isReturningStack.Pop();
            object returnValue = _returnValueStack.Pop();
            _stack.Pop();

            if (isReturning == false)
            {
                if (returnType == "void") return null;
                else
                {
                    throw new Exception($"No return was found for function {functionName}");
                }
            }
            else if (returnValue == null)
            {
                throw new Exception($"No return value was provided for function {functionName}");
            }



            return ConvertToType(returnType,returnValue);
        }

        public override object VisitFunctionBody([NotNull] MiniLanguageParser.FunctionBodyContext context)
        {
            foreach (var statement in context.children)
            {
                if (_isReturningStack.Peek()) break;
                Visit(statement);
            }
            return _isReturningStack.Peek() ? _returnValueStack.Peek() : null;
        }

        public override object VisitStatement([NotNull] MiniLanguageParser.StatementContext context)
        {
            if (_isReturningStack.Peek()) return null;
            return VisitChildren(context);
        }

        public override object VisitReturnStatement([NotNull] MiniLanguageParser.ReturnStatementContext context)
        {
            _returnValueStack.Pop();
            _returnValueStack.Push((context.expression() != null)? Visit(context.expression()) : null);
            _isReturningStack.Pop();
            _isReturningStack.Push(true);
            return _returnValueStack.Peek();
        }

        public void CallMain()
        {
            _stack.Push(new Scope(_globalScope));
            _isReturningStack.Push(false);
            _returnValueStack.Push(null);
            if (_functionDeclarations.ContainsKey("main"))
                Visit(_functionDeclarations["main"].Item2);
            else
            {
                throw new Exception($"Function main does not exist.");
            }
            _isReturningStack.Pop();
            _returnValueStack.Pop();
            _stack.Pop();
            return;
        }

        public override object VisitAssignation([NotNull] MiniLanguageParser.AssignationContext context)
        {
            string name = context.VARIABLE_NAME().GetText();
            object initialValue = _stack.Peek().GetVariable(name);
            object rightHandValue = Visit(context.expression());
            string op = context.GetChild(1).GetText();
            if (initialValue == null)
            {
                throw new KeyNotFoundException($"Variable '{name}' does not exist in the current scope.");
            }
            if (initialValue.GetType() == typeof(string))
            {
                switch (op)
                {
                    case "=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToString(rightHandValue));
                    case "+=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToString(initialValue) + Convert.ToString(rightHandValue));
                    default:
                        throw new InvalidOperationException($"Cannot use operator {op} on a string '{name}'.");
                }
            }
            if (initialValue.GetType() == typeof(double))
            {
                switch (op)
                {
                    case "=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToDouble(rightHandValue));
                    case "+=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToDouble(initialValue) + Convert.ToDouble(rightHandValue));
                    case "-=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToDouble(initialValue) - Convert.ToDouble(rightHandValue));
                    case "*=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToDouble(initialValue) * Convert.ToDouble(rightHandValue));
                    case "/=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToDouble(initialValue) / Convert.ToDouble(rightHandValue));
                    default:
                        throw new InvalidOperationException($"Cannot use operator {op} on a double '{name}'.");
                }
            }
            if (initialValue.GetType() == typeof(float))
            {
                switch (op)
                {
                    case "=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToSingle(rightHandValue));
                    case "+=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToSingle(initialValue) + Convert.ToSingle(rightHandValue));
                    case "-=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToSingle(initialValue) - Convert.ToSingle(rightHandValue));
                    case "*=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToSingle(initialValue) * Convert.ToSingle(rightHandValue));
                    case "/=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToSingle(initialValue) / Convert.ToSingle(rightHandValue));
                    default:
                        throw new InvalidOperationException($"Cannot use operator {op} on a float '{name}'.");
                }
            }
            if (initialValue.GetType() == typeof(int))
            {
                switch (op)
                {
                    case "=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToInt32(rightHandValue));
                    case "+=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToInt32(initialValue) + Convert.ToInt32(rightHandValue));
                    case "-=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToInt32(initialValue) - Convert.ToInt32(rightHandValue));
                    case "*=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToInt32(initialValue) * Convert.ToInt32(rightHandValue));
                    case "/=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToInt32(initialValue) / Convert.ToInt32(rightHandValue));
                    case "%=":
                        return _stack.Peek().UpdateVariable(name, Convert.ToInt32(initialValue) % Convert.ToInt32(rightHandValue));
                    default:
                        throw new InvalidOperationException($"Cannot use operator {op} on an int '{name}'.");
                }
            }
            return null;
        }

        public override object VisitIfBlock([NotNull] MiniLanguageParser.IfBlockContext context)
        {
            if(Convert.ToBoolean(Visit(context.condition())))
            {
                Visit(context.instructionSet());
            }
            else
            {
                if(context.elseBlock() != null)
                {
                    Visit(context.elseBlock());
                }
            }
            return null;
        }
        public override object VisitParanthesisCond([NotNull] MiniLanguageParser.ParanthesisCondContext context)
        {
            return Visit(context.condition());
        }

        public override object VisitNotCond([NotNull] MiniLanguageParser.NotCondContext context)
        {
            return !(Convert.ToBoolean(Visit(context.condition())));
        }

        public override object VisitLogicalOpCond([NotNull] MiniLanguageParser.LogicalOpCondContext context)
        {
            bool left = Convert.ToBoolean(Visit(context.condition(0)));
            bool right = Convert.ToBoolean(Visit(context.condition(1)));
            string op = context.GetChild(1).GetText();

            return op == "&&" ? left && right : left || right;
        }
        
        public override object VisitExpCond([NotNull] MiniLanguageParser.ExpCondContext context)
        {
            object expressionValue = Visit(context.expression());
            if(expressionValue != null)
            {
                if(expressionValue.GetType() == typeof(string))
                {
                    return true;
                }
            }
            return Convert.ToBoolean(expressionValue); 
        }

        public override object VisitRelationalOpCond([NotNull] MiniLanguageParser.RelationalOpCondContext context)
        {
            object left = Visit(context.expression(0));
            object right = Visit(context.expression(1));
            string op = context.GetChild(1).GetText();

            if (left.GetType() == typeof(string) || right.GetType() == typeof(string))
            {
                throw new InvalidOperationException($"Cannot use relational operator {op} on a string.");
            }
            if (left.GetType() == typeof(double) || right.GetType() == typeof(double)) {
                double leftDouble = Convert.ToDouble(left);
                double rightDouble = Convert.ToDouble(right);

                switch (op)
                {
                    case "<":
                        return leftDouble < rightDouble;
                    case ">":
                        return leftDouble > rightDouble;
                    case "<=":
                        return leftDouble <= rightDouble;
                    case ">=":
                        return leftDouble >= rightDouble;
                    case "==":
                        return leftDouble == rightDouble;
                    case "!=":
                        return leftDouble != rightDouble;
                }
            }
            if (left.GetType() == typeof(float) || right.GetType() == typeof(float)) {
                double leftFloat = Convert.ToSingle(left);
                double rightFloat = Convert.ToSingle(right);

                switch (op)
                {
                    case "<":
                        return leftFloat < rightFloat;
                    case ">":
                        return leftFloat > rightFloat;
                    case "<=":
                        return leftFloat <= rightFloat;
                    case ">=":
                        return leftFloat >= rightFloat;
                    case "==":
                        return leftFloat == rightFloat;
                    case "!=":
                        return leftFloat != rightFloat;
                }
            }
            double leftInt = Convert.ToInt32(left);
            double rightInt = Convert.ToInt32(right);

            switch (op)
            {
                case "<":
                    return leftInt < rightInt;
                case ">":
                    return leftInt > rightInt;
                case "<=":
                    return leftInt <= rightInt;
                case ">=":
                    return leftInt >= rightInt;
                case "==":
                    return leftInt == rightInt;
                case "!=":
                    return leftInt != rightInt;
                default: return false;
            }

        }


        public override object VisitWhileBlock([NotNull] MiniLanguageParser.WhileBlockContext context)
        {
            while(Convert.ToBoolean(Visit(context.condition())))
            {
                _stack.Push(new Scope(_stack.Peek()));
                if (_isReturningStack.Peek()) break;
                Visit(context.instructionSet());
                _stack.Pop();
            }
            return null;
        }

        public override object VisitForBlock([NotNull] MiniLanguageParser.ForBlockContext context)
        {
            if(context.declOrAssign()!=null)
            {
                Visit(context.declOrAssign());
            }
            if(context.statement()!=null)
            {
                if (context.condition() != null)
                {
                    while (Convert.ToBoolean(Visit(context.condition())))
                    {
                        _stack.Push(new Scope(_stack.Peek()));
                        if (_isReturningStack.Peek()) break;
                        Visit(context.instructionSet());
                        _stack.Pop();
                        Visit(context.statement());
                    }
                }
            }
            return null;
        }


        private static string ConvertObjectToStringType(Object obj)
        {
            if (obj is string) return "string";
            if (obj is double) return "double";
            if (obj is float) return "float";
            if (obj is int) return "int";
            return null;
        }

        public void PrintGlobalVariables(string globalVariablesFilePath)
        {
            using (StreamWriter writer = new StreamWriter(globalVariablesFilePath, false))
            {
                foreach (var variable in _globalScope.GetAllVariables())
                {
                    string type = ConvertObjectToStringType(variable.Value);
                    if (type == "string")
                        writer.WriteLine($"Name: {variable.Key}, Type: {type}, Value: \"{variable.Value}\"");
                    else
                        writer.WriteLine($"Name: {variable.Key}, Type: {type}, Value: {variable.Value}");
                }
            }
        }

        public void PrintFunctionDetails(string functionDetailsFilePath)
        {
            using (StreamWriter writer = new StreamWriter(functionDetailsFilePath, false))
            {
                foreach (var function in _functionDeclarations)
                {
                    string functionName = function.Key;
                    string returnType = function.Value.Item1;
                    MiniLanguageParser.FunctionBodyContext functionBody = function.Value.Item2;
                    List<(string, string)> parameters = function.Value.Item3;

                    bool isRecursive = IsFunctionRecursive(functionName, functionBody);
                    string functionType = functionName == "main" ? "main" : (isRecursive ? "recursive" : "iterative");

                    writer.WriteLine($"Funtion: {functionName}");
                    writer.WriteLine($"  Type: {functionType}");
                    writer.WriteLine($"  Return Type: {returnType}");
                    writer.WriteLine($"  Parameters: {string.Join(", ", parameters.Select(p => $"{p.Item1} {p.Item2}"))}");

                    writer.WriteLine($"  Local variables:");
                    var localVariables = GetLocalVariables(functionBody);
                    foreach (var variable in localVariables)
                    {
                        writer.WriteLine($"    {variable.Type} {variable.Name}");
                    }

                    writer.WriteLine($"  Control Structures:");
                    var controlStructures = GetControlStructures(functionBody);
                    foreach (var structure in controlStructures)
                    {
                        writer.WriteLine($"    <{structure.Type}, {structure.Line}>");
                    }

                    writer.WriteLine();
                }
            }
        }

        private bool IsFunctionRecursive(string functionName, ParserRuleContext context)
        {
            foreach (var child in context.children)
            {
                if (child is MiniLanguageParser.FunctionCallContext call)
                {
                    if (call.VARIABLE_NAME().GetText() == functionName)
                    {
                        return true;
                    }
                }
                else if (child is ParserRuleContext childContext)
                {
                    if (IsFunctionRecursive(functionName, childContext)) return true;
                }
                
            }

            return false;
        }


        private List<(string Type, string Name)> GetLocalVariables(ParserRuleContext functionBody)
        {
            var variables = new List<(string Type, string Name)>();
            foreach (var child in functionBody.children)
            {
                if (child is MiniLanguageParser.DeclarationContext declaration)
                {
                    string type = declaration.variableType().GetText();
                    string name = declaration.VARIABLE_NAME().GetText();
                    variables.Add((type, name));
                }
                else if (child is ParserRuleContext childContext)
                {
                    variables.AddRange(GetLocalVariables(childContext));
                }
            }

            return variables;
        }

        private List<(string Type, int Line)> GetControlStructures(ParserRuleContext context)
        {
            var structures = new List<(string Type, int Line)>();
            foreach (var child in context.children)
            {
                if (child is MiniLanguageParser.ElseBlockContext elseBlock)
                {
                    structures.Add(("else", elseBlock.Start.Line));
                }
                if (child is MiniLanguageParser.IfBlockContext ifBlock)
                {
                    structures.Add(("if", ifBlock.Start.Line));
                }
                else if (child is MiniLanguageParser.ForBlockContext forBlock)
                {
                    structures.Add(("for", forBlock.Start.Line));
                }
                else if (child is MiniLanguageParser.WhileBlockContext whileBlock)
                {
                    structures.Add(("while", whileBlock.Start.Line));
                }
                if (child is ParserRuleContext childContext)
                {
                    structures.AddRange(GetControlStructures(childContext));
                }
            }

            return structures;
        }

        public override object VisitPrintFunction([NotNull] MiniLanguageParser.PrintFunctionContext context)
        {
            object value = Visit(context.expression());
            Console.WriteLine(value);
            return value;
        }


    }

}
