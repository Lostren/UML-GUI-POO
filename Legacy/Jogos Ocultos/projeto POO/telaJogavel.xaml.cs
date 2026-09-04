using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Windows.Media.Effects;

namespace projeto_POO
{
    /// <summary>
    /// Interaction logic for telaJogavel.xaml
    /// </summary>
    public partial class telaJogavel : Window
    {
        private List<Image> imagensObjetos = new List<Image>();
        private Random random = new Random();

        private void VerificarVitoria()
        {
            List<Image> objetosDisponiveis = imagensObjetos
                .Where(Imagem => Imagem.Opacity > 0)
                .ToList();

            if (objetosDisponiveis.Count == 0)
            {
                vitoria ganhou = new vitoria();
                ganhou.Show();
                
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imagensObjetos.Add(aranhaItem);
            imagensObjetos.Add(ancoraItem);
            imagensObjetos.Add(anzolItem);
            imagensObjetos.Add(canetaItem);
            imagensObjetos.Add(chaveItem);
            imagensObjetos.Add(engrenagemItem);
            imagensObjetos.Add(mosqueteItem);
            imagensObjetos.Add(pistolaItem);
            imagensObjetos.Add(tuxItem);
        }
        public telaJogavel()
        {
            InitializeComponent();
        }

        private void esmaecerImagem(Image imagem,Label itemLista)
        {
            DoubleAnimation animacao = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1)
            };

            animacao.Completed += (s, e) =>
            {
                VerificarVitoria();
            };

            imagem.BeginAnimation(UIElement.OpacityProperty, animacao);

            itemLista.Opacity = 0.4;

            
        }

        private void indicarObjeto(Image imagem)
        {
            DropShadowEffect efeito = new DropShadowEffect()
            {
                Color = Colors.White,
                BlurRadius = 50,
                ShadowDepth = 0,
                Opacity = 0
            };

            imagem.Effect = efeito;

            DoubleAnimation animacaoBrilho = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.4),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(5)
            };

            ScaleTransform escala = new ScaleTransform(1, 1);
            imagem.RenderTransform = escala;
            imagem.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation animacaoEscala = new DoubleAnimation()
            {
                From = 1.0,
                To = 1.35,
                Duration = TimeSpan.FromSeconds(0.4),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(5)
            };

            efeito.BeginAnimation(DropShadowEffect.OpacityProperty, animacaoBrilho);
            escala.BeginAnimation(ScaleTransform.ScaleXProperty, animacaoEscala);
            escala.BeginAnimation(ScaleTransform.ScaleYProperty, animacaoEscala);
        }
        private void aranhaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(aranhaItem,aranhaLista);
        }

        private void ancoraItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(ancoraItem,ancoraLista);
        }

        private void anzolItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(anzolItem,anzolLista);
        }

        private void canetaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(canetaItem,canetaLista);
        }

        private void chaveItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(chaveItem,chaveLista);
        }

        private void engrenagemItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(engrenagemItem,engrenagemLista);
        }

        private void mosqueteItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(mosqueteItem,mosqueteLista);
        }

        private void pistolaItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(pistolaItem,pistolaLista);
        }

        private void tuxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            esmaecerImagem(tuxItem,tuxLista);
        }

        private void dicaButton_Click(object sender, RoutedEventArgs e)
        {
            List<Image> objetosDisponiveis = imagensObjetos
                .Where(Imagem => Imagem.Opacity > 0)
                .ToList();

            if (objetosDisponiveis.Count == 0)
            {
                return;
            }

            int indice = random.Next(objetosDisponiveis.Count);

            Image objetoEscolhido = objetosDisponiveis[indice];

            indicarObjeto(objetoEscolhido);
        }
    }
}
