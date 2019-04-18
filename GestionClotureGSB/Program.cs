/**
 * Script : Application console .NetCore pour la gestion automatique de la clôture de fiches de frais GSB.
 * Author : Alice BORD
 * Email : alice.bord1@gmail.com
 * Date : 31/03/2019
 */

using System;
using System.IO;
using System.Timers;
using MySql.Data.MySqlClient; //à insérer au cas où la connexion échoue (testé sans et fonctionne sans normalement) du côté de BDConnection de la classe MyTools.
using MyTools;

namespace GestionClotureGSB
{
    /*
     * Contient toutes les méthodes pour clôturer et mettre à remboursées les fiches de frais.
     */
    /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/Program/*'/>
    abstract class Program
    {
        // Propriétés.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/actualDate/*'/>
        private static DateTime actualDate = DateTime.Today;
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/maCnx/*'/>
        private static BDConnection maCnx;
        /// <include file = 'docGsb.xml' path='doc/members[@name="gsb"]/myTimer/*'/>
        private static System.Timers.Timer myTimer;

        // Connexion à la base de données.
        /// <include file = 'docGsb.xml' path='doc/members[@name="gsb"]/GetConnection/*'/>
        private static void GetConnection()
        {
            Program.maCnx = MyTools.BDConnection.GetBDConnection("localhost", "gsb_frais", "root", "root");
            //Program.maCnx = MyTools.BDConnection.GetBDConnection("sql105.byethost.com", "b3_23696328_gsb_frais", "b3_23696328", "btssioslam");
        }

        // Gestion des affichages console lors de l'exécution de l'application.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/OnExecute/*'/>
        private static void OnExecute()
        {
            //Console.WriteLine("Appuyez sur la touche 'Entrée' pour quitter l'application");
            //Console.WriteLine("L'application a commencé le " + DateTime.Now +"\n");
            WriteLog("L'application a commencé le " + DateTime.Now + "\n");

            Console.ReadLine();
            //Console.WriteLine("Terminaison de l'application...");
            WriteLog("Terminaison de l'application...\n");
            // Exécuté dès lors qu'on appuie sur une touche du clavier : 
            // Stoppe le timer.
            myTimer.Stop();
            //Libération des ressources.
            myTimer.Dispose();
            //Console.WriteLine("Fin.");
            WriteLog("Fin.");
        }

        // Exécuter l'application avec les paramètres par défaut du timer qui se déclenchera alors toutes les 2min.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/Execute2/*'/>
        public static void Execute() => Execute(120000);

        // Exécution de l'application en fixant un intervalle précis du délais de déclenchement du timer.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/Execute1/*'/>
        public static void Execute(double interval)
        {
            // Paramétrage du timer.
            SetTimer(interval);
            // Exé. de l'application.
            Program.OnExecute();
        }

        // Paramètrage du timer. 
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/SetTimer/*'/>
        private static void SetTimer(double interval)
        {
            // Création d'un timer avec envoi d'un intervalle en paramètre.
            myTimer = new System.Timers.Timer(interval);
            // Connexion de l'événement " au déclenchement du timer" sur le timer.
            myTimer.Elapsed += AuDeclenchementTimer;
            // Indique au timer qu'il doit se déclencher de manière répétée.
            myTimer.AutoReset = true;
            // Minuterie activée.
            myTimer.Enabled = true;
        }

        // Evénement qui se déclenche lorsque le timer est lui-même déclenché.
        // Contient les méthodes métier à exécuter.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/AuDeclenchementTimer/*'/>
        private static void AuDeclenchementTimer(Object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("L'événement 'chronométrage' a été déclenché le "+ e.SignalTime +"\n");
            WriteLog("L'événement 'chronométrage' a été déclenché le " + e.SignalTime + "\n");
            // Clôture des fiches de frais.
            CloturerFicheFrais();
            // Remboursement des fiches de frais.
            RembourserFicheFrais();
            // [Décommenter ci-dessous pour débug] Affichage du fichier de log à la fin de l'ensemble des opérations pour debug quant à la bonne ou mauvaise exécution des instructions.
            //ShowLog();
        }

        // Campagne de validation des fiches de frais,
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/CloturerFicheFrais/*'/>
        private static void CloturerFicheFrais()
        {
            // Si on se trouve actuellement entre le 1er et le 10 :
            if (MyTools.DateManagement.Between(1, 10))
            {
                //Console.WriteLine("Clôture des fiches de frais... ");
                WriteLog("Clôture des fiches de frais... ");
                // Connexion à la BDD.
                GetConnection();
                // Mise à jour de l'état de la fiche ("CR" à "CL").
                WriteLog(maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'CL' WHERE idetat = 'CR' AND mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "'"));
            } else
            {
                //Console.WriteLine("Pas de clôture : Nous ne sommes pas entre le 1er et le 10 du mois actuel.\n");
                WriteLog("Pas de clôture : Nous ne sommes pas entre le 1er et le 10 du mois actuel.\n");
            }
        }

        // Information de remboursement de fiches de frais.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/RembourserFicheFrais/*'/>
        private static void RembourserFicheFrais()
        {
            // A partir du 20ème jour du mois : 
            if (MyTools.DateManagement.Between(20, 31))
            {
                //Console.WriteLine("Remboursement des fiches de frais...");
                WriteLog("Remboursement des fiches de frais...");
                // Connexion à la BDD
                GetConnection();
                // Mise à jour de l'état de la fiche ("MP" à "RB").
                WriteLog(maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'RB' WHERE mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "' AND idetat = 'MP'"));
            } else
            {
                //Console.WriteLine("Pas de remboursement : Nous ne sommes pas entre le 20 et le dernier jour du mois actuel.\n");
                WriteLog("Pas de remboursement : Nous ne sommes pas entre le 20 et le dernier jour du mois actuel.\n");
            }
        }

        // Ecriture dans le fichier de logs comme un journal d'événements.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/WriteLog/*'/>
        private static void WriteLog(String textLog)
        {
            // Le fichier sera créé dans : C:\ProjetsVisualStudio\GestionClotureGSB\GestionClotureGSB\bin\Release\PublishOutput\netcoreapp2.1\systD'exploit
            // ou dans C:\Windows\SysWOW64 lorsque service Windows.
            using (StreamWriter w = File.AppendText("TestGSBService.txt"))
            {
                MyTools.DirAppend.Log(textLog, w);
            }
        }

        //Permet d'afficher dans la console le contenu du fichier de logs.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/ShowLog/*'/>
        private static void ShowLog()
        {
            using (StreamReader r = File.OpenText("TestGSBService.txt"))
            {
                MyTools.DirAppend.DumpLog(r);
            }
        }

        static void Main()
        {
            Program.Execute();
        }
    }
}
