using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class Analyze
    {
        public enum MetropolisMethodOption
        {
             CheckSolution
            ,SetSolution
        }

        [ScriptMethod(nameof(Analyze) + ".algorithm." + "Metropolis method",
            nameof(Analyze) + ".algorithm." + "[ {0} ] Metropolis method",
            "RS=>Analyze_MetropolisMethod"//"メトロポリス法：\nメトロポリス法を行います。\n<random> 乱数サンプル\n<objective> 目的関数\n<evaluate> 評価関数\n<changeEvent> 切替時発生イベント"
            )]
        public static double MetropolisMethod(
            ref double solution
            ,[param: ScriptParam("random f(index)")] Func<double, double> random
            ,[param: ScriptParam("objective f(input)")] Func<double, double> objective
            ,[param: ScriptParam("evaluate f(sample)")] Func<double, double> evaluate
            ,[param: ScriptParam("change event f(solution)")] Func<double, object> changeEvent
            ,double begin
            ,int count = 10
            ,double probability = 0.7
            ,MetropolisMethodOption eventMode = MetropolisMethodOption.SetSolution
            )
        {
            solution = begin;
            if (random is null || objective is null || evaluate is null)
                return solution;

            for (int i = 0; i < count; ++i)
            {
                double randomValue = random(i);
                double newSolution = solution + randomValue;
                double newSample = evaluate(objective(newSolution));
                double oldSample = evaluate(objective(solution));
                if (eventMode == MetropolisMethodOption.CheckSolution)
                    changeEvent?.Invoke(newSample);
                double rnd = RandomLib.RandomDouble();
                if (newSample < oldSample && rnd < probability)
                {
                    if (eventMode == MetropolisMethodOption.SetSolution)
                        changeEvent?.Invoke(newSolution);
                    solution = newSolution;
                }
                else if (oldSample > newSample && rnd < (1.0 - probability))
                {
                    if (eventMode == MetropolisMethodOption.SetSolution)
                        changeEvent?.Invoke(newSolution);
                    solution = newSolution;
                }
            }
            return solution;
        }
    }
}
