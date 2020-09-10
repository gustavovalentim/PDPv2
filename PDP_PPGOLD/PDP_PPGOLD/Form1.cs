using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDP_PPGOLD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            PDP MeuProblema = new PDP();
            MeuProblema.GerarNosAleatoriamente(5);
            MeuProblema.CalcularMatrizDistanciasEuclidiana();
            MeuProblema.GerarVeiculosAleatoriamente(5);
            MeuProblema.GerarRequisicoesAleatoriamente(10);
            //MeuProblema.LerArquivoVeiculos(@"D:\Dropbox\UFPR\PPGOLD\Modelagem na Cadeia de Suprimentos\PDP\Veiculos.txt");
            //MeuProblema.LerArquivoRequisicoes(@"D:\Dropbox\UFPR\PPGOLD\Modelagem na Cadeia de Suprimentos\PDP\Requisicoes.txt");
            //MeuProblema.LerArquivoDistancias(@"D:\Dropbox\UFPR\PPGOLD\Modelagem na Cadeia de Suprimentos\PDP\Distancias.txt");
            MeuProblema.CriarResolverProblema();
        }
    }
}
