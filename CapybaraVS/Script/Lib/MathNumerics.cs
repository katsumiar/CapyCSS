using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/*
 *
Math.NET Numerics License (MIT/X11)
Copyright (c) 2002-2019 Math.NET

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

namespace CapybaraVS.Script.Lib
{
    class MathNumerics
    {
        /// <summary>
        /// 平均
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(Mean), "",
            "RS=>MathNumerics_Mean"//"平均を求めます。"
            )]
        public static double Mean(List<double> sample)
        {
            return sample.Mean();
        }

        /// <summary>
        /// 中央値
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(Median), "",
            "RS=>MathNumerics_Median"//"中央値を求めます。"
            )]
        public static double Median(List<double> sample)
        {
            return sample.Median();
        }

        /// <summary>
        /// 順序統計量
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(OrderStatistic), "",
            "RS=>MathNumerics_OrderStatistic"//"順序統計量を求めます。"
            )]
        public static double OrderStatistic(List<double> sample, int order)
        {
            return Statistics.OrderStatistic(sample, order);
        }

        /// <summary>
        /// 母分散
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(PopulationVariance), "",
            "RS=>MathNumerics_PopulationVariance"//"母分散を求めます。"
            )]
        public static double PopulationVariance(List<double> sample)
        {
            return sample.PopulationVariance();
        }

        /// <summary>
        /// 標本分散
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(Variance), "",
            "RS=>MathNumerics_Variance"//"標本分散を求めます。"
            )]
        public static double Variance(List<double> sample)
        {
            return sample.Variance();
        }

        /// <summary>
        /// 母標準偏差
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(PopulationStandardDeviation), "",
            "RS=>MathNumerics_PopulationStandardDeviation"//"母標準偏差を求めます。"
            )]
        public static double PopulationStandardDeviation(List<double> sample)
        {
            return sample.PopulationStandardDeviation();
        }

        /// <summary>
        /// 標本標準偏差
        /// </summary>
        /// <param name="sample">サンプル</param>
        /// <returns></returns>
        [ScriptMethod("Statistics" + "." + nameof(StandardDeviation), "",
            "RS=>MathNumerics_StandardDeviation"//"標本標準偏差を求めます。"
            )]
        public static double StandardDeviation(List<double> sample)
        {
            return sample.StandardDeviation();
        }
    }


    //------------------------------------------------------------------------------------
    // dll 側のアセンブリは GetType で取得できないので独自の Vector クラスを作ってラッピングする
    public class Vector : ICbShowValue
    {
        public Vector()
        {
        }

        public Vector(MathNet.Numerics.LinearAlgebra.Vector<double> v)
        {
            vector = v;
        }

        public Vector(List<double> list)
        {
            vector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfEnumerable(list);
        }

        MathNet.Numerics.LinearAlgebra.Vector<double> vector = null;

        public string DataString => vector.ToString().Trim('\r', '\n');

        [ScriptMethod("Math" + ".Vector." + "CreateVectorFromVector", "",
            "RS=>MathNumerics_CreateV"//"Vector をもとに Vector を作成します。"
            )]
        public static Vector Create(Vector v)
        {
            return new Vector(v.vector);
        }

        [ScriptMethod("Math" + ".Vector." + "CreateVectorFromList", "",
            "RS=>MathNumerics_CreateL"//"double 型のリストをもとに Vector を作成します。"
            )]
        public static Vector Create(List<double> list)
        {
            return new Vector(list);
        }

        [ScriptMethod("Math" + ".Vector." + "+")]
        public Vector add(Vector v)
        {
            return new Vector(vector + v.vector);
        }

        [ScriptMethod("Math" + ".Vector." + "-")]
        public Vector sub(Vector v)
        {
            return new Vector(vector / v.vector);
        }

        [ScriptMethod("Math" + ".Vector." + "Dot", "",
            "RS=>MathNumerics_Dot"//"内積を求めます。"
            )]
        public double mul(Vector v)
        {
            return vector * v.vector;
        }

        [ScriptMethod("Math" + ".Vector." + "v/n")]
        public Vector div(double divisor)
        {
            return new Vector(vector / divisor);
        }

        [ScriptMethod("Math" + ".Vector." + "v%n")]
        public Vector mod(double divisor)
        {
            return new Vector(vector % divisor);
        }
    }

    //------------------------------------------------------------------------------------
    // dll 側のアセンブリは GetType で取得できないので独自の Matrix クラスを作ってラッピングする
    public class Matrix : ICbShowValue
    {
        Matrix()
        {
        }

        Matrix(MathNet.Numerics.LinearAlgebra.Matrix<double> m)
        {
            matrix = DenseMatrix.OfMatrix(m);
        }

        MathNet.Numerics.LinearAlgebra.Matrix<double> matrix = null;

        public string DataString => matrix.ToString().Trim('\r', '\n');

        //------------------------------------------------------------------------------------
        [ScriptMethod("Math" + ".Matrix.Create.OfColumns." + "1Column", "",
            "RS=>MathNumerics_CreateMatrixOf1Column"//"double 型のリストをもとに１列の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf1Column(List<double> m)
        {
            return new Matrix(DenseMatrix.OfColumns(new List<List<double>>() { m }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfColumns." + "2Columns", "",
            "RS=>MathNumerics_CreateMatrixOf2Columns"//"double 型のリストをもとに２列の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf2Columns(List<double> m1, List<double> m2)
        {
            return new Matrix(DenseMatrix.OfColumns(new List<List<double>>() { m1, m2 }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfColumns." + "3Columns", "",
            "RS=>MathNumerics_CreateMatrixOf3Columns"//"double 型のリストをもとに３列の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf3Columns(List<double> m1, List<double> m2, List<double> m3)
        {
            return new Matrix(DenseMatrix.OfColumns(new List<List<double>>() { m1, m2, m3 }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfColumns." + "4Columns", "",
            "RS=>MathNumerics_CreateMatrixOf4Columns"//"double 型のリストをもとに４列の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf4Columns(List<double> m1, List<double> m2, List<double> m3, List<double> m4)
        {
            return new Matrix(DenseMatrix.OfColumns(new List<List<double>>() { m1, m2, m3, m4 }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfColumns." + "Term", "",
            "RS=>MathNumerics_CreateMatrixOfTerm"//"<term> 次数をもとに <len> 行の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOfTerm(int term, int len)
        {
            var m = new List<List<double>>();
            for (int i = term; i >= 1; --i)
            {
                m.Add(ListFactory.MakeListDouble2(len, 0, 1, i));
            }
            m.Add(ListFactory.MakeListDouble(len, 1, 0));
            return new Matrix(DenseMatrix.OfColumns(m));
        }

        //------------------------------------------------------------------------------------
        [ScriptMethod("Math" + ".Matrix.Create.OfRows." + "1Row", "",
            "RS=>MathNumerics_CreateMatrixOf1Row"//"double 型のリストをもとに１行の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf1Row(List<double> m)
        {
            return new Matrix(DenseMatrix.OfRows(new List<List<double>>() { m }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfRows." + "2Rows", "",
            "RS=>MathNumerics_CreateMatrixOf2Rows"//"double 型のリストをもとに２行の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf2Rows(List<double> m1, List<double> m2)
        {
            return new Matrix(DenseMatrix.OfRows(new List<List<double>>() { m1, m2 }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfRows." + "3Rows", "",
            "RS=>MathNumerics_CreateMatrixOf3Rows"//"double 型のリストをもとに３行の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf3Rows(List<double> m1, List<double> m2, List<double> m3)
        {
            return new Matrix(DenseMatrix.OfRows(new List<List<double>>() { m1, m2, m3 }));
        }

        [ScriptMethod("Math" + ".Matrix.Create.OfRows." + "4Rows", "",
            "RS=>MathNumerics_CreateMatrixOf4Rows"//"double 型のリストをもとに４行の Matrix を作成します。"
            )]
        public static Matrix CreateMatrixOf4Rows(List<double> m1, List<double> m2, List<double> m3, List<double> m4)
        {
            return new Matrix(DenseMatrix.OfRows(new List<List<double>>() { m1, m2, m3, m4 }));
        }

        //------------------------------------------------------------------------------------
        [ScriptMethod("Math" + ".Matrix.Create." + nameof(CreateDivMatrix))]
        public static Matrix CreateDivMatrix(double dividend, Matrix m)
        {
            return new Matrix(dividend / m.matrix);
        }

        [ScriptMethod("Math" + ".Matrix.Create." + nameof(CreateModMatrix))]
        public static Matrix CreateModMatrix(double dividend, Matrix m)
        {
            return new Matrix(dividend % m.matrix);
        }

        [ScriptMethod("Math" + ".Matrix.Create." + nameof(MatrixList))]
        public static List<Matrix> MatrixList(List<Matrix> m)
        {
            return m;
        }

        [ScriptMethod("Math" + ".Matrix.Create." + "CreateVariable", "[ {0} ]")]
        public static Matrix CreateMatrixVariable(ref Matrix m)
        {
            return m;
        }

        [ScriptMethod("Math" + ".Matrix.Create." + "CreateVariableList", "[ {0} ]")]
        public static List<Matrix> CreateMatrixVariableList(ref List<Matrix> m)
        {
            return m;
        }

        //------------------------------------------------------------------------------------
        [ScriptMethod("Math" + ".Matrix." + nameof(MatrixToString), "",
            "RS=>MathNumerics_MatrixToString"//"Matrix を文字列に変換します。"
            )]
        public string MatrixToString()
        {
            return matrix.ToString();
        }

        [ScriptMethod("Math" + ".Matrix." + nameof(AsColumnMajorArray))]
        public List<double> AsColumnMajorArray()
        {
            return new List<double>(matrix.AsColumnMajorArray());
        }

        [ScriptMethod("Math" + ".Matrix." + nameof(AsRowMajorArray))]
        public List<double> AsRowMajorArray()
        {
            return new List<double>(matrix.AsRowMajorArray());
        }

        [ScriptMethod("Math" + ".Matrix." + nameof(Clear), "",
            "RS=>MathNumerics_Clear"//"Matrix をクリアします。"
            )]
        public void Clear()
        {
            matrix.Clear();
        }

        /// <summary>
        /// 逆行列を作成して返す
        /// </summary>
        /// <returns>逆行列</returns>
        [ScriptMethod("Math" + ".Matrix." + "Make Inverse", "",
            "RS=>MathNumerics_Inverse"//"逆行列を求めます。"
            )]
        public Matrix Inverse()
        {
            return new Matrix(matrix.Inverse());
        }

        [ScriptMethod("Math" + ".Matrix." + "+")]
        public Matrix add(Matrix m)
        {
            return new Matrix(matrix + m.matrix);
        }

        [ScriptMethod("Math" + ".Matrix." + "-")]
        public Matrix sub(Matrix m)
        {
            return new Matrix(matrix - m.matrix);
        }

        [ScriptMethod("Math" + ".Matrix." + "*")]
        public Matrix mul(Matrix m)
        {
            return new Matrix(matrix * m.matrix);
        }

        [ScriptMethod("Math" + ".Matrix." + "m/n")]
        public Matrix div(double divisor)
        {
            return new Matrix(matrix / divisor);
        }

        [ScriptMethod("Math" + ".Matrix." + "m%n")]
        public Matrix mod(double divisor)
        {
            return new Matrix(matrix % divisor);
        }

        /// <summary>
        /// 擬似逆行列を作成して返す
        /// </summary>
        /// <returns>擬似逆行列</returns>
        [ScriptMethod("Math" + ".Matrix." + nameof(PseudoInverse), "",
            "RS=>MathNumerics_PseudoInverse"//"擬似逆行列を求めます。"
            )]
        public Matrix PseudoInverse()
        {
            var svd = matrix.Svd(true);
            var S = new DiagonalMatrix(matrix.RowCount, matrix.ColumnCount, (1 / svd.S).ToArray());
            var m = svd.VT.Transpose() * S.Transpose() * svd.U.Transpose();
            return new Matrix(m);
        }

        [ScriptMethod("Math" + ".Matrix." + nameof(CreateVectorFromColumn), "",
            "RS=>MathNumerics_CreateVectorFromColumn"//"matrix の列から Vector を作成します。"
            )]
        public Vector CreateVectorFromColumn(int col)
        {
            return new Vector(matrix.Column(col));
        }

        [ScriptMethod("Math" + ".Matrix." + nameof(CreateVectorFromRow), "",
            "RS=>MathNumerics_CreateVectorFromRow"//"matrix の行から Vector を作成します。"
            )]
        public Vector CreateVectorFromRow(int row)
        {
            return new Vector(matrix.Row(row));
        }
    }
}
