
using System;

namespace Portalworkers.DocxTemplating.Grammar {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _string = 2;
	public const int maxT = 9;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;



	private Expression expression = null;

	public Expression Expression
	{
		get
		{
			return expression;
		}
	}

	public bool IsFollowedByBracket()
	{
		var next = scanner.Peek();
		return next.val == "[";
	}

	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void TemplatingGrammar() {
		ConditionalPlaceholderExpression cond = null;
		SimplePlaceholderExpression placeholder = null;
		AbstractCondition condition = null;
		
		if (IsFollowedByBracket()) {
			ConditionalPlaceholderExpression(out cond);
			expression = cond;
			
		} else if (la.kind == 3) {
			Get();
			Condition(out condition);
			Expect(4);
			expression = new ConditionExpression()
			{
			Condition = condition
			};
			
		} else if (la.kind == 1) {
			SimplePlaceholderExpression(out placeholder);
			expression = placeholder;
			
		} else SynErr(10);
	}

	void ConditionalPlaceholderExpression(out ConditionalPlaceholderExpression cond) {
		SimplePlaceholderExpression placeholder = null;
		AbstractCondition condition = null;
		
		SimplePlaceholderExpression(out placeholder);
		Expect(3);
		Condition(out condition);
		Expect(4);
		cond = new ConditionalPlaceholderExpression()
		{
		Placeholder = placeholder,
		Condition = condition
		};
		
	}

	void Condition(out AbstractCondition cond) {
		ComparisionCondition comp = null;
		Grammar.BinaryOperator op;
		AbstractCondition secondary = null;
		
		ComparisionCondition(out comp);
		cond = comp; 
		if (la.kind == 5 || la.kind == 6) {
			BinaryOperator(out op);
			Condition(out secondary);
			cond = new BinaryCondition(cond, op, secondary);
			
		}
	}

	void SimplePlaceholderExpression(out SimplePlaceholderExpression placeholder) {
		Expect(1);
		placeholder = new SimplePlaceholderExpression(t.val); 
	}

	void ComparisionCondition(out ComparisionCondition comp) {
		SimplePlaceholderExpression lhs = null;
		ConditionOperator op;
		LiteralExpression rhs = null;
		
		SimplePlaceholderExpression(out lhs);
		ConditionOperator(out op);
		LiteralExpression(out rhs);
		comp = new ComparisionCondition()
		{
		Lhs = lhs,
		Operator = op,
		Rhs = rhs
		};
		
	}

	void BinaryOperator(out Grammar.BinaryOperator op) {
		op = Grammar.BinaryOperator.LogicalAnd;
		
		if (la.kind == 5) {
			Get();
			op = Grammar.BinaryOperator.LogicalAnd; 
		} else if (la.kind == 6) {
			Get();
			op = Grammar.BinaryOperator.LogicalOr; 
		} else SynErr(11);
	}

	void ConditionOperator(out ConditionOperator op) {
		op = Grammar.ConditionOperator.Equals; 
		if (la.kind == 7) {
			Get();
			op = Grammar.ConditionOperator.Equals; 
		} else if (la.kind == 8) {
			Get();
			op = Grammar.ConditionOperator.NotEquals; 
		} else SynErr(12);
	}

	void LiteralExpression(out LiteralExpression lit) {
		Expect(2);
		lit = new LiteralExpression(t.val.Substring(1, t.val.Length - 2)); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		TemplatingGrammar();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "string expected"; break;
			case 3: s = "\"[\" expected"; break;
			case 4: s = "\"]\" expected"; break;
			case 5: s = "\"and\" expected"; break;
			case 6: s = "\"or\" expected"; break;
			case 7: s = "\"=\" expected"; break;
			case 8: s = "\"!=\" expected"; break;
			case 9: s = "??? expected"; break;
			case 10: s = "invalid TemplatingGrammar"; break;
			case 11: s = "invalid BinaryOperator"; break;
			case 12: s = "invalid ConditionOperator"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}