using Calculator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Calculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (TempData["buttonval"] == null)
            {
                TempData["buttonval"] = "0";
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public static string expression = ""; // input equation to be solved

        // input button
        public IActionResult inputButton(string button)
        {
            expression += button;
            TempData["buttonval"] = expression;

            return RedirectToAction("Index");
        }


        // if clear button is pressed
        public IActionResult clearButton()
        {
            expression = "";
            listOfNums.Clear();
            TempData["buttonval"] = "0";

            return RedirectToAction("Index");
        }

        public static List<string> listOfNums = new List<string>();

        public bool MultipleOperators(string op1, string op2)
        {
            bool qError = false;
            for (int i = 0; i < listOfNums.Count; i++)
            {
                if (i + 2 < listOfNums.Count)
                {
                    if ((listOfNums[i] == op1) && (listOfNums[i + 2] == op2) && (listOfNums[i + 1] == ""))
                    {
                        qError = true;
                        clearButton();
                        break;
                    }
                }
            }
            return qError;
        }

        // action = 0: remove + symbol
        // action = 1: convert negative symbol to the number
        // action = 2: replace double '--' symbol to '+' symbol
        public void FixExpression(string op1, string op2, int action)
        {
            for (int i = 0; i < listOfNums.Count; i++)
            {
                if (i + 2 < listOfNums.Count)
                {
                    if ((listOfNums[i] == op1) && (listOfNums[i + 2] == op2) && (listOfNums[i + 1] == ""))
                    {
                        if (action == 0) // remove + symbol
                        {
                            // remove '+' symbol
                            listOfNums.RemoveAt(2);
                            listOfNums.RemoveAt(2);
                        }
                        else if (action == 1)
                        {
                            // convert negative symbol to the number
                            listOfNums.RemoveAt(2);
                            listOfNums.RemoveAt(2);
                            listOfNums[2] = (Convert.ToDouble(listOfNums[2]) * -1).ToString();
                        }
                        else if (action == 2)
                        {
                            // replace double '--' symbol to '+' symbol
                            listOfNums[1] = "+";
                            listOfNums.RemoveAt(2);
                            listOfNums.RemoveAt(2);
                        }
                    }
                }
            }
        }

        // if the equals button is pressed
        public IActionResult solveButton(string button)
        {
            // -------------------------------------------------- solve --------------------------------------------------//

            bool qError = false; // sets to true if the question doesn't make sense

            // splits the expression into numbers and operators
            string pattern = @"\s*([-+/*])\s*";

            string[] substrings = Regex.Split(expression, pattern);
            foreach (string match in substrings)
            {
                listOfNums.Add(match);
            }

            FixExpression("-", "-", 2);
            FixExpression("/", "+", 0);
            FixExpression("/", "-", 1);
            FixExpression("*", "+", 0);
            FixExpression("*", "-", 1);
            FixExpression("+", "-", 1);
            FixExpression("-", "+", 0);

            // if the expression starts with a negative number
            if (expression[0] == '-')
            {
                listOfNums.RemoveAt(0);
                listOfNums.RemoveAt(0);
                listOfNums[0] = (Convert.ToDouble(listOfNums[0]) * -1).ToString();
            }

            // checks if the expression is valid
            if (expression[^1] == '/' || expression[^1] == '*' || expression[^1] == '+' || expression[^1] == '-' || expression[^1] == '.')
            {
                qError = true;
            }
            if (MultipleOperators("/", "/") || MultipleOperators("/", "*") || MultipleOperators("*", "/") || MultipleOperators("*", "*") || MultipleOperators("+", "/") || MultipleOperators("+", "*") || MultipleOperators("+", "+") || MultipleOperators("-", "/") || MultipleOperators("-", "*"))
            {
                qError = true;
            }
            if (expression[0] == '/' || expression[^1] == '*' || expression[^1] == '+')
            {
                qError = true;
            }
            if (qError)
            {
                TempData["buttonval"] = "ERROR";
            }
            else
            {
                // uses BIDMAS for these 4 operators to prioritise the correct parts of the question
                SolveOperators("/");
                SolveOperators("*");
                SolveOperators("+");
                SolveOperators("-");


                // output answer
                TempData["buttonval"] = listOfNums[0].ToString();
            }

            return RedirectToAction("Index");
        }

        public void SolveOperators(string operatorUsed)
        {
            bool running = true;
            while (running)
            {
                bool containsOperator = false;
                for (int i = 0; i < listOfNums.Count; i++)
                {
                    // calculate a subanswer and replace that subquestion with the answer in listOfNums
                    if (listOfNums[i] == operatorUsed)
                    {
                        if (operatorUsed == "/")
                        {
                            listOfNums[i - 1] = (Convert.ToDouble(listOfNums[i - 1]) / Convert.ToDouble(listOfNums[i + 1])).ToString();
                        }
                        if (operatorUsed == "*")
                        {
                            listOfNums[i - 1] = (Convert.ToDouble(listOfNums[i - 1]) * Convert.ToDouble(listOfNums[i + 1])).ToString();
                        }
                        if (operatorUsed == "+")
                        {
                            listOfNums[i - 1] = (Convert.ToDouble(listOfNums[i - 1]) + Convert.ToDouble(listOfNums[i + 1])).ToString();
                        }
                        if (operatorUsed == "-")
                        {
                            listOfNums[i - 1] = (Convert.ToDouble(listOfNums[i - 1]) - Convert.ToDouble(listOfNums[i + 1])).ToString();
                        }
                        listOfNums.RemoveAt(i); // removes operator
                        listOfNums.RemoveAt(i); // removes second number in that subquestion
                        containsOperator = true;
                    }
                }
                if (containsOperator == false)
                {
                    running = false;
                }
            }
        }
    }
}
