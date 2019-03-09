using System;
using System.Timers;
using MySql.Data.MySqlClient; //à insérer au cas où la connexion échoue (testé sans et fonctionne sans normalement) du côté de BDConnection de la classe MyTools.
using MyTools;

namespace GestionClotureGSB
{
    /// <summary>
    /// Classe Program. 
    /// Contient l'ensemble des méthodes permettant d'assurer la gestion de la campagne de validation des fiches de frais par les comptables de la société GSB.
    /// L'ensemble de l'application est déclenché automatiquement par un timer.
    /// Permet la clôture des fiches du mois N-1 entre le 1er et le 10 du mois.
    /// Permet de modifier une fiche de frais remboursée à partir du 20ème jour du mois (modification de son état de "MP" à "RB").
    /// </summary>
    class Program
    {
        // Propriétés.
        /// <value>Prend la valeur de la date du jour.</value>
        private static DateTime actualDate = DateTime.Today;
        /// <value>Contiendra une instance de la classe BDConnection.</value>
        private static BDConnection maCnx;
        private static System.Timers.Timer myTimer;

        /// <summary>
        /// Valorise la propriété privée et statique maCnx avec une nouvelle instance de la classe BDConnection.
        /// </summary>
        private static void GetConnection()
        {
            Program.maCnx = MyTools.BDConnection.GetBDConnection("localhost", "gsb_frais", "root", "root");
        }

        /// <summary>
        /// Paramétrage du timer en fonction de l'intervalle de déclenchement souhaité. 
        /// Si 0 envoyé en paramètre : 
        /// Comportement par défaut du timer : déclenchement toutes les 2 mins.
        /// </summary>
        /// <param name="interval">Intervalle auquel doit se déclencher le timer.</param>
        private static void ParametrageTimer(double interval)
        {
            // Paramétrage du timer
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

        /// <summary>
        ///  Méthode appelée dans le Main.
        /// S'occupe du lancement de l'application.
        /// Fonctionnalités de debug avec affichages consoles et possibilité de stopper à tout moment le processus.
        /// </summary>
        /// <param name="interval">Interval auquel doit se déclencher le timer.</param>
        public static void Application(double interval)
        {
            // Paramétrage du timer.
            ParametrageTimer(interval);

            Console.WriteLine("Appuyez sur la touche 'Entrée' pour quitter l'application");
            Console.WriteLine("L'application a commencé le" + DateTime.Now);
            Console.ReadLine();
            Console.WriteLine("Terminaison de l'application...");
            // Exécuté dès lors qu'on appuie sur une touche du clavier. 
            // Stoppe le timer.
            myTimer.Stop();
            // Libération des ressources.
            myTimer.Dispose();
            Console.WriteLine("Fin.");
        }

        /// <summary>
        /// Paramètrage du timer. 
        /// Instanciation d'un timer avec l'intervalle envoyé en paramètre.
        /// Ajout d'un événement déclencheur : déclenchement du timer.
        /// </summary>
        /// <param name="interval">Intervalle auquel doit se déclencher le timer.</param>
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

        /// <summary>
        /// Evénement qui se déclenche lorsque le timer est lui-même déclenché.
        /// Contient les méthodes métier à exécuter.
        /// </summary>
        /// <param name="source">Source du déclenchement de l'événement.</param>
        /// <param name="e">Fournit des données pour l'événement Elapsed (déclenché).</param>
        private static void AuDeclenchementTimer(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("\nl'événement 'chronométrage' a été déclenché le "+ e.SignalTime +"\n");
            CloturerFicheFrais();
            RembourserFicheFrais();
        }

        /// <summary>
        /// Campagne de validation des fiches de frais,
        /// Si nous nous trouvons entre le 1 et le 10 du mois courant, 
        /// cloture des fiches du mois N-1 en passant leur état de "CR" à "CL".
        /// </summary>
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

        /// <summary>
        /// A partir du 20ème jour du mois,
        /// mise à jour de l'état de la fiche de frais ("MP" à "RB"),
        /// pour informer que celle-ci a bien été remboursée.
        /// </summary>
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
