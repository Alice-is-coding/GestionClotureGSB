using System;
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
        /// <include file = 'docGsb.xml' path='doc/members[@name="gsb"]/myTimer/*'/>
        private static void GetConnection()
        {
            Program.maCnx = MyTools.BDConnection.GetBDConnection("localhost", "gsb_frais", "root", "root");
        }

        // Gestion des affichages console lors de l'exécution de l'application.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/Execute2/*'/>
        private static void OnExecute()
        {
            //Console.WriteLine("Appuyez sur la touche 'Entrée' pour quitter l'application");
            Console.WriteLine("L'application a commencé le " + DateTime.Now +"\n");
            Console.ReadLine();
            //Console.WriteLine("Terminaison de l'application...");
            // Exécuté dès lors qu'on appuie sur une touche du clavier. 
            // Stoppe le timer.
            //myTimer.Stop();
            //Libération des ressources.
            //myTimer.Dispose();
            //Console.WriteLine("Fin.");
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
            Console.WriteLine("L'événement 'chronométrage' a été déclenché le "+ e.SignalTime +"\n");
            CloturerFicheFrais();
            RembourserFicheFrais();
        }

        // Campagne de validation des fiches de frais,
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/CloturerFicheFrais/*'/>
        private static void CloturerFicheFrais()
        {
            // Si on se trouve actuellement entre le 1er et le 10 :
            if (MyTools.DateManagement.Between(1, 10))
            {
                Console.WriteLine("Clôture des fiches de frais... ");
                // Connexion à la BDD.
                GetConnection();
                // Mise à jour de l'état de la fiche ("CR" à "CL").
                maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'CL' WHERE idetat = 'CR' AND mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "'");
            }else
            {
                Console.WriteLine("Pas de clôture : Nous ne sommes pas entre le 1er et le 10 du mois actuel.\n");
            }
        }

        // Information de remboursement de fiches de frais.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/RembourserFicheFrais/*'/>
        private static void RembourserFicheFrais()
        {
            // A partir du 20ème jour du mois : 
            if (MyTools.DateManagement.Between(20, 31))
            {
                Console.WriteLine("Remboursement des fiches de frais...");
                // Connexion à la BDD
                GetConnection();
                // Mise à jour de l'état de la fiche ("MP" à "RB").
                maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'RB' WHERE mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "' AND idetat = 'MP'");
            } else
            {
                Console.WriteLine("Pas de remboursement : Nous ne sommes pas entre le 20 et le dernier jour du mois actuel.\n");
            }
        }
        static void Main()
        {
            Program.Execute();
        }
    }
}
