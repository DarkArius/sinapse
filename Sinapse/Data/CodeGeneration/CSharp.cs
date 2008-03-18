/***************************************************************************
 *   Sinapse Neural Networking Tool         http://sinapse.googlecode.com  *
 *  ---------------------------------------------------------------------- *
 *   Copyright (C) 2006-2008 Cesar Roberto de Souza <cesarsouza@gmail.com> *
 *                                                                         *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 3 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using AForge;
using AForge.Neuro;

using Sinapse.Data.Network;


namespace Sinapse.Data.CodeGeneration
{
    internal sealed class CSharp : CodeGenerator
    {

        public CSharp(NetworkContainer network)
            : base(network)
        {
        }


        protected override void build(StringBuilder cB)
        {
            cB.AppendLine("/*********************************************************************************");
            cB.AppendFormat(" *  Code generated by Sinapse Neural Networking Tool in {0}\n", DateTime.Now);
            cB.AppendLine(" * ----------------------------------------------------------------------------- *");
            cB.AppendLine(" *                                                                               *");
            cB.AppendLine(" *  You are free to use this code for any purpose you wish, in any application   *");
            cB.AppendLine(" *  under any licensing terms, but only if you add a user-visible reference to   *");
            cB.AppendLine(" *  the use of Sinapse inside your program and don't separate the generated code *");
            cB.AppendLine(" *  from this disclaimer. Also, please pay attention to the following notice:    *");
            cB.AppendLine(" *                                                                               *");
            cB.AppendLine(" *      This code was generated in the hope that it will be useful,              *");
            cB.AppendLine(" *      but WITHOUT ANY WARRANTY; without even the implied warranty              *");
            cB.AppendLine(" *      of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                  *");
            cB.AppendLine(" *                                                                               *");
            cB.AppendLine(" *      Sinapse developer(s) are not and cannot be liable for any direct,        *");
            cB.AppendLine(" *      indirect, incidental, special, exemplary or consequential damages,       *");
            cB.AppendLine(" *      including, but not limited to, procurement of substitute goods or        *");
            cB.AppendLine(" *      services; loss of use, data, profits or business interruption.           *");
            cB.AppendLine(" *                                                                               *");
            cB.AppendLine(" *********************************************************************************/");
            cB.AppendLine();
            cB.AppendLine("using System;");
            cB.AppendLine("using System.Collections.Generic;");
            cB.AppendLine("using System.Text;");
            cB.AppendLine();
            cB.AppendLine("namespace NNApplication");
            cB.AppendLine("{");
            cB.AppendLine("    class NeuralNetwork");
            cB.AppendLine("    {");
            cB.AppendLine();
            cB.AppendLine("        private double[][][] network;");
            cB.AppendLine("        private double[][] neuronsOutput;");
            cB.AppendLine();
            cB.AppendLine("        public NeuralNetwork()");
            cB.AppendLine("        {");
            cB.AppendLine();
            cB.AppendLine(ident(createNetworkGenerator(), 12));
            cB.AppendLine();
            cB.AppendLine("        }");
            cB.AppendLine();
            cB.AppendLine("        #region Computing Functions");
            cB.AppendLine(ident(createActivationFunction(), 8));
            cB.AppendLine();
            cB.AppendLine(ident(createComputeFunction(), 8));
            cB.AppendLine("        #endregion");
            cB.AppendLine();
            cB.AppendLine("        #region Data Normalization Functions");
            cB.AppendLine(ident(createNormalizationFunctions(), 8));
            cB.AppendLine();
            cB.AppendLine(ident(createRevertFunctions(),8));
            cB.AppendLine("        #endregion");
            cB.AppendLine("    }");
            cB.AppendLine("}");

        }


        //---------------------------------------------


        #region Network Compute Functions
        private string createComputeFunction()
        {
            StringBuilder sb = new StringBuilder();

            // Add function header
            sb.Append("private void compute(");
            for (int i = 0; i < this.Network.Schema.InputColumns.Length; ++i)
            {
                string col = this.Network.Schema.InputColumns[i];
                if (this.Network.Schema.IsCategory(col))
                {
                    sb.Append("string ");
                }
                else
                {
                    sb.Append("double ");
                }
                sb.Append(col.Replace(" ","_"));
            }
            
            for (int i = 0; i < this.Network.Schema.OutputColumns.Length; ++i)
            {
                string col = this.Network.Schema.OutputColumns[i];
                if (this.Network.Schema.IsCategory(col))
                {
                    sb.Append("out string ");
                }
                else
                {
                    sb.Append("out double ");
                }
                sb.Append(col.Replace(" ", "_"));
                
                if (i < this.Network.Schema.OutputColumns.Length - 1)
                    sb.Append(", ");
            }
            sb.Append("\n{");

            // Add function body
            sb.Append("for (int i = 0; i < network.Length; ++i)\n");
            sb.Append("{\n");
            sb.Append("   ");



            sb.Append("\n}");

            return sb.ToString();

        }

        private string createActivationFunction()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("private double activationFunction(double x)");
            sb.AppendLine("{");
      //      sb.AppendLine(ident(createActivationFunction(this.Network.ActivationNetwork[0][0].ActivationFunction),4));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string createActivationFunction(BipolarSigmoidFunction function)
        {
            return String.Format("return ((2 / (1 + Math.Exp( -{0} * x))) - 1);", function.Alpha);
        }

        private string createActivationFunction(SigmoidFunction function)
        {
            return String.Format("return ( 1 / ( 1 + Math.Exp( -{0} * x ) ) );", function.Alpha);
        }

        private string createActivationFunction(ThresholdFunction function)
        {
            return "return ( x >= 0 ) ? 1 : 0;";
        }
        #endregion


        //---------------------------------------------


        #region Network Weight Generation Functions
        private string createNetworkGenerator()
        {
            StringBuilder sb = new StringBuilder();
            AForge.Neuro.Network nNetwork = this.Network.ActivationNetwork;
            sb.AppendLine("#region Sinapse Network Weights");
            sb.AppendFormat("this.network = new double[{0}][][];\n",nNetwork.LayersCount);

            for (int i = 0; i < nNetwork.LayersCount; ++i)
            {
                sb.AppendFormat("\nthis.network[{0}] = new double [{1}][];\n", i, nNetwork[i].NeuronsCount);
                
                for (int j = 0; j < nNetwork[i].NeuronsCount; ++j)
                {
                    sb.AppendFormat("this.network[{0}][{1}] = new double[] {2};\n", i, j, createNeuronArray(i,j));
                }
            }
            sb.AppendLine("#endregion");
            return sb.ToString();
        }

        private string createNeuronArray(int layer, int neuron)
        {
            StringBuilder sb = new StringBuilder();
            Neuron nNeuron = this.Network.ActivationNetwork[layer][neuron];

            sb.Append("{ ");
            for (int i = 0; i < nNeuron.InputsCount; ++i)
            {
                sb.Append(nNeuron[i]);
                if (i < nNeuron.InputsCount - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" }");
            return sb.ToString();
        }
        #endregion

        
        //---------------------------------------------


        #region Normalization Functions
        private string createNormalizationFunctions()
        {
            StringBuilder sb = new StringBuilder();

            DoubleRange from, to;
            foreach (String col in this.Network.Schema.AllColumns)
            {

                from = this.Network.Schema.DataRanges.GetRange(col);
                to = this.Network.Schema.DataRanges.ActivationFunctionRange;
                      
                if ((this.Network.Schema.IsCategory(col)))
                {
                    sb.AppendFormat("private double normalize_{0}(string value)\n", col.Replace(" ", "_"));
                    sb.AppendLine("{");
                    sb.AppendLine("   double id;");
                    sb.AppendLine();
                    sb.AppendLine("   switch (value)");
                    sb.AppendLine("   {");

                    foreach (DataCategory category in this.Network.Schema.DataCategories.GetCaptionList(col))
                    {
                        sb.AppendFormat("      case {0}:\n", category.Value);
                        sb.AppendFormat("         id = {0};\n", category.Id);
                        sb.AppendLine  ("      break;\n\n");
                    }
                    sb.AppendLine("   }");
                    
                    sb.AppendLine("}");
                }
                else
                {
                    sb.AppendFormat("private double normalize_{0}(double value)\n", col.Replace(" ", "_"));
                    sb.AppendLine("{");
                }
                // return ((value - from.Min) * (to.Length) / (from.Length)) + to.Min;
                sb.AppendFormat("return ((value - {0}) * ({1}) / ({2}) + {3};",from.Min,to.Length,from.Length, to.Min);
                sb.Append("\n}\n\n");
            }

            return sb.ToString();
        }

        private string createRevertFunctions()
        {
            StringBuilder sb = new StringBuilder();

            DoubleRange from, to;
            foreach (String col in this.Network.Schema.AllColumns)
            {

                from = this.Network.Schema.DataRanges.ActivationFunctionRange;
                to = this.Network.Schema.DataRanges.GetRange(col);

                if ((this.Network.Schema.IsCategory(col)))
                {
                    sb.AppendFormat("private string revert_{0}(double value, double threshold)\n", col.Replace(" ", "_"));
                    sb.AppendLine("{");
                    sb.Append("   double normData = ");
                    sb.AppendFormat("((value - {0}) * ({1}) / ({2}) + {3};", from.Min, to.Length, from.Length, to.Min);

                    sb.AppendLine();
                    sb.AppendLine("   switch (normData)");
                    sb.AppendLine("   {");

                    foreach (DataCategory category in this.Network.Schema.DataCategories.GetCaptionList(col))
                    {
                        sb.AppendFormat("      case {0}:\n", category.Value);
                        sb.AppendFormat("         id = {0};\n", category.Id);
                        sb.AppendLine("      break;\n\n");
                    }
                    sb.AppendLine("   }");

                    sb.AppendLine("}");
                }
                else
                {
                    sb.AppendFormat("private string revert_{0}(double value)\n", col.Replace(" ", "_"));
                    sb.AppendLine("{");
                    sb.AppendFormat("return ((value - {0}) * ({1}) / ({2}) + {3};", from.Min, to.Length, from.Length, to.Min);
                }
                // return ((value - from.Min) * (to.Length) / (from.Length)) + to.Min;
                sb.Append("\n}\n\n");
            }

            return sb.ToString();
        }
        #endregion


    }
}