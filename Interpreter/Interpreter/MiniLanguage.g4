grammar MiniLanguage;

//parser rules

program: globalLine*;

globalLine: functionDeclaration | (declaration SEMICOLON);

//function

functionDeclaration:
	returnType VARIABLE_NAME OPENPARAN parameterList? CLOSEDPARAN functionBody;

returnType: variableType | VOID_TYPE;

printFunction: 'Print' OPENPARAN expression CLOSEDPARAN;

parameterList: parameter (COMMA parameter)*;

parameter: variableType VARIABLE_NAME;

functionBody: OPENBRAC instructionSet* CLOSEDBRAC;

returnStatement: RETURN_KW expression? SEMICOLON;

functionCall: VARIABLE_NAME OPENPARAN argumentList? CLOSEDPARAN;

argumentList: argument (COMMA argument)*;

argument: expression;

//\function

expression:
	OPENPARAN expression CLOSEDPARAN					# parenthesisExp
	| <assoc = right> expression POWER expression		# powerExp
	| MINUS expression									# minusExpression
	| expression (ASTERISK | SLASH | MOD) expression	# mulDivModExp
	| expression (PLUS | MINUS) expression				# addSubExp
	| functionCall										# functionExp
	| increment											# incrementExp
	| decrement											# decrementExp
	| VARIABLE_NAME										# numericAtomExp
	| constantValue										# constantAtomExp;

condition:
	OPENPARAN condition CLOSEDPARAN				# paranthesisCond
	| NOT condition								# notCond
	| expression relationalOperator expression	# relationalOpCond
	| condition (AND | OR) condition			# logicalOpCond
	| expression								# expCond;

increment:
	VARIABLE_NAME INCREMENTOP	# postIncrementation
	| INCREMENTOP VARIABLE_NAME	# preIncrementation;

decrement:
	VARIABLE_NAME DECREMENTOP	# postDecrementation
	| DECREMENTOP VARIABLE_NAME	# preDecrementation;

instructionSet: (statement? SEMICOLON)
	| block
	| returnStatement;

block: codeBlock | ifBlock | whileBlock | forBlock;

codeBlock: OPENBRAC instructionSet* CLOSEDBRAC;

ifBlock:
	IF_KW OPENPARAN condition CLOSEDPARAN instructionSet elseBlock?;
elseBlock: (ELSE_KW) instructionSet;

whileBlock:
	WHILE_KW OPENPARAN condition CLOSEDPARAN instructionSet;

forBlock:
	FOR_KW OPENPARAN (declOrAssign)? SEMICOLON (condition)? SEMICOLON (
		statement
	)? CLOSEDPARAN instructionSet;

statement: (
		increment
		| decrement
		| functionCall
		| declOrAssign
		| printFunction
	);

declOrAssign: declaration | assignation;

declaration: variableType VARIABLE_NAME (EQUALS expression)?;

assignation:
	VARIABLE_NAME (
		EQUALS
		| PLUSEQUALS
		| MINUSEQUALS
		| ASTERISKEQUALS
		| SLASHEQUALS
		| MODEQUALS
	) expression;

constantValue:
	INTEGER_VALUE	# intConstant
	| FLOAT_VALUE	# floatConstant
	| DOUBLE_VALUE	# doubleConstant
	| STRING_VALUE	# stringConstant;

variableType:
	INTEGER_TYPE
	| FLOAT_TYPE
	| DOUBLE_TYPE
	| STRING_TYPE;

relationalOperator:
	LESS
	| GREATER
	| LESSEQ
	| GREATEREQ
	| CONDITIONEQUAL
	| NOTEQUAL;

//lexer rules Keywords
INTEGER_TYPE: 'int';
FLOAT_TYPE: 'float';
DOUBLE_TYPE: 'double';
STRING_TYPE: 'string';
VOID_TYPE: 'void';
IF_KW: 'if';
ELSE_KW: 'else';
FOR_KW: 'for';
WHILE_KW: 'while';
RETURN_KW: 'return';
POWER: '^';
ASTERISK: '*';
SLASH: '/';
MOD: '%';
PLUS: '+';
MINUS: '-';
LESS: '<';
GREATER: '>';
LESSEQ: '<=';
GREATEREQ: '>=';
CONDITIONEQUAL: '==';
NOTEQUAL: '!=';
AND: '&&';
OR: '||';
NOT: '!';
PLUSEQUALS: '+=';
MINUSEQUALS: '-=';
ASTERISKEQUALS: '*=';
SLASHEQUALS: '/=';
MODEQUALS: '%=';
INCREMENTOP: '++';
DECREMENTOP: '--';
EQUALS: '=';
SEMICOLON: ';';
COMMA: ',';
OPENPARAN: '(';
CLOSEDPARAN: ')';
OPENBRAC: '{';
CLOSEDBRAC: '}';
INTEGER_VALUE: ('0' | [1-9][0-9]*);
FLOAT_VALUE: ('0' | [1-9][0-9]*) '.' [0-9]*;
DOUBLE_VALUE: ('0' | [1-9][0-9]*)+ '.' ([0-9]*) ('e' | 'E') (
		'+'
		| '-'
	)? ('0' | [1-9][0-9]*);
STRING_VALUE: '"' (ESC_SEQ | ~["\\])* '"';
fragment ESC_SEQ: '\\' [btnrf"\\];
VARIABLE_NAME: [a-zA-Z][a-zA-Z0-9]*;

WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;