﻿using System;
using System.Collections.Generic;
using MathParser.DataTypes;
using MathParser.DataTypes.DynamicDataTypes;


namespace MathParser
{





	public class MathParserOverdrive
	{
		public delegate void OnMathParserOverDriveSyncDelegate(ref MathParser.Solver sol);
		public static OnMathParserOverDriveSyncDelegate OnMathParserOverDriveSolverCreate;
		void mathParserOverdrive_keywords_sync(ref List<string> keyWordsList){
			keyWordsList.Add("root");
			keyWordsList.Add("eval"); 
		}

		// variables
		MathParserExpression solution;
		bool Processed = true;
		List<string> mathParserOverdrive_keywords;
		string givenExpression;
		public  List<string> oldSolver_KeyWords;
		public  string oldSolver_OperatorList;
		public  Dictionary<string, MathParserExpression> oldSolver_Constants;
		public  Dictionary<string, MathParserExpression> oldSolver_History;
		public  List<string> oldSolver_LeftRightFunctionKeywords = new List<string>();
		// variables




		public MathParserOverdrive(){}
		public MathParserOverdrive(string expression) {
			mathParserOverdrive_keywords = new List<string>();
			mathParserOverdrive_keywords_sync (ref mathParserOverdrive_keywords);
			this.givenExpression = expression;
		}
		public bool isProcessed() { return Processed;}
		public MathParserExpression getSolution() { return solution;}


		public void Solve(){
			// validate the given string.
			if (this.givenExpression.Length - this.givenExpression.Replace("->", "").Length > 2) {
				Processed = false;
				throw new MathParser.MathParserException($"Invalid expression '{this.givenExpression}' " +
														 ". Only write '->' after the overdrive function " +
														 "keyword");
			}
			string[] e = this.givenExpression.Split(new string[] {"->"},StringSplitOptions.None);
			e[0] = e[0].Trim();
			e[1] = e[1].Trim();
			if(e[1][0] == '<' && e[1][1] == '>'){
				e[1] = e[1].Substring(2);
			}
			string[] args = e[1].Trim(new char[] { '<', '>' }).Split(new string[] {"<>"}, StringSplitOptions.None);

			if(mathParserOverdrive_keywords.Contains(e[0])){
				Process(e[0], args, ref solution);
			}else{
				throw new MathParserException($"Given keyword '{e[0]}' does not exit.");
			}

		}

		MathParser.Solver makeMathParserSolverObject(){
			MathParser.Solver sol = new MathParser.Solver();
			OnMathParserOverDriveSolverCreate?.Invoke(ref sol);
			return sol;
		}


		void Process(string command, string[] args, ref MathParserExpression theSolution) {
			MathParser.Solver sol = makeMathParserSolverObject ();
			sol.SaveHistory();

			if(command == "eval"){
				
				foreach (var x in args)
				{
					sol.setMathExpression(x);
					sol.Solve();
				}
				theSolution = sol.getSolution();
				return;
			}


			throw new MathParserException("Keyword does not exist");
		}

	}
}
