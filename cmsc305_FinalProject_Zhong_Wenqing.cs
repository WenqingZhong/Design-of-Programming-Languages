using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace _1
{
	public class Procedure
	{
		dynamic parameter;
		dynamic body;


		public void procedure(dynamic parameter, dynamic body) //constuctor that gives the procedure parameters and body
		{
			this.parameter = parameter;
			this.body = body;
		}

		public dynamic getparameter()
		{
			return parameter;
		}

		public dynamic getbody()
		{
			return body;
		}

		public List<dynamic> substitud_proc(List<dynamic> outerbody, List<dynamic> input)
		{
			List<dynamic> newbd = new List<dynamic>();
					
			for (int i = 0; i < outerbody.Count; i++)
			{
				if (outerbody[i].GetType().Name.Equals("List`1")==false)//if the procedure body is not a nested list
				{
					int state = 0;
					for (int j = 0; j < parameter.Count; j++)
					{
						//if this element is a parameter
						if ((Convert.ToString(parameter[j])).Equals(Convert.ToString(outerbody[i])))
						{
							//substitude that parameter with a corresponding input
							newbd.Add(input[j]); 
							state = 1;
						}
					}
					if (state == 0)
					{
						// if this element is not a parameter, don't do anything to it
						newbd.Add(outerbody[i]);	
					}
				}
				else
				{
					//if the procedure body is nested, substitude it recursively and return a new list
					List<dynamic> innerbody = substitud_proc(outerbody[i], input);
					newbd.Add(innerbody);
				}
			}
			return newbd;
		}

	}

	public class Thunk:Program
	{
		dynamic thunkbody;

		public void lazy(dynamic exp)
		{
			//make a new list with a "thunk" and the expression that needs to be delayed
			List<dynamic> newbody = new List<dynamic>();
			newbody.Add("thunk");
			newbody.Add(exp);
			thunkbody = newbody;
		}

		public dynamic get_thunk_body()  
		{
			//return the expression that is put into the thunk list
			return thunkbody;
		}
	}


	public class Program : Procedure
	{

		//global environment

		public Dictionary<dynamic, dynamic> primitive_env()
		{
			Dictionary<dynamic, dynamic> env = new Dictionary<dynamic, dynamic>();

			Func<List<dynamic>, dynamic> add = (List<dynamic> x) => (x[0] + x[1]);
			env.Add("+", add);
			Func<List<dynamic>, dynamic> sub = (List<dynamic> x) => (x[0] - x[1]);
			env.Add("-", sub);
			Func<List<dynamic>, dynamic> mult = (List<dynamic> x) => (x[0] * x[1]);
			env.Add("*", mult);
			Func<List<dynamic>, dynamic> divi = (List<dynamic> x) => ((double)x[0] / (double)x[1]);
			env.Add("/", divi);
			Func<List<dynamic>, dynamic> larger = (List<dynamic> x) => (x[0] > x[1]);
			env.Add(">", larger);
			Func<List<dynamic>, dynamic> smaller = (List<dynamic> x) => (x[0] < x[1]);
			env.Add("<", smaller);
			Func<List<dynamic>, dynamic> same = (List<dynamic> x) => (x[0] == x[1]);
			env.Add("=", same);
			Func<List<dynamic>, dynamic> ge = (List<dynamic> x) => (x[0] >= x[1]);
			env.Add(">=", ge);
			Func<List<dynamic>, dynamic> le = (List<dynamic> x) => (x[0] <= x[1]);
			env.Add("<=", le);

			Func<List<dynamic>, dynamic> cos = (List<dynamic> x) => Math.Cos(x[0]);
			env.Add("cos", cos);
			Func<List<dynamic>, dynamic> sin = (List<dynamic> x) => Math.Sin(x[0]);
			env.Add("sin", sin);
			Func<List<dynamic>, dynamic> tan = (List<dynamic> x) => Math.Tan(x[0]);
			env.Add("tan", tan);
			Func<List<dynamic>, dynamic> sqrt = (List<dynamic> x) => Math.Sqrt(x[0]);
			env.Add("sqrt", sqrt);
			Func<List<dynamic>, dynamic> abs = (List<dynamic> x) => Math.Abs(x[0]);
			env.Add("abs", abs);
			return env;
		}		
		public Dictionary<dynamic, dynamic> define_env = new Dictionary<dynamic, dynamic>();


		//parse string
		public dynamic isnumber(string a)
		{
			dynamic result;
			int n;
			//if the subdtring can be turned into a number, turn it into a number
			if (int.TryParse(a, out n) == true)
			{
				result = n;
			}
			else
			{
				result = a;
			}
			return result;
		}

		public List<dynamic> FirstParse(string a)
		{
			//add blank spaces to special symbols so that they can be properly parsed later
			string[] words = a.Replace("(", " ( ").Replace(")", " ) ").Replace("'", " ' ").Replace("\""," \" ").Split(' ');

			List<dynamic> words3 = new List<dynamic>();

			for (int i = 0; i < words.Length; i++)
			{
				if (String.IsNullOrEmpty(words[i]) == false)
				{
					dynamic t = isnumber(words[i]);
					words3.Add(t);

				}
			}

			return words3;
		}

		private dynamic pop(List<dynamic> t)
		{
			//delete the first element  in a list and return that element
			dynamic a = t[0];
			t.RemoveAt(0);
			return a;
		}

		public List<dynamic> Tokenize(List<dynamic> t)
		{
			List<dynamic> final = new List<dynamic>();
			dynamic token = pop(t);
			if (token.Equals("("))//a correct expression must start with (
			{
				List<dynamic> L = new List<dynamic>();
				while (t[0].Equals(")") == false)//end the expression when ) is met
				{
					L.AddRange(Tokenize(t));//parse eveything inside the outer expression
				}
				dynamic close2 = pop(t);//delete )
				final.Add(L);
			}
			else if (token.Equals("'"))//if there's a quote mark in the expression
			{
				List<dynamic> L = new List<dynamic>();
				L.Add("quote"); //make a quote with a list of arguments
				dynamic afterquote = pop(t);
				if (afterquote.Equals("("))
				{
					List<dynamic> L1 = new List<dynamic>();
					while (t[0].Equals(")") == false)
					{
						L1.Add(pop(t));
					}
					dynamic close3 = pop(t);
					L.Add(L1);
				}
				else
				{
					L.Add(afterquote);
				}
				final.Add(L);
			}
			else if (token.Equals("\""))//if there is a string
			{
				List<dynamic> L = new List<dynamic>();
				string str = null;
				while (t[0].Equals("\"") == false)
				{
					str+= Convert.ToString(pop(t));//put everything inside " and " into one string
				}
				dynamic close4 = pop(t);
				L.Add("quote");// make a quote with only one argument, which is the string
				L.Add(str);
				final.Add(L);
			}
			else if (token.Equals(")"))//don't strat the expression with )
			{
				Console.WriteLine("cannot start with ')'");
				Environment.Exit(0);
			}
			else
			{
				final.Add(token);
			}
			return final;
		}

		public List<dynamic> SecondParse(List<dynamic> final)//get rid of extra () from Tokenize
		{
			List<dynamic> final2 = new List<dynamic>();
			foreach (dynamic f in final)
			{
				if ((f.GetType().Name.Equals("List`1"))==false)
				{
					final2.Add(f);
				}
				else
				{
					foreach (dynamic f2 in f)

					{
						final2.Add(f2);
					}
				}
			}
			return final2;
		}

		public List<dynamic> FinalParse(string val)//put scanner and parser together
		{
			List<dynamic> a = FirstParse(val);
			List<dynamic> tokens = Tokenize(a);
			List<dynamic> tokens2 = SecondParse(tokens);
			return tokens2;
		}


		//eval

		public dynamic isprimitive(dynamic exp)//check if a procedure is a primitive procedure
		{
			Dictionary<dynamic, dynamic> env = primitive_env();
			dynamic pri = env.ContainsKey(exp);
			return pri;
		}


		public dynamic eval_primitive(dynamic exp)
		{
			Dictionary<dynamic, dynamic> env = primitive_env();//get primitive env
			//eval only takes List<dynamic>, we need to change dynamic into List<dynamic>
			List<dynamic> exp2 = new List<dynamic>();
			foreach (dynamic a in exp)
			{
				exp2.Add(a);
			}
			dynamic func = env[exp2[0]];//get primitive procedure
			exp2.RemoveAt(0);
			List<dynamic> parameter = new List<dynamic>();
			foreach (dynamic token in exp2)//get parameter
			{
				dynamic r = eval(token);
				parameter.Add(r);
			}
			return func(parameter);//apply
		}

		public dynamic determinenumber(dynamic a)//check if an element is number
		{
			string input = Convert.ToString(a);
			int value;
			dynamic isnumber;
			if (int.TryParse(input, out value) == true)
			{
				isnumber = true;
			}
			else
			{
				isnumber = false;
			}
			return isnumber;
		}

		public dynamic eval_number(dynamic a)
		{
			return a;
		}


		public dynamic eval_difinition(dynamic exp)
		{
			dynamic result = "defined";

			if ((exp[1].GetType().FullName).Equals("System.String")) //(define x 5) or (define y (lambda(x)(+ x 1)))
			{
				dynamic a = eval(exp[2]);//eval the body
				define_env.Add(exp[1], a);//put defined value into define_env
			}
			else  //(define (y x)(+ x 1))                                             
			{
				dynamic prcname = exp[1][0];
				Procedure a = new Procedure();
				exp[1].RemoveAt(0);
				a.procedure(exp[1], exp[2]);//build new procedure
				define_env.Add(prcname, a);
			}
			return result;
		}

		public dynamic isdefined(dynamic name)//check if an element is a defined value/procedure
		{
			dynamic def = define_env.ContainsKey(name);
			return def;
		}

		public dynamic isproc(dynamic name)//check if a defined element is a procedure
		{
			dynamic val = define_env[name];
			dynamic proc = (val.GetType().Name).Equals("Procedure");
			return proc;
		}
		public dynamic call_proc(Procedure a, dynamic input)//call a procedure(this procedure can be defined or anonymous)
		{
			dynamic bo = a.getbody();
			List<dynamic> newbody = a.substitud_proc(bo, input);
			dynamic result = eval(newbody);
			return result;
		}

		public dynamic call_define_proc(dynamic exp)
		{
			dynamic name = exp[0];
			exp.RemoveAt(0);
			Procedure a = define_env[name];
			dynamic result = call_proc(a, exp);
			return result;
		}
		public dynamic return_defined_value(dynamic name)// return the defined value, this value can be a number or a procedure
		{
			dynamic value = define_env[name];
			return value;
		}

		public dynamic call_defined(dynamic exp)//call a defined value
		{
			dynamic result = null;
			if (isproc(exp[0]) == true)//if this value is a procedure
			{
				result = call_define_proc(exp);//call procedure
			}
			else
			{
				result = return_defined_value(exp);
			}
			return result;
		}

		

		public dynamic is_annony_proc(dynamic exp)//check anonymous procedure
		{
			dynamic proc = (eval(exp).GetType().Name).Equals("Procedure");
			return proc;
		}

		public dynamic eval_lambda(dynamic exp)//make an anonymous procedure
		{
			exp.RemoveAt(0);
			Procedure a = new Procedure();
			a.procedure(exp[0], exp[1]);
			return a;
		}
		public dynamic call_lambda(dynamic exp)//call an anonymous procedure
		{
			Procedure a = new Procedure();
			a.procedure(exp[0][0], exp[0][1]);//give the procedure parameters and body
			dynamic bo = a.getbody();
			exp.RemoveAt(0);
			List<dynamic> input = new List<dynamic>();
			foreach (dynamic i in exp)//generate inputs
			{
				input.Add(eval(i));
			}
			List<dynamic> newbody = a.substitud_proc(bo, input);
			dynamic result = eval(newbody);
			return result;
		}

		//if any argument is false, the entire and statement is false
		public dynamic eval_and(dynamic exp)
		{
			dynamic result = true;

			for (int i = 1; i < exp.Count; i++)
			{
				if (eval(exp[i]) == false)
				{
					result = false;
				}

			}	
			return result;
		}

		//if any argument is true, the entire and statement is true
		public dynamic eval_or(dynamic exp)
		{
			dynamic result = false;

			for (int i = 1; i < exp.Count; i++)
			{
				if (eval(exp[i]) == true)
				{
					result = true;
				}

			}
			return result;
		}

		//if the predicate is true, do the first part, else do the second part
		public dynamic eval_if(dynamic exp)
		{
			dynamic result;
			if (eval(exp[1]) == true) 
			{
				result = eval(exp[2]);
			}
			else
			{
				result = eval(exp[3]);
			}
			return result;
		}

		public dynamic eval_cond(dynamic exp)
		{
			dynamic result = false;
			exp.RemoveAt(0);
			foreach (dynamic test in exp)
			{
				if (eval(test[0]) == true) //if a predicate is true, eval the following argument
				{
					result = eval(test[1]);
				}

			}
			return result;
		}

		public dynamic eval_set(dynamic exp)
		{
			dynamic result = "set";
			exp.RemoveAt(0);
			// only defined value is immutable, difined procedure cannot be changed
			if ((isdefined(exp[0]) == true)&&(isproc(exp[0])==false))
			{
				define_env[exp[0]] = eval(exp[1]);
			}
			else
			{
				result = "bad input, can't set";
			}
			return result;
		}

		public dynamic eval_let(dynamic exp)
		{
			dynamic result = false;
			List<dynamic> temp = new List<dynamic>();
			exp.RemoveAt(0);
			foreach (dynamic group in exp[0])
			{
				define_env.Add(group[0], eval(group[1]));//add temporary defined values to the defined env
				temp.Add(group[0]);//remember the name of those temporary values
			}
			exp.RemoveAt(0);
			foreach (dynamic e in exp)
			{
				result = eval(e);//eval the body of let
			}

			foreach (dynamic t in temp)
			{
				define_env.Remove(t);//delete temporary values
			}
			return result;
		}

		public dynamic eval_quote(dynamic exp)
		{
			exp.RemoveAt(0);
			List<dynamic> result = new List<dynamic>();
			//if any non-list data is quoted, return that data without eval it
			if (exp[0].GetType().Name.Equals("List`1") == false)
			{	
				result.Add(exp[0]);				
			}
			// if a list is quoted, return that list without eval elements in the list
			else
			{
				foreach (dynamic n in exp[0])
				{
					result.Add(n);
				}

			}
				return result;
		}
		//return everything in the list except for the first element
		public dynamic eval_cdr(dynamic exp)
		{
			exp.RemoveAt(0);
			dynamic body = eval(exp[0]);
			body.RemoveAt(0);
			return body;
		}

		//return the first element
		public dynamic eval_car(dynamic exp)
		{
			exp.RemoveAt(0);
			dynamic body = eval(exp[0]);
			dynamic head = pop(body);
			return head;
		}
		//eval everything in the list and put each result into another list
		public List<dynamic> eval_list(dynamic exp)
		{
			exp.RemoveAt(0);
			List<dynamic> result = new List<dynamic>();
			foreach (dynamic e in exp)
			{
				dynamic littleresult = eval(e);
				result.Add(littleresult);
			}
			return result;
		}
		//put elements together
		public dynamic eval_cons(dynamic exp)
		{
			exp.RemoveAt(0);
			List<dynamic> result = new List<dynamic>();
			foreach (dynamic e in exp)
			{
				dynamic littleresult = eval(e);
				result.Add(littleresult);
			}
			return result;
		}
		//put two lists together
		public dynamic eval_append(dynamic exp)
		{
			exp.RemoveAt(0);//remove the word "append"
			dynamic finalreslt = null;
			List<dynamic> result = new List<dynamic>();
			foreach (dynamic e in exp)
			{
				dynamic littleresult = eval(e);
				if ((littleresult.GetType().Name.Equals("List`1")) == false)
				{
					finalreslt = "bad input, append() only takes lists";
				}
				else
				{
					foreach (dynamic r in littleresult)
					{
						result.Add(r);//add each element in two lists into a larger list
					}
					finalreslt = result;
				}
			}
			return finalreslt;
		}

		//eval evrything in the expression and print them
		public dynamic eval_display(dynamic exp)
		{
			exp.RemoveAt(0);
			dynamic result = eval(exp[0]);
			if (result.GetType().Name.Equals("List`1") == false)
			{
				Console.Write(result);
			}
			else
			{
				foreach (dynamic r in result)
				{
					Console.Write(r + " ");
				}
			}
			return result;
		}

		//eval each argument form left to right 
		public dynamic eval_begin(dynamic exp)
		{
			exp.RemoveAt(0);
			dynamic result = null;
			foreach (dynamic group in exp)
			{
				result = eval(group);
			}
			return result;
		}

		public dynamic eval_map(dynamic exp)
		{
			exp.RemoveAt(0);
			Procedure a = new Procedure();
			List<dynamic> result = new List<dynamic>();
			if (exp[0].GetType().Name.Equals("String"))//if the procedure is defined eg. (map y '(1 2 3))
			{
				a = eval(exp[0]);
			}
			else//if the procedure is anonymous eg.(map (lambda(x)(+ x 1)) '(1 2 3))
			{
				List<dynamic> getproc = new List<dynamic>();//put lambda expression into a list becuase eval takes list
				foreach (dynamic e in exp[0])
				{
					getproc.Add(e);
				}
				a = eval(getproc);//make procedure
			}

			dynamic input = eval(exp[1]);
			foreach (dynamic i in input)//every input needs to be a list instead of a dynamic
			{
				List<dynamic> change_input_type = new List<dynamic>();
				change_input_type.Add(i);
				dynamic littleresult = call_proc(a, change_input_type);
				result.Add(littleresult);
			}
			return result;
		}


		public dynamic eval_stream_car(dynamic exp)  //(stream-car ((+ 1 2) 3 4))
		{                                            
			exp.RemoveAt(0);//after remove:(((+ 1 2) 3 4))
			List<dynamic> result = new List<dynamic>();
			dynamic first = eval(exp[0][0]);//eval the first element
			result.Add(first);
			exp[0].RemoveAt(0);
			Thunk rest = new Thunk();//put everthing else into a thunk list, don't eval them
			rest.lazy(exp[0]);
			result.Add(rest);
			return result;
		}

		public dynamic eval_stream_cdr(dynamic exp)
		{
			List<dynamic> result = new List<dynamic>();
			int i = 0;
			int limit = (exp[1].Count);
			dynamic recursive = exp;//this dynamic value keeps changing because of the while loop
			while (i < limit)
			{
				dynamic result1 = eval_stream_car(recursive);
				Console.WriteLine(result1[0]);
				result.Add(result1[0]);
				Thunk rest = result1[1];//get the thunk from eval the first element
			    //get the body of that thunk, and use the thunk body as a new expression to call eval_strem_car again
				recursive = rest.get_thunk_body();
				i++;
			}
			result.RemoveAt(0);
			return result;
		}

		public dynamic eval(dynamic exp)
		{
			dynamic result = " ";

			if (determinenumber(exp) == true)//number
			{
				result = exp;
			}

			else if (exp.Equals("#t")) //if #t is in an expression, eg (and #t (< 1 2))
			{
				result = true;
			}
			else if (exp.Equals("#f"))
			{
				result = false;
			}
			else if (isdefined(exp) == true)//check defined value, eg. x
			{
				result = return_defined_value(exp);
			}
			else if (exp[0].Equals("#t") && exp.Count == 1)//check #t, #t is not in an expression, eg.#t
			{
				result = true;
			}
			else if (exp[0].Equals("#f") && exp.Count == 1)
			{
				result = false;
			}
			else if (exp[0].Equals("quote"))
			{
				result = eval_quote(exp);
			}
			else if (exp[0].Equals("list"))
			{
				result = eval_list(exp);
			}
			else if (exp[0].Equals("cdr"))
			{
				result = eval_cdr(exp);
			}
			else if (exp[0].Equals("car"))
			{
				result = eval_car(exp);
			}
			else if (exp[0].Equals("cons") && exp.Count == 3)
			{
				result = eval_cons(exp);
			}
			else if (exp[0].Equals("append"))//(append '(1 2) '(1 2 3))
			{
				result = eval_append(exp);
			}
			else if (exp[0].Equals("define"))
			{
				result = eval_difinition(exp);
			}
			else if (exp[0].Equals("and"))//(and (< 1 2) (> 3 4))
			{
				result = eval_and(exp);
			}
			else if (exp[0].Equals("or"))//(or #t (> 3 4))
			{
				result = eval_or(exp);
			}
			else if (exp[0].Equals("if"))//(if (< 1 2)(+ 1 1)(+ 2 2))
			{
				result = eval_if(exp);
			}
			else if (exp[0].Equals("cond"))//(cond ((< 1 2)(+ 1 1)) ((> 3 4)(+ 2 2)))
			{
				result = eval_cond(exp);
			}
			else if (exp[0].Equals("let"))//(let ((a 1)(b 2)) (+ a b))
			{
				result = eval_let(exp);
			}
			else if (exp[0].Equals("set!"))//(set! x 7)
			{
				result = eval_set(exp);
			}
			else if (exp[0].Equals("lambda"))// ((lambda (x) (+ x 1)) 3)
			{
				result = eval_lambda(exp);
			}
			else if (exp[0].Equals("display") && exp.Count == 2) //(display "asjdkhk")
			{
				eval_display(exp);
			}
			else if (exp[0].Equals("begin"))
			{
				result = eval_begin(exp);
			}
			else if (exp[0].Equals("map"))
			{
				result = eval_map(exp);
			}
			else if (exp[0].Equals("stream-car"))//(stream-car ((+ 1 2) 4 5 jfkldkj))
			{
				result = eval_stream_car(exp);
			}
			else if (exp[0].Equals("stream-cdr"))//(stream-cdr ((+ 1 2) 4 5 jfkldkj))
			{
				result = eval_stream_cdr(exp);
			}

			else if (isdefined(exp[0]) == true)//defined value is used, eg. (+ x 1)
			{
				if (exp.Count == 1)
				{
					result = return_defined_value(exp[0]);
				}
				else
				{
					result = call_defined(exp);
				}
			}

			else if (determinenumber(exp[0]) == true && exp.Count == 1) //pure number input
			{
				result = exp[0];
			}
			else if (isprimitive(exp[0]) == true)
			{
				result = eval_primitive(exp);
			}
			else if (is_annony_proc(exp[0]) == true)
			{
				result = call_lambda(exp);
			}
			return result;
		}
		
		


		
		public void repl()
		{
			while (true)
			{
				string val;
				Console.Write("Enter Expression: ");
				val = Console.ReadLine();
				if (val.Equals("exit"))//exit the program
				{
					Console.WriteLine("bye :)");
					Environment.Exit(0);
				}
				List<dynamic> tokens = FinalParse(val);				
				dynamic a = eval(tokens);
				if (a.GetType().Name.Equals("List`1")==false)//rules to write out the eval result
				{
					Console.WriteLine(a);
				}
				else//write everything in the list
				{
					foreach (dynamic a1 in a)
					{
						Console.Write(a1+" ");
					}

					Console.WriteLine();
				}
			}
		}

		static void Main(string[] args)
		{
			Program p = new Program();
			p.repl();

		}
	}
}


