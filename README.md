# C# Interpreter using ANTLR4  

## Overview  
This project is a C# interpreter built using **ANTLR4** that processes a custom programming language with a **C-like syntax**. It supports **functional programming**, **scope management**, and **error detection**. The interpreter parses and executes the input code when no errors are found.  

## 🔹 Features  
- ✅ **C-like Syntax:** Recognizes and interprets a structured language similar to C.  
- ✅ **Functional Programming:** Supports function definitions and calls.  
- ✅ **Scope Management:** Proper handling of local and global variables.  
- ✅ **Error Detection:** Identifies syntax and runtime errors before execution.  
- ✅ **Lexical Analysis:** Extracts lexemes from the source code.  
- ✅ **Function & Variable Tracking:** Outputs details of functions and global variables.  
- ✅ **File Output:** Generates logs containing errors, lexemes, function details, and variable declarations.  

## 🚀 Installation  
1. **Clone the repository:**  
2. **Install Dependencies:**
Ensure you have .NET SDK installed. Then, install ANTLR4.
3. **Build the Project:**

## Example Code (input.txt)

```
int addIntegers(int first, int second)
{
 return first + second;
}

double globalVariable = 4.27; //Using global variables is bad practice

int main()
{
 int myFirstVariable = 17;
 int mySecondVariable = 45;
 int myThirdVariable = 3;

 for (int i = 0; i < myThirdVariable; ++i)
 {
 myFirstVariable += i;
 }

 string myString = "";
 if (myFirstVariable >= mySecondVariable)
 {
 myString = "Condition is true";
 }
 else
 {
 myString = "Condition is false";
 int temp = myFirstVariable + 5;
 }

 Print(myString);

 myThirdVariable = addIntegers(myFirstVariable, mySecondVariable);

 return 0;
} 
```

#### ⚠️ Important Notes  
- To modify lexer and parser rules and run, first install ANTLR for VSCode and latest version of Java SDK.

