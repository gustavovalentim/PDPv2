using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace PDP_PPGOLD
{
    class PDP
    {
        public int QuantidadeVeiculos;
        public int QuantidadeRequisicoes;
        public int QuantidadeNos;
        public Veiculo[] Veiculos;
        public Requisicao[] Requisicoes;
        public No[] Nos;
        public double[,] MatrizDistancias;
        public GRBEnv Ambiente;
        public GRBModel Modelo;
        public void CriarResolverProblema()
        {
            Ambiente = new GRBEnv(@"C:\Teste\ModeloPDP.log");
            Modelo = new GRBModel(Ambiente);
            GRBVar[,,] X = new GRBVar[QuantidadeNos, QuantidadeNos, QuantidadeVeiculos];
            GRBVar[,,,] Y = new GRBVar[QuantidadeNos, QuantidadeNos, QuantidadeVeiculos, QuantidadeRequisicoes];
            GRBVar[,,] Z = new GRBVar[QuantidadeNos, QuantidadeNos, QuantidadeVeiculos];
            //função objetivo, (10) e (11)
            for (int k=0;k<QuantidadeVeiculos;k++)
            {
                for(int i=0;i<QuantidadeNos;i++)
                {
                    for(int j=0;j<QuantidadeNos;j++)
                    {
                        if(i!=j)
                        {
                            X[i, j, k] = Modelo.AddVar(0, 1, MatrizDistancias[i, j] * Veiculos[k].CustoPorDistancia, GRB.BINARY, "x_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString());
                        }
                    }
                }
            }
            for(int r=0;r<QuantidadeRequisicoes;r++)
            {
                for (int k = 0; k < QuantidadeVeiculos; k++)
                {
                    for (int i = 0; i < QuantidadeNos; i++)
                    {
                        for (int j = 0; j < QuantidadeNos; j++)
                        {
                            if(i!=j)
                            {
                                Y[i, j, k, r] = Modelo.AddVar(0, 1, 0, GRB.BINARY, "y_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString() + "_" + r.ToString());
                            }
                        }
                    }
                }
            }
            for (int k = 0; k < QuantidadeVeiculos; k++)
            {
                for (int i = 0; i < QuantidadeNos; i++)
                {
                    for (int j = 0; j < QuantidadeNos; j++)
                    {
                        if (i != j)
                        {
                            Z[i, j, k] = Modelo.AddVar(0, 1, 0, GRB.BINARY, "z_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString());
                        }
                    }
                }
            }
            GRBLinExpr expr = new GRBLinExpr();
            GRBLinExpr expr1 = new GRBLinExpr();
            GRBLinExpr expr2 = new GRBLinExpr();
            //Conjunto (1)
            for (int k = 0; k < QuantidadeVeiculos; k++)
            {
                expr.Clear();
                int OrigemAtual = Veiculos[k].Origem;
                for (int j = 0; j < QuantidadeNos; j++)
                {
                    if (j != OrigemAtual)
                    {
                        expr.AddTerm(1, X[OrigemAtual, j, k]);
                    }
                }
                Modelo.AddConstr(expr <= 1, "R1_" + k.ToString());
            }
            //Conjunto (2)
            for (int k=0;k<QuantidadeVeiculos;k++)
            {
                expr1.Clear();
                expr2.Clear();
                int OrigemAtual = Veiculos[k].Origem;
                int DestinoAtual = Veiculos[k].Destino;
                for(int j=0;j<QuantidadeNos;j++)
                {
                    if(j != OrigemAtual)
                    {
                        expr1.AddTerm(1, X[OrigemAtual, j, k]);
                    }
                }
                for(int j=0;j<QuantidadeNos;j++)
                {
                    if(j != DestinoAtual)
                    {
                        expr2.AddTerm(1, X[j, DestinoAtual, k]);
                    }
                }
                Modelo.AddConstr(expr1 == expr2, "R2_" + k.ToString());
            }
            // conjunto (3)

            for(int k=0;k<QuantidadeVeiculos;k++)
            {
                int OrigemAtual = Veiculos[k].Origem;
                int DestinoAtual = Veiculos[k].Destino;
                for(int i=0;i<QuantidadeNos;i++)
                {
                    if(i!=OrigemAtual && i!=DestinoAtual)
                    {
                        expr1.Clear();
                        expr2.Clear();
                        for(int j=0;j<QuantidadeNos;j++)
                        {
                            if(j!=i)
                            {
                                expr1.AddTerm(1, X[i, j, k]);
                            }
                        }
                        for(int j=0;j<QuantidadeNos;j++)
                        {
                            if(i!=j)
                            {
                                expr2.AddTerm(1, X[j, i, k]);
                            }
                        }
                        Modelo.AddConstr(expr1 - expr2 == 0, "R3_" + k.ToString() + "_" + i.ToString());
                    }
                }
            }
            //conjunto (4)
            for(int r=0;r<QuantidadeRequisicoes;r++)
            {
                int ColetaAtual = Requisicoes[r].Coleta;
                expr.Clear();
                for(int k=0;k<QuantidadeVeiculos;k++)
                {
                    for(int j=0;j<QuantidadeNos;j++)
                    {
                        if(j != ColetaAtual)
                        {
                            expr.AddTerm(1, Y[ColetaAtual, j, k, r]);
                        }
                    }
                }
                Modelo.AddConstr(expr == 1, "R4_" + r.ToString());
            }
            //conjunto (5)
            for(int r=0;r<QuantidadeRequisicoes;r++)
            {
                int EntregaAtual = Requisicoes[r].Entrega;
                expr.Clear();
                for(int k=0;k<QuantidadeVeiculos;k++)
                {
                    for(int j=0;j<QuantidadeNos;j++)
                    {
                        if(j!=EntregaAtual)
                        {
                            expr.AddTerm(1, Y[j, EntregaAtual, k, r]);
                        }
                    }
                }
                Modelo.AddConstr(expr == 1, "R5_" + r.ToString());
            }
            //conjunto (6)
            //faremos depois

            //conjunto (7)
            for(int k=0;k<QuantidadeVeiculos;k++)
            {
                for(int r=0;r<QuantidadeRequisicoes;r++)
                {
                    int ColetaAtual = Requisicoes[r].Coleta;
                    int EntregaAtual = Requisicoes[r].Entrega;
                    for(int i=0;i<QuantidadeNos;i++)
                    {
                        if(i!=ColetaAtual && i!=EntregaAtual)
                        {
                            expr1.Clear();
                            expr2.Clear();
                            for (int j = 0; j < QuantidadeNos; j++)
                            {
                                if (j != i)
                                {
                                    expr1.AddTerm(1, Y[i, j, k, r]);
                                }
                            }
                            for (int j = 0; j < QuantidadeNos; j++)
                            {
                                if (j != i)
                                {
                                    expr2.AddTerm(1, Y[j, i, k, r]);
                                }
                            }
                            Modelo.AddConstr(expr1 - expr2 == 0, "R7_" + k.ToString() + "_" + r.ToString() + "_" + i.ToString());
                        }
                    }
                }
            }
            //conjunto (8)
            for(int i=0;i<QuantidadeNos;i++)
            {
                for(int j=0;j<QuantidadeNos;j++)
                {
                    if(i!=j)
                    {
                        for(int k=0; k<QuantidadeVeiculos;k++)
                        {
                            for(int r=0;r<QuantidadeRequisicoes;r++)
                            {
                                Modelo.AddConstr(Y[i, j, k, r] <= X[i, j, k], "R8_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString() + "_" + r.ToString());
                            }
                        }
                    }
                }
            }
            //conjunto (9)
            for(int k=0;k<QuantidadeVeiculos;k++)
            {
                for(int i=0;i<QuantidadeNos;i++)
                {
                    for(int j=0;j<QuantidadeNos;j++)
                    {
                        if(i!=j)
                        {
                            expr.Clear();
                            for(int r=0;r<QuantidadeRequisicoes;r++)
                            {
                                expr.AddTerm(Requisicoes[r].Quantidade, Y[i, j, k, r]);
                            }
                            Modelo.AddConstr(expr <= Veiculos[k].Capacidade * X[i, j, k], "R9_" + k.ToString() + "_" + i.ToString() + "_" + j.ToString());
                        }
                    }
                }
            }
            //Conjunto (12)
            for(int i=0;i<QuantidadeNos;i++)
            {
                for(int j=0;j<QuantidadeNos;j++)
                {
                    for(int k=0;k<QuantidadeNos;k++)
                    {
                        if(i!=j)// && i!=Veiculos[k].Origem && j!=Veiculos[k].Destino)
                        {
                            Modelo.AddConstr(X[i, j, k] <= Z[i, j, k], "R12_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString());
                        }
                    }
                }
            }
            //Conjunto (13)
            for (int i = 0; i < QuantidadeNos; i++)
            {
                for (int j = 0; j < QuantidadeNos; j++)
                {
                    for (int k = 0; k < QuantidadeNos; k++)
                    {
                        if (i!=j && i != Veiculos[k].Origem && j != Veiculos[k].Destino)
                        {
                            Modelo.AddConstr(Z[i, j, k] + Z[j, i, k] == 1, "R13_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString());
                        }
                    }
                }
            }
            //conjunto (14)
            for (int i = 0; i < QuantidadeNos; i++)
            {
                for (int j = 0; j < QuantidadeNos; j++)
                {
                    for(int l=0;l<QuantidadeNos;l++)
                    {
                        for (int k = 0; k < QuantidadeNos; k++)
                        {
                            if (i!=j && j!=l && l!=i && i != Veiculos[k].Origem && j != Veiculos[k].Origem && l!=Veiculos[k].Destino)
                            {
                                Modelo.AddConstr(Z[i, j, k] + Z[j, l, k] + Z[l, i, k] <= 2, "R14_" + i.ToString() + "_" + j.ToString() + "_" + l.ToString() + "_" + k.ToString());
                            }
                        }
                    }
                }
                //1 -> 5 -> 3 -> 7 -> 2 -> 1
                //z_1_5=1; z_1_3=1; z_1_7=1; z_1_2=1
                //z_3_7=1; z_3_2=1; z_3_1;
                //z_7_2=1; z_7_1=1
                //z_1_3 + z_3_7 + z_7_1 <= 2
            }
            //Modelo.Parameters.TimeLimit = 60;
            //Modelo.Parameters.MIPGap = 0.05;
            Modelo.Optimize();
            MessageBox.Show(Modelo.ObjVal.ToString());
            Modelo.Write("C:\\Teste\\ModeloPDP.lp");
            Modelo.Write("C:\\Teste\\ModeloPDP.sol");
        }
        public void GerarNosAleatoriamente(int qtdNos)
        {
            QuantidadeNos = qtdNos;
            Nos = new No[QuantidadeNos];
            Random Aleatorio = new Random(3);
            for(int i=0;i<QuantidadeNos;i++)
            {
                Nos[i] = new No();
                Nos[i].Numero = i;
                Nos[i].Latitude = Aleatorio.NextDouble() * 20;
                Nos[i].Longitude = Aleatorio.NextDouble() * 20;
            }
        }
        public void CalcularMatrizDistanciasEuclidiana()
        {
            MatrizDistancias = new double[QuantidadeNos, QuantidadeNos];
            for(int i=0;i<QuantidadeNos;i++)
            {
                for(int j=0;j<QuantidadeNos;j++)
                {
                    double DeltaLat = Nos[i].Latitude - Nos[j].Latitude;
                    double DeltaLon = Nos[i].Longitude - Nos[j].Longitude;
                    MatrizDistancias[i, j] = Math.Sqrt(DeltaLat * DeltaLat + DeltaLon * DeltaLon);
                }
            }
        }
        public void GerarVeiculosAleatoriamente(int qtdVeiculos)
        {
            QuantidadeVeiculos = qtdVeiculos;
            Veiculos = new Veiculo[QuantidadeVeiculos];
            Random Aleatorio = new Random(1);
            for (int i = 0; i < QuantidadeVeiculos; i++)
            {
                Veiculos[i] = new Veiculo();
                Veiculos[i].Numero = i;
                Veiculos[i].Origem = Aleatorio.Next(0,QuantidadeNos);
                Veiculos[i].Destino = Aleatorio.Next(0, QuantidadeNos);
                if(Veiculos[i].Origem == Veiculos[i].Destino)
                {
                    //0 0 -> 0 1; 1 1 -> 1 2; ...; 4 4 -> 4 0
                    Veiculos[i].Destino = (Veiculos[i].Destino + 1) % QuantidadeNos;
                }
                Veiculos[i].Capacidade = 5 * Aleatorio.Next(1, 5);
                if(Veiculos[i].Capacidade==5)
                {
                    Veiculos[i].CustoPorDistancia = 1;
                }
                else if (Veiculos[i].Capacidade == 10)
                {
                    Veiculos[i].CustoPorDistancia = 1.8;
                }
                else if (Veiculos[i].Capacidade == 15)
                {
                    Veiculos[i].CustoPorDistancia = 2.5;
                }
                else 
                {
                    Veiculos[i].CustoPorDistancia = 3.0;
                }
            }
        }
        public void LerArquivoVeiculos(string Caminho)
        {
            string[] s = File.ReadAllLines(Caminho);
            QuantidadeVeiculos = s.GetLength(0) - 1;
            Veiculos = new Veiculo[QuantidadeVeiculos];
            for(int i=0;i<QuantidadeVeiculos;i++)
            {
                string[] s1 = s[i + 1].Split(';');
                Veiculos[i] = new Veiculo();
                Veiculos[i].Numero = int.Parse(s1[0]);
                Veiculos[i].Origem = int.Parse(s1[1]);
                Veiculos[i].Destino = int.Parse(s1[2]);
                Veiculos[i].Capacidade = double.Parse(s1[3]);
                Veiculos[i].CustoPorDistancia = double.Parse(s1[4]);
            }
        }
        public void GerarRequisicoesAleatoriamente(int qtdRequisicoes)
        {
            QuantidadeRequisicoes = qtdRequisicoes;
            Requisicoes = new Requisicao[QuantidadeRequisicoes];
            Random Aleatorio = new Random(2);
            for (int i = 0; i < QuantidadeRequisicoes; i++)
            {                
                Requisicoes[i] = new Requisicao();
                Requisicoes[i].Numero = i;
                Requisicoes[i].Coleta = Aleatorio.Next(0, QuantidadeNos);
                Requisicoes[i].Entrega = Aleatorio.Next(0, QuantidadeNos);
                if(Requisicoes[i].Entrega == Requisicoes[i].Coleta)
                {
                    Requisicoes[i].Entrega = (Requisicoes[i].Entrega + 1) % QuantidadeNos;
                }
                Requisicoes[i].Quantidade = Aleatorio.Next(2, 11);
            }
        }
        public void LerArquivoRequisicoes(string Caminho)
        {
            string[] s = File.ReadAllLines(Caminho);
            QuantidadeRequisicoes = s.GetLength(0) - 1;
            Requisicoes = new Requisicao[QuantidadeRequisicoes];
            for (int i = 0; i < QuantidadeRequisicoes; i++)
            {
                string[] s1 = s[i + 1].Split(';');
                Requisicoes[i] = new Requisicao();
                Requisicoes[i].Numero = int.Parse(s1[0]);
                Requisicoes[i].Coleta = int.Parse(s1[1]);
                Requisicoes[i].Entrega = int.Parse(s1[2]);
                Requisicoes[i].Quantidade = double.Parse(s1[3]);
            }
        }
        public void LerArquivoDistancias(string Caminho)
        {
            string[] s = File.ReadAllLines(Caminho);
            QuantidadeNos = int.Parse(s[0]);
            MatrizDistancias = new double[QuantidadeNos, QuantidadeNos];
            int QtdLinhas = s.GetLength(0);
            for (int i = 2; i < QtdLinhas; i++)
            {
                string[] s1 = s[i].Split(';');
                MatrizDistancias[int.Parse(s1[0]), int.Parse(s1[1])] = double.Parse(s1[2]);
                MatrizDistancias[int.Parse(s1[1]), int.Parse(s1[0])] = double.Parse(s1[2]);
            }
        }
    }
    class Veiculo
    {
        public int Numero;
        public int Origem;
        public int Destino;
        public double Capacidade;
        public double CustoPorDistancia;        
    }
    class Requisicao
    {
        public int Numero;
        public int Coleta;
        public int Entrega;
        public double Quantidade;
    }
    class No
    {
        public int Numero;
        public double Latitude;
        public double Longitude;
    }
}
