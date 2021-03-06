﻿using System;
using MathParser.DataTypes;
using System.Collections.Generic;
using System.Linq;


namespace MathParser
{
	/// <summary>
	/// The class that is concerned with the solving those mathematical expressions
	/// which donot have an equality symbol in them.
	/// The solution is autonamed here.
	/// </summary>
	/// 
	/// This class communicates with the DMAS Solver class, the BODMAS Solver class and 
	/// Checker class and OperatorHandler class.
	/// 
	/// Gives its solution to String Observer.
	/// 
	class NonEquation : ISolver
	{
		string givenExpression;
		bool Processed = true;
		MathParserExpression Solution;
		List<string> ExpressionElements = new List<string>();
		Dictionary<string, MathParserExpression> theData = new Dictionary<string, MathParserExpression>();
		char[] theBasicOperatorList;
		public string theBasicOperatorsString;
		public delegate void OperatorSyncDelegate(ref char[] theBasicOperatorList);
		public delegate void ConstantSyncDelegate(ref Dictionary<string, MathParserExpression> ConstantList);
		public Dictionary<string, MathParserExpression> History;
		int matrixAutoCounter = 0, numberAutoCounter = 0;

		public OperatorSyncDelegate OnOperatorSync = null;
		public static OperatorSyncDelegate staticOnOperatorSync = null;
		public static ConstantSyncDelegate staticOnConstantSync = null;
		public List<string> ConstantName = new List<string>();

		void ConstantSync()
		{
			Checker.Constants = new Dictionary<string, MathParserExpression>();
			staticOnConstantSync?.Invoke(ref Checker.Constants);
			string[] arr = new string[Checker.Constants.Count + 2];
			Checker.Constants.Keys.CopyTo(arr, Checker.Constants.Count);

			//Checker.Constants.get
			ConstantName = new List<string>(arr);
		}

		public MathParserExpression getSolution()
		{
			ExpressionElements.Clear();
			theData.Clear();
			return Solution;
		}
		public bool isProcessed() { return Processed; }

		public NonEquation() { }
		public NonEquation(string givenString)
		{
			givenExpression = givenString.Trim();
		}

		void OperatorSync()   // Syncs operators.
		{
			if (OnOperatorSync != null)
			{
				OnOperatorSync(ref theBasicOperatorList);
			}
			else if (staticOnOperatorSync != null)
			{
				staticOnOperatorSync(ref theBasicOperatorList);
			}
			else
			{
				theBasicOperatorList = ("+-÷/*^" + new string(((char)215), 1)).ToCharArray();
			}

			theBasicOperatorsString = new string(theBasicOperatorList);
			theBasicOperatorsString += "`()#";

			Checker.OperatorList = theBasicOperatorsString;

		}    // end Operator syncor functions.

		public void Solve()   // the method deals with the prelimanry making of the Expression Understading.
		{
			OperatorSync();    // Syncs the operators to be considered.
			SyncKeyWord();
			ConstantSync();
			if (givenExpression.Contains("(") || givenExpression.Contains(")"))
			{

				Only_Number_And_Operator_List_Maker();
				if (!(ExpressionElements[ExpressionElements.Count - 1] == ")") && theBasicOperatorsString.Contains(ExpressionElements[ExpressionElements.Count - 1]))
				{
					Processed = false;
					throw new MathParserException($"" +
						"Invalid operator entry. No basic operator ({theBasicOperatorList.ToString()}) can be at the end of the expression.");
				}
				the_Data_List_Maker();
				OperatorHandler
				oh = new OperatorHandler(ref ExpressionElements);
				oh.BasicOperatorsString = theBasicOperatorsString;
				oh.Process();
				if (!oh.isCorrectOperatorSequence())
				{
					Processed = false;
					throw new MathParserException("Bad operator sequence.");
				}

				// Balancing brakkets.
				if (ExpressionElements.Contains("(") || ExpressionElements.Contains("-(") || ExpressionElements.Contains(")"))
				{

					int nsb = 0;
					int neb = 0;
					foreach (string s in ExpressionElements)
					{
						if (s.Contains("("))
							nsb++;
						if (s.Contains(")"))
							neb++;
					}
					if (nsb > neb)
					{
						for (int c = 0; c < (nsb - neb); c++)
						{
							ExpressionElements.Add(")");
						}
					}
					if (neb > nsb)
					{
						List<string> dumy = new List<string>();
						for (int c = 0; c < (-nsb + neb); c++)
						{
							dumy.Add("(");
						}
						foreach (string x in ExpressionElements)
						{
							dumy.Add(x);
						}
						ExpressionElements = dumy;
					}
				}
				// end balancing brakkets


				MathParser.BraketSolver sol = new BraketSolver(ExpressionElements, theData);
				if (sol.isProcessed())
				{
					Solution = sol.getSolution();
				}
				else
				{
					Processed = false;
					throw new MathParserException("Error");
				}
			}
			else if ((givenExpression.Contains("[") && !givenExpression.Contains("]")) || (!givenExpression.Contains("[") && givenExpression.Contains(("]"))))   // if the expession string contans an invalid matrix formate.
			{
				throw (new MathParserException("Invalid Matrix Formate. Entered."));
			}
			else   // if the expression string contain only operators and numbers.
			{
				Only_Number_And_Operator_List_Maker();
				if (theBasicOperatorsString.Contains(ExpressionElements[ExpressionElements.Count - 1]))
				{
					Processed = false;

					throw new MathParserException($"Invalid operator entry. No basic operator ({theBasicOperatorList.ToString()}) can be at the end of the expression.");
				}
				else
				{
					the_Data_List_Maker();
					OperatorHandler oh = new OperatorHandler(ref ExpressionElements);
					oh.BasicOperatorsString = theBasicOperatorsString;
					oh.Process();

					if (!oh.isCorrectOperatorSequence())
					{
						Processed = false;
						throw new MathParserException("Bad operator sequence.");
					}
					else
					{
						DMASSolver DS = new DMASSolver(ExpressionElements, theData);
						DS.Solve();
						if (DS.isProcessed())
						{
							Solution = new MathParserExpression(DS.getSolution());
						}
						else
						{
							Processed = false;
						}
					}
				} // end 
			}  // end
		}    // end solve




