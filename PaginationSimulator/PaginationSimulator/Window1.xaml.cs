﻿using PaginationSimulator.src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using PaginationSimulator.tables;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Data;

namespace PaginationSimulator
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>

    public partial class Window1 : Window
    {
        MainWindow mw;
        public PagBajoDem sim;
        Thread t;
        Instruc[] instruc;
        ManualResetEvent mrse;
        ObservableCollection<ParseInst> instList;
        ObservableCollection<Mem> memList;

        public Window1(MainWindow mw, PagBajoDem sim)
        {
            InitializeComponent();

            this.mw = mw;
            this.sim = sim;

            instList = new ObservableCollection<ParseInst>();
            memList = new ObservableCollection<Mem>();
            //t = null;
            mrse = new ManualResetEvent(initialState: true);

            this.Closing += new CancelEventHandler(Window1_Closing);
            List<tempSim> list = new List<tempSim>();
            list.Add(new tempSim(sim.tamMarco + " Bytes", sim.tamSO + " Bytes", sim.tamProc + " Bytes", sim.tamMP + " Bytes"));
            tamValues.ItemsSource = list;

            initInst();
            initMP();
            initMS();
            initTablaPag();
        }

        private void initInst()
        {
            InstDG.DataContext = instList;
        }

        private void initMP()
        {
            int i = 0;
            for (; i < sim.numMarcosSO; i++)
                memList.Add(new Mem(i, "SO"));

            for (; i < sim.numMarcos; i++)
                memList.Add(new Mem(i, "Libre"));

            memDG.ItemsSource = memList;
        }

        private void initMS()
        {
            // Secondary memory table
            // Generate aleatory positions
            List<MemSec> tempSec = new List<MemSec>();
            for (int i = 0; i < sim.numPagProc; i++)
                tempSec.Add(new MemSec(i));
            secDG.ItemsSource = tempSec;
        }

        private void initTablaPag()
        {
            pageTableDG.ItemsSource = sim.tablaPag;
        }

        private void Run()
        {
            for (int i = 0; i < instruc.Length; i++)
            {
                Thread.Sleep(3000);
                mrse.WaitOne();
                Console.WriteLine("New instruction...");
                sim.ExInstruc(instruc[i], i);
            }
            Console.WriteLine("All done!");
            this.Dispatcher.Invoke(() => resetSimul());
            
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("PLAY");

            play.Visibility = Visibility.Hidden;
            method.Visibility = Visibility.Hidden;
            pause.Visibility = Visibility.Visible;
            Reset.Visibility = Visibility.Visible;

            //sim.InitMarcos(genMarcosInit(sim.numMarcos));
            //bool[] marcos = Mem.parse(memList);
            sim.InitMarcos(Mem.parse(memList));
            sim.alg = method.Text == "FIFO" ? PagBajoDem.FIFO : PagBajoDem.LRU;
            //instruc = genInstruc(sim.tamProc, 10);
            instruc = ParseInst.parse(instList);

            //for (int i = 0; i < InstDG.Items.Count; i++)
            //    Console.WriteLine($"instruc: (dir={instruc[i].dir}, lec={instruc[i].lec})");
            //    //Console.WriteLine($"instruc: (dir={instList[i].dir}, lec={instList[i].lec})");

            //for (int i = 0; i < memDG.Items.Count; i++)
            //    //Console.WriteLine(memList[i].okupa);
            //    Console.WriteLine(marcos[i]);

            //for(int i = 0; i < instruc.Length; i++)
            //{
            //    sim.ExInstruc(instruc[i], i);
            //}

            //return;
            if (true)
            {
                Console.WriteLine("Creando nuevo thread...");
                t = new Thread(new ThreadStart(Run));
                t.Start();
            }
            else
            {
                Console.WriteLine("Thread ya existente...");
                mrse.Set();
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            resetSimul();
        }

        private void resetSimul()
        {
            Console.WriteLine("RESET");
            play.Visibility = Visibility.Visible;
            method.Visibility = Visibility.Visible;
            pause.Visibility = Visibility.Hidden;
            Reset.Visibility = Visibility.Hidden;
            resume.Visibility = Visibility.Hidden;

            mrse.Reset();
            try
            {
                t.Abort();
            }catch(ThreadAbortException e)
            {
                Console.WriteLine("akjbsdjlanmsdkña,s");
            }
            
            mrse.Set();
            sim.Clear();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("PAUSE");
            resume.Visibility = Visibility.Visible;
            pause.Visibility = Visibility.Hidden;
            mrse.Reset();
        }

        private void resume_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("RESUME");
            pause.Visibility = Visibility.Visible;
            resume.Visibility = Visibility.Hidden;
            mrse.Set();
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow win = new MainWindow();
            //win.Show();
            mw.Show();
            this.Hide();
            //t = null;
        }

        void Window1_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
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

        private void addInst_Click(object sender, RoutedEventArgs e)
        {   
            ParseInst tempInst = new ParseInst(0, "L");
            instList.Add(tempInst);
        }

        private void OnKeyUpDir (object sender, KeyEventArgs e)
        {
            TextBox tb = ((TextBox)sender);

            if (tb.Text.Length == 2 && tb.Text.Substring(0, 1) == "0")
                tb.Text = tb.Text.Substring(1, 1);
            try
            {
                while (int.Parse(tb.Text) >= sim.tamProc)
                    tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
                
                instList[InstDG.SelectedIndex].dir = int.Parse(tb.Text);
            }
            catch (FormatException ex)
            {
                tb.Text = "";
                instList[InstDG.SelectedIndex].dir = 0;
            }
            
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = ((TextBox)sender);
            if (tb.Text == "") tb.Text = "0";
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox temp = (TextBox)sender;
            temp.Text = temp.Text == "L" ? "E" : "L";
            instList[InstDG.SelectedIndex].lec = temp.Text;
        }

        private void TextBox_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            TextBox temp = (TextBox)sender;
            if (temp.Text == "SO") return;
            temp.Text = temp.Text == "Libre" ? "Ocupado" : "Libre";
            memList[memDG.SelectedIndex].okupa = temp.Text;
        }

        private static bool IsKeyADigit(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9);
        }

        private void readCSVInst_Click(object sender, RoutedEventArgs e)
        {
            // erase previous list
            instList.Clear();

            // read csv and save it into observable list
            //selectt path
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                string path = dialog.FileName;
                StreamReader reader = null;
                try
                {
                    reader = new StreamReader($@"{path}");
                }
                catch
                {
                    using (var dialog_error = new Ookii.Dialogs.Wpf.TaskDialog())
                    {
                        dialog_error.WindowTitle = "Error de lectura";
                        dialog_error.Content = "Error inesperado, verifique que el archivo no se esté usando por otro programa.";

                        var continueButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Continue");
                        dialog_error.CustomMainIcon = SystemIcons.Warning;
                        dialog_error.Buttons.Add(continueButton);

                        Ookii.Dialogs.Wpf.TaskDialogButton button = dialog_error.ShowDialog();
                        //if (button == continueButton)
                        return;
                    }
                }
                

                //read from path    
                List<int> listDir = new List<int>();
                List<string> listLect = new List<string>();
                for (int i = 0; i < 2; i++)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if(i == 0)
                    {
                        // add logic dir
                        foreach (string item in values)
                        {   
                            if((int.Parse(item)) < sim.tamProc){
                                listDir.Add(int.Parse(item));
                            }
                            else
                            {
                                //error dialog
                                using (var dialog_error = new Ookii.Dialogs.Wpf.TaskDialog())
                                {
                                    dialog_error.WindowTitle = "Error de lectura";
                                    dialog_error.Content = "Una de las direcciones ingresadas no es valida para la configuración usada, por favor cambie la configuración de la simulación o cambie las instrucciones.";

                                    var continueButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Continue");
                                    dialog_error.CustomMainIcon = SystemIcons.Warning;
                                    dialog_error.Buttons.Add(continueButton);

                                    Ookii.Dialogs.Wpf.TaskDialogButton button = dialog_error.ShowDialog();
                                    //if (button == continueButton)
                                    return; 
                                }
                            }
 
                        }
                    }
                    else if (i == 1)
                    {
                        // add type of intruction
                        foreach (string item in values)
                        {
                            if(item == "L" || item == "E")
                            {
                                listLect.Add(item);
                            }
                            else
                            {
                                //error dialog
                                using (var dialog_error = new Ookii.Dialogs.Wpf.TaskDialog())
                                {
                                    dialog_error.WindowTitle = "Error de lectura";
                                    dialog_error.Content = "Existe un error en el formato de la fila usada para determinar si es lectura o escritura.";

                                    var continueButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Continue");
                                    dialog_error.CustomMainIcon = SystemIcons.Warning;
                                    dialog_error.Buttons.Add(continueButton);

                                    Ookii.Dialogs.Wpf.TaskDialogButton button = dialog_error.ShowDialog();
                                    //if (button == continueButton)
                                    return;
                                }
                            }
                            
                        }
                    }
                }
                if(listLect.Count != listDir.Count)
                {
                    //error dialog
                    using (var dialog_error = new Ookii.Dialogs.Wpf.TaskDialog())
                    {
                        dialog_error.WindowTitle = "Error de lectura";
                        dialog_error.Content = "El número de direccíones difiere del número de indicaciones de lectura y escritura";

                        var continueButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Continue");
                        dialog_error.CustomMainIcon = SystemIcons.Warning;
                        dialog_error.Buttons.Add(continueButton);

                        Ookii.Dialogs.Wpf.TaskDialogButton button = dialog_error.ShowDialog();
                        //if (button == continueButton)
                        return;
                    }
                }
                else
                {
                    for (int i = 0; i < listDir.Count; i++)
                    {
                        ParseInst tempInst = new ParseInst(listDir[i], listLect[i]);
                        instList.Add(tempInst);
                    }
                }
                
            }   

        }


    }

    public class ParseInst
    {
        public ParseInst(int dir, string lec)
        {
            this.dir = dir;
            this.lec = lec;
        }

        public static Instruc[] parse(ObservableCollection<ParseInst> p)
        {
            Instruc[] instruc = new Instruc[p.Count];
            for (int i = 0; i < p.Count; i++)
                instruc[i] = new Instruc(p[i].dir, p[i].lec == "L");
            return instruc;
        }

        public int dir { get; set; }
        public string lec { get; set; }
    }

    public class Mem
    {
        public Mem(int marco, string okupa)
        {
            this.marco = marco;
            this.okupa = okupa;
        }

        public static bool[] parse(ObservableCollection<Mem> m)
        {
            bool[] marcos = new bool[m.Count];
            for (int i = 0; i < m.Count; i++)
                marcos[i] = m[i].okupa == "Libre";
            return marcos;
        }

        public int marco { get; set; }
        public string okupa { get; set; }
    }
    public class MemSec
    {
        public MemSec(int num)
        {
            this.num = num;
        }

        public int num { get; set; }
    }
}
