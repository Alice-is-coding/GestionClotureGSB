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
    class Program
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

        // Paramétrage du timer en fonction de l'intervalle de déclenchement souhaité. 
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/ParametrageTimer/*'/>
        private static void ParametrageTimer(double interval)
        {
            // Paramétrage du timer.
            if (interval != 0)
            {
                // Contient une valeur fixée par l'appelant de l'application dans le main.
                SetTimer(interval);
            }
            else
            {
                // 0 envoyé par défaut dans l'appel de la méthode Application dans le main.
                // Dans ce cas là, le comportement par défaut du timer sera de se déclencher toutes les 2mins.
                SetTimer(120000);
            }
        }

        // Méthode appelée dans le Main : S'occupe du lancement de l'application.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/Application/*'/>
        public static void Application(double interval)
        {
            // Paramétrage du timer.
            ParametrageTimer(interval);

            Console.WriteLine("Appuyez sur la touche 'Entrée' pour quitter l'application");
            Console.WriteLine("L'application a commencé le " + DateTime.Now);
            Console.ReadLine();
            Console.WriteLine("Terminaison de l'application...");
            // Exécuté dès lors qu'on appuie sur une touche du clavier. 
            // Stoppe le timer.
            myTimer.Stop();
            // Libération des ressources.
            myTimer.Dispose();
            Console.WriteLine("Fin.");
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
        /// Contient les méthodes métier à exécuter.
        /// <include file='docGsb.xml' path='doc/members[@name="gsb"]/AuDeclenchementTimer/*'/>
        private static void AuDeclenchementTimer(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("\nl'événement 'chronométrage' a été déclenché le "+ e.SignalTime +"\n");
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
            }
        }

        // Information de remboursement de fiches de frais.
        /// <include file="docGsb.xml" path="doc/members[name='gsb']/RembourserFicheFrais/*"/>
        private static void RembourserFicheFrais()
        {
            // A partir du 20ème jour du mois : 
            if (MyTools.DateManagement.Between(9, 31))
            {
                Console.WriteLine("Remboursement des fiches de frais...");
                // Connexion à la BDD
                GetConnection();
                // Mise à jour de l'état de la fiche ("MP" à "RB").
                maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'RB' WHERE mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "' AND idetat = 'MP'");
            }
        }
        static void Main(string[] args)
        {
            // Lancement de l'application permettant de gérer la campagne de validation et de remboursement par les comptables.
            // [Informations] veuillez-laisser le paramètre à 0 si vous souhaitez que le timer se déclenche toutes les deux minutes.
            Application(0);
        }
    }
}