		void Only_Number_And_Operator_List_Maker()
		{
			string dumy = "";
			bool matrixBatch = false;
			foreach (char c in givenExpression)
			{
				if (c == '[' && !matrixBatch)
				{
					matrixBatch = true;
				}

				if (theBasicOperatorsString.Contains(c.ToString()) && !matrixBatch)
				{
					if (string.IsNullOrEmpty(dumy) || string.IsNullOrWhiteSpace(dumy))
					{
						dumy = "";
					}
					else
					{
						ExpressionElements.Add(dumy.Trim());
					}
					ExpressionElements.Add(c.ToString());
					dumy = "";
				}
				else
				{
					dumy += c.ToString();
				}

				if (c == ']' && matrixBatch)
				{
					matrixBatch = false;
					ExpressionElements.Add(dumy);
					dumy = "";
				}
			}
			if (!string.IsNullOrEmpty(dumy) && !string.IsNullOrWhiteSpace(dumy))
			{
				ExpressionElements.Add(dumy);
			}
			dumy = "";
		}

		public List<string> theKeyWordsList = new List<string>();

		public delegate void KeyWordSyncDelegate(ref List<string> theKeyWordList);
		public static KeyWordSyncDelegate staticOnKeyWordSync;

		Dictionary<string, string> dumyKeyWordsList = new Dictionary<string, string>();

		void SyncKeyWord()
		{
			theKeyWordsList = new List<string>();
			staticOnKeyWordSync?.Invoke(ref theKeyWordsList);
			theKeyWordsList = theKeyWordsList.OrderByDescending(x => x.Length).ToList();

			if (theKeyWordsList != null && !givenExpression.Contains("#"))
			{
				int the_counter = 0;
				foreach (string keyword in theKeyWordsList)
				{
					string key = "¿?" + "internal_matrix_sAADIww___" + the_counter + "¿?";
					givenExpression = givenExpression.Replace(keyword, "#" + key + "`");
					dumyKeyWordsList.Add(key, keyword);
					the_counter++;
				}

				foreach (var item in dumyKeyWordsList)
				{
					givenExpression = givenExpression.Replace(item.Key, item.Value);
				}

			}
			Checker.KeyWords = theKeyWordsList;
		}




		void the_Data_List_Maker()
		{
			for (int c = 0; c < ExpressionElements.Count; c++)
			{
				string Element = ExpressionElements[c].Trim(' ', '-');
				if (!theBasicOperatorsString.Contains(Element) && !theKeyWordsList.Contains(Element))
				{
					string name = "";

					if (Checker.MatrixSyntax(Element, "[", "]", ",", ";"))
					{
						name = autoMatrixNamer();
						theData.Add(name, new MathParserExpression(MathParser.DataTypes.DynamicDataTypes.Matrix.Parse(Element.Trim())));
					}
					else if (History != null &&
					  History.ContainsKey(Element))
					{
						name = autoNumberNamer();
						theData.Add(name, new MathParserExpression(History[Element]));
					}
					else if (Element.Contains("@"))
					{
					}
					else if (ConstantName.Contains(Element))
					{
						name = autoNumberNamer();
						theData.Add(name, Checker.Constants[Element]);
					}
					else
					{
						try
						{
							name = autoNumberNamer();
							theData.Add(name, new MathParserExpression(MathParser.DataTypes.DynamicDataTypes.Number.Parse(Element.Trim())));
						}
						catch
						{
							Processed = false;

							throw new MathParserException($"Invalid entry '{Element}'.");
						}
					}

					ExpressionElements[c] = name;
				}   // if there is an expression element other than a basic operator.
			}    // end foreach loop for making the data list entries.
		}     // end data list maker.


		string autoMatrixNamer()
		{
			matrixAutoCounter++;
			string name = "internal_matrix_sAADIww___" + matrixAutoCounter.ToString();
			if (theData.ContainsKey(name))
			{
				return autoMatrixNamer();
			}
			else
			{
				return name;
			}
		}

		string autoNumberNamer()
		{
			numberAutoCounter++;
			string name = "internal_Number_sAADIww___" + numberAutoCounter.ToString();
			if (theData.ContainsKey(name))
			{
				return autoNumberNamer();
			}
			else
			{
				return name;
			}
		}


	}
}

