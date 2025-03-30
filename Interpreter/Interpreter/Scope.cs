using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Interpreter
{
    public class Scope
    {
        private Dictionary<string, object> _variables { get; set; }
        private Scope _parent {  get; set; }
        public Scope(Scope parent = null) { 
            _variables = new Dictionary<string, object>();
            _parent = parent;
        }
        public Dictionary<string, object> GetAllVariables()
        {
            return _variables;
        }
        public object GetVariable(string name)
        {
            if (_variables.ContainsKey(name))
            {
                return _variables[name];
            }
            if (_parent != null)
            {
                return _parent.GetVariable(name);
            }
            throw new KeyNotFoundException($"Variable '{name}' doesn't exists in the current scope.");
        }

        public bool IsObjectFound(string name)
        {
            if (_variables.ContainsKey(name))
            {
                return true;
            }
            if (_parent != null)
            {
                return _parent.IsObjectFound(name);
            }
            return false;
        }

        public object AddVariable(string name, object value)
        {
            if (IsObjectFound(name)) {
                throw new InvalidOperationException($"Variable '{name}' already exists in the current scope.");
            }
            _variables[name] = value;
            return value;
        }
        
        public object UpdateVariable(string name, object value)
        {
            if (_variables.ContainsKey(name))
            {
                if (_variables[name].GetType() == typeof(string) ^ value.GetType() == typeof(string))
                {
                    throw new InvalidCastException($"Cannot update variable '{name}' with a value of incompatible type.");
                }
                if (_variables[name].GetType()==typeof(double))
                {
                    _variables[name] = MyVisitor.ConvertToType("double",value);
                }
                if (_variables[name].GetType() == typeof(float))
                {
                    _variables[name] = MyVisitor.ConvertToType("float", value);
                }
                if (_variables[name].GetType() == typeof(int))
                {
                    _variables[name] = MyVisitor.ConvertToType("int", value);
                }
                if (_variables[name].GetType() == typeof(string))
                {
                    _variables[name] = MyVisitor.ConvertToType("string", value);
                }
                return _variables[name];
            }
            if (_parent != null)
            {
                return _parent.UpdateVariable(name, value);
            }
            throw new KeyNotFoundException($"Variable '{name}' does not exist in the current scope.");
        }

    }
}
