﻿using PaginationSimulator.src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

namespace PaginationSimulator
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>

    public partial class Window1 : Window
    {
        public PagBajoDem sim;
        Thread t;
        List<Instruc> instruc;
        ManualResetEvent mrse;

        public Window1(PagBajoDem sim)
        {
            InitializeComponent();
            
            this.sim = sim;
            this.Closing += new CancelEventHandler(Window1_Closing);
            List<PagBajoDem> list = new List<PagBajoDem>();
            list.Add(sim);
            tamValues.ItemsSource = list;

            mrse = new ManualResetEvent(false);
            t = null;
            
        }

        private void Run()
        {
            for(int i = 0; i < instruc.Count; i++)
            {
                Console.WriteLine("asdasd");
                mrse.WaitOne();
                sim.ExInstruc(instruc[i], i);
                Thread.Sleep(1000);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            play.Visibility = Visibility.Hidden;
            method.Visibility = Visibility.Hidden;
            pause.Visibility = Visibility.Visible;
            Reset.Visibility = Visibility.Visible;

            sim.InitMarcos(genMarcosInit(sim.numMarcos));
            sim.alg = PagBajoDem.FIFO;
            instruc = genInstruc(sim.tamProc, 10);

            if (t == null)
            {
                Console.WriteLine("a");
                t = new Thread(new ThreadStart(Run));
                t.Start();
            }
            else 
            {
                Console.WriteLine("b");
                mrse.Set();
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            play.Visibility = Visibility.Visible;
            method.Visibility = Visibility.Visible;
            pause.Visibility = Visibility.Hidden;
            Reset.Visibility = Visibility.Hidden;

            t = null;
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            play.Visibility = Visibility.Visible;
            pause.Visibility = Visibility.Hidden;
            mrse.Reset();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow();
            win.Show();
            this.Hide();
        }
        

        void Window1_Closing(object sender, CancelEventArgs e)

        {

            Application.Current.Shutdown();

        }

        private void DG3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private bool[] genMarcosInit(int numMarcos)
        {
            Random rnd = new Random();
            bool[] marcos = new bool[numMarcos];
            for (int i = 0; i < numMarcos; i++)
                marcos[i] = rnd.Next(10) >= 3;
            return marcos;
        }

        private List<Instruc> genInstruc(int tamProc, int numInst)
        {
            List<Instruc> instruc = new List<Instruc>();
            Random rnd = new Random();
            for (int i = 0; i < numInst; i++)
                instruc.Add(new Instruc(rnd.Next(0, tamProc), rnd.Next(10) < 5));
            return instruc;
        }
    }
}
